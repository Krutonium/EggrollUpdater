using System;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;


namespace EggrollUpdater
{
    class Program
    {
        static WebClient client = new WebClient();
        private static bool downloadDone = false;
        static void Main(string[] args)
        {
            client.Headers["User-Agent"] = "Mozilla/5.0 (X11; Linux i686; rv:90.0) Gecko/20100101 Firefox/90.0";
            
            var Release = JsonConvert.DeserializeObject<Release>(
                client.DownloadString(
                    "https://api.github.com/repos/" +
                    "GloriousEggroll/proton-ge-custom/releases/latest"));
            
            Console.WriteLine("Latest Release is " + Release.name);
            Console.WriteLine("Downloading...");
            client.DownloadProgressChanged += ClientOnDownloadProgressChanged;

            string DownloadedRelease = "";
            foreach (var item in Release.assets)
            {
                if (item.name.ToUpper().EndsWith(".TAR.GZ"))
                {
                    bar = new ProgressBar();
                    client.DownloadFileAsync(new Uri(item.browser_download_url),  item.name);
                    DownloadedRelease = item.name;
                }
            }

            if (DownloadedRelease == "")
            {
                Console.WriteLine("GitHub didn't give us a Download?!");
                Environment.Exit(1);
            }
            while (downloadDone == false)
            {
                System.Threading.Thread.Sleep(100);
            }
            bar.Dispose();
            Console.WriteLine("Unpacking...");
            Program program = new Program();
            
            var ExtractPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".steam/root/compatibilitytools.d/", Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(DownloadedRelease)));
            Console.WriteLine(ExtractPath);
            program.ExtractTGZ(DownloadedRelease, ExtractPath);
            Console.WriteLine("Done! Just restart steam!");
            Environment.Exit(0);
        }

        private static ProgressBar bar;
        private static void ClientOnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            bar.Report((double) e.ProgressPercentage / 100);
            if (e.ProgressPercentage == 100)
            {
                downloadDone = true;
            }
        }
        public void ExtractTGZ(String gzArchiveName, String destFolder)
        {
            Stream inStream = File.OpenRead(gzArchiveName);
            Stream gzipStream = new GZipInputStream(inStream);
            
            TarArchive tarArchive = TarArchive.CreateInputTarArchive(gzipStream);
            tarArchive.ExtractContents(destFolder);
            tarArchive.Close();
            
            gzipStream.Close();
            inStream.Close();
        }

    }
}
