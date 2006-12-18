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
	/// Step 2 of the Firebird Data	Adapter	Configuration Wizard
	/// </summary>
	internal class FbDataAdapterConfigurationStep2 : ActionStep
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private	System.ComponentModel.IContainer components = null;

		private	GroupBox groupBox1;
		private	TextBox	txtConnectionString;
		private	Button cmdNewConnection;
		private	ComboBox cboDataConnections;
		private	Label label1;
		private	Label label2;

		#region Constructors

		/// <summary>
		/// Initializes	a new instance of the <b>WizardStep</b>	class.
		/// </summary>
		public FbDataAdapterConfigurationStep2()
		{
			InitializeComponent();
		}

		#endregion

		#region Overriden methods

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void	Dispose(bool disposing)
		{
			if (disposing && (components !=	null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Methods

		/// <summary>
		/// Shows the step in thw wizard
		/// </summary>
		public override	void ShowStep()
		{
			base.ShowStep();
			
			this.FillConnections();
			this.RefreshSelection();
		}

		/// <summary>
		/// Performs the step validations.
		/// </summary>
		/// <returns></returns>
		public override	bool IsValid()
		{
			if (this.cboDataConnections.SelectedItem ==	null)
			{
				this.ErrorProvider.SetError(this.cboDataConnections, "You need to select a Connection");
				return false;
			}
			else
			{
				this.ErrorProvider.SetError(this.cboDataConnections, "");
				return true;
			}
		}

		public override	void SaveSettings()
		{
			FbDataAdapterWizardSettings.UpdateConnection(this.cboDataConnections.SelectedItem);
		}

		#endregion

		#region Private	Methods

		private	IComponent[] GetConnections()
		{
			IDesignerHost host = FbDataAdapterWizardSettings.GetDesignerHost();
			ArrayList components = new ArrayList();

			foreach	(IComponent	component in host.Container.Components)
			{
				if (component is FbConnection)
				{
					components.Add(component);
				}
			}

			return (IComponent[])components.ToArray(typeof(IComponent));
		}

		private	void FillConnections()
		{
			IComponent[] components = this.GetConnections();

			this.cboDataConnections.Items.Clear();

			foreach	(IComponent	component in components)
			{
				this.cboDataConnections.Items.Add(component);
			}
		}

		private	void RefreshSelection()
		{
			if (FbDataAdapterWizardSettings.GetConnection()	!= null)
			{
				this.cboDataConnections.SelectedItem = FbDataAdapterWizardSettings.GetConnection();
				this.txtConnectionString.Text = FbDataAdapterWizardSettings.GetConnection().ConnectionString;
			}
			else
			{
				FbDataAdapter adapter = FbDataAdapterWizardSettings.GetDataAdapter();
				if (adapter.SelectCommand != null && adapter.SelectCommand.Connection != null)
				{
					this.cboDataConnections.SelectedItem = adapter.SelectCommand.Connection;
					this.txtConnectionString.Text = adapter.SelectCommand.Connection.ConnectionString;					  
				}
			}

			this.PerformValidation();
		}

		private	void CreateNewConnection()
		{
			FbConnectionStringEditor dialog = new FbConnectionStringEditor();
			dialog.ShowDialog();

			if (dialog.DialogResult == DialogResult.OK)
			{
				IDesignerHost host = FbDataAdapterWizardSettings.GetDesignerHost();
				FbConnection c = (FbConnection)host.CreateComponent(typeof(FbConnection));
				c.ConnectionString = dialog.GetConnectionString();

				if (c.ConnectionString != null && c.ConnectionString.Length	> 0)
				{
					this.FillConnections();
					this.cboDataConnections.SelectedItem = c;
				}
				this.PerformValidation();
			}

			dialog.Dispose();
		}

		#endregion

		#region Component Designer generated code

		/// <summary> 
		/// Required method	for	Designer support - do not modify 
		/// the	contents of	this method	with the code editor.
		/// </summary>
		private	void InitializeComponent()
		{
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.txtConnectionString = new System.Windows.Forms.TextBox();
			this.cmdNewConnection = new System.Windows.Forms.Button();
			this.cboDataConnections = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.designArea.SuspendLayout();
			// ((System.ComponentModel.ISupportInitialize)(this.imgLogo)).BeginInit();
			// ((System.ComponentModel.ISupportInitialize)(this.ErrorProvider)).BeginInit();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// lblCaption
			// 
			this.lblCaption.Size = new System.Drawing.Size(268, 22);
			this.lblCaption.Text = "Select your\tData Connection";
			// 
			// lblDescription
			// 
			this.lblDescription.Location = new System.Drawing.Point(29, 33);
			this.lblDescription.Size = new System.Drawing.Size(349, 24);
			this.lblDescription.Text = "The data adapter will execute queries using this connection to load and update da" +
				"ta.";
			// 
			// designArea
			// 
			this.designArea.Controls.Add(this.groupBox1);
			this.designArea.Controls.Add(this.cmdNewConnection);
			this.designArea.Controls.Add(this.cboDataConnections);
			this.designArea.Controls.Add(this.label1);
			this.designArea.Controls.Add(this.label2);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.txtConnectionString);
			this.groupBox1.Location = new System.Drawing.Point(11, 108);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(470, 121);
			this.groupBox1.TabIndex = 11;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Connection Details";
			// 
			// txtConnectionString
			// 
			this.txtConnectionString.AutoSize = false;
			this.txtConnectionString.Location = new System.Drawing.Point(7, 21);
			this.txtConnectionString.Multiline = true;
			this.txtConnectionString.Name = "txtConnectionString";
			this.txtConnectionString.ReadOnly = true;
			this.txtConnectionString.Size = new System.Drawing.Size(457, 94);
			this.txtConnectionString.TabIndex = 0;
			// 
			// cmdNewConnection
			// 
			this.cmdNewConnection.Location = new System.Drawing.Point(361, 78);
			this.cmdNewConnection.Name = "cmdNewConnection";
			this.cmdNewConnection.Size = new System.Drawing.Size(121, 23);
			this.cmdNewConnection.TabIndex = 8;
			this.cmdNewConnection.Text = "New Connection...";
			this.cmdNewConnection.Click += new System.EventHandler(this.cmdNewConnection_Click);
			// 
			// cboDataConnections
			// 
			this.cboDataConnections.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboDataConnections.Location = new System.Drawing.Point(11, 80);
			this.cboDataConnections.Name = "cboDataConnections";
			this.cboDataConnections.Size = new System.Drawing.Size(325, 21);
			this.cboDataConnections.TabIndex = 7;
			this.cboDataConnections.SelectedIndexChanged += new System.EventHandler(this.cboDataConnections_SelectedIndexChanged);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(11, 20);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(457, 27);
			this.label1.TabIndex = 9;
			this.label1.Text = "Choose an existent connection or create a new one if the one you want to use is n" +
				"ot listed.";
			// 
			// label2
			// 
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.Location = new System.Drawing.Point(11, 59);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(325, 14);
			this.label2.TabIndex = 10;
			this.label2.Text = "Which data connection should the data adapter use ?";
			// 
			// FbDataAdapterConfigurationStep2
			// 
			this.Name = "FbDataAdapterConfigurationStep2";
			this.designArea.ResumeLayout(false);
			// ((System.ComponentModel.ISupportInitialize)(this.imgLogo)).EndInit();
			// ((System.ComponentModel.ISupportInitialize)(this.ErrorProvider)).EndInit();
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		#region Event handlers

		private	void cmdNewConnection_Click(object sender, EventArgs e)
		{
			this.CreateNewConnection();
		}

		private	void cboDataConnections_SelectedIndexChanged(object	sender,	EventArgs e)
		{
			this.PerformValidation();
			this.txtConnectionString.Text = ((FbConnection)this.cboDataConnections.SelectedItem).ConnectionString;
		}

		#endregion
	}
}

#endif