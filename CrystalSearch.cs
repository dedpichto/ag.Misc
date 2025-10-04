using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Win32;

class RegistryCrystalSearch
{
    private static List<string> results = new List<string>();
    private static int keysScanned = 0;
    private static int matchesFound = 0;

    static void Main()
    {
        Console.WriteLine("=== Registry Search for 'Crystal' ===");
        Console.WriteLine("This may take several minutes...\n");

        DateTime startTime = DateTime.Now;

        // Search all major hives
        SearchHive(Registry.ClassesRoot, "HKEY_CLASSES_ROOT");
        SearchHive(Registry.CurrentUser, "HKEY_CURRENT_USER");
        SearchHive(Registry.LocalMachine, "HKEY_LOCAL_MACHINE");
        SearchHive(Registry.Users, "HKEY_USERS");
        SearchHive(Registry.CurrentConfig, "HKEY_CURRENT_CONFIG");

        DateTime endTime = DateTime.Now;
        TimeSpan duration = endTime - startTime;

        Console.WriteLine($"\n\n=== Search Complete ===");
        Console.WriteLine($"Keys scanned: {keysScanned:N0}");
        Console.WriteLine($"Matches found: {matchesFound:N0}");
        Console.WriteLine($"Time taken: {duration.TotalSeconds:F1} seconds\n");

        // Display results
        if (results.Count > 0)
        {
            Console.WriteLine("=== Results ===\n");
            foreach (var result in results)
            {
                Console.WriteLine(result);
            }

            // Save to file
            string outputFile = "CrystalRegistrySearch.txt";
            File.WriteAllLines(outputFile, results);
            Console.WriteLine($"\n\nResults saved to: {Path.GetFullPath(outputFile)}");
        }
        else
        {
            Console.WriteLine("No matches found.");
        }

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }

    static void SearchHive(RegistryKey hive, string hiveName)
    {
        Console.WriteLine($"\nSearching {hiveName}...");

        try
        {
            SearchKey(hive, hiveName, "");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error searching {hiveName}: {ex.Message}");
        }
    }

