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

namespace FirebirdSql.WizardFramework
{
	internal class WelcomeStep : FirebirdSql.WizardFramework.WizardStep
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		protected Panel panel1;
		protected Panel panel2;
		protected Label lblTitle;
		protected Label lblWelcomeText;
		protected Label label1;

		#region Constructors

		/// <summary>
		/// Initializes	a new instance of the <b>WelcomeStep</b> class.
		/// </summary>
		protected WelcomeStep()
		{
			InitializeComponent();
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method	for	Designer support - do not modify 
		/// the	contents of	this method	with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.panel1 = new System.Windows.Forms.Panel();
			this.panel2 = new System.Windows.Forms.Panel();
			this.label1 = new System.Windows.Forms.Label();
			this.lblWelcomeText = new System.Windows.Forms.Label();
			this.lblTitle = new System.Windows.Forms.Label();
			//((System.ComponentModel.ISupportInitialize)(this.ErrorProvider)).BeginInit();
			this.panel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.SystemColors.HotTrack;
			this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(147, 317);
			this.panel1.TabIndex = 0;
			// 
			// panel2
			// 
			this.panel2.BackColor = System.Drawing.SystemColors.Window;
			this.panel2.Controls.Add(this.label1);
			this.panel2.Controls.Add(this.lblWelcomeText);
			this.panel2.Controls.Add(this.lblTitle);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel2.Location = new System.Drawing.Point(147, 0);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(345, 317);
			this.panel2.TabIndex = 1;
			// 
			// label1
			// 
			this.label1.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(15, 195);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(133, 15);
			this.label1.TabIndex = 2;
			this.label1.Text = "Click Next to continue.";
			// 
			// lblWelcomeText
			// 
			this.lblWelcomeText.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblWelcomeText.Location = new System.Drawing.Point(15, 72);
			this.lblWelcomeText.Name = "lblWelcomeText";
			this.lblWelcomeText.Size = new System.Drawing.Size(205, 15);
			this.lblWelcomeText.TabIndex = 1;
			this.lblWelcomeText.Text = "Welcome to the FirebirdSQL wizard";
			// 
			// lblTitle
			// 
			this.lblTitle.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblTitle.Location = new System.Drawing.Point(15, 23);
			this.lblTitle.Name = "lblTitle";
			this.lblTitle.Size = new System.Drawing.Size(107, 26);
			this.lblTitle.TabIndex = 0;
			this.lblTitle.Text = "Welcome";
			// 
			// WelcomeStep
			// 
			this.CanCancel = true;
			this.CanMoveNext = true;
			this.Controls.Add(this.panel2);
			this.Controls.Add(this.panel1);
			this.Name = "WelcomeStep";
			//((System.ComponentModel.ISupportInitialize)(this.ErrorProvider)).EndInit();
			this.panel2.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion
	}
}

#endif