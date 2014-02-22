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
            this.label8 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtLic = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtNetCustomerCode = new System.Windows.Forms.TextBox();
            this.btnGetLicenseKey = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.txtCode = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(346, 277);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(124, 13);
            this.label8.TabIndex = 15;
            this.label8.Text = "Copyright © Dotissi 2014";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 54);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 13);
            this.label2.TabIndex = 12;
            this.label2.Text = "LicenseKey:";
            // 
            // txtLic
            // 
            this.txtLic.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLic.BackColor = System.Drawing.SystemColors.Info;
            this.txtLic.Location = new System.Drawing.Point(12, 70);
            this.txtLic.Multiline = true;
            this.txtLic.Name = "txtLic";
            this.txtLic.ReadOnly = true;
            this.txtLic.Size = new System.Drawing.Size(447, 87);
            this.txtLic.TabIndex = 11;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(81, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Customer code:";
            // 
            // txtNetCustomerCode
            // 
            this.txtNetCustomerCode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtNetCustomerCode.Location = new System.Drawing.Point(12, 22);
            this.txtNetCustomerCode.Name = "txtNetCustomerCode";
            this.txtNetCustomerCode.Size = new System.Drawing.Size(355, 20);
            this.txtNetCustomerCode.TabIndex = 9;
            // 
            // btnGetLicenseKey
            // 
            this.btnGetLicenseKey.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGetLicenseKey.Location = new System.Drawing.Point(373, 20);
            this.btnGetLicenseKey.Name = "btnGetLicenseKey";
            this.btnGetLicenseKey.Size = new System.Drawing.Size(87, 23);
            this.btnGetLicenseKey.TabIndex = 8;
            this.btnGetLicenseKey.Text = "Get Key";
            this.btnGetLicenseKey.UseVisualStyleBackColor = true;
            this.btnGetLicenseKey.Click += new System.EventHandler(this.btnGetLicenseKey_Click_1);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 171);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(323, 13);
            this.label3.TabIndex = 16;
            this.label3.Text = "Copy the line bellow and put it before opening a Siaqodb database:";
            // 
            // txtCode
            // 
            this.txtCode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCode.BackColor = System.Drawing.SystemColors.Info;
            this.txtCode.Location = new System.Drawing.Point(12, 187);
            this.txtCode.Multiline = true;
            this.txtCode.Name = "txtCode";
            this.txtCode.ReadOnly = true;
            this.txtCode.Size = new System.Drawing.Size(447, 57);
            this.txtCode.TabIndex = 17;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(471, 299);
            this.Controls.Add(this.txtCode);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtLic);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtNetCustomerCode);
            this.Controls.Add(this.btnGetLicenseKey);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Siaqodb License Activation";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtLic;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtNetCustomerCode;
        private System.Windows.Forms.Button btnGetLicenseKey;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtCode;

    }
}

