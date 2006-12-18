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
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

using FirebirdSql.Data.FirebirdClient;

namespace FirebirdSql.Data.Design.DataAdapter
{
	internal class FbDataAdapterDataSetGenerator : System.Windows.Forms.Form
	{
		#region · Designer Fields ·

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.RadioButton rbExisting;
		private System.Windows.Forms.ComboBox comboBox1;
		private System.Windows.Forms.RadioButton rbNew;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button cmdOk;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.CheckBox chkDesignerAdd;
		private System.Windows.Forms.TextBox txtDataSetName;

		#endregion

		#region · Fields ·

		private FbDataAdapter adapter;
		private ErrorProvider ErrorProvider;
		private CheckBox chkProperCase;
		private CheckBox chkTypedDataSet;
		private ComboBox cboVSVersion;
		private CheckBox chkUseAdapter;
		private ListBox lstTables;
		private Label label2;
		private IDesignerHost designerHost;

		#endregion

		#region · Constructors ·

		public FbDataAdapterDataSetGenerator(IDesignerHost designerHost, FbDataAdapter dataAdapter)
		{
			if (designerHost == null)
			{
				throw new ArgumentNullException("designerHost cannot be null");
			}
			if (dataAdapter == null)
			{
				throw new ArgumentNullException("dataAdapter cannot be null");
			}

			this.designerHost = designerHost;
			this.adapter = dataAdapter;

			InitializeComponent();

#if	(!VISUAL_STUDIO)
			this.chkTypedDataSet.Enabled = false;
#endif
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

		#endregion

		#region Windows	Form Designer generated	code

		/// <summary>
		/// Required method	for	Designer support - do not modify
		/// the	contents of	this method	with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.rbExisting = new System.Windows.Forms.RadioButton();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.rbNew = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtDataSetName = new System.Windows.Forms.TextBox();
            this.cmdOk = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.chkDesignerAdd = new System.Windows.Forms.CheckBox();
            this.ErrorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.chkProperCase = new System.Windows.Forms.CheckBox();
            this.chkTypedDataSet = new System.Windows.Forms.CheckBox();
            this.cboVSVersion = new System.Windows.Forms.ComboBox();
            this.chkUseAdapter = new System.Windows.Forms.CheckBox();
            this.lstTables = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(271, 14);
            this.label1.TabIndex = 0;
            this.label1.Text = "Generate a DataSet that includes the specified tables";
            // 
            // rbExisting
            // 
            this.rbExisting.AutoSize = true;
            this.rbExisting.Enabled = false;
            this.rbExisting.Location = new System.Drawing.Point(24, 30);
            this.rbExisting.Name = "rbExisting";
            this.rbExisting.Size = new System.Drawing.Size(64, 17);
            this.rbExisting.TabIndex = 0;
            this.rbExisting.Text = "Existing:";
            // 
            // comboBox1
            // 
            this.comboBox1.Enabled = false;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(91, 28);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(315, 21);
            this.comboBox1.TabIndex = 1;
            // 
            // rbNew
            // 
            this.rbNew.AutoSize = true;
            this.rbNew.Checked = true;
            this.rbNew.Location = new System.Drawing.Point(24, 77);
            this.rbNew.Name = "rbNew";
            this.rbNew.Size = new System.Drawing.Size(50, 17);
            this.rbNew.TabIndex = 2;
            this.rbNew.TabStop = true;
            this.rbNew.Text = "New:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtDataSetName);
            this.groupBox1.Controls.Add(this.rbExisting);
            this.groupBox1.Controls.Add(this.rbNew);
            this.groupBox1.Controls.Add(this.comboBox1);
            this.groupBox1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.groupBox1.Location = new System.Drawing.Point(13, 44);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(444, 110);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Choose a DataSet";
            // 
            // txtDataSetName
            // 
            this.txtDataSetName.Location = new System.Drawing.Point(91, 75);
            this.txtDataSetName.Name = "txtDataSetName";
            this.txtDataSetName.Size = new System.Drawing.Size(315, 20);
            this.txtDataSetName.TabIndex = 3;
            this.txtDataSetName.TextChanged += new System.EventHandler(this.txtDataSetName_TextChanged);
            // 
            // cmdOk
            // 
            this.cmdOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.cmdOk.Enabled = false;
            this.cmdOk.Location = new System.Drawing.Point(308, 368);
            this.cmdOk.Name = "cmdOk";
            this.cmdOk.Size = new System.Drawing.Size(75, 23);
            this.cmdOk.TabIndex = 7;
            this.cmdOk.Text = "OK";
            this.cmdOk.Click += new System.EventHandler(this.cmdOk_Click);
            // 
            // button2
            // 
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Location = new System.Drawing.Point(385, 368);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 8;
            this.button2.Text = "Cancel";
            // 
            // chkDesignerAdd
            // 
            this.chkDesignerAdd.Checked = true;
            this.chkDesignerAdd.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDesignerAdd.Location = new System.Drawing.Point(27, 324);
            this.chkDesignerAdd.Name = "chkDesignerAdd";
            this.chkDesignerAdd.Size = new System.Drawing.Size(257, 17);
            this.chkDesignerAdd.TabIndex = 5;
            this.chkDesignerAdd.Text = "Add this DataSet to the Designer";
            // 
            // ErrorProvider
            // 
            this.ErrorProvider.ContainerControl = this;
            // 
            // chkProperCase
            // 
            this.chkProperCase.Location = new System.Drawing.Point(27, 374);
            this.chkProperCase.Name = "chkProperCase";
            this.chkProperCase.Size = new System.Drawing.Size(257, 17);
            this.chkProperCase.TabIndex = 10;
            this.chkProperCase.Text = "Use proper casing for tables and column names";
            // 
            // chkTypedDataSet
            // 
            this.chkTypedDataSet.Location = new System.Drawing.Point(27, 300);
            this.chkTypedDataSet.Name = "chkTypedDataSet";
            this.chkTypedDataSet.Size = new System.Drawing.Size(170, 17);
            this.chkTypedDataSet.TabIndex = 3;
            this.chkTypedDataSet.Text = "Generate as Typed DataSet for";
            this.chkTypedDataSet.CheckedChanged += new System.EventHandler(this.chkTypedDataSet_CheckedChanged);
            // 
            // cboVSVersion
            // 
            this.cboVSVersion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboVSVersion.Enabled = false;
            this.cboVSVersion.FormattingEnabled = true;
            this.cboVSVersion.Location = new System.Drawing.Point(205, 299);
            this.cboVSVersion.Name = "cboVSVersion";
            this.cboVSVersion.Size = new System.Drawing.Size(130, 21);
            this.cboVSVersion.TabIndex = 4;
            // 
            // chkUseAdapter
            // 
            this.chkUseAdapter.Location = new System.Drawing.Point(27, 350);
            this.chkUseAdapter.Name = "chkUseAdapter";
            this.chkUseAdapter.Size = new System.Drawing.Size(216, 17);
            this.chkUseAdapter.TabIndex = 11;
            this.chkUseAdapter.Text = "Use DataAdapter Select command";
            this.chkUseAdapter.CheckedChanged += new System.EventHandler(this.chkUseAdapter_CheckedChanged);
            // 
            // lstTables
            // 
            this.lstTables.FormattingEnabled = true;
            this.lstTables.Location = new System.Drawing.Point(27, 191);
            this.lstTables.Name = "lstTables";
            this.lstTables.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lstTables.Size = new System.Drawing.Size(410, 95);
            this.lstTables.TabIndex = 12;
            this.lstTables.SelectedValueChanged += new System.EventHandler(this.lstTables_SelectedValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 170);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(160, 13);
            this.label2.TabIndex = 13;
            this.label2.Text = "Choose which table(s) to include";
            // 
            // FbDataAdapterDataSetGenerator
            // 
            this.ClientSize = new System.Drawing.Size(469, 404);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lstTables);
            this.Controls.Add(this.chkUseAdapter);
            this.Controls.Add(this.chkTypedDataSet);
            this.Controls.Add(this.cboVSVersion);
            this.Controls.Add(this.chkProperCase);
            this.Controls.Add(this.chkDesignerAdd);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.cmdOk);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FbDataAdapterDataSetGenerator";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Generate DataSet";
            this.Load += new System.EventHandler(this.FbDataAdapterDataSetGenerator_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorProvider)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		#region · Private Methods ·

