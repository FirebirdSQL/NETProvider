/*
 *  Firebird ADO.NET Data provider for .NET and Mono 
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
 *  Copyright (c) 2002, 2006 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

#if (NET)

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using FirebirdSql.Data.FirebirdClient;

namespace FirebirdSql.Data.Design.Command
{
	internal class FbCommandTextEditor : System.Windows.Forms.Form
	{
		private System.Windows.Forms.TabControl tabCommon;
		private System.Windows.Forms.TabPage tabResult;
		private System.Windows.Forms.TabPage tabPlan;
		private System.Windows.Forms.TabPage tabError;
		private System.Windows.Forms.TextBox txtPlan;
		private System.Windows.Forms.TextBox txtError;
		private System.Windows.Forms.Button cmdExecute;
		private System.Windows.Forms.Button cmdCancel;
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.Button cmdOk;
		private System.Windows.Forms.TextBox txtCommandText;
		private System.Data.DataSet dsResult;
		private System.Data.DataTable tblCommandResult;
		private System.Windows.Forms.DataGrid grdResult;
		private FbCommand					command;

		private FbCommandTextEditor()
		{
			InitializeComponent();
		}

		public FbCommandTextEditor(FbCommand command) : this()
		{
			this.command				= command;
			this.txtCommandText.Text	= command.CommandText;
		}

		protected override void Dispose( bool disposing )
		{
			if ( disposing )
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		private void InitializeComponent()
		{
            this.txtCommandText = new System.Windows.Forms.TextBox();
            this.tabCommon = new System.Windows.Forms.TabControl();
            this.tabResult = new System.Windows.Forms.TabPage();
            this.grdResult = new System.Windows.Forms.DataGrid();
            this.dsResult = new System.Data.DataSet();
            this.tblCommandResult = new System.Data.DataTable();
            this.tabPlan = new System.Windows.Forms.TabPage();
            this.txtPlan = new System.Windows.Forms.TextBox();
            this.tabError = new System.Windows.Forms.TabPage();
            this.txtError = new System.Windows.Forms.TextBox();
            this.cmdExecute = new System.Windows.Forms.Button();
            this.cmdOk = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.tabCommon.SuspendLayout();
            this.tabResult.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grdResult)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dsResult)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tblCommandResult)).BeginInit();
            this.tabPlan.SuspendLayout();
            this.tabError.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtCommandText
            // 
            this.txtCommandText.Location = new System.Drawing.Point(8, 8);
            this.txtCommandText.Multiline = true;
            this.txtCommandText.Name = "txtCommandText";
            this.txtCommandText.Size = new System.Drawing.Size(472, 128);
            this.txtCommandText.TabIndex = 0;
            this.txtCommandText.TextChanged += new System.EventHandler(this.txtCommandText_TextChanged);
            // 
            // tabCommon
            // 
            this.tabCommon.Controls.Add(this.tabResult);
            this.tabCommon.Controls.Add(this.tabPlan);
            this.tabCommon.Controls.Add(this.tabError);
            this.tabCommon.Location = new System.Drawing.Point(8, 160);
            this.tabCommon.Name = "tabCommon";
            this.tabCommon.SelectedIndex = 0;
            this.tabCommon.Size = new System.Drawing.Size(472, 224);
            this.tabCommon.TabIndex = 1;
            // 
            // tabResult
            // 
            this.tabResult.Controls.Add(this.grdResult);
            this.tabResult.Location = new System.Drawing.Point(4, 22);
            this.tabResult.Name = "tabResult";
            this.tabResult.Size = new System.Drawing.Size(464, 198);
            this.tabResult.TabIndex = 0;
            this.tabResult.Text = "Result";
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
            this.grdResult.Size = new System.Drawing.Size(464, 198);
            this.grdResult.TabIndex = 0;
            // 
            // dsResult
            // 
            this.dsResult.DataSetName = "CommandResult";
            this.dsResult.Locale = new System.Globalization.CultureInfo("");
            this.dsResult.Tables.AddRange(new System.Data.DataTable[] {
            this.tblCommandResult});
            // 
            // tblCommandResult
            // 
            this.tblCommandResult.TableName = "CommandResult";
            // 
            // tabPlan
            // 
            this.tabPlan.Controls.Add(this.txtPlan);
            this.tabPlan.Location = new System.Drawing.Point(4, 22);
            this.tabPlan.Name = "tabPlan";
            this.tabPlan.Size = new System.Drawing.Size(464, 198);
            this.tabPlan.TabIndex = 1;
            this.tabPlan.Text = "Plan";
            // 
            // txtPlan
            // 
            this.txtPlan.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtPlan.Location = new System.Drawing.Point(0, 0);
            this.txtPlan.Multiline = true;
            this.txtPlan.Name = "txtPlan";
            this.txtPlan.ReadOnly = true;
            this.txtPlan.Size = new System.Drawing.Size(464, 198);
            this.txtPlan.TabIndex = 0;
            // 
            // tabError
            // 
            this.tabError.Controls.Add(this.txtError);
            this.tabError.Location = new System.Drawing.Point(4, 22);
            this.tabError.Name = "tabError";
            this.tabError.Size = new System.Drawing.Size(464, 198);
            this.tabError.TabIndex = 2;
            this.tabError.Text = "Error";
            // 
            // txtError
            // 
            this.txtError.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtError.Location = new System.Drawing.Point(0, 0);
            this.txtError.Multiline = true;
            this.txtError.Name = "txtError";
            this.txtError.ReadOnly = true;
            this.txtError.Size = new System.Drawing.Size(464, 198);
            this.txtError.TabIndex = 0;
            // 
            // cmdExecute
            // 
            this.cmdExecute.Enabled = false;
            this.cmdExecute.Location = new System.Drawing.Point(8, 136);
            this.cmdExecute.Name = "cmdExecute";
            this.cmdExecute.Size = new System.Drawing.Size(75, 23);
            this.cmdExecute.TabIndex = 2;
            this.cmdExecute.Text = "&Execute";
            this.cmdExecute.Click += new System.EventHandler(this.cmdExecute_Click);
            // 
            // cmdOk
            // 
            this.cmdOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.cmdOk.Location = new System.Drawing.Point(320, 136);
            this.cmdOk.Name = "cmdOk";
            this.cmdOk.Size = new System.Drawing.Size(80, 23);
            this.cmdOk.TabIndex = 3;
            this.cmdOk.Text = "&Accept";
            this.cmdOk.Click += new System.EventHandler(this.cmdAccept_Click);
            // 
            // cmdCancel
            // 
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(400, 136);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(75, 23);
            this.cmdCancel.TabIndex = 4;
            this.cmdCancel.Text = "&Cancel";
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // FbCommandTextEditor
            // 
            this.AcceptButton = this.cmdOk;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(483, 389);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdOk);
            this.Controls.Add(this.cmdExecute);
            this.Controls.Add(this.tabCommon);
            this.Controls.Add(this.txtCommandText);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FbCommandTextEditor";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "CommandText Editor";
            this.tabCommon.ResumeLayout(false);
            this.tabResult.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.grdResult)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dsResult)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tblCommandResult)).EndInit();
            this.tabPlan.ResumeLayout(false);
            this.tabPlan.PerformLayout();
            this.tabError.ResumeLayout(false);
            this.tabError.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		#region � Methods �

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

        #region � Private methods �

        private void cmdExecute_Click(object sender, System.EventArgs e)
		{
			this.execute();
		}

		private void cmdAccept_Click(object sender, System.EventArgs e)
		{
			this.command.CommandText = this.txtCommandText.Text;
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

        #endregion
    }
}

#endif