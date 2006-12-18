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
using System.Data;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Windows.Forms;

using FirebirdSql.WizardFramework;

namespace FirebirdSql.Data.Firebird.Design
{
	/// <summary>
	/// Step 6 of the Firebird Data	Adapter	Configuration Wizard
	/// </summary>
	internal class FbDataAdapterConfigurationStep6 : ActionStep
	{
		private	bool initialized;
		private	Label label1;
		private	ComboBox cboDeleteSP;
		private	Label label4;
		private	ComboBox cboUpdateSP;
		private	Label label5;
		private	ComboBox cboInsertSP;
		private	Label label3;
		private	ComboBox cboSelectSP;
		private	Label label2;
		private	DataGrid grdSpParameters;
		private	System.Data.DataSet	procedures;
		private	System.Data.DataSet	parameters;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private	System.ComponentModel.IContainer components = null;

		#region Constructors

		/// <summary>
		/// Initializes	a new instance of the <b>WizardStep</b>	class.
		/// </summary>
		public FbDataAdapterConfigurationStep6()
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
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.cboSelectSP = new System.Windows.Forms.ComboBox();
			this.procedures = new System.Data.DataSet();
			this.cboInsertSP = new System.Windows.Forms.ComboBox();
			this.parameters = new System.Data.DataSet();
			this.label3 = new System.Windows.Forms.Label();
			this.cboDeleteSP = new System.Windows.Forms.ComboBox();
			this.label4 = new System.Windows.Forms.Label();
			this.cboUpdateSP = new System.Windows.Forms.ComboBox();
			this.label5 = new System.Windows.Forms.Label();
			this.grdSpParameters = new System.Windows.Forms.DataGrid();
			this.designArea.SuspendLayout();
			// ((System.ComponentModel.ISupportInitialize)(this.imgLogo)).BeginInit();
			// ((System.ComponentModel.ISupportInitialize)(this.ErrorProvider)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.procedures)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.parameters)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.grdSpParameters)).BeginInit();
			this.SuspendLayout();
			// 
			// lblCaption
			// 
			this.lblCaption.Size = new System.Drawing.Size(323, 22);
			this.lblCaption.Text = "Use existing Stored Procedures";
			// 
			// lblDescription
			// 
			this.lblDescription.Location = new System.Drawing.Point(29, 33);
			this.lblDescription.Size = new System.Drawing.Size(332, 25);
			this.lblDescription.Text = "Choose the Stored Procedures to call and specifcy any required parameters.";
			// 
			// designArea
			// 
			this.designArea.Controls.Add(this.grdSpParameters);
			this.designArea.Controls.Add(this.cboDeleteSP);
			this.designArea.Controls.Add(this.label4);
			this.designArea.Controls.Add(this.cboUpdateSP);
			this.designArea.Controls.Add(this.label5);
			this.designArea.Controls.Add(this.cboInsertSP);
			this.designArea.Controls.Add(this.label3);
			this.designArea.Controls.Add(this.cboSelectSP);
			this.designArea.Controls.Add(this.label2);
			this.designArea.Controls.Add(this.label1);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(9, 24);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(470, 34);
			this.label1.TabIndex = 0;
			this.label1.Text = "Select the stored procedure for each operation.  If the procedure requires parame" +
				"ters, specify which column in the data row contains the parameter value.";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(9, 95);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(34, 15);
			this.label2.TabIndex = 1;
			this.label2.Text = "Select";
			// 
			// cboSelectSP
			// 
			this.cboSelectSP.DataSource = this.procedures;
			this.cboSelectSP.Location = new System.Drawing.Point(60, 89);
			this.cboSelectSP.Name = "cboSelectSP";
			this.cboSelectSP.Size = new System.Drawing.Size(165, 21);
			this.cboSelectSP.TabIndex = 2;
			this.cboSelectSP.SelectedIndexChanged += new System.EventHandler(this.cboSelectSP_SelectedIndexChanged);
			this.cboSelectSP.Enter += new System.EventHandler(this.cboSelectSP_Enter);
			// 
			// procedures
			// 
			this.procedures.DataSetName = "procedures";
			this.procedures.Locale = new System.Globalization.CultureInfo("es-ES");
			// 
			// cboInsertSP
			// 
			this.cboInsertSP.DataSource = this.procedures;
			this.cboInsertSP.Location = new System.Drawing.Point(59, 125);
			this.cboInsertSP.Name = "cboInsertSP";
			this.cboInsertSP.Size = new System.Drawing.Size(165, 21);
			this.cboInsertSP.TabIndex = 4;
			this.cboInsertSP.SelectedIndexChanged += new System.EventHandler(this.cboInsertSP_SelectedIndexChanged);
			this.cboInsertSP.Enter += new System.EventHandler(this.cboInsertSP_Enter);
			// 
			// parameters
			// 
			this.parameters.DataSetName = "parameters";
			this.parameters.Locale = new System.Globalization.CultureInfo("es-ES");
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(8, 128);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(34, 15);
			this.label3.TabIndex = 3;
			this.label3.Text = "Insert";
			// 
			// cboDeleteSP
			// 
			this.cboDeleteSP.DataSource = this.procedures;
			this.cboDeleteSP.Location = new System.Drawing.Point(58, 197);
			this.cboDeleteSP.Name = "cboDeleteSP";
			this.cboDeleteSP.Size = new System.Drawing.Size(165, 21);
			this.cboDeleteSP.TabIndex = 8;
			this.cboDeleteSP.SelectedIndexChanged += new System.EventHandler(this.cboDeleteSP_SelectedIndexChanged);
			this.cboDeleteSP.Enter += new System.EventHandler(this.cboDeleteSP_Enter);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(7, 202);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(36, 15);
			this.label4.TabIndex = 7;
			this.label4.Text = "Delete";
			// 
			// cboUpdateSP
			// 
			this.cboUpdateSP.DataSource = this.procedures;
			this.cboUpdateSP.Location = new System.Drawing.Point(59, 161);
			this.cboUpdateSP.Name = "cboUpdateSP";
			this.cboUpdateSP.Size = new System.Drawing.Size(165, 21);
			this.cboUpdateSP.TabIndex = 6;
			this.cboUpdateSP.SelectedIndexChanged += new System.EventHandler(this.cboUpdateSP_SelectedIndexChanged);
			this.cboUpdateSP.Enter += new System.EventHandler(this.cboUpdateSP_Enter);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(8, 165);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(40, 15);
			this.label5.TabIndex = 5;
			this.label5.Text = "Update";
			// 
			// grdSpParameters
			// 
			this.grdSpParameters.AllowNavigation = false;
			this.grdSpParameters.DataMember = "";
			this.grdSpParameters.DataSource = this.parameters;
			this.grdSpParameters.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.grdSpParameters.Location = new System.Drawing.Point(237, 89);
			this.grdSpParameters.Name = "grdSpParameters";
			this.grdSpParameters.PreferredColumnWidth = 150;
			this.grdSpParameters.ReadOnly = true;
			this.grdSpParameters.RowHeadersVisible = false;
			this.grdSpParameters.Size = new System.Drawing.Size(240, 129);
			this.grdSpParameters.TabIndex = 9;
			// 
			// FbDataAdapterConfigurationStep6
			// 
			this.Name = "FbDataAdapterConfigurationStep6";
			this.Load += new System.EventHandler(this.FbDataAdapterConfigurationStep6_Load);
			this.designArea.ResumeLayout(false);
			this.designArea.PerformLayout();
			// ((System.ComponentModel.ISupportInitialize)(this.imgLogo)).EndInit();
			// ((System.ComponentModel.ISupportInitialize)(this.ErrorProvider)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.procedures)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.parameters)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.grdSpParameters)).EndInit();
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
			this.UpdateStoredProcs();

