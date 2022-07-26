using Ionic.Zip;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HiveProfitSwitcher
{
    public class AppUpdater
    {
        public void HandleUpdate()
        {
            if (Directory.Exists("Update"))
            {
                Console.WriteLine("Deleting update folder");
                Directory.Delete("Update", true);
            }
            if (File.Exists("Update.cmd"))
            {
                File.Delete("Update.cmd");
            }
            if (File.Exists("Update.sh"))
            {
                File.Delete("Update.sh");
            }
            if (File.Exists("release.zip"))
            {
                File.Delete("release.zip");
            }
            bool autoUpdate = Convert.ToBoolean(ConfigurationManager.AppSettings["AutoUpdate"]);
            string autoUpdateUrl = ConfigurationManager.AppSettings["UpdateUrl"];
            var releaseApi = ConfigurationManager.AppSettings["ReleaseApi"];
            if (autoUpdate && !String.IsNullOrEmpty(autoUpdateUrl) && !String.IsNullOrEmpty(releaseApi))
            {
                if (!string.IsNullOrEmpty(autoUpdateUrl))
                {
                    var currentRelease = "";
                    if (!File.Exists("version.txt"))
                    {
                        using (var writer = new StreamWriter("version.txt"))
                        {
                            writer.Write("v.0.0.1");
                        }
                    }
                    currentRelease = File.ReadAllText("version.txt").Trim();
                    if (String.IsNullOrEmpty(currentRelease))
                    {
                        currentRelease = "v.0.0.1";
                    }

                    RestClient client = new RestClient(releaseApi);
                    RestRequest request = new RestRequest("");
                    var response = client.Get(request);
                    dynamic responseContent = JsonConvert.DeserializeObject(response.Content);
                    var releaseTag = responseContent.tag_name.Value;
                    Console.WriteLine("Current Version: " + currentRelease);
                    Console.WriteLine("New Version: " + releaseTag);

                    if (currentRelease != releaseTag)
                    {
                        string configPath = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).FilePath;
                        string configFileName = new FileInfo(configPath).Name;
                        new WebClient().DownloadFile(autoUpdateUrl, "release.zip");
                        var updateFolder = Path.Combine(Directory.GetCurrentDirectory(), "Update");
                        Console.WriteLine(updateFolder);

                        if (!Directory.Exists(updateFolder))
                        {
                            Directory.CreateDirectory(updateFolder);
                        }
                        using (ZipFile zipFile = ZipFile.Read("release.zip"))
                        {
                            foreach (ZipEntry contentFile in zipFile)
                            {
                                contentFile.Extract(updateFolder, ExtractExistingFileAction.OverwriteSilently);
                            }
                            File.Copy(configPath, Path.Combine(updateFolder, configFileName), true);
                            using (var writer = new StreamWriter(Path.Combine(updateFolder, "version.txt")))
                            {
                                writer.Write(releaseTag);
                            }
                        }

                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                        {
                            using (var batFile = new StreamWriter("Update.sh"))
                            {
                                batFile.WriteLine("sleep 1s");
                                batFile.WriteLine("killall \"{0}\"", new FileInfo(Environment.GetCommandLineArgs()[0]).Name);
                                batFile.WriteLine("mv {0} {1}", Path.Combine(updateFolder, "*"), new DirectoryInfo(updateFolder).Parent.FullName);
                                batFile.WriteLine("mono \"{0}\"", Environment.GetCommandLineArgs()[0]);
                            }

                            ProcessStartInfo chmodStartInfo = new ProcessStartInfo("chmod");
                            chmodStartInfo.Arguments = String.Format("+x {0}", new FileInfo("Update.sh").FullName);
                            chmodStartInfo.CreateNoWindow = true;
                            chmodStartInfo.UseShellExecute = true;
                            chmodStartInfo.WorkingDirectory = new DirectoryInfo(updateFolder).Parent.FullName;
                            Process.Start(chmodStartInfo).WaitForExit();

                            ProcessStartInfo execStartInfo = new ProcessStartInfo("sh");
                            execStartInfo.Arguments = new FileInfo("Update.sh").FullName;
                            execStartInfo.CreateNoWindow = true;
                            execStartInfo.UseShellExecute = true;
                            execStartInfo.WorkingDirectory = new DirectoryInfo(updateFolder).Parent.FullName;
                            Process.Start(execStartInfo);
                        }
                        else
                        {
                            using (var batFile = new StreamWriter("Update.cmd"))
                            {
                                batFile.WriteLine("@ECHO OFF");
                                batFile.WriteLine("TIMEOUT /t 1 /nobreak");
                                batFile.WriteLine("TASKKILL /IM \"{0}\"", new FileInfo(Process.GetCurrentProcess().MainModule.FileName).Name);
                                batFile.WriteLine("MOVE \"{0}\" \"{1}\"", Path.Combine(updateFolder, "*"), new DirectoryInfo(updateFolder).Parent.FullName);
                                batFile.WriteLine("START \"\" /B \"{0}\"", Process.GetCurrentProcess().MainModule.FileName);
                            }
                            ProcessStartInfo startInfo = new ProcessStartInfo(new FileInfo(Path.Combine(new DirectoryInfo(updateFolder).Parent.FullName, "Update.cmd")).FullName);
                            startInfo.CreateNoWindow = true;
                            startInfo.UseShellExecute = true;
                            startInfo.WorkingDirectory = new DirectoryInfo(updateFolder).Parent.FullName;
                            Process.Start(startInfo);
                        }                        

                        Environment.Exit(0);
                    }
                }
            }
        }
    }
}
