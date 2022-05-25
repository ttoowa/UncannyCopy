using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UncannyUpdater {
	public class Program {
		public static void Main(string[] args) {
			UpdateModules();

			Console.ReadLine();
		}
		private static void UpdateModules() {
			string currentAppFilename = Process.GetCurrentProcess().MainModule.FileName;
			string currentAppFilenameOnly = Path.GetFileNameWithoutExtension(currentAppFilename);
			string currentAppDir = Path.GetDirectoryName(currentAppFilename);
			string configFilename = Path.Combine(currentAppDir, $"{currentAppFilenameOnly}.config");

			Directory.SetCurrentDirectory(currentAppDir);

			if (!File.Exists(configFilename)) {
				Console.WriteLine("Config file not found.");
				return;
			}

			string configJson = File.ReadAllText(configFilename, Encoding.UTF8);
			JObject jConfig = JObject.Parse(configJson);
			string srcDir = jConfig["srcDir"].ToObject<string>();
			string dstDir = jConfig["dstDir"].ToObject<string>();
			JArray jFilenames = jConfig["filenames"] as JArray;
			foreach (JValue jFilename in jFilenames) {
				string filename = jFilename.ToObject<string>();

				string srcFilename = Path.Combine(srcDir, filename);
				string dstFilename = Path.Combine(dstDir, filename);

				if (File.Exists(srcFilename)) {
					Console.WriteLine($"SrcFile '{filename}' not found.");
					return;
				}

				Console.Write($"Copy {filename}... ");

				string dstSubDir = Path.GetDirectoryName(dstFilename);
				if (!Directory.Exists(dstSubDir)) {
					Directory.CreateDirectory(dstSubDir);
				}
				if (new FileInfo(srcFilename).LastWriteTimeUtc > new FileInfo(dstFilename).LastWriteTimeUtc) {
					try {
						File.Copy(srcFilename, dstFilename, true);
						Console.Write("Complete");
					} catch (Exception ex) {
						Console.Write("Error");
					}
				} else {
					Console.Write("Skip");
				}
				Console.WriteLine();
			}

			Console.WriteLine("All task completed.");
		}
	}
}