			this.PerformValidation();
			base.ShowStep();
		}

		/// <summary>
		/// Saves step settings
		/// </summary>
		public override void SaveSettings()
		{
			FbDataAdapterWizardSettings.UpdateSelectStoredProcedure(this.cboSelectSP.Text);
			FbDataAdapterWizardSettings.UpdateInsertStoredProcedure(this.cboInsertSP.Text);
			FbDataAdapterWizardSettings.UpdateUpdateStoredProcedure(this.cboUpdateSP.Text);
			FbDataAdapterWizardSettings.UpdateDeleteStoredProcedure(this.cboDeleteSP.Text);
		}

		#endregion

		#region Private	Methods

		private void UpdateStoredProcs()
		{
			FbDataAdapter adapter = FbDataAdapterWizardSettings.GetDataAdapter();
			string selectSPName = FbDataAdapterWizardSettings.GetSelectStoredProcedure();
			string insertSPName = FbDataAdapterWizardSettings.GetInsertStoredProcedure();
			string updateSPName = FbDataAdapterWizardSettings.GetUpdateStoredProcedure();
			string deleteSPName = FbDataAdapterWizardSettings.GetDeleteStoredProcedure();

			// Select Stored Procedure
			if (selectSPName == null &&
				adapter.SelectCommand != null &&
				adapter.SelectCommand.CommandType == CommandType.StoredProcedure)
			{
				this.cboSelectSP.SelectedText = adapter.SelectCommand.CommandText;
			}

			// Insert Stored Procedure
			if (insertSPName == null &&
				adapter.InsertCommand != null &&
				adapter.InsertCommand.CommandType == CommandType.StoredProcedure)
			{
				this.cboInsertSP.SelectedText = adapter.InsertCommand.CommandText;
			}

			// Update Stored Procedure
			if (updateSPName == null &&
				adapter.UpdateCommand != null &&
				adapter.UpdateCommand.CommandType == CommandType.StoredProcedure)
			{
				this.cboUpdateSP.SelectedText = adapter.UpdateCommand.CommandText;
			}

			// Delete Stored Procedure
			if (deleteSPName == null &&
				adapter.DeleteCommand != null &&
				adapter.DeleteCommand.CommandType == CommandType.StoredProcedure)
			{
				this.cboDeleteSP.SelectedText = adapter.DeleteCommand.CommandText;
			}
		}