		private void LoadTables()
		{
			if (this.adapter != null && this.adapter.SelectCommand != null)
			{
				if (this.adapter.SelectCommand.Connection != null)
				{
					FbConnection c = new FbConnection();
					try
					{
						c.ConnectionString = this.adapter.SelectCommand.Connection.ConnectionString;
						c.Open();

						DataTable tables = c.GetSchema("Tables", new string[] { null, null, null, "TABLE" });

						foreach (DataRow table in tables.Rows)
						{
							this.lstTables.Items.Add(table["TABLE_NAME"]);
						}
					}
					catch
					{
						this.lstTables.Items.Clear();
					}
					finally
					{
						if (c != null)
						{
							c.Close();
						}
					}
				}
			}
		}

		private void DoValidation()
		{
			if ((this.rbNew.Checked && this.txtDataSetName.Text.Length > 0))
			{
				if (this.chkUseAdapter.Checked)
				{
					this.cmdOk.Enabled = true;
				}
				else
				{
					if (this.lstTables.SelectedItems.Count > 0)
					{
						this.cmdOk.Enabled = true;
					}
					else
					{
						this.cmdOk.Enabled = false;
					}
				}
			}
			else
			{
				this.cmdOk.Enabled = false;
			}
		}

		private void LoadProductTypes()
		{
#if	(VISUAL_STUDIO)

			string[] productNames = VSExtensibility.GetAvailableProductNames();

			if (productNames.Length == 0)
			{
				this.chkTypedDataSet.Enabled = false;
				this.cboVSVersion.Enabled = false;
			}
			else
			{
				this.cboVSVersion.Items.AddRange(productNames);
			}
#endif
		}

