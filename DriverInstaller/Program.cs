using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DriverInstaller
{
    class Program
    {
        static void Main(string[] args)
        {
            using (WebClient wc = new WebClient())
            {
                Console.WriteLine("Скачивание драйвера");
                wc.DownloadFile("https://github.com/Nekiplay/MacrosAPI-v2/raw/master/Driver/Driver.zip", "Driver.zip");

                if (File.Exists("Driver.zip"))
                {
                    Directory.CreateDirectory("Driver");
                    Console.WriteLine("Распоковка драйвера");
                    try { ZipFile.ExtractToDirectory("Driver.zip", "Files"); } catch { }
                    try { Process.Start("Files\\Driver\\InstallDriver.bat"); } catch { }
                }
            }
        }
    }
}
