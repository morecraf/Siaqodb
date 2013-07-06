namespace LicenseActivation
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.label6 = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.txtLic = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtNetCustomerCode = new System.Windows.Forms.TextBox();
            this.btnGetLicenseKey = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.label7 = new System.Windows.Forms.Label();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.txtAssembly = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.btnSaveSilv = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.txtLicSilv = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtCodeSilv = new System.Windows.Forms.TextBox();
            this.btnGetKeySilv = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(494, 345);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.label8);
            this.tabPage1.Controls.Add(this.label6);
            this.tabPage1.Controls.Add(this.btnSave);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.txtLic);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Controls.Add(this.txtNetCustomerCode);
            this.tabPage1.Controls.Add(this.btnGetLicenseKey);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(486, 319);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = ".NET & Mono*";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.label6.Dock = System.Windows.Forms.DockStyle.Top;
            this.label6.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.label6.Location = new System.Drawing.Point(3, 3);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(480, 20);
            this.label6.TabIndex = 6;
            this.label6.Text = "Get LicenseKey for .NET, WinRT, Mono, Xamarin.iOS, Xamarin.Android, Compact Frame" +
    "work";
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Enabled = false;
            this.btnSave.Location = new System.Drawing.Point(388, 269);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 5;
            this.btnSave.Text = "Save as...";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 120);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(62, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "LicenseKey";
            // 
            // txtLic
            // 
            this.txtLic.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLic.Location = new System.Drawing.Point(104, 117);
            this.txtLic.Multiline = true;
            this.txtLic.Name = "txtLic";
            this.txtLic.ReadOnly = true;
            this.txtLic.Size = new System.Drawing.Size(359, 147);
            this.txtLic.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 70);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(78, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Customer code";
            // 
            // txtNetCustomerCode
            // 
            this.txtNetCustomerCode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtNetCustomerCode.Location = new System.Drawing.Point(104, 67);
            this.txtNetCustomerCode.Name = "txtNetCustomerCode";
            this.txtNetCustomerCode.Size = new System.Drawing.Size(267, 20);
            this.txtNetCustomerCode.TabIndex = 1;
            // 
            // btnGetLicenseKey
            // 
            this.btnGetLicenseKey.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGetLicenseKey.Location = new System.Drawing.Point(389, 65);
            this.btnGetLicenseKey.Name = "btnGetLicenseKey";
            this.btnGetLicenseKey.Size = new System.Drawing.Size(75, 23);
            this.btnGetLicenseKey.TabIndex = 0;
            this.btnGetLicenseKey.Text = "Get Key";
            this.btnGetLicenseKey.UseVisualStyleBackColor = true;
            this.btnGetLicenseKey.Click += new System.EventHandler(this.btnGetLicenseKey_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.label9);
            this.tabPage2.Controls.Add(this.label7);
            this.tabPage2.Controls.Add(this.btnBrowse);
            this.tabPage2.Controls.Add(this.txtAssembly);
            this.tabPage2.Controls.Add(this.label5);
            this.tabPage2.Controls.Add(this.btnSaveSilv);
            this.tabPage2.Controls.Add(this.label3);
            this.tabPage2.Controls.Add(this.txtLicSilv);
            this.tabPage2.Controls.Add(this.label4);
            this.tabPage2.Controls.Add(this.txtCodeSilv);
            this.tabPage2.Controls.Add(this.btnGetKeySilv);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(486, 319);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Silverlight & WindowsPhone";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.label7.Dock = System.Windows.Forms.DockStyle.Top;
            this.label7.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.label7.Location = new System.Drawing.Point(3, 3);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(480, 20);
            this.label7.TabIndex = 15;
            this.label7.Text = "Get LicenseKey for Silverlight or WindowsPhone";
            // 
            // btnBrowse
            // 
            this.btnBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowse.Location = new System.Drawing.Point(312, 118);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnBrowse.TabIndex = 14;
            this.btnBrowse.Text = "Browse...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // txtAssembly
            // 
            this.txtAssembly.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtAssembly.Location = new System.Drawing.Point(19, 119);
            this.txtAssembly.Name = "txtAssembly";
            this.txtAssembly.ReadOnly = true;
            this.txtAssembly.Size = new System.Drawing.Size(287, 20);
            this.txtAssembly.TabIndex = 13;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(16, 103);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(289, 13);
            this.label5.TabIndex = 12;
            this.label5.Text = "Silverlight or WindowsPhone Assembly that will use Siaqodb";
            // 
            // btnSaveSilv
            // 
            this.btnSaveSilv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveSilv.Enabled = false;
            this.btnSaveSilv.Location = new System.Drawing.Point(403, 267);
            this.btnSaveSilv.Name = "btnSaveSilv";
            this.btnSaveSilv.Size = new System.Drawing.Size(75, 23);
            this.btnSaveSilv.TabIndex = 11;
            this.btnSaveSilv.Text = "Save as...";
            this.btnSaveSilv.UseVisualStyleBackColor = true;
            this.btnSaveSilv.Click += new System.EventHandler(this.btnSaveSilv_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 151);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(62, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "LicenseKey";
            // 
            // txtLicSilv
            // 
            this.txtLicSilv.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLicSilv.Location = new System.Drawing.Point(19, 167);
            this.txtLicSilv.Multiline = true;
            this.txtLicSilv.Name = "txtLicSilv";
            this.txtLicSilv.ReadOnly = true;
            this.txtLicSilv.Size = new System.Drawing.Size(461, 94);
            this.txtLicSilv.TabIndex = 9;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(16, 55);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(78, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Customer code";
            // 
            // txtCodeSilv
            // 
            this.txtCodeSilv.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCodeSilv.Location = new System.Drawing.Point(19, 71);
            this.txtCodeSilv.Name = "txtCodeSilv";
            this.txtCodeSilv.Size = new System.Drawing.Size(287, 20);
            this.txtCodeSilv.TabIndex = 7;
            // 
            // btnGetKeySilv
            // 
            this.btnGetKeySilv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGetKeySilv.Location = new System.Drawing.Point(312, 70);
            this.btnGetKeySilv.Name = "btnGetKeySilv";
            this.btnGetKeySilv.Size = new System.Drawing.Size(75, 23);
            this.btnGetKeySilv.TabIndex = 6;
            this.btnGetKeySilv.Text = "Get Key";
            this.btnGetKeySilv.UseVisualStyleBackColor = true;
            this.btnGetKeySilv.Click += new System.EventHandler(this.btnGetKeySilv_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(3, 301);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(124, 13);
            this.label8.TabIndex = 7;
            this.label8.Text = "Copyright © Dotissi 2013";
            // 
            // label9
            // 
            this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(3, 301);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(124, 13);
            this.label9.TabIndex = 16;
            this.label9.Text = "Copyright © Dotissi 2013";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(494, 345);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Siaqodb License Activation";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtNetCustomerCode;
        private System.Windows.Forms.Button btnGetLicenseKey;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtLic;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.TextBox txtAssembly;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnSaveSilv;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtLicSilv;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtCodeSilv;
        private System.Windows.Forms.Button btnGetKeySilv;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
    }
}

