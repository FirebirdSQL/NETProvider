/*
 *  Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.ibphoenix.com/main.nfs?a=ibphoenix&l=;PAGES;NAME='ibp_idpl'
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2002, 2004 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using System;
using System.Data;
using System.Drawing;
using System.Data.Common;
using System.ComponentModel;
using System.ComponentModel.Design;


namespace FirebirdSql.Data.Firebird
{	
	/// <include file='xmldoc/fbdataadapter.xml' path='doc/member[@name="T:FbDataAdapter"]/*'/>
	#if (!_MONO)
	[ToolboxBitmap(typeof(FbDataAdapter), "Resources.ToolboxBitmaps.FbDataAdapter.bmp")]	
	#endif
	[DefaultEvent("RowUpdated")]
	public sealed class FbDataAdapter : DbDataAdapter, IDbDataAdapter
	{
		#region EVENTS

		/// <include file='xmldoc/fbdataadapter.xml' path='doc/member[@name="F:EventRowUpdated"]/*'/>
		private static readonly object EventRowUpdated = new object(); 
		/// <include file='xmldoc/fbdataadapter.xml' path='doc/member[@name="F:EventRowUpdating"]/*'/>
		private static readonly object EventRowUpdating = new object(); 

		#endregion

		#region FIELDS

		private FbCommand	selectCommand;
		private FbCommand	insertCommand;
		private FbCommand	updateCommand;
		private FbCommand	deleteCommand;

		private bool		disposed;

		#endregion

		#region PROPERTIES

		/// <include file='xmldoc/fbdataadapter.xml' path='doc/member[@name="P:System#Data#IDbDataAdapter#SelectCommand"]/*'/>
		IDbCommand IDbDataAdapter.SelectCommand 
		{
			get { return selectCommand; }
			set { selectCommand = (FbCommand)value; }
		}

		/// <include file='xmldoc/fbdataadapter.xml' path='doc/member[@name="P:SelectCommand"]/*'/>
		public FbCommand SelectCommand 
		{
			get { return selectCommand; }
			set { selectCommand = value; }
		}

		/// <include file='xmldoc/fbdataadapter.xml' path='doc/member[@name="P:System#Data#IDbDataAdapter#InsertCommand"]/*'/>
		IDbCommand IDbDataAdapter.InsertCommand 
		{
			get { return insertCommand; }
			set { insertCommand = (FbCommand)value; }
		}

		/// <include file='xmldoc/fbdataadapter.xml' path='doc/member[@name="P:InsertCommand"]/*'/>
		public FbCommand InsertCommand 
		{
			get { return insertCommand; }
			set { insertCommand = value; }
		}

		/// <include file='xmldoc/fbdataadapter.xml' path='doc/member[@name="P:System#Data#IDbDataAdapter#UpdateCommand"]/*'/>
		IDbCommand IDbDataAdapter.UpdateCommand 
		{
			get { return updateCommand; }
			set { updateCommand = (FbCommand)value; }
		}
		
		/// <include file='xmldoc/fbdataadapter.xml' path='doc/member[@name="P:UpdateCommand"]/*'/>
		public FbCommand UpdateCommand 
		{
			get { return updateCommand; }
			set { updateCommand = value; }
		}

		/// <include file='xmldoc/fbdataadapter.xml' path='doc/member[@name="P:System#Data#IDbDataAdapter#DeleteCommand"]/*'/>
		IDbCommand IDbDataAdapter.DeleteCommand 
		{
			get { return deleteCommand; }
			set { deleteCommand = (FbCommand)value; }
		}

		/// <include file='xmldoc/fbdataadapter.xml' path='doc/member[@name="P:DeleteCommand"]/*'/>
		public FbCommand DeleteCommand 
		{
			get { return deleteCommand; }
			set { deleteCommand = value; }
		}

		ITableMappingCollection IDataAdapter.TableMappings 
		{
			get { return TableMappings; }
		}

		#endregion

		#region CONSTRUCTORS

		/// <include file='xmldoc/fbdataadapter.xml' path='doc/member[@name="M:#ctor"]/*'/>
		public FbDataAdapter()
		{
			disposed = false;
		}

		/// <include file='xmldoc/fbdataadapter.xml' path='doc/member[@name="M:#ctor(FirebirdSql.Data.Firebird.FbCommand)"]/*'/>
		public FbDataAdapter(FbCommand selectCommand) : this()
		{
			this.SelectCommand	= selectCommand;
		}
		
		/// <include file='xmldoc/fbdataadapter.xml' path='doc/member[@name="M:#ctor(System.String,FirebirdSql.Data.Firebird.FbConnection)"]/*'/>
		public FbDataAdapter(string selectCommandText, FbConnection selectConnection) : this()
		{
			this.SelectCommand	= new FbCommand(selectCommandText, selectConnection);
		}

		/// <include file='xmldoc/fbdataadapter.xml' path='doc/member[@name="M:#ctor(System.String,System.String)"]/*'/>
		public FbDataAdapter(string selectCommandText, string selectConnectionString) : this()
		{
			this.SelectCommand	= new FbCommand(selectCommandText, new FbConnection(selectConnectionString));
		}

		#endregion

		#region DESTRUCTORS 

		/// <include file='xmldoc/fbcommand.xml' path='doc/member[@name="M:Dispose(System.Boolean)"]/*'/>
		protected override void Dispose(bool disposing)
		{
			if (!disposed)
			{
				try
				{
					if (disposing)
					{
						if (SelectCommand != null)
						{
							this.SelectCommand.Dispose();
						}
						if (InsertCommand != null)
						{
							this.InsertCommand.Dispose();
						}
						if (UpdateCommand != null)
						{
							this.UpdateCommand.Dispose();
						}
					}
					
					// release any unmanaged resources
					
					disposed = true;
				}
				finally 
				{
					base.Dispose(disposing);
				}
			}
		}

		#endregion

		#region ABSTRACT_METHODS
		
		/// <include file='xmldoc/fbdataadapter.xml' path='doc/member[@name="M:CreateRowUpdatedEvent(System.Data.DataRow,System.Data.IDbCommand,System.Data.StatementType,System.Data.Common.DataTableMapping)"]/*'/>		
		protected override RowUpdatedEventArgs CreateRowUpdatedEvent(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
		{
			return new FbRowUpdatedEventArgs(dataRow, command, statementType, tableMapping);
		}

		/// <include file='xmldoc/fbdataadapter.xml' path='doc/member[@name="M:CreateRowUpdatingEvent(System.Data.DataRow,System.Data.IDbCommand,System.Data.StatementType,System.Data.Common.DataTableMapping)"]/*'/>
		protected override RowUpdatingEventArgs CreateRowUpdatingEvent(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
		{
			return new FbRowUpdatingEventArgs(dataRow, command, statementType, tableMapping);
		}

		/// <include file='xmldoc/fbdataadapter.xml' path='doc/member[@name="M:OnRowUpdated(System.Data.Common.RowUpdatedEventArgs)"]/*'/>	
		protected override void OnRowUpdated(RowUpdatedEventArgs value)
		{
			FbRowUpdatedEventHandler handler = (FbRowUpdatedEventHandler) Events[EventRowUpdated];
			if ((null != handler) && (value is FbRowUpdatedEventArgs)) 
			{
				handler(this, (FbRowUpdatedEventArgs) value);
			}
		}

		/// <include file='xmldoc/fbdataadapter.xml' path='doc/member[@name="M:OnRowUpdating(System.Data.Common.RowUpdatingEventArgs)"]/*'/>
		override protected void OnRowUpdating(RowUpdatingEventArgs value)
		{
			FbRowUpdatingEventHandler handler = (FbRowUpdatingEventHandler) Events[EventRowUpdating];
			if ((null != handler) && (value is FbRowUpdatingEventArgs)) 
			{
				handler(this, (FbRowUpdatingEventArgs) value);
			}
		}

		/// <include file='xmldoc/fbdataadapter.xml' path='doc/member[@name="E:FirebirdSql.Data.Firebird.FbDataAdapter.RowUpdated"]/*'/>
		public event FbRowUpdatedEventHandler RowUpdated
		{
			add { Events.AddHandler(EventRowUpdated, value); }
			remove { Events.RemoveHandler(EventRowUpdated, value); }
		}

		/// <include file='xmldoc/fbdataadapter.xml' path='doc/member[@name="E:FirebirdSql.Data.Firebird.FbDataAdapter.RowUpdating"]/*'/>
		public event FbRowUpdatingEventHandler RowUpdating
		{
			add { Events.AddHandler(EventRowUpdating, value); }
			remove { Events.RemoveHandler(EventRowUpdating, value); }
		}

		#endregion
	}

	/// <include file='xmldoc/fbdataadapter.xml' path='doc/member[@name="T:FbRowUpdatedEventHandler"]/*'/>
	public delegate void FbRowUpdatedEventHandler(object sender, FbRowUpdatedEventArgs e);
	/// <include file='xmldoc/fbdataadapter.xml' path='doc/member[@name="T:FbRowUpdatingEventHandler"]/*'/>
	public delegate void FbRowUpdatingEventHandler(object sender, FbRowUpdatingEventArgs e);

	/// <summary>
	/// Provides data for the RowUpdating event. 
	/// This class cannot be inherited.
	/// </summary>	
	public class FbRowUpdatingEventArgs : RowUpdatingEventArgs
	{		
		/// <summary>
		/// Initializes a new instance of the FbRowUpdatingEventArgs class.
		/// </summary>
		/// <param name="row">The DataRow to update.</param>
		/// <param name="command">The FbCommand to execute during the update operation. </param>
		/// <param name="statementType">One of the System.Data.StatementType values that specifies the type of query executed. </param>
		/// <param name="tableMapping">The DataTableMapping sent through Update. </param>
		public FbRowUpdatingEventArgs(DataRow row, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
			: base(row, command, statementType, tableMapping) 
		{
		}

		/// <summary>
		/// Gets or sets the FbCommand to execute when Update is called.
		/// </summary>		
		new public FbCommand Command
		{
			get  { return (FbCommand)base.Command; }
			set  { base.Command = value; }
		}
	}

	/// <summary>
	/// Provides data for the RowUpdated event. 
	/// This class cannot be inherited.
	/// </summary>	
	public sealed class FbRowUpdatedEventArgs : RowUpdatedEventArgs
	{
		/// <summary>
		/// Initializes a new instance of the FbRowUpdatedEventArgs class.
		/// </summary>
		/// <param name="row">The DataRow sent through an update operation. </param>
		/// <param name="command">The FbCommand executed when Update is called. </param>
		/// <param name="statementType">One of the System.Data.StatementType values that specifies the type of query executed. </param>
		/// <param name="tableMapping">The System.Data.Common.DataTableMapping sent through Update.</param>
		public FbRowUpdatedEventArgs(DataRow row, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
			: base(row, command, statementType, tableMapping) 
		{
		}
		
		/// <summary>
		/// Gets the FbCommand executed when Update is called.
		/// </summary>
		/// <value>
		/// The FbCommand executed when Update is called.
		/// </value>
		new public FbCommand Command
		{
			get  { return (FbCommand)base.Command; }
		}
	}
}