		private void GenerateDataSet()
		{
			DataSet		ds		= null;
			string		cs		= this.adapter.SelectCommand.Connection.ConnectionString;
			string		name	= this.txtDataSetName.Text;
			bool		pcase	= this.chkProperCase.Checked;
			ArrayList	tables	= new ArrayList();

			try
			{
				// Generate	the	DataSet
				if (this.chkUseAdapter.Checked)
				{
					ds = DataSetGenerator.GenerateDataset(
						this.adapter, name, pcase);
				}
				else
				{
					foreach (string tableName in this.lstTables.SelectedItems)
					{
						tables.Add(tableName);
					}
					ds = DataSetGenerator.GenerateDataset(cs, tables, name, pcase);
				}

				if (this.chkTypedDataSet.Checked)
				{
#if	(VISUAL_STUDIO)
					// Generate	the	C# Code	and	DataSet	Schema
					string typeName = DataSetGenerator.SerializeTypedDataSet(
						ds,
						this.chkTypedDataSet.Checked,
						this.cboVSVersion.Text);

					if (this.chkDesignerAdd.Checked)
					{
						try
						{
							this.designerHost.CreateComponent(Type.GetType(typeName), this.txtDataSetName.Text);
						}
						catch
						{
						}
					}
#endif
				}
				else
				{
					if (this.chkDesignerAdd.Checked)
					{
						this.designerHost.Container.Add(ds, this.txtDataSetName.Text);

						foreach (DataTable table in ds.Tables)
						{
							this.designerHost.Container.Add(table, table.TableName);

                            /*
							foreach (DataColumn column in table.Columns)
							{
								this.designerHost.Container.Add(column, column.ColumnName);
							}
                            */
						}
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
				this.DialogResult = DialogResult.Cancel;
			}
		}

		#endregion

		#region · Event Handlers ·

		private void FbDataAdapterDataSetGenerator_Load(object sender, EventArgs e)
		{
			if (this.adapter == null)
			{
				this.chkUseAdapter.Enabled = false;
			}

			this.LoadTables();
			this.LoadProductTypes();
			this.DoValidation();
		}

		private void txtDataSetName_TextChanged(object sender, EventArgs e)
		{
			if (this.rbNew.Checked && this.txtDataSetName.Text.Length == 0)
			{
				this.ErrorProvider.SetError(this.txtDataSetName, "The DataSet must have a name.");
			}
			else
			{
				this.ErrorProvider.SetError(this.txtDataSetName, "");
			}

			this.DoValidation();
		}

		private void lstTables_SelectedValueChanged(object sender, EventArgs e)
		{
			if (this.lstTables.SelectedIndices.Count == 0)
			{
				this.ErrorProvider.SetError(this.lstTables, "You should select at least one Table.");
			}
			else
			{
				this.ErrorProvider.SetError(this.lstTables, "");
			}

			this.DoValidation();
		}

		private void chkUseAdapter_CheckedChanged(object sender, EventArgs e)
		{
			if (this.chkUseAdapter.Checked)
			{
				this.lstTables.Enabled = false;
				this.ErrorProvider.SetError(this.lstTables, "");
			}
			else
			{
				this.lstTables.Enabled = true;
				if (this.lstTables.SelectedItems.Count == 0)
				{
					this.ErrorProvider.SetError(this.lstTables, "You should select at least one Table.");
				}
			}

			this.DoValidation();
		}

		private void cmdOk_Click(object sender, EventArgs e)
		{
			this.GenerateDataSet();
		}

		private void chkTypedDataSet_CheckedChanged(object sender, EventArgs e)
		{
			if (this.chkTypedDataSet.Checked)
			{
				this.cboVSVersion.Enabled = true;
				if (this.cboVSVersion.SelectedItem == null)
				{
					this.cboVSVersion.SelectedIndex = 0;
				}
			}
			else
			{
				this.cboVSVersion.Enabled = false;
			}
		}

		#endregion
	}
}

#endif