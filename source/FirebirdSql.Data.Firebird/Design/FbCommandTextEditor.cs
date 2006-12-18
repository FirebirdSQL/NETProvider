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
using System.Data;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace FirebirdSql.Data.Firebird.Design
{
	internal class FbCommandTextEditor : System.Windows.Forms.Form
	{
		private System.Windows.Forms.TabPage tabError;
		private System.Windows.Forms.Button cmdOk;
		private System.Windows.Forms.DataGrid grdResult;
		private System.Windows.Forms.TextBox txtPlan;
		private System.Windows.Forms.Button cmdExecute;
		private System.Windows.Forms.Button cmdCancel;
		private System.Windows.Forms.TabPage tabPlan;
		private System.Data.DataSet dsResult;
		private System.Windows.Forms.TextBox txtCommandText;
		private System.Windows.Forms.TabControl tabCommon;
		private System.Windows.Forms.TabPage tabResult;
		private System.Windows.Forms.CheckBox cbGenParamColl;
		private System.Windows.Forms.TextBox txtError;
		private System.Data.DataTable tblCommandResult;
		private System.ComponentModel.Container components = null;
		private FbCommand command;

		private FbCommandTextEditor()
		{
			InitializeComponent();
		}

		public FbCommandTextEditor(FbCommand command) : this()
		{
			this.command				= command;
			this.txtCommandText.Text	= command.CommandText;
			
			if (command.CommandType == CommandType.StoredProcedure)
			{
				this.cbGenParamColl.Enabled = true;
			}
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
		private void InitializeComponent() {
			this.tblCommandResult = new System.Data.DataTable();
			this.txtError = new System.Windows.Forms.TextBox();
			this.cbGenParamColl = new System.Windows.Forms.CheckBox();
			this.tabResult = new System.Windows.Forms.TabPage();
			this.tabCommon = new System.Windows.Forms.TabControl();
			this.txtCommandText = new System.Windows.Forms.TextBox();
			this.dsResult = new System.Data.DataSet();
			this.tabPlan = new System.Windows.Forms.TabPage();
			this.cmdCancel = new System.Windows.Forms.Button();
			this.cmdExecute = new System.Windows.Forms.Button();
			this.txtPlan = new System.Windows.Forms.TextBox();
			this.grdResult = new System.Windows.Forms.DataGrid();
			this.cmdOk = new System.Windows.Forms.Button();
			this.tabError = new System.Windows.Forms.TabPage();
			((System.ComponentModel.ISupportInitialize)(this.tblCommandResult)).BeginInit();
			this.tabResult.SuspendLayout();
			this.tabCommon.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dsResult)).BeginInit();
			this.tabPlan.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.grdResult)).BeginInit();
			this.tabError.SuspendLayout();
			this.SuspendLayout();
			// 
			// tblCommandResult
			// 
			this.tblCommandResult.TableName = "CommandResult";
			// 
			// txtError
			// 
			this.txtError.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtError.Location = new System.Drawing.Point(0, 0);
			this.txtError.Multiline = true;
			this.txtError.Name = "txtError";
			this.txtError.ReadOnly = true;
			this.txtError.Size = new System.Drawing.Size(465, 190);
			this.txtError.TabIndex = 0;
			this.txtError.Text = "";
			// 
			// cbGenParamColl
			// 
			this.cbGenParamColl.Enabled = false;
			this.cbGenParamColl.Location = new System.Drawing.Point(136, 137);
			this.cbGenParamColl.Name = "cbGenParamColl";
			this.cbGenParamColl.Size = new System.Drawing.Size(184, 24);
			this.cbGenParamColl.TabIndex = 5;
			this.cbGenParamColl.Text = "Generate parameters collection";
			// 
			// tabResult
			// 
			this.tabResult.Controls.Add(this.grdResult);
			this.tabResult.Location = new System.Drawing.Point(4, 22);
			this.tabResult.Name = "tabResult";
			this.tabResult.Size = new System.Drawing.Size(465, 190);
			this.tabResult.TabIndex = 0;
			this.tabResult.Text = "Result";
			// 
			// tabCommon
			// 
			this.tabCommon.Controls.Add(this.tabResult);
			this.tabCommon.Controls.Add(this.tabPlan);
			this.tabCommon.Controls.Add(this.tabError);
			this.tabCommon.Location = new System.Drawing.Point(8, 168);
			this.tabCommon.Name = "tabCommon";
			this.tabCommon.SelectedIndex = 0;
			this.tabCommon.Size = new System.Drawing.Size(473, 216);
			this.tabCommon.TabIndex = 1;
			// 
			// txtCommandText
			// 
			this.txtCommandText.Location = new System.Drawing.Point(8, 8);
			this.txtCommandText.Multiline = true;
			this.txtCommandText.Name = "txtCommandText";
			this.txtCommandText.Size = new System.Drawing.Size(472, 120);
			this.txtCommandText.TabIndex = 0;
			this.txtCommandText.Text = "";
			this.txtCommandText.TextChanged += new System.EventHandler(this.txtCommandText_TextChanged);
			// 
			// dsResult
			// 
			this.dsResult.DataSetName = "CommandResult";
			this.dsResult.Locale = new System.Globalization.CultureInfo("");
			this.dsResult.Tables.AddRange(new System.Data.DataTable[] {
						this.tblCommandResult});
			// 
			// tabPlan
			// 
			this.tabPlan.Controls.Add(this.txtPlan);
			this.tabPlan.Location = new System.Drawing.Point(4, 22);
			this.tabPlan.Name = "tabPlan";
			this.tabPlan.Size = new System.Drawing.Size(465, 190);
			this.tabPlan.TabIndex = 1;
			this.tabPlan.Text = "Plan";
			// 
			// cmdCancel
			// 
			this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cmdCancel.Location = new System.Drawing.Point(405, 136);
			this.cmdCancel.Name = "cmdCancel";
			this.cmdCancel.TabIndex = 4;
			this.cmdCancel.Text = "&Cancel";
			this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
			// 
			// cmdExecute
			// 
			this.cmdExecute.Enabled = false;
			this.cmdExecute.Location = new System.Drawing.Point(8, 136);
			this.cmdExecute.Name = "cmdExecute";
			this.cmdExecute.TabIndex = 2;
			this.cmdExecute.Text = "&Execute";
			this.cmdExecute.Click += new System.EventHandler(this.cmdExecute_Click);
			// 
			// txtPlan
			// 
			this.txtPlan.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtPlan.Location = new System.Drawing.Point(0, 0);
			this.txtPlan.Multiline = true;
			this.txtPlan.Name = "txtPlan";
			this.txtPlan.ReadOnly = true;
			this.txtPlan.Size = new System.Drawing.Size(465, 190);
			this.txtPlan.TabIndex = 0;
			this.txtPlan.Text = "";
			// 
			// grdResult
			// 
			this.grdResult.DataMember = "CommandResult";
			this.grdResult.DataSource = this.dsResult;
			this.grdResult.Dock = System.Windows.Forms.DockStyle.Fill;
			this.grdResult.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.grdResult.Location = new System.Drawing.Point(0, 0);
			this.grdResult.Name = "grdResult";
			this.grdResult.ReadOnly = true;
			this.grdResult.Size = new System.Drawing.Size(465, 190);
			this.grdResult.TabIndex = 0;
			// 
			// cmdOk
			// 
			this.cmdOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.cmdOk.Location = new System.Drawing.Point(317, 136);
			this.cmdOk.Name = "cmdOk";
			this.cmdOk.Size = new System.Drawing.Size(80, 23);
			this.cmdOk.TabIndex = 3;
			this.cmdOk.Text = "&Accept";
			this.cmdOk.Click += new System.EventHandler(this.cmdAccept_Click);
			// 
			// tabError
			// 
			this.tabError.Controls.Add(this.txtError);
			this.tabError.Location = new System.Drawing.Point(4, 22);
			this.tabError.Name = "tabError";
			this.tabError.Size = new System.Drawing.Size(465, 190);
			this.tabError.TabIndex = 2;
			this.tabError.Text = "Error";
			// 
			// FbCommandTextEditor
			// 
			this.AcceptButton = this.cmdOk;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.cmdCancel;
			this.ClientSize = new System.Drawing.Size(490, 392);
			this.Controls.Add(this.cmdCancel);
			this.Controls.Add(this.cmdOk);
			this.Controls.Add(this.cmdExecute);
			this.Controls.Add(this.tabCommon);
			this.Controls.Add(this.txtCommandText);
			this.Controls.Add(this.cbGenParamColl);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "FbCommandTextEditor";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "CommandTextEditor";
			((System.ComponentModel.ISupportInitialize)(this.tblCommandResult)).EndInit();
			this.tabResult.ResumeLayout(false);
			this.tabCommon.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dsResult)).EndInit();
			this.tabPlan.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.grdResult)).EndInit();
			this.tabError.ResumeLayout(false);
			this.ResumeLayout(false);
		}
		#endregion

		#region Methods

		private void execute()
		{
			FbConnection connection = command.Connection;
			try
			{
				// Set up dataset
				this.dsResult.Tables[0].Clear();
				this.dsResult.Tables[0].Columns.Clear();
				this.dsResult.Clear();

				// Open connection
				connection.Open();

				// Update CommandText
				this.command.CommandText = this.txtCommandText.Text;

				// Execute command & fill result
				FbDataAdapter adapter = new FbDataAdapter(this.command);
				adapter.Fill(this.dsResult, "CommandResult");

				// Fill Plan
				this.txtPlan.Text = command.CommandPlan;

				// Set restlt tab as active tab
				this.tabCommon.SelectedTab = this.tabResult;
			}
			catch (Exception ex)
			{
				this.dsResult.Clear();
				this.txtError.Text = ex.Message;
				this.tabCommon.SelectedTab = this.tabError;
			}
			finally
			{
				connection.Close();
			}
		}

		#endregion

		private void cmdExecute_Click(object sender, System.EventArgs e)
		{
			this.execute();
		}

		private void cmdAccept_Click(object sender, System.EventArgs e)
		{
			this.command.CommandText = this.txtCommandText.Text;
			
			if (this.cbGenParamColl.Checked)
			{
				this.command.Parameters.Clear();
				this.command.Connection.Open();

				try
				{
					FbCommandBuilder.DeriveParameters(this.command);
				}
				finally
				{
					this.command.Connection.Close();
				}
			}
		}

		private void cmdCancel_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;		
		}

		private void txtCommandText_TextChanged(object sender, System.EventArgs e)
		{
			if (this.txtCommandText.Text != null &&
				this.txtCommandText.Text.Trim().Length > 0)
			{
				this.cmdExecute.Enabled = true;
			}
			else
			{
				this.cmdExecute.Enabled = false;
			}
		}
	}
}
