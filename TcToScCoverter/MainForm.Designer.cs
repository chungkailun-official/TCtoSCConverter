namespace TcToScCoverter
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Button btnImportText;
        private System.Windows.Forms.Button btnConvert;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnCopyConverted;
        private System.Windows.Forms.Button btnPasteClipboard;
        private System.Windows.Forms.Button btnExportText;
        private System.Windows.Forms.TextBox txtOriginal;
        private System.Windows.Forms.TextBox txtConverted;
        private System.Windows.Forms.Label lblOriginal;
        private System.Windows.Forms.Label lblConverted;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.btnImportText = new System.Windows.Forms.Button();
            this.btnConvert = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnCopyConverted = new System.Windows.Forms.Button();
            this.btnPasteClipboard = new System.Windows.Forms.Button();
            this.btnExportText = new System.Windows.Forms.Button();
            this.txtOriginal = new System.Windows.Forms.TextBox();
            this.txtConverted = new System.Windows.Forms.TextBox();
            this.lblOriginal = new System.Windows.Forms.Label();
            this.lblConverted = new System.Windows.Forms.Label();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.SuspendLayout();
            // 
            // btnImportText
            // 
            this.btnImportText.Location = new System.Drawing.Point(18, 16);
            this.btnImportText.Name = "btnImportText";
            this.btnImportText.Size = new System.Drawing.Size(110, 32);
            this.btnImportText.TabIndex = 0;
            this.btnImportText.Text = "Import Text";
            this.btnImportText.UseVisualStyleBackColor = true;
            this.btnImportText.Click += new System.EventHandler(this.btnImportText_Click);
            // 
            // btnConvert
            // 
            this.btnConvert.Location = new System.Drawing.Point(140, 16);
            this.btnConvert.Name = "btnConvert";
            this.btnConvert.Size = new System.Drawing.Size(110, 32);
            this.btnConvert.TabIndex = 1;
            this.btnConvert.Text = "Convert";
            this.btnConvert.UseVisualStyleBackColor = true;
            this.btnConvert.Click += new System.EventHandler(this.btnConvert_Click);
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(262, 16);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(110, 32);
            this.btnClear.TabIndex = 2;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnCopyConverted
            // 
            this.btnCopyConverted.Location = new System.Drawing.Point(384, 16);
            this.btnCopyConverted.Name = "btnCopyConverted";
            this.btnCopyConverted.Size = new System.Drawing.Size(138, 32);
            this.btnCopyConverted.TabIndex = 3;
            this.btnCopyConverted.Text = "Copy Converted";
            this.btnCopyConverted.UseVisualStyleBackColor = true;
            this.btnCopyConverted.Click += new System.EventHandler(this.btnCopyConverted_Click);
            // 
            // btnPasteClipboard
            // 
            this.btnPasteClipboard.Location = new System.Drawing.Point(534, 16);
            this.btnPasteClipboard.Name = "btnPasteClipboard";
            this.btnPasteClipboard.Size = new System.Drawing.Size(150, 32);
            this.btnPasteClipboard.TabIndex = 4;
            this.btnPasteClipboard.Text = "Paste Clipboard";
            this.btnPasteClipboard.UseVisualStyleBackColor = true;
            this.btnPasteClipboard.Click += new System.EventHandler(this.btnPasteClipboard_Click);
            // 
            // btnExportText
            // 
            this.btnExportText.Location = new System.Drawing.Point(696, 16);
            this.btnExportText.Name = "btnExportText";
            this.btnExportText.Size = new System.Drawing.Size(168, 32);
            this.btnExportText.TabIndex = 5;
            this.btnExportText.Text = "Export Converted .txt";
            this.btnExportText.UseVisualStyleBackColor = true;
            this.btnExportText.Click += new System.EventHandler(this.btnExportText_Click);
            // 
            // txtOriginal
            // 
            this.txtOriginal.Location = new System.Drawing.Point(18, 84);
            this.txtOriginal.Multiline = true;
            this.txtOriginal.Name = "txtOriginal";
            this.txtOriginal.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtOriginal.Size = new System.Drawing.Size(415, 360);
            this.txtOriginal.TabIndex = 4;
            // 
            // txtConverted
            // 
            this.txtConverted.Location = new System.Drawing.Point(449, 84);
            this.txtConverted.Multiline = true;
            this.txtConverted.Name = "txtConverted";
            this.txtConverted.ReadOnly = true;
            this.txtConverted.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtConverted.Size = new System.Drawing.Size(415, 360);
            this.txtConverted.TabIndex = 5;
            // 
            // lblOriginal
            // 
            this.lblOriginal.AutoSize = true;
            this.lblOriginal.Location = new System.Drawing.Point(15, 62);
            this.lblOriginal.Name = "lblOriginal";
            this.lblOriginal.Size = new System.Drawing.Size(78, 16);
            this.lblOriginal.TabIndex = 6;
            this.lblOriginal.Text = "Original Text";
            // 
            // lblConverted
            // 
            this.lblConverted.AutoSize = true;
            this.lblConverted.Location = new System.Drawing.Point(446, 62);
            this.lblConverted.Name = "lblConverted";
            this.lblConverted.Size = new System.Drawing.Size(98, 16);
            this.lblConverted.TabIndex = 7;
            this.lblConverted.Text = "Converted Text";
            // 
            // openFileDialog
            // 
            this.openFileDialog.DefaultExt = "txt";
            this.openFileDialog.Filter = "Text files (*.txt)|*.txt|CSV files (*.csv)|*.csv|All files (*.*)|*.*";
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.DefaultExt = "txt";
            this.saveFileDialog.FileName = "converted.txt";
            this.saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(884, 462);
            this.Controls.Add(this.lblConverted);
            this.Controls.Add(this.lblOriginal);
            this.Controls.Add(this.txtConverted);
            this.Controls.Add(this.txtOriginal);
            this.Controls.Add(this.btnExportText);
            this.Controls.Add(this.btnPasteClipboard);
            this.Controls.Add(this.btnCopyConverted);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.btnConvert);
            this.Controls.Add(this.btnImportText);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "TC to SC Coverter";
            this.AllowDrop = true;
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