    static void SearchKey(RegistryKey key, string hiveName, string subKeyPath)
    {
        if (key == null) return;

        keysScanned++;

        // Show progress every 1000 keys
        if (keysScanned % 1000 == 0)
        {
            Console.Write($"\rScanned: {keysScanned:N0} keys, Found: {matchesFound:N0} matches");
        }

        string fullPath = string.IsNullOrEmpty(subKeyPath)
            ? hiveName
            : $"{hiveName}\\{subKeyPath}";

        try
        {
            // Check if key name contains "crystal"
            if (key.Name.ToLower().Contains("crystal"))
            {
                matchesFound++;
                results.Add($"\n[KEY] {fullPath}");
            }

            // Check all value names and data
            string[] valueNames = key.GetValueNames();
            foreach (string valueName in valueNames)
            {
                bool valueNameMatch = valueName.ToLower().Contains("crystal");

                try
                {
                    object valueData = key.GetValue(valueName);
                    string valueDataStr = valueData?.ToString() ?? "";
                    bool valueDataMatch = valueDataStr.ToLower().Contains("crystal");

                    if (valueNameMatch || valueDataMatch)
                    {
                        matchesFound++;

                        string displayName = string.IsNullOrEmpty(valueName) ? "(Default)" : valueName;
                        RegistryValueKind kind = key.GetValueKind(valueName);

                        StringBuilder resultBuilder = new StringBuilder();
                        resultBuilder.AppendLine($"\n[VALUE] {fullPath}");
                        resultBuilder.AppendLine($"  Name: {displayName}");
                        resultBuilder.AppendLine($"  Type: {kind}");

                        // Format the data based on type
                        if (kind == RegistryValueKind.Binary)
                        {
                            byte[] binaryData = (byte[])valueData;
                            if (binaryData.Length <= 32)
                            {
                                resultBuilder.AppendLine($"  Data: {BitConverter.ToString(binaryData)}");
                            }
                            else
                            {
                                resultBuilder.AppendLine($"  Data: [Binary data, {binaryData.Length} bytes]");
                            }
                        }
                        else if (kind == RegistryValueKind.MultiString)
                        {
                            string[] multiString = (string[])valueData;
                            resultBuilder.AppendLine($"  Data:");
                            foreach (string str in multiString.Take(10))
                            {
                                resultBuilder.AppendLine($"    - {str}");
                            }
                            if (multiString.Length > 10)
                            {
                                resultBuilder.AppendLine($"    ... and {multiString.Length - 10} more");
                            }
                        }
                        else
                        {
                            string dataStr = valueDataStr;
                            if (dataStr.Length > 200)
                            {
                                dataStr = dataStr.Substring(0, 200) + "...";
                            }
                            resultBuilder.AppendLine($"  Data: {dataStr}");
                        }

                        results.Add(resultBuilder.ToString().TrimEnd());
                    }
                }
                catch
                {
                    // Skip values we can't read
                }
            }

            // Recursively search subkeys
            string[] subKeyNames = key.GetSubKeyNames();
            foreach (string subKeyName in subKeyNames)
            {
                try
                {
                    string newSubKeyPath = string.IsNullOrEmpty(subKeyPath)
                        ? subKeyName
                        : $"{subKeyPath}\\{subKeyName}";

                    using (RegistryKey subKey = key.OpenSubKey(subKeyName, false))
                    {
                        SearchKey(subKey, hiveName, newSubKeyPath);
                    }
                }
                catch (System.Security.SecurityException)
                {
                    // Skip keys we don't have permission to read
                }
                catch (UnauthorizedAccessException)
                {
                    // Skip keys we don't have permission to read
                }
                catch
                {
                    // Skip other errors
                }
            }
        }
        catch (System.Security.SecurityException)
        {
            // Skip keys we don't have permission to read
        }
        catch (UnauthorizedAccessException)
        {
            // Skip keys we don't have permission to read
        }
        catch
        {
            // Skip other errors
        }
    }
}

/* 
ALTERNATIVE: PowerShell version (paste in PowerShell as Administrator):

$results = @()

Write-Host "Searching HKEY_CLASSES_ROOT..." -ForegroundColor Yellow
Get-ChildItem -Path "Registry::HKEY_CLASSES_ROOT" -Recurse -ErrorAction SilentlyContinue | 
    Where-Object { $_.Name -like "*crystal*" } | 
    ForEach-Object { $results += $_.Name }

Write-Host "Searching HKEY_LOCAL_MACHINE..." -ForegroundColor Yellow
Get-ChildItem -Path "Registry::HKEY_LOCAL_MACHINE" -Recurse -ErrorAction SilentlyContinue | 
    Where-Object { $_.Name -like "*crystal*" } | 
    ForEach-Object { $results += $_.Name }

Write-Host "Searching HKEY_CURRENT_USER..." -ForegroundColor Yellow
Get-ChildItem -Path "Registry::HKEY_CURRENT_USER" -Recurse -ErrorAction SilentlyContinue | 
    Where-Object { $_.Name -like "*crystal*" } | 
    ForEach-Object { $results += $_.Name }

$results | Out-File -FilePath "CrystalRegistryKeys.txt"
Write-Host "`nFound $($results.Count) matches" -ForegroundColor Green
Write-Host "Results saved to: CrystalRegistryKeys.txt"

# Also search for values containing "crystal"
Write-Host "`nSearching for values containing 'crystal'..." -ForegroundColor Yellow
Get-ItemProperty -Path "Registry::HKEY_CLASSES_ROOT\*" -ErrorAction SilentlyContinue | 
    Where-Object { $_.PSObject.Properties.Name -like "*crystal*" -or $_.PSObject.Properties.Value -like "*crystal*" }
*/
