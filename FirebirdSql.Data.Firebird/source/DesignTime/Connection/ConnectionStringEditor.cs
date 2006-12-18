/*
 *  Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.ibphoenix.com/main.nfs?a=ibphoenix&l=;PAGES;NAME='ibp_idpl'
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2002, 2004 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace FirebirdSql.Data.Firebird.Design
{
	internal class ConnectionStringEditor : System.Windows.Forms.Form
	{
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.Button cmdTest;
		private System.Windows.Forms.Button cmdAccept;
		private System.Windows.Forms.Button cmdCancel;
		private System.Windows.Forms.Label lblDataSource;
		private System.Windows.Forms.TextBox txtDataSource;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label lblDatabase;
		private System.Windows.Forms.Button cmdGetFile;
		private System.Windows.Forms.OpenFileDialog openFirebirdDB;
		private System.Windows.Forms.GroupBox grbLogin;
		private System.Windows.Forms.TextBox txtUserName;
		private System.Windows.Forms.Label lblUser;
		private System.Windows.Forms.Label lblPassword;
		private System.Windows.Forms.TextBox txtPassword;
		private System.Windows.Forms.GroupBox grbSettings;
		private System.Windows.Forms.TextBox txtPort;
		private System.Windows.Forms.TextBox txtDatabase;
		private System.Windows.Forms.CheckBox chkPooling;
		private System.Windows.Forms.Label lblRole;
		private System.Windows.Forms.TextBox txtRole;
		private System.Windows.Forms.Label lblLifeTime;
		private System.Windows.Forms.NumericUpDown txtLifeTime;
		private System.Windows.Forms.Label lblTimeout;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.NumericUpDown txtTimeout;
		private System.Windows.Forms.ComboBox cboCharset;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label lblDialect;
		private System.Windows.Forms.ComboBox cboPacketSize;
		private FbConnection connection;
		private System.Windows.Forms.ComboBox cboDialect;

		public ConnectionStringEditor()
		{
			InitializeComponent();
		}

		public ConnectionStringEditor(FbConnection connection) : this()
		{
			this.connection = connection;

			InitializeData();
		}

		private void InitializeData()
		{
			this.txtDataSource.Text = connection.Parameters.DataSource;
			this.txtPort.Text		= connection.Parameters.Port.ToString();
			this.txtDatabase.Text	= connection.Parameters.Database;
			this.txtUserName.Text	= connection.Parameters.UserName;
			this.txtPassword.Text	= connection.Parameters.UserPassword;
			this.txtRole.Text		= connection.Parameters.Role;
			this.cboPacketSize.Text	= connection.Parameters.PacketSize.ToString();
			this.cboDialect.Text	= connection.Parameters.Dialect.ToString();
			this.txtLifeTime.Value	= connection.Parameters.LifeTime;
			this.txtTimeout.Value	= connection.Parameters.Timeout;
			this.cboCharset.Text	= connection.Parameters.Charset.Name;
			this.chkPooling.Checked	= connection.Parameters.Pooling;
		}

		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		private void InitializeComponent()
		{
			this.cmdTest = new System.Windows.Forms.Button();
			this.cmdAccept = new System.Windows.Forms.Button();
			this.cmdCancel = new System.Windows.Forms.Button();
			this.lblDataSource = new System.Windows.Forms.Label();
			this.txtDataSource = new System.Windows.Forms.TextBox();
			this.panel1 = new System.Windows.Forms.Panel();
			this.cboDialect = new System.Windows.Forms.ComboBox();
			this.lblLifeTime = new System.Windows.Forms.Label();
			this.grbSettings = new System.Windows.Forms.GroupBox();
			this.cboPacketSize = new System.Windows.Forms.ComboBox();
			this.cboCharset = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.txtLifeTime = new System.Windows.Forms.NumericUpDown();
			this.txtTimeout = new System.Windows.Forms.NumericUpDown();
			this.lblTimeout = new System.Windows.Forms.Label();
			this.chkPooling = new System.Windows.Forms.CheckBox();
			this.label3 = new System.Windows.Forms.Label();
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
			this.openFirebirdDB = new System.Windows.Forms.OpenFileDialog();
			this.panel1.SuspendLayout();
			this.grbSettings.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.txtLifeTime)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.txtTimeout)).BeginInit();
			this.grbLogin.SuspendLayout();
			this.SuspendLayout();
			// 
			// cmdTest
			// 
			this.cmdTest.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cmdTest.Location = new System.Drawing.Point(7, 208);
			this.cmdTest.Name = "cmdTest";
			this.cmdTest.TabIndex = 0;
			this.cmdTest.Text = "&Test";
			this.cmdTest.Click += new System.EventHandler(this.cmdTest_Click);
			// 
			// cmdAccept
			// 
			this.cmdAccept.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.cmdAccept.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cmdAccept.Location = new System.Drawing.Point(339, 208);
			this.cmdAccept.Name = "cmdAccept";
			this.cmdAccept.TabIndex = 1;
			this.cmdAccept.Text = "&Accept";
			this.cmdAccept.Click += new System.EventHandler(this.cmdAccept_Click);
			// 
			// cmdCancel
			// 
			this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cmdCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cmdCancel.Location = new System.Drawing.Point(413, 208);
			this.cmdCancel.Name = "cmdCancel";
			this.cmdCancel.TabIndex = 2;
			this.cmdCancel.Text = "&Cancel";
			// 
			// lblDataSource
			// 
			this.lblDataSource.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblDataSource.Location = new System.Drawing.Point(16, 26);
			this.lblDataSource.Name = "lblDataSource";
			this.lblDataSource.Size = new System.Drawing.Size(64, 16);
			this.lblDataSource.TabIndex = 3;
			this.lblDataSource.Text = "Server name";
			// 
			// txtDataSource
			// 
			this.txtDataSource.Location = new System.Drawing.Point(80, 24);
			this.txtDataSource.Name = "txtDataSource";
			this.txtDataSource.Size = new System.Drawing.Size(136, 20);
			this.txtDataSource.TabIndex = 4;
			this.txtDataSource.Text = "";
			// 
			// panel1
			// 
			this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panel1.Controls.Add(this.cboDialect);
			this.panel1.Controls.Add(this.lblLifeTime);
			this.panel1.Controls.Add(this.grbSettings);
			this.panel1.Controls.Add(this.grbLogin);
			this.panel1.Controls.Add(this.cmdGetFile);
			this.panel1.Controls.Add(this.txtDatabase);
			this.panel1.Controls.Add(this.lblDatabase);
			this.panel1.Controls.Add(this.txtPort);
			this.panel1.Controls.Add(this.label1);
			this.panel1.Controls.Add(this.lblDialect);
			this.panel1.Location = new System.Drawing.Point(8, 8);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(480, 192);
			this.panel1.TabIndex = 5;
			// 
			// cboDialect
			// 
			this.cboDialect.Items.AddRange(new object[] {
						"1",
						"3"});
			this.cboDialect.Location = new System.Drawing.Point(403, 12);
			this.cboDialect.Name = "cboDialect";
			this.cboDialect.Size = new System.Drawing.Size(63, 21);
			this.cboDialect.TabIndex = 21;
			// 
			// lblLifeTime
			// 
			this.lblLifeTime.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblLifeTime.Location = new System.Drawing.Point(232, 88);
			this.lblLifeTime.Name = "lblLifeTime";
			this.lblLifeTime.Size = new System.Drawing.Size(48, 16);
			this.lblLifeTime.TabIndex = 13;
			this.lblLifeTime.Text = "Life Time";
			// 
			// grbSettings
			// 
			this.grbSettings.Controls.Add(this.cboPacketSize);
			this.grbSettings.Controls.Add(this.cboCharset);
			this.grbSettings.Controls.Add(this.label2);
			this.grbSettings.Controls.Add(this.txtLifeTime);
			this.grbSettings.Controls.Add(this.txtTimeout);
			this.grbSettings.Controls.Add(this.lblTimeout);
			this.grbSettings.Controls.Add(this.chkPooling);
			this.grbSettings.Controls.Add(this.label3);
			this.grbSettings.Location = new System.Drawing.Point(212, 64);
			this.grbSettings.Name = "grbSettings";
			this.grbSettings.Size = new System.Drawing.Size(256, 120);
			this.grbSettings.TabIndex = 12;
			this.grbSettings.TabStop = false;
			this.grbSettings.Text = "Connection Settings";
			// 
			// cboPacketSize
			// 
			this.cboPacketSize.Items.AddRange(new object[] {
						"1024",
						"2048",
						"4096",
						"8192",
						"16384"});
			this.cboPacketSize.Location = new System.Drawing.Point(184, 54);
			this.cboPacketSize.Name = "cboPacketSize";
			this.cboPacketSize.Size = new System.Drawing.Size(60, 21);
			this.cboPacketSize.TabIndex = 19;
			// 
			// cboCharset
			// 
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
			this.cboCharset.Location = new System.Drawing.Point(72, 86);
			this.cboCharset.Name = "cboCharset";
			this.cboCharset.Size = new System.Drawing.Size(121, 21);
			this.cboCharset.TabIndex = 18;
			// 
			// label2
			// 
			this.label2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label2.Location = new System.Drawing.Point(16, 88);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(48, 16);
			this.label2.TabIndex = 17;
			this.label2.Text = "Charset";
			// 
			// txtLifeTime
			// 
			this.txtLifeTime.Location = new System.Drawing.Point(72, 24);
			this.txtLifeTime.Name = "txtLifeTime";
			this.txtLifeTime.Size = new System.Drawing.Size(48, 20);
			this.txtLifeTime.TabIndex = 6;
			// 
			// txtTimeout
			// 
			this.txtTimeout.Location = new System.Drawing.Point(200, 22);
			this.txtTimeout.Name = "txtTimeout";
			this.txtTimeout.Size = new System.Drawing.Size(48, 20);
			this.txtTimeout.TabIndex = 14;
			// 
			// lblTimeout
			// 
			this.lblTimeout.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblTimeout.Location = new System.Drawing.Point(144, 24);
			this.lblTimeout.Name = "lblTimeout";
			this.lblTimeout.Size = new System.Drawing.Size(48, 16);
			this.lblTimeout.TabIndex = 15;
			this.lblTimeout.Text = "Timeout";
			// 
			// chkPooling
			// 
			this.chkPooling.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.chkPooling.Location = new System.Drawing.Point(16, 56);
			this.chkPooling.Name = "chkPooling";
			this.chkPooling.Size = new System.Drawing.Size(96, 12);
			this.chkPooling.TabIndex = 0;
			this.chkPooling.Text = "Enable pooling";
			// 
			// label3
			// 
			this.label3.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label3.Location = new System.Drawing.Point(121, 56);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(59, 16);
			this.label3.TabIndex = 14;
			this.label3.Text = "Packet size";
			// 
			// grbLogin
			// 
			this.grbLogin.Controls.Add(this.lblRole);
			this.grbLogin.Controls.Add(this.txtRole);
			this.grbLogin.Controls.Add(this.lblPassword);
			this.grbLogin.Controls.Add(this.txtPassword);
			this.grbLogin.Controls.Add(this.lblUser);
			this.grbLogin.Controls.Add(this.txtUserName);
			this.grbLogin.Location = new System.Drawing.Point(8, 64);
			this.grbLogin.Name = "grbLogin";
			this.grbLogin.Size = new System.Drawing.Size(200, 120);
			this.grbLogin.TabIndex = 11;
			this.grbLogin.TabStop = false;
			this.grbLogin.Text = "Login";
			// 
			// lblRole
			// 
			this.lblRole.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblRole.Location = new System.Drawing.Point(16, 86);
			this.lblRole.Name = "lblRole";
			this.lblRole.Size = new System.Drawing.Size(48, 14);
			this.lblRole.TabIndex = 13;
			this.lblRole.Text = "Role";
			// 
			// txtRole
			// 
			this.txtRole.Location = new System.Drawing.Point(72, 86);
			this.txtRole.Name = "txtRole";
			this.txtRole.Size = new System.Drawing.Size(112, 20);
			this.txtRole.TabIndex = 12;
			this.txtRole.Text = "";
			// 
			// lblPassword
			// 
			this.lblPassword.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblPassword.Location = new System.Drawing.Point(16, 55);
			this.lblPassword.Name = "lblPassword";
			this.lblPassword.Size = new System.Drawing.Size(48, 16);
			this.lblPassword.TabIndex = 11;
			this.lblPassword.Text = "Password";
			// 
			// txtPassword
			// 
			this.txtPassword.Location = new System.Drawing.Point(72, 55);
			this.txtPassword.Name = "txtPassword";
			this.txtPassword.PasswordChar = '*';
			this.txtPassword.Size = new System.Drawing.Size(112, 20);
			this.txtPassword.TabIndex = 10;
			this.txtPassword.Text = "";
			// 
			// lblUser
			// 
			this.lblUser.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblUser.Location = new System.Drawing.Point(16, 27);
			this.lblUser.Name = "lblUser";
			this.lblUser.Size = new System.Drawing.Size(48, 14);
			this.lblUser.TabIndex = 9;
			this.lblUser.Text = "User";
			// 
			// txtUserName
			// 
			this.txtUserName.Location = new System.Drawing.Point(72, 24);
			this.txtUserName.Name = "txtUserName";
			this.txtUserName.Size = new System.Drawing.Size(112, 20);
			this.txtUserName.TabIndex = 0;
			this.txtUserName.Text = "";
			// 
			// cmdGetFile
			// 
			this.cmdGetFile.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cmdGetFile.Location = new System.Drawing.Point(440, 40);
			this.cmdGetFile.Name = "cmdGetFile";
			this.cmdGetFile.Size = new System.Drawing.Size(24, 23);
			this.cmdGetFile.TabIndex = 10;
			this.cmdGetFile.Text = "...";
			this.cmdGetFile.Click += new System.EventHandler(this.cmdGetFile_Click);
			// 
			// txtDatabase
			// 
			this.txtDatabase.Location = new System.Drawing.Point(70, 40);
			this.txtDatabase.Name = "txtDatabase";
			this.txtDatabase.Size = new System.Drawing.Size(360, 20);
			this.txtDatabase.TabIndex = 9;
			this.txtDatabase.Text = "";
			// 
			// lblDatabase
			// 
			this.lblDatabase.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblDatabase.Location = new System.Drawing.Point(16, 40);
			this.lblDatabase.Name = "lblDatabase";
			this.lblDatabase.Size = new System.Drawing.Size(48, 16);
			this.lblDatabase.TabIndex = 8;
			this.lblDatabase.Text = "Database";
			// 
			// txtPort
			// 
			this.txtPort.Location = new System.Drawing.Point(312, 12);
			this.txtPort.Name = "txtPort";
			this.txtPort.Size = new System.Drawing.Size(40, 20);
			this.txtPort.TabIndex = 7;
			this.txtPort.Text = "";
			// 
			// label1
			// 
			this.label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label1.Location = new System.Drawing.Point(216, 14);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(88, 16);
			this.label1.TabIndex = 6;
			this.label1.Text = "Data Source Port";
			// 
			// lblDialect
			// 
			this.lblDialect.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblDialect.Location = new System.Drawing.Point(360, 14);
			this.lblDialect.Name = "lblDialect";
			this.lblDialect.Size = new System.Drawing.Size(48, 16);
			this.lblDialect.TabIndex = 20;
			this.lblDialect.Text = "Dialect";
			// 
			// openFirebirdDB
			// 
			this.openFirebirdDB.Filter = "Firebird Database(*.FDB)|*.FDB|Firebird/Interbase Database (*.GDB)" +  
				"|*.GDB";
			// 
			// ConnectionStringEditor
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(498, 239);
			this.Controls.Add(this.txtDataSource);
			this.Controls.Add(this.lblDataSource);
			this.Controls.Add(this.cmdCancel);
			this.Controls.Add(this.cmdAccept);
			this.Controls.Add(this.cmdTest);
			this.Controls.Add(this.panel1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ConnectionStringEditor";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Connection String Editor";
			this.panel1.ResumeLayout(false);
			this.grbSettings.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.txtLifeTime)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.txtTimeout)).EndInit();
			this.grbLogin.ResumeLayout(false);
			this.ResumeLayout(false);
		}
		#endregion

		private void cmdAccept_Click(object sender, System.EventArgs e)
		{
			try
			{
				connection.ConnectionString = getConnectionString();
			}
			catch (Exception)
			{
				MessageBox.Show("Invalid Connection String.");
			}
		}

		private void cmdCancel_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
		}

		private void cmdTest_Click(object sender, System.EventArgs e)
		{
			try
			{
				connection.ConnectionString = getConnectionString();
				connection.Open();

				MessageBox.Show("Connection successful.", "Connection String Editor");
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);	
			}
			finally
			{
				connection.Close();
			}
		}

		private void cmdGetFile_Click(object sender, System.EventArgs e)
		{
			DialogResult result = this.openFirebirdDB.ShowDialog();

			if (result == DialogResult.OK)
			{
				this.txtDatabase.Text = this.openFirebirdDB.FileName;
			}
		}

		private string getConnectionString()
		{
			System.Text.StringBuilder cs = new System.Text.StringBuilder();
			string csTemplate = "User={0};Password={1};Database={2};DataSource={3};Port={4};Dialect={5};Charset={6};Role={7};Connection lifetime={8};Connection timeout={9};Pooling={10};Packet Size={11}";

			cs.AppendFormat(csTemplate,
				this.txtUserName.Text,
				this.txtPassword.Text,
				this.txtDatabase.Text,
				this.txtDataSource.Text,
				this.txtPort.Text,
				this.cboDialect.Text,
				this.cboCharset.Text,
				this.txtRole.Text,
				this.txtLifeTime.Text,
				this.txtTimeout.Text,
				this.chkPooling.Checked.ToString(),
				this.cboPacketSize.Text);

			return cs.ToString();
		}
	}
}
