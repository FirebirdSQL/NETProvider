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
using System.Drawing;
using System.Windows.Forms;

using FirebirdSql.WizardFramework;

namespace FirebirdSql.Data.Firebird.Design
{
	/// <summary>
	/// Step 1 of the Firebird Data	Adapter	Configuration Wizard
	/// </summary>
	internal class FbDataAdapterConfigurationStep1 : WelcomeStep
	{
		#region Designer Fields

		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private	System.ComponentModel.IContainer components = null;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes	a new instance of the <b>FbDataAdapterConfigurationStep1</b> class.
		/// </summary>
		public FbDataAdapterConfigurationStep1()
		{
			InitializeComponent();
		}

		#endregion

		#region Component Designer generated code

		/// <summary> 
		/// Required method	for	Designer support - do not modify 
		/// the	contents of	this method	with the code editor.
		/// </summary>
		private	void InitializeComponent()
		{
			this.panel2.SuspendLayout();
			// ((System.ComponentModel.ISupportInitialize)(this.ErrorProvider)).BeginInit();
			this.SuspendLayout();
			// 
			// lblTitle
			// 
			this.lblTitle.Location = new System.Drawing.Point(7, 19);
			this.lblTitle.Size = new System.Drawing.Size(306, 79);
			this.lblTitle.Text = "Welcome to the Data Adapter Configuration wizard";
			// 
			// lblWelcomeText
			// 
			this.lblWelcomeText.Location = new System.Drawing.Point(15, 116);
			this.lblWelcomeText.Size = new System.Drawing.Size(313, 114);
			this.lblWelcomeText.Text = @"This wizard helps you to specify the connection and database commands that the DataAdapter uses to select data and handles changes to the Database. 
You need to provider connection information and make decisions about how you want the Database commands stored and executed.
Your ability to complete this wizard may depend on the permissions you have in the database.";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(15, 268);
			// 
			// FbDataAdapterConfigurationStep1
			// 
			this.Name = "FbDataAdapterConfigurationStep1";
			this.panel2.ResumeLayout(false);
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

		#endregion
	}
}

#endif