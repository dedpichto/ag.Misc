using System;
using System.IO;
using System.Linq;
using Microsoft.Win32;

class FindCrystalFiles
{
    static void Main()
    {
        Console.WriteLine("=== Crystal Reports File Location Detective ===\n");
        
        // 1. Search entire system for Crystal Reports files
        SearchForCrystalFiles();
        
        Console.WriteLine("\n" + new string('=', 50) + "\n");
        
        // 2. Check what the registry thinks the files should be
        CheckRegistryPaths();
        
        Console.WriteLine("\n" + new string('=', 50) + "\n");
        
        // 3. Look in common installation directories
        CheckCommonLocations();
        
        Console.WriteLine("\n" + new string('=', 50) + "\n");
        
        // 4. Check GAC for .NET assemblies
        CheckGAC();
        
        Console.ReadLine();
    }
    
    static void SearchForCrystalFiles()
    {
        Console.WriteLine("1. Searching for Crystal Reports files on entire system...\n");
        
        string[] filesToFind = {
            "crpe32.dll",
            "craxdrt.dll", 
            "crqe32.dll",
            "CrystalDecisions.CrystalReports.Engine.dll",
            "CrystalDecisions.Shared.dll",
            "CrystalDecisions.Windows.Forms.dll"
        };
        
        string[] searchPaths = {
            @"C:\Windows\System32",
            @"C:\Windows\SysWOW64", 
            @"C:\Program Files",
            @"C:\Program Files (x86)",
            @"C:\Windows\Microsoft.NET\Framework",
            @"C:\Windows\Microsoft.NET\Framework64",
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
        };
        
        foreach (string fileName in filesToFind)
        {
            Console.WriteLine($"Searching for: {fileName}");
            bool found = false;
            
            foreach (string searchPath in searchPaths)
            {
                if (Directory.Exists(searchPath))
                {
                    try
                    {
                        var files = Directory.GetFiles(searchPath, fileName, SearchOption.AllDirectories);
                        foreach (var file in files)
                        {
                            Console.WriteLine($"  ✓ Found: {file}");
                            
                            // Get file info
                            var fileInfo = new FileInfo(file);
                            var versionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(file);
                            
                            Console.WriteLine($"    Size: {fileInfo.Length} bytes");
                            Console.WriteLine($"    Modified: {fileInfo.LastWriteTime}");
                            Console.WriteLine($"    Version: {versionInfo.FileVersion ?? "Unknown"}");
                            Console.WriteLine($"    Description: {versionInfo.FileDescription ?? "Unknown"}");
                            
                            found = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Skip directories we can't access
                        if (ex is UnauthorizedAccessException == false)
                        {
                            Console.WriteLine($"  Error searching {searchPath}: {ex.Message}");
                        }
                    }
                }
            }
            
            if (!found)
            {
                Console.WriteLine($"  ✗ Not found anywhere");
            }
            Console.WriteLine();
        }
    }
    
    static void CheckRegistryPaths()
    {
        Console.WriteLine("2. Checking what paths are in the registry...\n");
        
        // Get CLSID for CrystalRuntime.Application
        string clsid = GetCLSIDFromProgID("CrystalRuntime.Application");
        if (!string.IsNullOrEmpty(clsid))
        {
            Console.WriteLine($"CrystalRuntime.Application CLSID: {clsid}");
            CheckCLSIDPaths(clsid);
        }
        
        // Check other Crystal ProgIDs
        string[] progIds = {
            "Crystal.CrystalReport",
            "Crystal.CRPE.Application"
        };
        
        foreach (var progId in progIds)
        {
            string progClsid = GetCLSIDFromProgID(progId);
            if (!string.IsNullOrEmpty(progClsid))
            {
                Console.WriteLine($"\n{progId} CLSID: {progClsid}");
                CheckCLSIDPaths(progClsid);
            }
        }
    }
    
    static string GetCLSIDFromProgID(string progId)
    {
        try
        {
            using (var key = Registry.ClassesRoot.OpenSubKey($"{progId}\\CLSID"))
            {
                return key?.GetValue(null)?.ToString();
            }
        }
        catch
        {
            return null;
        }
    }
    
    static void CheckCLSIDPaths(string clsid)
    {
        try
        {
            using (var key = Registry.ClassesRoot.OpenSubKey($"CLSID\\{clsid}"))
            {
                if (key == null)
                {
                    Console.WriteLine("  ✗ CLSID not found in registry");
                    return;
                }
                
                // Check InprocServer32
                using (var inprocKey = key.OpenSubKey("InprocServer32"))
                {
                    if (inprocKey != null)
                    {
                        string dllPath = inprocKey.GetValue(null)?.ToString();
                        Console.WriteLine($"  InprocServer32: {dllPath}");
                        
                        if (!string.IsNullOrEmpty(dllPath))
                        {
                            bool exists = File.Exists(dllPath);
                            Console.WriteLine($"  File exists: {(exists ? "✓ YES" : "✗ NO")}");
                            
                            if (!exists)
                            {
                                // Try to find the file elsewhere
                                string fileName = Path.GetFileName(dllPath);
                                Console.WriteLine($"  Looking for {fileName} in other locations...");
                                
                                string[] searchDirs = {
                                    @"C:\Windows\System32",
                                    @"C:\Windows\SysWOW64",
                                    Environment.GetFolderPath(Environment.SpecialFolder.System),
                                    Environment.GetFolderPath(Environment.SpecialFolder.SystemX86)
                                };
                                
                                foreach (var dir in searchDirs)
                                {
                                    string altPath = Path.Combine(dir, fileName);
                                    if (File.Exists(altPath))
                                    {
                                        Console.WriteLine($"    ✓ Found alternative: {altPath}");
                                    }
                                }
                            }
                        }
                    }
                }
                
                // Check LocalServer32
                using (var localKey = key.OpenSubKey("LocalServer32"))
                {
                    if (localKey != null)
                    {
                        string exePath = localKey.GetValue(null)?.ToString();
                        Console.WriteLine($"  LocalServer32: {exePath}");
                        
                        if (!string.IsNullOrEmpty(exePath))
                        {
                            bool exists = File.Exists(exePath);
                            Console.WriteLine($"  File exists: {(exists ? "✓ YES" : "✗ NO")}");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Error: {ex.Message}");
        }
    }
    
    static void CheckCommonLocations()
    {
        Console.WriteLine("3. Checking common Crystal Reports installation locations...\n");
        
        string[] commonPaths = {
            @"C:\Program Files\SAP BusinessObjects",
            @"C:\Program Files (x86)\SAP BusinessObjects",
            @"C:\Program Files\Business Objects",
            @"C:\Program Files (x86)\Business Objects",
            @"C:\Program Files\Crystal Decisions",
            @"C:\Program Files (x86)\Crystal Decisions",
            @"C:\Program Files\Common Files\SAP BusinessObjects",
            @"C:\Program Files (x86)\Common Files\SAP BusinessObjects",
            @"C:\Program Files\Microsoft Visual Studio 8\Crystal Reports",
            @"C:\Program Files (x86)\Microsoft Visual Studio 8\Crystal Reports",
            @"C:\Program Files\Microsoft Visual Studio 9.0\Crystal Reports",
            @"C:\Program Files (x86)\Microsoft Visual Studio 9.0\Crystal Reports",
            @"C:\Program Files\Microsoft Visual Studio 10.0\Crystal Reports",
            @"C:\Program Files (x86)\Microsoft Visual Studio 10.0\Crystal Reports"
        };
        
        foreach (string path in commonPaths)
        {
            if (Directory.Exists(path))
            {
                Console.WriteLine($"✓ Found directory: {path}");
                
                try
                {
                    var subdirs = Directory.GetDirectories(path, "*", SearchOption.AllDirectories)
                        .Where(d => d.ToLower().Contains("bin") || d.ToLower().Contains("crystal"))
                        .Take(10);
                    
                    foreach (var subdir in subdirs)
                    {
                        Console.WriteLine($"  - {subdir}");
                        
                        // Look for DLLs in bin directories
                        if (subdir.ToLower().Contains("bin"))
                        {
                            try
                            {
                                var dlls = Directory.GetFiles(subdir, "*.dll");
                                var crystalDlls = dlls.Where(f => Path.GetFileName(f).ToLower().Contains("cr"));
                                foreach (var dll in crystalDlls.Take(5))
                                {
                                    Console.WriteLine($"    📄 {Path.GetFileName(dll)}");
                                }
                            }
                            catch { }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  Error exploring: {ex.Message}");
                }
                
                Console.WriteLine();
            }
        }
    }
    
    static void CheckGAC()
    {
        Console.WriteLine("4. Checking Global Assembly Cache for Crystal Reports .NET assemblies...\n");
        
        try
        {
            string gacPath = @"C:\Windows\Microsoft.NET\assembly";
            if (Directory.Exists(gacPath))
            {
                var crystalDirs = Directory.GetDirectories(gacPath, "*Crystal*", SearchOption.AllDirectories);
                
                foreach (var dir in crystalDirs)
                {
                    Console.WriteLine($"✓ Found in GAC: {dir}");
                }
                
                if (crystalDirs.Length == 0)
                {
                    Console.WriteLine("✗ No Crystal Reports assemblies found in GAC");
                }
            }
            
            // Also check legacy GAC
            string legacyGac = @"C:\Windows\assembly";
            if (Directory.Exists(legacyGac))
            {
                var legacyCrystal = Directory.GetDirectories(legacyGac, "*Crystal*", SearchOption.TopDirectoryOnly);
                
                foreach (var dir in legacyCrystal)
                {
                    Console.WriteLine($"✓ Found in legacy GAC: {dir}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking GAC: {ex.Message}");
        }
    }
}
