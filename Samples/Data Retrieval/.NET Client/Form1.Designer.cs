namespace HWS_API_Example
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.startToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.writingDataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.liveDataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.liveStreamToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.discoverWebServicesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.readingPerformanceTestToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label1 = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.dataExtensionCheckBox = new System.Windows.Forms.CheckBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.liveModeTimer = new System.Windows.Forms.Timer(this.components);
            this.button1 = new System.Windows.Forms.Button();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // richTextBox1
            // 
            this.richTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBox1.DetectUrls = false;
            this.richTextBox1.Location = new System.Drawing.Point(3, 21);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(780, 426);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = "";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startToolStripMenuItem,
            this.writingDataToolStripMenuItem,
            this.liveDataToolStripMenuItem,
            this.liveStreamToolStripMenuItem,
            this.discoverWebServicesToolStripMenuItem,
            this.readingPerformanceTestToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1098, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // startToolStripMenuItem
            // 
            this.startToolStripMenuItem.Name = "startToolStripMenuItem";
            this.startToolStripMenuItem.Size = new System.Drawing.Size(89, 20);
            this.startToolStripMenuItem.Text = "Reading Data";
            this.startToolStripMenuItem.Click += new System.EventHandler(this.startToolStripMenuItem_Click_1);
            // 
            // writingDataToolStripMenuItem
            // 
            this.writingDataToolStripMenuItem.Name = "writingDataToolStripMenuItem";
            this.writingDataToolStripMenuItem.Size = new System.Drawing.Size(85, 20);
            this.writingDataToolStripMenuItem.Text = "Writing Data";
            this.writingDataToolStripMenuItem.Click += new System.EventHandler(this.writingDataToolStripMenuItem_Click);
            // 
            // liveDataToolStripMenuItem
            // 
            this.liveDataToolStripMenuItem.Name = "liveDataToolStripMenuItem";
            this.liveDataToolStripMenuItem.Size = new System.Drawing.Size(67, 20);
            this.liveDataToolStripMenuItem.Text = "Live Data";
            this.liveDataToolStripMenuItem.Click += new System.EventHandler(this.liveDataToolStripMenuItem_Click);
            // 
            // liveStreamToolStripMenuItem
            // 
            this.liveStreamToolStripMenuItem.Name = "liveStreamToolStripMenuItem";
            this.liveStreamToolStripMenuItem.Size = new System.Drawing.Size(80, 20);
            this.liveStreamToolStripMenuItem.Text = "Live Stream";
            this.liveStreamToolStripMenuItem.Click += new System.EventHandler(this.liveDataToolStripMenuItem_Click);
            // 
            // discoverWebServicesToolStripMenuItem
            // 
            this.discoverWebServicesToolStripMenuItem.Name = "discoverWebServicesToolStripMenuItem";
            this.discoverWebServicesToolStripMenuItem.Size = new System.Drawing.Size(136, 20);
            this.discoverWebServicesToolStripMenuItem.Text = "Discover Web Services";
            this.discoverWebServicesToolStripMenuItem.Click += new System.EventHandler(this.discoverWebServicesToolStripMenuItem_Click);
            // 
            // readingPerformanceTestToolStripMenuItem
            // 
            this.readingPerformanceTestToolStripMenuItem.Name = "readingPerformanceTestToolStripMenuItem";
            this.readingPerformanceTestToolStripMenuItem.Size = new System.Drawing.Size(157, 20);
            this.readingPerformanceTestToolStripMenuItem.Text = "Reading Performance Test";
            this.readingPerformanceTestToolStripMenuItem.Click += new System.EventHandler(this.readingPerformanceTestToolStripMenuItem_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(45, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Results:";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 24);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.dataExtensionCheckBox);
            this.splitContainer1.Panel1.Controls.Add(this.checkBox1);
            this.splitContainer1.Panel1.Controls.Add(this.richTextBox1);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.label2);
            this.splitContainer1.Panel2.Controls.Add(this.listView1);
            this.splitContainer1.Size = new System.Drawing.Size(1098, 450);
            this.splitContainer1.SplitterDistance = 786;
            this.splitContainer1.TabIndex = 3;
            // 
            // dataExtensionCheckBox
            // 
            this.dataExtensionCheckBox.AutoSize = true;
            this.dataExtensionCheckBox.Location = new System.Drawing.Point(96, 3);
            this.dataExtensionCheckBox.Name = "dataExtensionCheckBox";
            this.dataExtensionCheckBox.Size = new System.Drawing.Size(98, 17);
            this.dataExtensionCheckBox.TabIndex = 4;
            this.dataExtensionCheckBox.Text = "Data Extension";
            this.dataExtensionCheckBox.UseVisualStyleBackColor = true;
            this.dataExtensionCheckBox.CheckedChanged += new System.EventHandler(this.dataExtensionCheckBox_CheckedChanged);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(411, 2);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(83, 17);
            this.checkBox1.TabIndex = 3;
            this.checkBox1.Text = "Zip Packets";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 7);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Live Data";
            // 
            // listView1
            // 
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
            this.listView1.Location = new System.Drawing.Point(0, 21);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(308, 429);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Tag Name";
            this.columnHeader1.Width = 83;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Value";
            this.columnHeader2.Width = 72;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Timestamp";
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Quality";
            // 
            // liveModeTimer
            // 
            this.liveModeTimer.Interval = 250;
            this.liveModeTimer.Tick += new System.EventHandler(this.liveModeTimer_Tick);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(815, 0);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "Stop";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Visible = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1098, 474);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Historian Web Service API Examples";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem startToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem writingDataToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem liveDataToolStripMenuItem;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.Timer liveModeTimer;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ToolStripMenuItem discoverWebServicesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem readingPerformanceTestToolStripMenuItem;
        public System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.ToolStripMenuItem liveStreamToolStripMenuItem;
        private System.Windows.Forms.Button button1;
        public System.Windows.Forms.CheckBox dataExtensionCheckBox;
    }
}

