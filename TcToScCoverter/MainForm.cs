using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using KmbTextConversion;

namespace TcToScCoverter
{
    public partial class MainForm : Form
    {
        private readonly IChineseScriptConverter _converter;

        public MainForm()
        {
            InitializeComponent();
            _converter = new ChineseScriptConverter();
        }

        private void btnImportText_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            txtOriginal.Text = File.ReadAllText(openFileDialog.FileName, Encoding.UTF8);
            txtConverted.Clear();
        }

        private void btnConvert_Click(object sender, EventArgs e)
        {
            txtConverted.Text = _converter.ToSimplified(txtOriginal.Text);
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtOriginal.Clear();
            txtConverted.Clear();
        }

        private void btnCopyConverted_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtConverted.Text))
            {
                MessageBox.Show(this, "There is no converted text to copy.", "Copy converted text", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Clipboard.SetText(txtConverted.Text);
            MessageBox.Show(this, "Converted text copied to clipboard.", "Copy converted text", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnPasteClipboard_Click(object sender, EventArgs e)
        {
            if (!Clipboard.ContainsText())
            {
                MessageBox.Show(this, "Clipboard does not contain text.", "Paste clipboard", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            txtOriginal.Text = Clipboard.GetText();
            txtConverted.Clear();
        }

        private void btnExportText_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtConverted.Text))
            {
                MessageBox.Show(this, "There is no converted text to export.", "Export converted text", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (saveFileDialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            File.WriteAllText(saveFileDialog.FileName, txtConverted.Text, Encoding.UTF8);
            MessageBox.Show(this, "Converted text exported successfully.", "Export converted text", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files == null || files.Length != 1 || !IsSupportedTextFile(files[0]))
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            e.Effect = DragDropEffects.Copy;
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files == null || files.Length != 1 || !IsSupportedTextFile(files[0]))
            {
                MessageBox.Show(this, "Please drop a single .txt or .csv file.", "Drag and drop", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            txtOriginal.Text = File.ReadAllText(files[0], Encoding.UTF8);
            txtConverted.Clear();
        }

        private static bool IsSupportedTextFile(string filePath)
        {
            string extension = Path.GetExtension(filePath);
            return string.Equals(extension, ".txt", StringComparison.OrdinalIgnoreCase)
                || string.Equals(extension, ".csv", StringComparison.OrdinalIgnoreCase);
        }
    }
}
