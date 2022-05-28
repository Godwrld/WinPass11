using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinPass11.Helpers;

namespace WinPass11
{
    public partial class Form : System.Windows.Forms.Form
    {
        public static string mTempWorkingDir = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), "WinPass11");

        public Form()
        {
            InitializeComponent();
            InitializeWorkingDir();

            MaximizeBox = false;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterScreen;
        }

        private void InitializeWorkingDir()
        {
            try
            {
                if (Directory.Exists(mTempWorkingDir))
                    Directory.Delete(mTempWorkingDir, true);
                Directory.CreateDirectory(mTempWorkingDir);
            }
            catch { }
        }

        private void InstallButtonClick(object sender, EventArgs e)
        {
            string regTweaksDownloadPath = $@"{mTempWorkingDir}\regtweaks.reg";

            try
            {
                Utils.DownloadFile(Constants.Url.RegTweaks, regTweaksDownloadPath);
                Utils.ShowMessageBox(string.Format(Strings.Body.DownloadSuccess, "registry tweaks"), MessageBoxType.Information);
            }
            // Create an error box if download fails
            catch
            {
                Utils.ShowMessageBox(string.Format(Strings.Body.DownloadFailed, "registry tweaks"), MessageBoxType.Error);
            }

            if (selectionBox.Text == "Dev")
            {
                DialogResult result = MessageBox.Show(Strings.Body.InstallButtonDialog, "WinPass11 Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result.Equals(DialogResult.Yes))
                {
                    if (File.Exists(regTweaksDownloadPath))
                    {
                        try
                        {
                            string text = File.ReadAllText(regTweaksDownloadPath);
                            text = text.Replace("Beta", "Dev");
                            File.WriteAllText(regTweaksDownloadPath, text);
                        }
                        catch
                        {
                            Utils.ShowMessageBox($"{Strings.Body.RegApplyFailed}\nStage 1 Failed", MessageBoxType.Error);
                        }

                        try
                        {
                            int ret = Utils.StartProcess("regedit.exe", $"/s {regTweaksDownloadPath}", true);
                            Console.WriteLine("regedit exited with exit code of {0}", ret);
                            Utils.ShowMessageBox(Strings.Body.RegApplySuccess, MessageBoxType.Information);
                        }
                        // Create an error box if registry applicaation fails
                        catch
                        {
                            Utils.ShowMessageBox($"{Strings.Body.RegApplyFailed}\nStage 2 Failed", MessageBoxType.Error);
                        }
                    }
                    else
                    {
                        Utils.ShowMessageBox(Strings.Body.RegFileNotDownloaded, MessageBoxType.Error);
                    }
                    string usoClient = "UsoClient";
                    int ust = Utils.StartProcess(usoClient, "StartInteractiveScan", true);
                    Console.WriteLine($"{usoClient} exited with exit code of {0}", ust);

                    // debug: MessageBox.Show("Invoked System Update");
                    Handlers.AppraiserRes obj = new Handlers.AppraiserRes();

                    // Creating thread
                    // Using thread class
                    Thread thread = new Thread(new ThreadStart(obj.checkForExist));
                    thread.Start();
                }
                else
                {
                    Utils.ShowMessageBox(Strings.Body.InstallationCanceled, MessageBoxType.Information);
                }
            }
            else if (selectionBox.Text == "Beta") // For future releases
            {
                DialogResult result = MessageBox.Show(Strings.Body.InstallButtonDialog, "WinPass11 Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result.Equals(DialogResult.Yes))
                {
                    if (File.Exists(regTweaksDownloadPath))
                    {
                        try
                        {
                            string text = File.ReadAllText(regTweaksDownloadPath);
                            text = text.Replace("Dev", "Beta");
                            File.WriteAllText(regTweaksDownloadPath, text);
                        }
                        catch
                        {
                            Utils.ShowMessageBox($"{Strings.Body.RegApplyFailed}\nStage 1 Failed", MessageBoxType.Error);
                        }

                        try
                        {
                            int ret = Utils.StartProcess("regedit.exe", $"/s {regTweaksDownloadPath}", true);
                            Console.WriteLine("regedit exited with exit code of {0}", ret);
                            Utils.ShowMessageBox(Strings.Body.RegApplySuccess, MessageBoxType.Information);
                        }
                        // Create an error box if registry applicaation fails
                        catch
                        {
                            Utils.ShowMessageBox($"{Strings.Body.RegApplyFailed}\nStage 2 Failed", MessageBoxType.Error);
                        }
                    }
                    else
                    {
                        Utils.ShowMessageBox(Strings.Body.RegFileNotDownloaded, MessageBoxType.Error);
                    }
                    string usoClient = "UsoClient";
                    int ust = Utils.StartProcess(usoClient, "StartInteractiveScan", true);
                    Console.WriteLine($"{usoClient} exited with exit code of {0}", ust);

                    // debug: MessageBox.Show("Invoked System Update");
                    Handlers.AppraiserRes obj = new Handlers.AppraiserRes();

                    // Creating thread
                    // Using thread class
                    Thread thread = new Thread(new ThreadStart(obj.checkForExist));
                    thread.Start();
                }
                else
                {
                    Utils.ShowMessageBox(Strings.Body.InstallationCanceled, MessageBoxType.Information);
                }
            }
            else if (selectionBox.Text == "Release")
            {
                release();

            }
            else
            {
                Utils.ShowMessageBox(Strings.Body.InvalidChannel, MessageBoxType.Error);
            }
        }
        private async void release()
        {
            Utils.ShowMessageBox("This option will require you to download an ISO file from https://www.microsoft.com/en-us/software-download/windows11", MessageBoxType.Information);
            openFileDialog1.Filter = "Windows ISO (*.iso)|*.iso|All files (*.*)|*.*"; // get file

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {

                await Task.Run(() => Utils.ExtractISO(openFileDialog1.FileName, $@"{mTempWorkingDir}\ISO\")); // ISO extractor

                string[] pathparser1 = openFileDialog1.FileName.Split('\\');
                string extractdir = mTempWorkingDir + "\\ISO\\" + pathparser1[pathparser1.Length - 1].Remove(pathparser1[pathparser1.Length - 1].Length - 4, 4) + "\\"; // hellish filepath parsing

                File.Delete($"{extractdir}\\Sources\\appraiserres.dll"); // File Replacing
                Utils.DownloadFile(Constants.Url.AppraiserRes, $"{extractdir}\\Sources\\appraiserres.dll");

                Utils.ShowMessageBox("Continue Setup in Windows 11 Installer", MessageBoxType.Information);
                Utils.StartProcess($"{extractdir}\\Setup.exe", "");

                Utils.ShowMessageBox("VERY IMPORTANT!\r\n\r\nPlease Click \"Change how setup downloads updates\" and click \"Not Now\"", MessageBoxType.Information);

            }
        }
        private void selectionBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void openFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }
    }
}
