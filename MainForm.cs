using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KmbRouteDownloader
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            txtOutputPath.Text = AppDomain.CurrentDomain.BaseDirectory;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            folderBrowserDialog.SelectedPath = Directory.Exists(txtOutputPath.Text.Trim())
                ? txtOutputPath.Text.Trim()
                : AppDomain.CurrentDomain.BaseDirectory;

            if (folderBrowserDialog.ShowDialog(this) == DialogResult.OK)
            {
                txtOutputPath.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private async void btnDownload_Click(object sender, EventArgs e)
        {
            string outputFolder = txtOutputPath.Text.Trim();
            if (string.IsNullOrWhiteSpace(outputFolder))
            {
                MessageBox.Show(this, "Please choose an output folder.", "Missing folder path", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ToggleUi(false);
            txtLog.Clear();

            try
            {
                AppendLog("Downloading KMB route data...");

                var exporter = new KmbExporter(AppendLog);
                ExportResult result = await exporter.ExportAsync(outputFolder);

                AppendLog(string.Empty);
                AppendLog("Completed successfully.");
                AppendLog("Rows exported: " + result.RowCount);
                AppendLog("Files created: " + result.FileCount);
                AppendLog("Folder: " + outputFolder);

                MessageBox.Show(this, "Download completed and route CSV files have been created.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                AppendLog(string.Empty);
                AppendLog("Error: " + ex.Message);
                MessageBox.Show(this, ex.ToString(), "Download failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                ToggleUi(true);
            }
        }

        private void ToggleUi(bool enabled)
        {
            btnBrowse.Enabled = enabled;
            btnDownload.Enabled = enabled;
            txtOutputPath.Enabled = enabled;
            Cursor = enabled ? Cursors.Default : Cursors.WaitCursor;
        }

        private void AppendLog(string message)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<string>(AppendLog), message);
                return;
            }

            txtLog.AppendText(message + Environment.NewLine);
        }
    }
}
