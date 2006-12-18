/*
 *	Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *	   The contents of this file are subject to the Initial 
 *	   Developer's Public License Version 1.0 (the "License"); 
 *	   you may not use this file except in compliance with the 
 *	   License. You may obtain a copy of the License at 
 *	   http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *	   Software distributed under the License is distributed on 
 *	   an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *	   express or implied. See the License for the specific 
 *	   language governing rights and limitations under the License.
 * 
 *	Copyright (c) 2002, 2005 Carlos Guzman Alvarez
 *	All Rights Reserved.
 */

#if	(NET)

using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Windows.Forms;

using FirebirdSql.WizardFramework;

namespace FirebirdSql.Data.Firebird.Design
{
	/// <summary>
	/// Step 4 of the Firebird Data	Adapter	Configuration Wizard
	/// </summary>
	internal class FbDataAdapterConfigurationStep4 : ActionStep
	{
		private TextBox txtSelectSQL;
		private Label label1;
		private CheckBox chkGenerateStatements;
		private System.Windows.Forms.CheckBox chkUseQuotedIdentifiers;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Constructors

		/// <summary>
		/// Initializes	a new instance of the <b>WizardStep</b>	class.
		/// </summary>
		public FbDataAdapterConfigurationStep4()
			: base()
		{
			InitializeComponent();
		}

		#endregion

		#region Component Designer generated code

		/// <summary> 
		/// Required method	for	Designer support - do not modify 
		/// the	contents of	this method	with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.label1 = new System.Windows.Forms.Label();
			this.txtSelectSQL = new System.Windows.Forms.TextBox();
			this.chkGenerateStatements = new System.Windows.Forms.CheckBox();
			this.chkUseQuotedIdentifiers = new System.Windows.Forms.CheckBox();
			this.designArea.SuspendLayout();
			this.SuspendLayout();
			// 
			// lblCaption
			// 
			this.lblCaption.Name = "lblCaption";
			this.lblCaption.Size = new System.Drawing.Size(307, 22);
			this.lblCaption.Text = "Generate the SQL Statements";
			// 
			// lblDescription
			// 
			this.lblDescription.Location = new System.Drawing.Point(29, 33);
			this.lblDescription.Name = "lblDescription";
			this.lblDescription.Size = new System.Drawing.Size(361, 27);
			this.lblDescription.Text = "The Select statement will be used to create Insert, Update and Delete statements." +
				"";
			// 
			// designArea
			// 
			this.designArea.Controls.Add(this.chkUseQuotedIdentifiers);
			this.designArea.Controls.Add(this.chkGenerateStatements);
			this.designArea.Controls.Add(this.txtSelectSQL);
			this.designArea.Controls.Add(this.label1);
			this.designArea.Name = "designArea";
			// 
			// imgLogo
			// 
			this.imgLogo.Name = "imgLogo";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 23);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(176, 17);
			this.label1.TabIndex = 0;
			this.label1.Text = "Type in your select SQL statement";
			// 
			// txtSelectSQL
			// 
			this.txtSelectSQL.AutoSize = false;
			this.txtSelectSQL.Location = new System.Drawing.Point(12, 48);
			this.txtSelectSQL.Multiline = true;
			this.txtSelectSQL.Name = "txtSelectSQL";
			this.txtSelectSQL.Size = new System.Drawing.Size(467, 136);
			this.txtSelectSQL.TabIndex = 2;
			this.txtSelectSQL.Text = "";
			this.txtSelectSQL.TextChanged += new System.EventHandler(this.txtSelectSQL_TextChanged);
			// 
			// chkGenerateStatements
			// 
			this.chkGenerateStatements.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.chkGenerateStatements.Location = new System.Drawing.Point(12, 192);
			this.chkGenerateStatements.Name = "chkGenerateStatements";
			this.chkGenerateStatements.Size = new System.Drawing.Size(336, 17);
			this.chkGenerateStatements.TabIndex = 3;
			this.chkGenerateStatements.Text = "Generate Insert, Update and Delete statements.";
			this.chkGenerateStatements.CheckedChanged += new System.EventHandler(this.chkGenerateStatements_CheckedChanged);
			// 
			// chkUseQuotedIdentifiers
			// 
			this.chkUseQuotedIdentifiers.Enabled = false;
			this.chkUseQuotedIdentifiers.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.chkUseQuotedIdentifiers.Location = new System.Drawing.Point(24, 216);
			this.chkUseQuotedIdentifiers.Name = "chkUseQuotedIdentifiers";
			this.chkUseQuotedIdentifiers.Size = new System.Drawing.Size(184, 17);
			this.chkUseQuotedIdentifiers.TabIndex = 8;
			this.chkUseQuotedIdentifiers.Text = "Use Quoted Identifiers ";
			// 
			// FbDataAdapterConfigurationStep4
			// 
			this.Name = "FbDataAdapterConfigurationStep4";
			this.designArea.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		#region Overriden methods

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		/// <summary>
		/// Shows the step in the wizard
		/// </summary>
		public override void ShowStep()
		{
			this.UpdateCommandText();

			this.PerformValidation();
			base.ShowStep();
		}

		/// <summary>
		/// Performs the step validations.
		/// </summary>
		/// <returns></returns>
		public override bool IsValid()
		{
			if (this.txtSelectSQL.Text == null || this.txtSelectSQL.Text.Trim().Length == 0)
			{
				this.ErrorProvider.SetError(this.txtSelectSQL, "You need to provide a Select SQL statement");
				return false;
			}
			else
			{
				this.ErrorProvider.SetError(this.txtSelectSQL, "");
				return true;
			}
		}

		#endregion

		#region Methods

		public override void SaveSettings()
		{
			FbDataAdapterWizardSettings.UpdateCommandText(this.txtSelectSQL.Text);
			FbDataAdapterWizardSettings.UpdateDmlGeneration(this.chkGenerateStatements.Checked);
			FbDataAdapterWizardSettings.UpdateUseQuotedIdentifiers(this.chkUseQuotedIdentifiers.Checked);
		}

		#endregion

		#region Private	Methods

		private void UpdateCommandText()
		{
			FbDataAdapter adapter = FbDataAdapterWizardSettings.GetDataAdapter();
			string commandText = FbDataAdapterWizardSettings.GetCommandText();

			if (commandText == null && adapter != null && adapter.SelectCommand != null)
			{
				this.txtSelectSQL.Text = adapter.SelectCommand.CommandText;
			}
		}

		#endregion

		#region EventHandlers

		private void txtSelectSQL_TextChanged(object sender, EventArgs e)
		{
			this.PerformValidation();
		}

		private void chkGenerateStatements_CheckedChanged(object sender, System.EventArgs e)
		{
			this.chkUseQuotedIdentifiers.Enabled = this.chkGenerateStatements.Checked;
			this.chkUseQuotedIdentifiers.Checked = this.chkGenerateStatements.Checked;
		}

		#endregion
	}
}

#endif