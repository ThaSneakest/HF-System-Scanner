namespace Wildlands_System_Scanner
{
    partial class MainApp
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
            this.labelControlProgress = new DevExpress.XtraEditors.LabelControl();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.groupControl2 = new DevExpress.XtraEditors.GroupControl();
            this.comboBoxEdit1 = new DevExpress.XtraEditors.ComboBoxEdit();
            this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
            this.groupControl1 = new DevExpress.XtraEditors.GroupControl();
            this.checkEdit7 = new DevExpress.XtraEditors.CheckEdit();
            this.checkEdit6 = new DevExpress.XtraEditors.CheckEdit();
            this.checkEdit5 = new DevExpress.XtraEditors.CheckEdit();
            this.checkEdit4 = new DevExpress.XtraEditors.CheckEdit();
            this.checkEdit3 = new DevExpress.XtraEditors.CheckEdit();
            this.checkEdit2 = new DevExpress.XtraEditors.CheckEdit();
            this.checkEdit8 = new DevExpress.XtraEditors.CheckEdit();
            this.simpleButtonExit = new DevExpress.XtraEditors.SimpleButton();
            this.simpleButtonStartScan = new DevExpress.XtraEditors.SimpleButton();
            this.simpleButtonStartFix = new DevExpress.XtraEditors.SimpleButton();
            this.labelControlProgressB = new DevExpress.XtraEditors.LabelControl();
            this.checkEditAutoDeleteBlacklist = new DevExpress.XtraEditors.CheckEdit();
            this.checkEditSkipScan = new DevExpress.XtraEditors.CheckEdit();
            this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl2)).BeginInit();
            this.groupControl2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxEdit1.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1)).BeginInit();
            this.groupControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.checkEdit7.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEdit6.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEdit5.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEdit4.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEdit3.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEdit2.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEdit8.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEditAutoDeleteBlacklist.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEditSkipScan.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // labelControlProgress
            // 
            this.labelControlProgress.Location = new System.Drawing.Point(25, 313);
            this.labelControlProgress.Name = "labelControlProgress";
            this.labelControlProgress.Size = new System.Drawing.Size(0, 13);
            this.labelControlProgress.TabIndex = 2;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(25, 332);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(818, 23);
            this.progressBar1.TabIndex = 4;
            // 
            // groupControl2
            // 
            this.groupControl2.Controls.Add(this.checkEditSkipScan);
            this.groupControl2.Controls.Add(this.checkEditAutoDeleteBlacklist);
            this.groupControl2.Controls.Add(this.comboBoxEdit1);
            this.groupControl2.Controls.Add(this.labelControl2);
            this.groupControl2.Location = new System.Drawing.Point(497, 73);
            this.groupControl2.Name = "groupControl2";
            this.groupControl2.Size = new System.Drawing.Size(346, 153);
            this.groupControl2.TabIndex = 11;
            this.groupControl2.Text = "Options";
            // 
            // comboBoxEdit1
            // 
            this.comboBoxEdit1.Location = new System.Drawing.Point(22, 43);
            this.comboBoxEdit1.Name = "comboBoxEdit1";
            this.comboBoxEdit1.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.comboBoxEdit1.Size = new System.Drawing.Size(100, 20);
            this.comboBoxEdit1.TabIndex = 1;
            // 
            // labelControl2
            // 
            this.labelControl2.Location = new System.Drawing.Point(22, 24);
            this.labelControl2.Name = "labelControl2";
            this.labelControl2.Size = new System.Drawing.Size(42, 13);
            this.labelControl2.TabIndex = 0;
            this.labelControl2.Text = "File Age:";
            // 
            // groupControl1
            // 
            this.groupControl1.Controls.Add(this.checkEdit7);
            this.groupControl1.Controls.Add(this.checkEdit6);
            this.groupControl1.Controls.Add(this.checkEdit5);
            this.groupControl1.Controls.Add(this.checkEdit4);
            this.groupControl1.Controls.Add(this.checkEdit3);
            this.groupControl1.Controls.Add(this.checkEdit2);
            this.groupControl1.Controls.Add(this.checkEdit8);
            this.groupControl1.Location = new System.Drawing.Point(25, 73);
            this.groupControl1.Name = "groupControl1";
            this.groupControl1.Size = new System.Drawing.Size(368, 153);
            this.groupControl1.TabIndex = 10;
            this.groupControl1.Text = "Whitelist Options";
            this.groupControl1.Paint += new System.Windows.Forms.PaintEventHandler(this.groupControl1_Paint);
            // 
            // checkEdit7
            // 
            this.checkEdit7.Location = new System.Drawing.Point(187, 87);
            this.checkEdit7.Name = "checkEdit7";
            this.checkEdit7.Properties.Caption = "Use Signature Whitelist";
            this.checkEdit7.Size = new System.Drawing.Size(147, 19);
            this.checkEdit7.TabIndex = 6;
            // 
            // checkEdit6
            // 
            this.checkEdit6.Location = new System.Drawing.Point(187, 61);
            this.checkEdit6.Name = "checkEdit6";
            this.checkEdit6.Properties.Caption = "Use Microsoft File Whitelist";
            this.checkEdit6.Size = new System.Drawing.Size(165, 19);
            this.checkEdit6.TabIndex = 5;
            // 
            // checkEdit5
            // 
            this.checkEdit5.Location = new System.Drawing.Point(187, 35);
            this.checkEdit5.Name = "checkEdit5";
            this.checkEdit5.Properties.Caption = "Use File Whitelist";
            this.checkEdit5.Size = new System.Drawing.Size(147, 19);
            this.checkEdit5.TabIndex = 4;
            // 
            // checkEdit4
            // 
            this.checkEdit4.Location = new System.Drawing.Point(15, 113);
            this.checkEdit4.Name = "checkEdit4";
            this.checkEdit4.Properties.Caption = "Use Driver Whitelist";
            this.checkEdit4.Size = new System.Drawing.Size(129, 19);
            this.checkEdit4.TabIndex = 3;
            // 
            // checkEdit3
            // 
            this.checkEdit3.Location = new System.Drawing.Point(15, 87);
            this.checkEdit3.Name = "checkEdit3";
            this.checkEdit3.Properties.Caption = "Use Service Whitelist";
            this.checkEdit3.Size = new System.Drawing.Size(129, 19);
            this.checkEdit3.TabIndex = 2;
            // 
            // checkEdit2
            // 
            this.checkEdit2.Location = new System.Drawing.Point(15, 61);
            this.checkEdit2.Name = "checkEdit2";
            this.checkEdit2.Properties.Caption = "Use Process Whitelist";
            this.checkEdit2.Size = new System.Drawing.Size(129, 19);
            this.checkEdit2.TabIndex = 1;
            // 
            // checkEdit8
            // 
            this.checkEdit8.Location = new System.Drawing.Point(15, 35);
            this.checkEdit8.Name = "checkEdit8";
            this.checkEdit8.Properties.Caption = "Use Registry Whitelist";
            this.checkEdit8.Size = new System.Drawing.Size(144, 19);
            this.checkEdit8.TabIndex = 0;
            this.checkEdit8.CheckedChanged += new System.EventHandler(this.checkEdit8_CheckedChanged);
            // 
            // simpleButtonExit
            // 
            this.simpleButtonExit.Location = new System.Drawing.Point(737, 430);
            this.simpleButtonExit.Name = "simpleButtonExit";
            this.simpleButtonExit.Size = new System.Drawing.Size(129, 24);
            this.simpleButtonExit.TabIndex = 15;
            this.simpleButtonExit.Text = "Exit";
            this.simpleButtonExit.Click += new System.EventHandler(this.simpleButtonExit_Click);
            // 
            // simpleButtonStartScan
            // 
            this.simpleButtonStartScan.Location = new System.Drawing.Point(573, 430);
            this.simpleButtonStartScan.Name = "simpleButtonStartScan";
            this.simpleButtonStartScan.Size = new System.Drawing.Size(128, 24);
            this.simpleButtonStartScan.TabIndex = 14;
            this.simpleButtonStartScan.Text = "Start Scan";
            this.simpleButtonStartScan.Click += new System.EventHandler(this.simpleButtonStartScan_Click);
            // 
            // simpleButtonStartFix
            // 
            this.simpleButtonStartFix.Location = new System.Drawing.Point(25, 419);
            this.simpleButtonStartFix.Name = "simpleButtonStartFix";
            this.simpleButtonStartFix.Size = new System.Drawing.Size(129, 24);
            this.simpleButtonStartFix.TabIndex = 13;
            this.simpleButtonStartFix.Text = "Start Fix";
            this.simpleButtonStartFix.Click += new System.EventHandler(this.simpleButtonStartFix_Click);
            // 
            // labelControlProgressB
            // 
            this.labelControlProgressB.Location = new System.Drawing.Point(129, 371);
            this.labelControlProgressB.Name = "labelControlProgressB";
            this.labelControlProgressB.Size = new System.Drawing.Size(0, 13);
            this.labelControlProgressB.TabIndex = 16;
            // 
            // checkEditAutoDeleteBlacklist
            // 
            this.checkEditAutoDeleteBlacklist.Location = new System.Drawing.Point(22, 80);
            this.checkEditAutoDeleteBlacklist.Name = "checkEditAutoDeleteBlacklist";
            this.checkEditAutoDeleteBlacklist.Properties.Caption = "Automatically Delete Blacklist Detections?";
            this.checkEditAutoDeleteBlacklist.Size = new System.Drawing.Size(226, 19);
            this.checkEditAutoDeleteBlacklist.TabIndex = 2;
            // 
            // checkEditSkipScan
            // 
            this.checkEditSkipScan.Location = new System.Drawing.Point(22, 113);
            this.checkEditSkipScan.Name = "checkEditSkipScan";
            this.checkEditSkipScan.Properties.Caption = "Skip Scan";
            this.checkEditSkipScan.Size = new System.Drawing.Size(83, 19);
            this.checkEditSkipScan.TabIndex = 3;
            // 
            // labelControl3
            // 
            this.labelControl3.Location = new System.Drawing.Point(25, 371);
            this.labelControl3.Name = "labelControl3";
            this.labelControl3.Size = new System.Drawing.Size(98, 13);
            this.labelControl3.TabIndex = 19;
            this.labelControl3.Text = "Percent Completed: ";
            // 
            // MainApp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(895, 468);
            this.Controls.Add(this.labelControl3);
            this.Controls.Add(this.labelControlProgressB);
            this.Controls.Add(this.simpleButtonExit);
            this.Controls.Add(this.simpleButtonStartScan);
            this.Controls.Add(this.simpleButtonStartFix);
            this.Controls.Add(this.groupControl2);
            this.Controls.Add(this.groupControl1);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.labelControlProgress);
            this.Name = "MainApp";
            this.Text = "Wildlands System Scanner";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.groupControl2)).EndInit();
            this.groupControl2.ResumeLayout(false);
            this.groupControl2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxEdit1.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1)).EndInit();
            this.groupControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.checkEdit7.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEdit6.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEdit5.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEdit4.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEdit3.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEdit2.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEdit8.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEditAutoDeleteBlacklist.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEditSkipScan.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        public DevExpress.XtraEditors.LabelControl labelControlProgress;
        public System.Windows.Forms.ProgressBar progressBar1;
        public DevExpress.XtraEditors.GroupControl groupControl2;
        public DevExpress.XtraEditors.ComboBoxEdit comboBoxEdit1;
        public DevExpress.XtraEditors.LabelControl labelControl2;
        public DevExpress.XtraEditors.GroupControl groupControl1;
        public DevExpress.XtraEditors.CheckEdit checkEdit7;
        public DevExpress.XtraEditors.CheckEdit checkEdit6;
        public DevExpress.XtraEditors.CheckEdit checkEdit5;
        public DevExpress.XtraEditors.CheckEdit checkEdit4;
        public DevExpress.XtraEditors.CheckEdit checkEdit3;
        public DevExpress.XtraEditors.CheckEdit checkEdit2;
        public DevExpress.XtraEditors.CheckEdit checkEdit8;
        public DevExpress.XtraEditors.SimpleButton simpleButtonExit;
        public DevExpress.XtraEditors.SimpleButton simpleButtonStartScan;
        public DevExpress.XtraEditors.SimpleButton simpleButtonStartFix;
        private DevExpress.XtraEditors.LabelControl labelControlProgressB;
        private DevExpress.XtraEditors.CheckEdit checkEditAutoDeleteBlacklist;
        private DevExpress.XtraEditors.CheckEdit checkEditSkipScan;
        private DevExpress.XtraEditors.LabelControl labelControl3;
    }
}

