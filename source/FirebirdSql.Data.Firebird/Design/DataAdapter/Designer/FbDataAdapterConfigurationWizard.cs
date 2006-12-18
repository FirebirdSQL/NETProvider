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
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using FirebirdSql.Data.Firebird;
using FirebirdSql.WizardFramework;

namespace FirebirdSql.Data.Firebird.Design
{
	internal class FbDataAdapterConfigurationWizard : WizardForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Constructors

		public FbDataAdapterConfigurationWizard()
		{
			InitializeComponent();

			base.InitializeWizard();
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

		#endregion

		#region Windows	Form Designer generated	code

		/// <summary>
		/// Required method	for	Designer support - do not modify
		/// the	contents of	this method	with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.SuspendLayout();
			// 
			// FbDataAdapterConfigurationWizard
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(493, 356);
			this.Name = "FbDataAdapterConfigurationWizard";
			this.Text = "Data Adapter Configuration Wizard";
			this.LoadSteps += new System.EventHandler(this.FbDataAdapterConfigurationWizard_LoadSteps);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		#region Event handlers

		private void FbDataAdapterConfigurationWizard_LoadSteps(object sender, EventArgs e)
		{
			// Create wizard steps
			FbDataAdapterConfigurationStep1 step1 = new FbDataAdapterConfigurationStep1();
			FbDataAdapterConfigurationStep2 step2 = new FbDataAdapterConfigurationStep2();
			FbDataAdapterConfigurationStep3 step3 = new FbDataAdapterConfigurationStep3();
			FbDataAdapterConfigurationStep4 step4 = new FbDataAdapterConfigurationStep4();
			FbDataAdapterConfigurationStep5 step5 = new FbDataAdapterConfigurationStep5();
			FbDataAdapterConfigurationStep6 step6 = new FbDataAdapterConfigurationStep6();
			FbDataAdapterConfigurationStep7 step7 = new FbDataAdapterConfigurationStep7();

			// Configure navigations
			step1.NextSteps.Add(step2);

			step2.BackSteps.Add(step1);
			step2.NextSteps.Add(step3);

			step3.BackSteps.Add(step2);
			step3.NextSteps.Add(step4);
			step3.NextSteps.Add(step5);
			step3.NextSteps.Add(step6);

			step4.BackSteps.Add(step3);
			step4.NextSteps.Add(step7);

			step5.BackSteps.Add(step3);
			step5.NextSteps.Add(step7);

			step6.BackSteps.Add(step3);
			step6.NextSteps.Add(step7);

			step7.BackSteps.Add(step4);
			step7.BackSteps.Add(step5);
			step7.BackSteps.Add(step6);

			// Add wizard steps
			this.Steps.Add(step1);
			this.Steps.Add(step2);
			this.Steps.Add(step3);
			this.Steps.Add(step4);
			this.Steps.Add(step5);
			this.Steps.Add(step6);
			this.Steps.Add(step7);
		}

		#endregion
	}
}

#endif