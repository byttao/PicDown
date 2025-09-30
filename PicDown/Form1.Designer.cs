namespace PicDown
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            tableLayoutPanel1 = new TableLayoutPanel();
            rtbHtml = new RichTextBox();
            richTextBoxLog = new RichTextBox();
            tableLayoutPanel2 = new TableLayoutPanel();
            label1 = new Label();
            txtOutputFolder = new TextBox();
            btnFloder = new Button();
            pictureBox1 = new PictureBox();
            btnDown = new Button();
            richTextBox1 = new RichTextBox();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            tableLayoutPanel1.Controls.Add(rtbHtml, 0, 0);
            tableLayoutPanel1.Controls.Add(richTextBoxLog, 0, 2);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 0, 1);
            tableLayoutPanel1.Controls.Add(pictureBox1, 1, 0);
            tableLayoutPanel1.Controls.Add(btnDown, 1, 1);
            tableLayoutPanel1.Controls.Add(richTextBox1, 1, 2);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.Padding = new Padding(10);
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(882, 844);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // rtbHtml
            // 
            rtbHtml.Dock = DockStyle.Fill;
            rtbHtml.Location = new Point(13, 13);
            rtbHtml.Name = "rtbHtml";
            rtbHtml.Size = new Size(756, 240);
            rtbHtml.TabIndex = 0;
            rtbHtml.Text = "";
            // 
            // richTextBoxLog
            // 
            richTextBoxLog.Dock = DockStyle.Fill;
            richTextBoxLog.Location = new Point(13, 309);
            richTextBoxLog.Name = "richTextBoxLog";
            richTextBoxLog.ReadOnly = true;
            richTextBoxLog.Size = new Size(756, 522);
            richTextBoxLog.TabIndex = 1;
            richTextBoxLog.Text = "";
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 3;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70F));
            tableLayoutPanel2.Controls.Add(label1, 0, 0);
            tableLayoutPanel2.Controls.Add(txtOutputFolder, 1, 0);
            tableLayoutPanel2.Controls.Add(btnFloder, 2, 0);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(13, 259);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 1;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Size = new Size(756, 44);
            tableLayoutPanel2.TabIndex = 3;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Right;
            label1.AutoSize = true;
            label1.Location = new Point(39, 13);
            label1.Name = "label1";
            label1.Size = new Size(68, 17);
            label1.TabIndex = 0;
            label1.Text = "保存目录：";
            // 
            // txtOutputFolder
            // 
            txtOutputFolder.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtOutputFolder.BorderStyle = BorderStyle.FixedSingle;
            txtOutputFolder.Location = new Point(113, 10);
            txtOutputFolder.Name = "txtOutputFolder";
            txtOutputFolder.ReadOnly = true;
            txtOutputFolder.Size = new Size(570, 23);
            txtOutputFolder.TabIndex = 1;
            txtOutputFolder.TextChanged += txtOutputFolder_TextChanged;
            // 
            // btnFloder
            // 
            btnFloder.Anchor = AnchorStyles.Left;
            btnFloder.Location = new Point(689, 10);
            btnFloder.Name = "btnFloder";
            btnFloder.Size = new Size(26, 23);
            btnFloder.TabIndex = 2;
            btnFloder.Text = "...";
            btnFloder.UseVisualStyleBackColor = true;
            btnFloder.Click += btnFloder_Click;
            // 
            // pictureBox1
            // 
            pictureBox1.Dock = DockStyle.Fill;
            pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new Point(775, 13);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(94, 240);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 4;
            pictureBox1.TabStop = false;
            // 
            // btnDown
            // 
            btnDown.Anchor = AnchorStyles.None;
            btnDown.Location = new Point(778, 261);
            btnDown.Name = "btnDown";
            btnDown.Size = new Size(88, 40);
            btnDown.TabIndex = 2;
            btnDown.Text = "图包下载";
            btnDown.UseVisualStyleBackColor = true;
            btnDown.Click += btnDown_Click;
            // 
            // richTextBox1
            // 
            richTextBox1.BackColor = Color.White;
            richTextBox1.BorderStyle = BorderStyle.None;
            richTextBox1.Dock = DockStyle.Fill;
            richTextBox1.Location = new Point(775, 309);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.ReadOnly = true;
            richTextBox1.Size = new Size(94, 522);
            richTextBox1.TabIndex = 5;
            richTextBox1.Text = "V1.2 250927\n更新：视频下载\n\nV1.1 250927\n更新：SKU图下载\n\nV1.0 250926\n更新：主图、详情图下载";
            // 
            // Form1
            // 
            AcceptButton = btnDown;
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(882, 844);
            Controls.Add(tableLayoutPanel1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "Form1";
            Text = "图包下载器 V1.2 250927";
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private RichTextBox rtbHtml;
        private RichTextBox richTextBoxLog;
        private Button btnDown;
        private TableLayoutPanel tableLayoutPanel2;
        private Label label1;
        private TextBox txtOutputFolder;
        private Button btnFloder;
        private PictureBox pictureBox1;
        private RichTextBox richTextBox1;
    }
}
