using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;

class CrystalReportsDiscovery
{
    static void Main()
    {
        /Console.WriteLine("=== Crystal Reports COM Object Discovery ===\n");

        //// 1. Find all Crystal-related ProgIDs
        //FindCrystalProgIDs();

        //Console.WriteLine("\n" + new string('=', 50) + "\n");

        //// 2. Test common Crystal Reports ProgIDs
        //TestCommonProgIDs();

        //Console.WriteLine("\n" + new string('=', 50) + "\n");

        //// 3. Check for Crystal Reports installations
        //CheckCrystalInstallations();

        string[] cr10ProgIDs = {
    "CrystalRuntime.Application.10",
    "CrystalRuntime.Application",
    "Crystal.CRPE.Application.10",
    "Crystal.CRPE.Application",
    "Crystal.CrystalReport.10",
    "Crystal.CrystalReport"
};

        dynamic crApp = null;
        foreach (var progId in cr10ProgIDs)
        {
            try
            {
                Console.WriteLine($"Trying: {progId}");
                crApp = Activator.CreateInstance(Type.GetTypeFromProgID(progId));
                Console.WriteLine($"✓ Success with: {progId}");
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Failed: {ex.Message}");
            }
        }

        Console.ReadLine();
    }

    static void FindCrystalProgIDs()
    {
        Console.WriteLine("1. Scanning registry for Crystal-related ProgIDs...\n");

        try
        {
            using (var classesRoot = Registry.ClassesRoot)
            {
                var crystalKeys = classesRoot.GetSubKeyNames()
                    .Where(name => name.ToLower().Contains("crystal") ||
                                  name.ToLower().Contains("crpe") ||
                                  name.ToLower().Contains("sap") ||
                                  name.ToLower().Contains("businessobjects"))
                    .OrderBy(x => x)
                    .ToList();

                if (crystalKeys.Any())
                {
                    foreach (var key in crystalKeys)
                    {
                        Console.WriteLine($"Found ProgID: {key}");

                        // Try to get CLSID
                        try
                        {
                            using (var clsidKey = classesRoot.OpenSubKey(key + "\\CLSID"))
                            {
                                if (clsidKey != null)
                                {
                                    string clsid = clsidKey.GetValue(null)?.ToString();
                                    Console.WriteLine($"  CLSID: {clsid}");

                                    // Test if we can get the type
                                    var type = Type.GetTypeFromProgID(key);
                                    if (type != null)
                                    {
                                        Console.WriteLine($"  Type: {type.FullName}");
                                        Console.WriteLine($"  Is COM Object: {type.IsCOMObject}");
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"  Error getting CLSID: {ex.Message}");
                        }

                        Console.WriteLine();
                    }
                }
                else
                {
                    Console.WriteLine("No Crystal Reports ProgIDs found in registry.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error scanning registry: {ex.Message}");
        }
    }

    static void TestCommonProgIDs()
    {
        Console.WriteLine("2. Testing common Crystal Reports ProgIDs...\n");

        string[] commonProgIDs = {
            "Crystal.CRPE.Application",
            "CrystalRuntime.Application",
            "CrystalRuntime.Application.11",
            "CrystalRuntime.Application.12",
            "CrystalRuntime.Application.13",
            "Crystal.CrystalReport",
            "Crystal.CrystalReport.11",
            "Crystal.CrystalReport.12",
            "Crystal.CrystalReport.13",
            "Crystal.Application",
            "SAP.CrystalReports.Engine",
            "CrystalDecisions.CrystalReports.Engine"
        };

        foreach (var progId in commonProgIDs)
        {
            Console.Write($"Testing {progId}... ");

            try
            {
                var type = Type.GetTypeFromProgID(progId);
                if (type != null)
                {
                    Console.WriteLine("✓ Type found");

                    // Try to create instance
                    try
                    {
                        var instance = Activator.CreateInstance(type);
                        Console.WriteLine($"  ✓ Instance created successfully!");

                        // Clean up
                        if (System.Runtime.InteropServices.Marshal.IsComObject(instance))
                        {
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(instance);
                        }
                    }
                    catch (Exception createEx)
                    {
                        Console.WriteLine($"  ✗ Instance creation failed: {createEx.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("✗ Type not found");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error: {ex.Message}");
            }
        }
    }

    static void CheckCrystalInstallations()
    {
        Console.WriteLine("3. Checking for Crystal Reports installations...\n");

        // Check common installation paths
        string[] paths = {
            @"C:\Program Files\SAP BusinessObjects",
            @"C:\Program Files (x86)\SAP BusinessObjects",
            @"C:\Program Files\Common Files\Crystal Decisions",
            @"C:\Program Files (x86)\Common Files\Crystal Decisions",
            @"C:\Program Files\Microsoft Visual Studio*\Crystal Reports",
            @"C:\Windows\System32\crpe32.dll",
            @"C:\Windows\SysWOW64\crpe32.dll"
        };

        foreach (var path in paths)
        {
            if (System.IO.Directory.Exists(path) || System.IO.File.Exists(path))
            {
                Console.WriteLine($"✓ Found: {path}");
            }
        }

        // Check registry for installation info
        try
        {
            CheckRegistryPath(@"HKEY_LOCAL_MACHINE\SOFTWARE\Crystal Decisions");
            CheckRegistryPath(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Crystal Decisions");
            CheckRegistryPath(@"HKEY_LOCAL_MACHINE\SOFTWARE\SAP BusinessObjects");
            CheckRegistryPath(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\SAP BusinessObjects");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking registry installations: {ex.Message}");
        }
    }

    static void CheckRegistryPath(string path)
    {
        try
        {
            var parts = path.Split('\\');
            var hive = parts[0] switch
            {
                "HKEY_LOCAL_MACHINE" => Registry.LocalMachine,
                "HKEY_CURRENT_USER" => Registry.CurrentUser,
                _ => Registry.LocalMachine
            };

            var keyPath = string.Join("\\", parts.Skip(1));

            using (var key = hive.OpenSubKey(keyPath))
            {
                if (key != null)
                {
                    Console.WriteLine($"✓ Registry key found: {path}");

                    // List subkeys
                    var subkeys = key.GetSubKeyNames();
                    foreach (var subkey in subkeys.Take(5)) // Show first 5
                    {
                        Console.WriteLine($"  - {subkey}");
                    }
                    if (subkeys.Length > 5)
                    {
                        Console.WriteLine($"  ... and {subkeys.Length - 5} more");
                    }
                }
            }
        }
        catch
        {
            // Key doesn't exist or access denied
        }
    }

}
