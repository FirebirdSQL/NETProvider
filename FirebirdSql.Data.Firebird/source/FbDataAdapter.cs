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
using System.Data.Common;
using System.Drawing;
using System.ComponentModel;
using System.ComponentModel.Design;

using FirebirdSql.Data.Firebird.Design;

namespace FirebirdSql.Data.Firebird
{	
	/// <include file='Doc/en_EN/FbDataAdapter.xml' path='doc/class[@name="FbDataAdapter"]/overview/*'/>
	[ToolboxItem(true),
	ToolboxBitmap(typeof(FbDataAdapter), "Resources.ToolboxBitmaps.FbDataAdapter.bmp"),
	DefaultEvent("RowUpdated")]
	public sealed class FbDataAdapter : DbDataAdapter, IDbDataAdapter
	{
		#region EVENTS

		/// <include file='Doc/en_EN/FbDataAdapter.xml' path='doc/class[@name="FbDataAdapter"]/event[@name="RowUpdated"]/*'/>
		public event FbRowUpdatedEventHandler RowUpdated
		{
			add { Events.AddHandler(EventRowUpdated, value); }
			remove { Events.RemoveHandler(EventRowUpdated, value); }
		}

		/// <include file='Doc/en_EN/FbDataAdapter.xml' path='doc/class[@name="FbDataAdapter"]/event[@name="RowUpdating"]/*'/>
		public event FbRowUpdatingEventHandler RowUpdating
		{
			add { Events.AddHandler(EventRowUpdating, value); }
			remove { Events.RemoveHandler(EventRowUpdating, value); }
		}

		#endregion

		#region STATIC_FIELDS

		private static readonly object EventRowUpdated = new object(); 
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
		
		IDbCommand IDbDataAdapter.SelectCommand 
		{
			get { return selectCommand; }
			set { selectCommand = (FbCommand)value; }
		}

		/// <include file='Doc/en_EN/FbDataAdapter.xml' path='doc/class[@name="FbDataAdapter"]/property[@name="SelectCommand"]/*'/>
		[Category("Fill"), DefaultValue(null)]
		public FbCommand SelectCommand 
		{
			get { return selectCommand; }
			set { selectCommand = value; }
		}

		IDbCommand IDbDataAdapter.InsertCommand 
		{
			get { return insertCommand; }
			set { insertCommand = (FbCommand)value; }
		}

		/// <include file='Doc/en_EN/FbDataAdapter.xml' path='doc/class[@name="FbDataAdapter"]/property[@name="InsertCommand"]/*'/>
		[Category("Update"), DefaultValue(null)]
		public FbCommand InsertCommand 
		{
			get { return insertCommand; }
			set { insertCommand = value; }
		}

		IDbCommand IDbDataAdapter.UpdateCommand 
		{
			get { return updateCommand; }
			set { updateCommand = (FbCommand)value; }
		}

		/// <include file='Doc/en_EN/FbDataAdapter.xml' path='doc/class[@name="FbDataAdapter"]/property[@name="UpdateCommand"]/*'/>		
		[Category("Update"), DefaultValue(null)]
		public FbCommand UpdateCommand 
		{
			get { return updateCommand; }
			set { updateCommand = value; }
		}

		IDbCommand IDbDataAdapter.DeleteCommand 
		{
			get { return deleteCommand; }
			set { deleteCommand = (FbCommand)value; }
		}

		/// <include file='Doc/en_EN/FbDataAdapter.xml' path='doc/class[@name="FbDataAdapter"]/property[@name="DeleteCommand"]/*'/>
		[Category("Update"), DefaultValue(null)]
		public FbCommand DeleteCommand 
		{
			get { return deleteCommand; }
			set { deleteCommand = value; }
		}

		#endregion

		#region CONSTRUCTORS

		/// <include file='Doc/en_EN/FbDataAdapter.xml' path='doc/class[@name="FbDataAdapter"]/constructor[@name="ctor"]/*'/>
		public FbDataAdapter() : base()
		{
			GC.SuppressFinalize(this);
		}

		/// <include file='Doc/en_EN/FbDataAdapter.xml' path='doc/class[@name="FbDataAdapter"]/constructor[@name="ctor(FbCommand)"]/*'/>
		public FbDataAdapter(FbCommand selectCommand) : this()
		{
			this.SelectCommand	= selectCommand;
		}

		/// <include file='Doc/en_EN/FbDataAdapter.xml' path='doc/class[@name="FbDataAdapter"]/constructor[@name="ctor(System.String,FbConnection)"]/*'/>		
		public FbDataAdapter(string selectCommandText, FbConnection selectConnection) : this()
		{
			this.SelectCommand	= new FbCommand(selectCommandText, selectConnection);
		}

		/// <include file='Doc/en_EN/FbDataAdapter.xml' path='doc/class[@name="FbDataAdapter"]/constructor[@name="ctor(System.String,System.String)"]/*'/>
		public FbDataAdapter(string selectCommandText, string selectConnectionString) : this()
		{
			this.SelectCommand	= new FbCommand(selectCommandText, new FbConnection(selectConnectionString));
		}

		#endregion

		#region DISPOSE_METHODS

		/// <include file='Doc/en_EN/FbDataAdapter.xml' path='doc/class[@name="FbDataAdapter"]/method[@name="Dispose(System.Boolean)"]/*'/>
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

		/// <include file='Doc/en_EN/FbDataAdapter.xml' path='doc/class[@name="FbDataAdapter"]/method[@name="CreateRowUpdatedEvent(System.Data.DataRow,System.Data.IDbCommand,System.Data.StatementType,System.Data.Common.DataTableMapping)"]/*'/>
		protected override RowUpdatedEventArgs CreateRowUpdatedEvent(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
		{
			return new FbRowUpdatedEventArgs(dataRow, command, statementType, tableMapping);
		}

		/// <include file='Doc/en_EN/FbDataAdapter.xml' path='doc/class[@name="FbDataAdapter"]/method[@name="CreateRowUpdatingEvent(System.Data.DataRow,System.Data.IDbCommand,System.Data.StatementType,System.Data.Common.DataTableMapping)"]/*'/>
		protected override RowUpdatingEventArgs CreateRowUpdatingEvent(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
		{
			return new FbRowUpdatingEventArgs(dataRow, command, statementType, tableMapping);
		}

		/// <include file='Doc/en_EN/FbDataAdapter.xml' path='doc/class[@name="FbDataAdapter"]/method[@name="OnRowUpdated(System.Data.Common.RowUpdatedEventArgs)"]/*'/>
		protected override void OnRowUpdated(RowUpdatedEventArgs value)
		{
			FbRowUpdatedEventHandler handler = (FbRowUpdatedEventHandler) Events[EventRowUpdated];
			if ((null != handler) && (value is FbRowUpdatedEventArgs)) 
			{
				handler(this, (FbRowUpdatedEventArgs) value);
			}
		}

		/// <include file='Doc/en_EN/FbDataAdapter.xml' path='doc/class[@name="FbDataAdapter"]/method[@name="OnRowUpdating(System.Data.Common.RowUpdatingEventArgs)"]/*'/>
		protected override void OnRowUpdating(RowUpdatingEventArgs value)
		{
			FbRowUpdatingEventHandler handler = (FbRowUpdatingEventHandler) Events[EventRowUpdating];
			if ((null != handler) && (value is FbRowUpdatingEventArgs)) 
			{
				handler(this, (FbRowUpdatingEventArgs) value);
			}
		}

		#endregion
	}
}
