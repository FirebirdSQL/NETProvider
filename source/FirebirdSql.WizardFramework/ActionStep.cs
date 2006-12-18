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
	internal class ActionStep : WizardStep
	{
		#region Fields

		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		private Panel topPanel;
		protected Label lblCaption;
		protected Label lblDescription;
		private Splitter separator;
		protected Panel designArea;
		protected PictureBox imgLogo;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes	a new instance of the <b>ActionStep</b>	class.
		/// </summary>
		protected ActionStep()
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
			this.topPanel = new System.Windows.Forms.Panel();
			this.imgLogo = new System.Windows.Forms.PictureBox();
			this.lblDescription = new System.Windows.Forms.Label();
			this.lblCaption = new System.Windows.Forms.Label();
			this.separator = new System.Windows.Forms.Splitter();
			this.designArea = new System.Windows.Forms.Panel();
			this.topPanel.SuspendLayout();
			//((System.ComponentModel.ISupportInitialize)(this.imgLogo)).BeginInit();
			this.SuspendLayout();
			// 
			// topPanel
			// 
			this.topPanel.BackColor = System.Drawing.SystemColors.Window;
			this.topPanel.Controls.Add(this.imgLogo);
			this.topPanel.Controls.Add(this.lblDescription);
			this.topPanel.Controls.Add(this.lblCaption);
			this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.topPanel.Location = new System.Drawing.Point(0, 0);
			this.topPanel.Name = "topPanel";
			this.topPanel.Size = new System.Drawing.Size(492, 65);
			this.topPanel.TabIndex = 0;
			// 
			// imgLogo
			// 
			this.imgLogo.Location = new System.Drawing.Point(431, 8);
			this.imgLogo.Name = "imgLogo";
			this.imgLogo.Size = new System.Drawing.Size(48, 48);
			this.imgLogo.TabIndex = 2;
			this.imgLogo.TabStop = false;
			// 
			// lblDescription
			// 
			this.lblDescription.Location = new System.Drawing.Point(29, 37);
			this.lblDescription.Name = "lblDescription";
			this.lblDescription.Size = new System.Drawing.Size(105, 17);
			this.lblDescription.TabIndex = 1;
			this.lblDescription.Text = "Step description";
			// 
			// lblCaption
			// 
			this.lblCaption.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblCaption.Location = new System.Drawing.Point(9, 8);
			this.lblCaption.Name = "lblCaption";
			this.lblCaption.Size = new System.Drawing.Size(90, 22);
			this.lblCaption.TabIndex = 0;
			this.lblCaption.Text = "Step title";
			// 
			// separator
			// 
			this.separator.Dock = System.Windows.Forms.DockStyle.Top;
			this.separator.Enabled = false;
			this.separator.Location = new System.Drawing.Point(0, 65);
			this.separator.Name = "separator";
			this.separator.Size = new System.Drawing.Size(492, 4);
			this.separator.TabIndex = 1;
			this.separator.TabStop = false;
			// 
			// designArea
			// 
			this.designArea.Dock = System.Windows.Forms.DockStyle.Fill;
			this.designArea.Location = new System.Drawing.Point(0, 69);
			this.designArea.Name = "designArea";
			this.designArea.Size = new System.Drawing.Size(492, 248);
			this.designArea.TabIndex = 2;
			// 
			// ActionStep
			// 
			this.CanCancel = true;
			this.CanMoveBack = true;
			this.CanMoveNext = true;
			this.Controls.Add(this.designArea);
			this.Controls.Add(this.separator);
			this.Controls.Add(this.topPanel);
			this.Name = "ActionStep";
			this.topPanel.ResumeLayout(false);
			//((System.ComponentModel.ISupportInitialize)(this.imgLogo)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion
	}
}

#endif