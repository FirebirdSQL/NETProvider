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
	/// Step 3 of the Firebird Data	Adapter	Configuration Wizard
	/// </summary>
	internal class FbDataAdapterConfigurationStep3 : ActionStep
	{
		private Label label1;
		private Label label4;
		private Label label3;
		private Label label2;
		private RadioButton rbExistingSP;
		private RadioButton rbNewSP;
		private RadioButton rbSqlStatements;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Constructors

		/// <summary>
		/// Initializes	a new instance of the <b>WizardStep</b>	class.
		/// </summary>
		public FbDataAdapterConfigurationStep3()
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
			this.rbSqlStatements = new System.Windows.Forms.RadioButton();
			this.rbNewSP = new System.Windows.Forms.RadioButton();
			this.rbExistingSP = new System.Windows.Forms.RadioButton();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.designArea.SuspendLayout();
			// ((System.ComponentModel.ISupportInitialize)(this.imgLogo)).BeginInit();
			// ((System.ComponentModel.ISupportInitialize)(this.ErrorProvider)).BeginInit();
			this.SuspendLayout();
			// 
			// lblCaption
			// 
			this.lblCaption.Size = new System.Drawing.Size(246, 22);
			this.lblCaption.Text = "Choose a Command Type";
			// 
			// lblDescription
			// 
			this.lblDescription.Size = new System.Drawing.Size(366, 17);
			this.lblDescription.Text = "The data adapter uses SQL statements or stored procedures.";
			// 
			// designArea
			// 
			this.designArea.Controls.Add(this.label4);
			this.designArea.Controls.Add(this.label3);
			this.designArea.Controls.Add(this.label2);
			this.designArea.Controls.Add(this.rbExistingSP);
			this.designArea.Controls.Add(this.rbNewSP);
			this.designArea.Controls.Add(this.rbSqlStatements);
			this.designArea.Controls.Add(this.label1);
			// 
			// label1
			// 
			this.label1.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(9, 11);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(332, 15);
			this.label1.TabIndex = 0;
			this.label1.Text = "How should the data adapter access the database ?";
			// 
			// rbSqlStatements
			// 
			this.rbSqlStatements.Checked = true;
			this.rbSqlStatements.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.rbSqlStatements.Location = new System.Drawing.Point(9, 41);
			this.rbSqlStatements.Name = "rbSqlStatements";
			this.rbSqlStatements.Size = new System.Drawing.Size(152, 17);
			this.rbSqlStatements.TabIndex = 1;
			this.rbSqlStatements.Text = "Use SQL Statements";
			// 
			// rbNewSP
			// 
			this.rbNewSP.Enabled = false;
			this.rbNewSP.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.rbNewSP.Location = new System.Drawing.Point(9, 109);
			this.rbNewSP.Name = "rbNewSP";
			this.rbNewSP.Size = new System.Drawing.Size(220, 17);
			this.rbNewSP.TabIndex = 2;
			this.rbNewSP.Text = "Create new Stored Procedures";
			// 
			// rbExistingSP
			// 
			this.rbExistingSP.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.rbExistingSP.Location = new System.Drawing.Point(9, 174);
			this.rbExistingSP.Name = "rbExistingSP";
			this.rbExistingSP.Size = new System.Drawing.Size(227, 17);
			this.rbExistingSP.TabIndex = 3;
			this.rbExistingSP.Text = "Use Existing Stored Procedures";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(29, 65);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(437, 27);
			this.label2.TabIndex = 4;
			this.label2.Text = "Specify a Select statement to load data, and the wizard will generate the Insert," +
				" Update and Delete statements to save data changes.";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(29, 133);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(437, 32);
			this.label3.TabIndex = 5;
			this.label3.Text = "Specify a Select statement, and the wizard will generate new Stored\tProcedures fo" +
				"r Insert, Update and Delete records";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(29, 198);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(437, 29);
			this.label4.TabIndex = 6;
			this.label4.Text = "Choose an existing Stored Procedure for each operation (select, insert, update an" +
				"d delete)";
			// 
			// FbDataAdapterConfigurationStep3
			// 
			this.Name = "FbDataAdapterConfigurationStep3";
			this.designArea.ResumeLayout(false);
			// ((System.ComponentModel.ISupportInitialize)(this.imgLogo)).EndInit();
			// ((System.ComponentModel.ISupportInitialize)(this.ErrorProvider)).EndInit();
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

		public override void SaveSettings()
		{
			int option = 0;

			if (this.rbSqlStatements.Checked)
			{
				option = 0;
			}
			else if (this.rbNewSP.Checked)
			{
				option = 1;
			}
			else if (this.rbExistingSP.Checked)
			{
				option = 2;
			}

			FbDataAdapterWizardSettings.UpdateCommandType(option);
		}

		protected override WizardStep GetNextStep()
		{
			if (this.rbSqlStatements.Checked)
			{
				return this.NextSteps[0];
			}
			else if (this.rbNewSP.Checked)
			{
				return this.NextSteps[1];
			}
			else if (this.rbExistingSP.Checked)
			{
				return this.NextSteps[2];
			}

			return null;
		}

		#endregion
	}
}

#endif