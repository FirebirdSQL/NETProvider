/*
 *  Visual Studio 2005 DDEX Provider for Firebird
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2005 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

namespace FirebirdSql.VisualStudio.DataTools
{
    partial class FbDataConnectionUIControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.cmdTest = new System.Windows.Forms.Button();
            this.lblDataSource = new System.Windows.Forms.Label();
            this.cboDialect = new System.Windows.Forms.ComboBox();
            this.grbSettings = new System.Windows.Forms.GroupBox();
            this.cboServerType = new System.Windows.Forms.ComboBox();
            this.lblServerType = new System.Windows.Forms.Label();
            this.grbLogin = new System.Windows.Forms.GroupBox();
            this.lblRole = new System.Windows.Forms.Label();
            this.txtRole = new System.Windows.Forms.TextBox();
            this.lblPassword = new System.Windows.Forms.Label();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.lblUser = new System.Windows.Forms.Label();
            this.txtUserName = new System.Windows.Forms.TextBox();
            this.cmdGetFile = new System.Windows.Forms.Button();
            this.txtDatabase = new System.Windows.Forms.TextBox();
            this.lblDatabase = new System.Windows.Forms.Label();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lblDialect = new System.Windows.Forms.Label();
            this.txtDataSource = new System.Windows.Forms.TextBox();
            this.lblCharset = new System.Windows.Forms.Label();
            this.cboCharset = new System.Windows.Forms.ComboBox();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.grbSettings.SuspendLayout();
            this.grbLogin.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmdTest
            // 
            this.cmdTest.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cmdTest.Location = new System.Drawing.Point(-162, 180);
            this.cmdTest.Name = "cmdTest";
            this.cmdTest.Size = new System.Drawing.Size(75, 23);
            this.cmdTest.TabIndex = 6;
            this.cmdTest.Text = "&Test";
            // 
            // lblDataSource
            // 
            this.lblDataSource.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblDataSource.Location = new System.Drawing.Point(0, 0);
            this.lblDataSource.Margin = new System.Windows.Forms.Padding(0);
            this.lblDataSource.Name = "lblDataSource";
            this.lblDataSource.Size = new System.Drawing.Size(64, 16);
            this.lblDataSource.TabIndex = 27;
            this.lblDataSource.Text = "Data Source";
            // 
            // cboDialect
            // 
            this.cboDialect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDialect.FormattingEnabled = true;
            this.cboDialect.Items.AddRange(new object[] {
            "1",
            "3"});
            this.cboDialect.Location = new System.Drawing.Point(239, 17);
            this.cboDialect.Name = "cboDialect";
            this.cboDialect.Size = new System.Drawing.Size(63, 21);
            this.cboDialect.TabIndex = 18;
            this.cboDialect.SelectedIndex = 1;
            this.cboDialect.TextChanged += new System.EventHandler(this.SetProperty);
            // 
            // grbSettings
            // 
            this.grbSettings.Controls.Add(this.cboServerType);
            this.grbSettings.Controls.Add(this.lblServerType);
            this.grbSettings.Location = new System.Drawing.Point(201, 89);
            this.grbSettings.Name = "grbSettings";
            this.grbSettings.Size = new System.Drawing.Size(256, 109);
            this.grbSettings.TabIndex = 25;
            this.grbSettings.TabStop = false;
            this.grbSettings.Text = "Connection Settings";
            // 
            // cboServerType
            // 
            this.cboServerType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboServerType.FormattingEnabled = true;
            this.cboServerType.Items.AddRange(new object[] {
            "Super/Classic Server",
            "Embedded Server"});
            this.cboServerType.Location = new System.Drawing.Point(96, 24);
            this.cboServerType.Name = "cboServerType";
            this.cboServerType.Size = new System.Drawing.Size(144, 21);
            this.cboServerType.TabIndex = 13;
            this.cboServerType.SelectedIndex = 0;
            this.cboServerType.TextChanged += new System.EventHandler(this.SetProperty);
            // 
            // lblServerType
            // 
            this.lblServerType.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblServerType.Location = new System.Drawing.Point(16, 24);
            this.lblServerType.Name = "lblServerType";
            this.lblServerType.Size = new System.Drawing.Size(64, 16);
            this.lblServerType.TabIndex = 13;
            this.lblServerType.Text = "Server Type";
            // 
            // grbLogin
            // 
            this.grbLogin.Controls.Add(this.lblRole);
            this.grbLogin.Controls.Add(this.txtRole);
            this.grbLogin.Controls.Add(this.lblPassword);
            this.grbLogin.Controls.Add(this.txtPassword);
            this.grbLogin.Controls.Add(this.lblUser);
            this.grbLogin.Controls.Add(this.txtUserName);
            this.grbLogin.Location = new System.Drawing.Point(0, 89);
            this.grbLogin.Name = "grbLogin";
            this.grbLogin.Size = new System.Drawing.Size(192, 109);
            this.grbLogin.TabIndex = 24;
            this.grbLogin.TabStop = false;
            this.grbLogin.Text = "Login";
            // 
            // lblRole
            // 
            this.lblRole.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblRole.Location = new System.Drawing.Point(8, 86);
            this.lblRole.Name = "lblRole";
            this.lblRole.Size = new System.Drawing.Size(48, 14);
            this.lblRole.TabIndex = 4;
            this.lblRole.Text = "Role";
            // 
            // txtRole
            // 
            this.txtRole.Location = new System.Drawing.Point(64, 83);
            this.txtRole.Name = "txtRole";
            this.txtRole.Size = new System.Drawing.Size(112, 20);
            this.txtRole.TabIndex = 5;
            this.txtRole.TextChanged += new System.EventHandler(this.SetProperty);
            // 
            // lblPassword
            // 
            this.lblPassword.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblPassword.Location = new System.Drawing.Point(8, 55);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(48, 16);
            this.lblPassword.TabIndex = 2;
            this.lblPassword.Text = "Password";
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(64, 53);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size = new System.Drawing.Size(112, 20);
            this.txtPassword.TabIndex = 3;
            this.txtPassword.TextChanged += new System.EventHandler(this.SetProperty);
            // 
            // lblUser
            // 
            this.lblUser.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblUser.Location = new System.Drawing.Point(8, 27);
            this.lblUser.Name = "lblUser";
            this.lblUser.Size = new System.Drawing.Size(48, 14);
            this.lblUser.TabIndex = 0;
            this.lblUser.Text = "User";
            // 
            // txtUserName
            // 
            this.txtUserName.Location = new System.Drawing.Point(64, 24);
            this.txtUserName.Name = "txtUserName";
            this.txtUserName.Size = new System.Drawing.Size(112, 20);
            this.txtUserName.TabIndex = 1;
            this.txtUserName.TextChanged += new System.EventHandler(this.SetProperty);
            // 
            // cmdGetFile
            // 
            this.cmdGetFile.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cmdGetFile.Location = new System.Drawing.Point(433, 57);
            this.cmdGetFile.Name = "cmdGetFile";
            this.cmdGetFile.Size = new System.Drawing.Size(24, 23);
            this.cmdGetFile.TabIndex = 23;
            this.cmdGetFile.Text = "...";
            this.cmdGetFile.Click += new System.EventHandler(this.cmdGetFile_Click);
            // 
            // txtDatabase
            // 
            this.txtDatabase.Location = new System.Drawing.Point(0, 58);
            this.txtDatabase.Name = "txtDatabase";
            this.txtDatabase.Size = new System.Drawing.Size(431, 20);
            this.txtDatabase.TabIndex = 22;
            this.txtDatabase.TextChanged += new System.EventHandler(this.SetProperty);
            // 
            // lblDatabase
            // 
            this.lblDatabase.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblDatabase.Location = new System.Drawing.Point(0, 41);
            this.lblDatabase.Name = "lblDatabase";
            this.lblDatabase.Size = new System.Drawing.Size(48, 16);
            this.lblDatabase.TabIndex = 21;
            this.lblDatabase.Text = "Database";
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(145, 17);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(40, 20);
            this.txtPort.TabIndex = 16;
            this.txtPort.Text = "3050";
            this.txtPort.TextChanged += new System.EventHandler(this.SetProperty);
            // 
            // label1
            // 
            this.label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label1.Location = new System.Drawing.Point(145, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 16);
            this.label1.TabIndex = 15;
            this.label1.Text = "Data Source Port";
            // 
            // lblDialect
            // 
            this.lblDialect.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblDialect.Location = new System.Drawing.Point(239, 0);
            this.lblDialect.Name = "lblDialect";
            this.lblDialect.Size = new System.Drawing.Size(48, 16);
            this.lblDialect.TabIndex = 17;
            this.lblDialect.Text = "Dialect";
            // 
            // txtDataSource
            // 
            this.txtDataSource.Location = new System.Drawing.Point(0, 17);
            this.txtDataSource.Name = "txtDataSource";
            this.txtDataSource.Size = new System.Drawing.Size(136, 20);
            this.txtDataSource.TabIndex = 14;
            this.txtDataSource.TextChanged += new System.EventHandler(this.SetProperty);
            // 
            // lblCharset
            // 
            this.lblCharset.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblCharset.Location = new System.Drawing.Point(313, 0);
            this.lblCharset.Name = "lblCharset";
            this.lblCharset.Size = new System.Drawing.Size(48, 16);
            this.lblCharset.TabIndex = 19;
            this.lblCharset.Text = "Charset";
            // 
            // cboCharset
            // 
            this.cboCharset.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCharset.FormattingEnabled = true;
            this.cboCharset.Items.AddRange(new object[] {
            "NONE",
            "ASCII",
            "BIG_5Big5",
            "DOS437",
            "DOS850",
            "DOS860",
            "DOS861",
            "DOS863",
            "DOS865",
            "EUCJ_0208",
            "GB_2312",
            "ISO8859_1",
            "ISO8859_2",
            "KSC_5601",
            "ISO2022-JP",
            "SJIS_0208",
            "UNICODE_FSS",
            "WIN1250",
            "WIN1251",
            "WIN1252",
            "WIN1253",
            "WIN1254",
            "WIN1257"});
            this.cboCharset.Location = new System.Drawing.Point(313, 17);
            this.cboCharset.Name = "cboCharset";
            this.cboCharset.Size = new System.Drawing.Size(144, 21);
            this.cboCharset.TabIndex = 20;
            this.cboCharset.SelectedIndex = 0;
            this.cboCharset.TextChanged += new System.EventHandler(this.SetProperty);
            // 
            // openFileDialog
            // 
            this.openFileDialog.DefaultExt = "FDB";
            this.openFileDialog.Filter = "Firebird Databases|*.fdb|Interbase Databases|*.gdb|All files|*.*";
            // 
            // FbDataConnectionUIControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblDataSource);
            this.Controls.Add(this.cboDialect);
            this.Controls.Add(this.grbSettings);
            this.Controls.Add(this.grbLogin);
            this.Controls.Add(this.cmdGetFile);
            this.Controls.Add(this.txtDatabase);
            this.Controls.Add(this.lblDatabase);
            this.Controls.Add(this.txtPort);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblDialect);
            this.Controls.Add(this.txtDataSource);
            this.Controls.Add(this.lblCharset);
            this.Controls.Add(this.cboCharset);
            this.Controls.Add(this.cmdTest);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.MinimumSize = new System.Drawing.Size(457, 198);
            this.Name = "FbDataConnectionUIControl";
            this.Size = new System.Drawing.Size(457, 198);
            this.grbSettings.ResumeLayout(false);
            this.grbLogin.ResumeLayout(false);
            this.grbLogin.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cmdTest;
        private System.Windows.Forms.Label lblDataSource;
        private System.Windows.Forms.ComboBox cboDialect;
        private System.Windows.Forms.GroupBox grbSettings;
        private System.Windows.Forms.ComboBox cboServerType;
        private System.Windows.Forms.Label lblServerType;
        private System.Windows.Forms.GroupBox grbLogin;
        private System.Windows.Forms.Label lblRole;
        private System.Windows.Forms.TextBox txtRole;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label lblUser;
        private System.Windows.Forms.TextBox txtUserName;
        private System.Windows.Forms.Button cmdGetFile;
        private System.Windows.Forms.TextBox txtDatabase;
        private System.Windows.Forms.Label lblDatabase;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblDialect;
        private System.Windows.Forms.TextBox txtDataSource;
        private System.Windows.Forms.Label lblCharset;
        private System.Windows.Forms.ComboBox cboCharset;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
    }
}