		private void FillTables()
		{
			FbConnection connection = (FbConnection)((ICloneable)FbDataAdapterWizardSettings.GetConnection()).Clone();

			FbConnectionStringBuilder cs = new FbConnectionStringBuilder(connection.ConnectionString);
			cs.Pooling = false;

			connection.ConnectionString = cs.ToString();

			try
			{
				connection.Open();

				DataTable sp = connection.GetSchema("Procedures");
				DataTable spParameters = connection.GetSchema("ProcedureParameters");

				connection.Close();

				// Add procedure tables
				sp.TableName = "SelectSP";
				this.procedures.Tables.Add(sp.Copy());

				sp.TableName = "InsertSP";
				this.procedures.Tables.Add(sp.Copy());

				sp.TableName = "UpdateSP";
				this.procedures.Tables.Add(sp.Copy());

				sp.TableName = "DeleteSP";
				this.procedures.Tables.Add(sp.Copy());

				// Add parameters tables
				spParameters.TableName = "SelectParams";
				this.parameters.Tables.Add(spParameters.Copy());

				spParameters.TableName = "InsertParams";
				this.parameters.Tables.Add(spParameters.Copy());

				spParameters.TableName = "UpdateParams";
				this.parameters.Tables.Add(spParameters.Copy());

				spParameters.TableName = "DeleteParams";
				this.parameters.Tables.Add(spParameters.Copy());

				// Configure DataBinding
				this.cboSelectSP.DisplayMember = "SelectSP.PROCEDURE_NAME";
				this.cboSelectSP.ValueMember = "SelectSP.PROCEDURE_NAME";

				this.cboInsertSP.DisplayMember = "InsertSP.PROCEDURE_NAME";
				this.cboInsertSP.ValueMember = "InsertSP.PROCEDURE_NAME";

				this.cboUpdateSP.DisplayMember = "UpdateSP.PROCEDURE_NAME";
				this.cboUpdateSP.ValueMember = "UpdateSP.PROCEDURE_NAME";

				this.cboDeleteSP.DisplayMember = "DeleteSP.PROCEDURE_NAME";
				this.cboDeleteSP.ValueMember = "DeleteSP.PROCEDURE_NAME";

				// DataGrid	condiguration

				foreach (DataTable table in this.parameters.Tables)
				{
					foreach (DataColumn column in table.Columns)
					{
						if (column.ColumnName != "PARAMETER_NAME")
						{
							column.ColumnMapping = MappingType.Hidden;
						}
						else
						{
							column.ColumnName = "Parameter name";
						}
					}
				}
			}
			catch (Exception)
			{
				throw;
			}
		}

		private void FilterParameters(string tableName, string spName)
		{
			string filter = String.Format("PROCEDURE_NAME = '{0}'", spName);

			this.parameters.Tables[tableName].DefaultView.RowStateFilter = DataViewRowState.CurrentRows;
			this.parameters.Tables[tableName].DefaultView.RowFilter = filter;
			this.grdSpParameters.SetDataBinding(this.parameters.Tables[tableName].DefaultView, null);
		}

		#endregion

		#region Event handlers

		private void FbDataAdapterConfigurationStep6_Load(object sender, EventArgs e)
		{
			this.FillTables();
			this.initialized = true;
		}

		private void cboSelectSP_Enter(object sender, EventArgs e)
		{
			if (this.initialized)
			{
				this.FilterParameters("SelectParams", this.cboSelectSP.Text);
			}
		}

		private void cboInsertSP_Enter(object sender, EventArgs e)
		{
			if (this.initialized)
			{
				this.FilterParameters("InsertParams", this.cboInsertSP.Text);
			}
		}

		private void cboUpdateSP_Enter(object sender, EventArgs e)
		{
			if (this.initialized)
			{
				this.FilterParameters("UpdateParams", this.cboUpdateSP.Text);
			}
		}

		private void cboDeleteSP_Enter(object sender, EventArgs e)
		{
			if (this.initialized)
			{
				this.FilterParameters("DeleteParams", this.cboDeleteSP.Text);
			}
		}

		private void cboSelectSP_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (this.initialized)
			{
				this.FilterParameters("SelectParams", this.cboSelectSP.Text);
			}
		}

		private void cboInsertSP_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (this.initialized)
			{
				this.FilterParameters("InsertParams", this.cboInsertSP.Text);
			}
		}

		private void cboUpdateSP_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (this.initialized)
			{
				this.FilterParameters("UpdateParams", this.cboUpdateSP.Text);
			}
		}

		private void cboDeleteSP_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (this.initialized)
			{
				this.FilterParameters("DeleteParams", this.cboDeleteSP.Text);
			}
		}

		#endregion
	}
}

#endif