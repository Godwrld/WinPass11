using DiscUtils;
using DiscUtils.Udf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinPass11.Helpers
{
    class Utils
    {
        public static void ExtractISO(string ISOName, string ExtractionPath)
        {
            using (FileStream ISOStream = File.Open(ISOName, FileMode.Open))
            {
                UdfReader Reader = new UdfReader(ISOStream);
                ExtractDirectory(Reader.Root, ExtractionPath + Path.GetFileNameWithoutExtension(ISOName) + "\\", "");
                Reader.Dispose();
            }
        }
        static void ExtractDirectory(DiscDirectoryInfo Dinfo, string RootPath, string PathinISO)
        {
            if (!string.IsNullOrWhiteSpace(PathinISO))
            {
                PathinISO += "\\" + Dinfo.Name;
            }
            RootPath += "\\" + Dinfo.Name;
            AppendDirectory(RootPath);
            foreach (DiscDirectoryInfo dinfo in Dinfo.GetDirectories())
            {
                ExtractDirectory(dinfo, RootPath, PathinISO);
            }
            foreach (DiscFileInfo finfo in Dinfo.GetFiles())
            {
                using (Stream FileStr = finfo.OpenRead())
                {
                    using (FileStream Fs = File.Create(RootPath + "\\" + finfo.Name)) // Here you can Set the BufferSize Also e.g. File.Create(RootPath + "\\" + finfo.Name, 4 * 1024)
                    {
                        FileStr.CopyTo(Fs, 4 * 1024); // Buffer Size is 4 * 1024 but you can modify it in your code as per your need
                    }
                }
            }
        }
        static void AppendDirectory(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch (DirectoryNotFoundException Ex)
            {
                AppendDirectory(Path.GetDirectoryName(path));
            }
            catch (PathTooLongException Exx)
            {
                AppendDirectory(Path.GetDirectoryName(path));
            }
        }
        public static void DownloadFile(string url, string dest, bool overwrite = false)
        {
            if (File.Exists(dest) && !overwrite)
                return;

            string dirName = Path.GetDirectoryName(dest);
            if (!Directory.Exists(dirName))
                Directory.CreateDirectory(dirName);

            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadFile(url, dest);
            }
        }

        public static int StartProcess(string exePath, string args, bool waitForExit = true)
        {
            Process process = new Process();
            process.StartInfo.FileName = exePath;
            process.StartInfo.Arguments = args;

            process.Start();

            if (waitForExit)
            {
                process.WaitForExit();
                return process.ExitCode;
            }
            else
                return 0;
        }

        public static void ShowMessageBox(string msg, MessageBoxType type)
        {
            string title = Strings.Titles.Information;
            MessageBoxIcon icon = MessageBoxIcon.Information;

            if (type == MessageBoxType.Error)
            {
                title = Strings.Titles.Error;
                icon = MessageBoxIcon.Error;
            }
            MessageBox.Show(msg, title, MessageBoxButtons.OK, icon);
        }
    }
}