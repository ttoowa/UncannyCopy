using System.Diagnostics;
using System.Text;
using Newtonsoft.Json.Linq;

namespace UncannyCopy; 

public class Program {
    public static void Main(string[] args) {
        List<string> configFileList = new List<string>();
        foreach (var arg in args) {
            string actualArg = arg.Trim();
            if (actualArg.EndsWith(".json")) {
                configFileList.Add(actualArg);
            }
        }

        if (configFileList.Count == 0) {
            ExecuteCopy();
        } else {
            foreach (var configFile in configFileList) {
                ExecuteCopy(configFile);
            }
        }
        
        Console.ReadLine();
    }

    private static void ExecuteCopy(string configFile = null) {
        var currentAppFilename = Process.GetCurrentProcess().MainModule.FileName;
        var currentAppFilenameOnly = Path.GetFileNameWithoutExtension(currentAppFilename);
        var currentAppDir = Path.GetDirectoryName(currentAppFilename);
        
        if (configFile == null) {
            configFile = Path.Combine(currentAppDir, $"{currentAppFilenameOnly}.json");
        }
        
        string configFilenameOnly = Path.GetFileName(configFile);

        Directory.SetCurrentDirectory(currentAppDir);

        if (!File.Exists(configFile)) {
            Console.WriteLine($"Config file not found. '{configFile}'");
            return;
        }

        var configJson = File.ReadAllText(configFile, Encoding.UTF8);
        var jRoot = JObject.Parse(configJson);
        var jItemGroups = jRoot["itemGroups"] as JArray;
        var index = 0;
        foreach (var jItemGroup in jItemGroups) {
            var srcDir = jItemGroup["srcDir"].ToObject<string>();
            var dstDir = jItemGroup["dstDir"].ToObject<string>();

            Console.WriteLine("───────────────────────────────────────");
            Console.WriteLine($"[ItemGroup {index++}] \n    Src : '{srcDir}'\n    Dst : '{dstDir}'");

            var jFilenames = jItemGroup["filenames"] as JArray;
            foreach (JValue jFilename in jFilenames) {
                var filename = jFilename.ToObject<string>();

                var srcFilename = Path.Combine(srcDir, filename);
                var dstFilename = Path.Combine(dstDir, filename);

                if (!File.Exists(srcFilename)) {
                    Console.WriteLine($"SrcFile '{filename}' not found.");
                    return;
                }

                Console.Write($"Copy {filename}... ");

                var dstSubDir = Path.GetDirectoryName(dstFilename);
                if (!Directory.Exists(dstSubDir)) Directory.CreateDirectory(dstSubDir);
                if (new FileInfo(srcFilename).LastWriteTimeUtc > new FileInfo(dstFilename).LastWriteTimeUtc)
                    try {
                        File.Copy(srcFilename, dstFilename, true);
                        Console.Write("Complete");
                    }
                    catch (Exception ex) {
                        Console.Write("Error");
                    }
                else
                    Console.Write("Skip");

                Console.WriteLine();
            }
        }

        Console.WriteLine($"Task completed for '{configFilenameOnly}'");
    }
}