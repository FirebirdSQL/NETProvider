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
 *	Copyright (c) 2002, 2006 Carlos Guzman Alvarez
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

namespace FirebirdSql.Data.Design.DataAdapter
{
	/// <summary>
	/// Step 5 of the Firebird Data	Adapter	Configuration Wizard
	/// </summary>
	internal class FbDataAdapterConfigurationStep5 : ActionStep
	{
		private CheckBox chkGenerateStatements;
		private TextBox txtSelectSQL;
		private Label label1;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region · Constructors ·

		/// <summary>
		/// Initializes	a new instance of the <b>WizardStep</b>	class.
		/// </summary>
		public FbDataAdapterConfigurationStep5()
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
            this.chkGenerateStatements = new System.Windows.Forms.CheckBox();
            this.txtSelectSQL = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.designArea.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgLogo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(316, 22);
            this.lblCaption.Text = "Generate the Stored Procedures";
            // 
            // lblDescription
            // 
            this.lblDescription.Location = new System.Drawing.Point(29, 33);
            this.lblDescription.Size = new System.Drawing.Size(371, 25);
            this.lblDescription.Text = "The Select statement will be used to create Insert, Update and Delete stored proc" +
                "edures.";
            // 
            // designArea
            // 
            this.designArea.Controls.Add(this.chkGenerateStatements);
            this.designArea.Controls.Add(this.txtSelectSQL);
            this.designArea.Controls.Add(this.label1);
            // 
            // chkGenerateStatements
            // 
            this.chkGenerateStatements.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkGenerateStatements.Location = new System.Drawing.Point(12, 209);
            this.chkGenerateStatements.Name = "chkGenerateStatements";
            this.chkGenerateStatements.Size = new System.Drawing.Size(336, 17);
            this.chkGenerateStatements.TabIndex = 6;
            this.chkGenerateStatements.Text = "Generate Insert, Update and Delete statements.";
            // 
            // txtSelectSQL
            // 
            this.ErrorProvider.SetIconAlignment(this.txtSelectSQL, System.Windows.Forms.ErrorIconAlignment.TopRight);
            this.txtSelectSQL.Location = new System.Drawing.Point(12, 49);
            this.txtSelectSQL.Multiline = true;
            this.txtSelectSQL.Name = "txtSelectSQL";
            this.txtSelectSQL.Size = new System.Drawing.Size(467, 149);
            this.txtSelectSQL.TabIndex = 5;
            this.txtSelectSQL.TextChanged += new System.EventHandler(this.txtSelectSQL_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(215, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Type in your Select Stored Procedure name";
            // 
            // FbDataAdapterConfigurationStep5
            // 
            this.Name = "FbDataAdapterConfigurationStep5";
            this.designArea.ResumeLayout(false);
            this.designArea.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgLogo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorProvider)).EndInit();
            this.ResumeLayout(false);

		}

		#endregion

		#region · Overriden methods ·

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

		#region · Methods ·

		public override void SaveSettings()
		{
			FbDataAdapterWizardSettings.UpdateCommandText(this.txtSelectSQL.Text);
			FbDataAdapterWizardSettings.UpdateDmlGeneration(this.chkGenerateStatements.Checked);
		}

		#endregion

		#region · Event Handlers ·

		private void txtSelectSQL_TextChanged(object sender, EventArgs e)
		{
			this.PerformValidation();
		}

		#endregion
	}
}

#endif