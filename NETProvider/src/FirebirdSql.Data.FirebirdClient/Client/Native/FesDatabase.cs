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
 *	Copyright (c) 2002, 2007 Carlos Guzman Alvarez
 *	All Rights Reserved.
 *
 *  Contributors:
 *      Jiri Cincura (jiri@cincura.net)
 */

using System;
using System.Collections;
using System.Threading;
using System.Text;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Native
{
	internal sealed class FesDatabase : IDatabase
	{
		#region Callbacks

		public WarningMessageCallback WarningMessage
		{
			get { return _warningMessage; }
			set { _warningMessage = value; }
		}

		#endregion

		#region Fields

		private WarningMessageCallback _warningMessage;

		private int _handle;
		private int _transactionCount;
		private string _serverVersion;
		private Charset _charset;
		private short _packetSize;
		private short _dialect;
		private bool _disposed;
		private IntPtr[] _statusVector;
		private object _syncObject;

		private IFbClient _fbClient;

		#endregion

		#region Properties

		public int Handle
		{
			get { return _handle; }
		}

		public int TransactionCount
		{
			get { return _transactionCount; }
			set { _transactionCount = value; }
		}

		public string ServerVersion
		{
			get { return _serverVersion; }
		}

		public Charset Charset
		{
			get { return _charset; }
			set { _charset = value; }
		}

		public short PacketSize
		{
			get { return _packetSize; }
			set { _packetSize = value; }
		}

		public short Dialect
		{
			get { return _dialect; }
			set { _dialect = value; }
		}

		public bool HasRemoteEventSupport
		{
			get { return false; }
		}

		public IFbClient FbClient
		{
			get { return _fbClient; }
		}

		public object SyncObject
		{
			get
			{
				if (_syncObject == null)
				{
					Interlocked.CompareExchange(ref _syncObject, new object(), null);
				}

				return _syncObject;
			}
		}

		#endregion

		#region Constructors

		public FesDatabase()
			: this(null, null)
		{
		}

		public FesDatabase(string dllName, Charset charset)
		{
			_fbClient = FbClientFactory.GetFbClient(dllName);
			_charset = (charset != null ? charset : Charset.DefaultCharset);
			_dialect = 3;
			_packetSize = 8192;
			_statusVector = new IntPtr[IscCodes.ISC_STATUS_LENGTH];
		}

		#endregion

		#region Finalizer

		~FesDatabase()
		{
			Dispose(false);
		}

		#endregion

		#region IDisposable methods

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			lock (this)
			{
				if (!_disposed)
				{
					try
					{
						Detach();
					}
					catch
					{ }

					if (disposing)
					{
						_warningMessage = null;
						_charset = null;
						_serverVersion = null;
						_statusVector = null;
						_transactionCount = 0;
						_dialect = 0;
						_handle = 0;
						_packetSize = 0;
					}

					_disposed = true;
				}
			}
		}

		#endregion

		#region Database Methods

		public void CreateDatabase(DatabaseParameterBuffer dpb, string dataSource, int port, string database)
		{
			lock (this)
			{
				byte[] databaseBuffer = Encoding.Default.GetBytes(database);
				int dbHandle = Handle;

				// Clear status vector
				ClearStatusVector();

				_fbClient.isc_create_database(
					_statusVector,
					(short)databaseBuffer.Length,
					databaseBuffer,
					ref	dbHandle,
					(short)dpb.Length,
					dpb.ToArray(),
					0);

				ParseStatusVector(_statusVector);

				_handle = dbHandle;

				Detach();
			}
		}

		public void DropDatabase()
		{
			lock (this)
			{
				int dbHandle = Handle;

				// Clear status vector
				ClearStatusVector();

				_fbClient.isc_drop_database(_statusVector, ref dbHandle);

				ParseStatusVector(_statusVector);

				_handle = 0;
			}
		}

		#endregion

		#region Remote Events Methods

		public void CloseEventManager()
		{
			throw new NotSupportedException();
		}

		public RemoteEvent CreateEvent()
		{
			throw new NotSupportedException();
		}

		public void QueueEvents(RemoteEvent events)
		{
			throw new NotSupportedException();
		}

		public void CancelEvents(RemoteEvent events)
		{
			throw new NotSupportedException();
		}

		#endregion

		#region Methods

		public void Attach(DatabaseParameterBuffer dpb, string dataSource, int port, string database)
		{
			lock (this)
			{
				byte[] databaseBuffer = Encoding.Default.GetBytes(database);
				int dbHandle = 0;

				// Clear status vector
				ClearStatusVector();

				_fbClient.isc_attach_database(
					_statusVector,
					(short)databaseBuffer.Length,
					databaseBuffer,
					ref dbHandle,
					(short)dpb.Length,
					dpb.ToArray());

				ParseStatusVector(_statusVector);

				// Update the database handle
				_handle = dbHandle;

				// Get server version
				_serverVersion = GetServerVersion();
			}
		}

		public void AttachWithTrustedAuth(DatabaseParameterBuffer dpb, string dataSource, int port, string database)
		{
			throw new NotImplementedException("Trusted Auth isn't supported on Embedded Firebird.");
		}

		public void Detach()
		{
			lock (this)
			{
				if (TransactionCount > 0)
				{
					throw new IscException(IscCodes.isc_open_trans, TransactionCount);
				}

				int dbHandle = Handle;

				// Clear status vector
				ClearStatusVector();

				_fbClient.isc_detach_database(_statusVector, ref dbHandle);

				_handle = dbHandle;

				FesConnection.ParseStatusVector(_statusVector, _charset);
			}
		}

		#endregion

		#region Transaction Methods

		public ITransaction BeginTransaction(TransactionParameterBuffer tpb)
		{
			FesTransaction transaction = new FesTransaction(this);
			transaction.BeginTransaction(tpb);

			return transaction;
		}

		#endregion

		#region Cancel Methods

		public void CancelOperation(int kind)
		{
			int dbHandle = Handle;

			IntPtr[] localStatusVector = new IntPtr[IscCodes.ISC_STATUS_LENGTH];

			_fbClient.fb_cancel_operation(localStatusVector, ref dbHandle, kind);

			FesConnection.ParseStatusVector(localStatusVector, _charset);
		}

		#endregion

		#region Statement Creation Methods

		public StatementBase CreateStatement()
		{
			return new FesStatement(this);
		}

		public StatementBase CreateStatement(ITransaction transaction)
		{
			return new FesStatement(this, transaction as FesTransaction);
		}

		#endregion

		#region Database Information Methods

		public string GetServerVersion()
		{
			byte[] items = new byte[]
			{
				IscCodes.isc_info_firebird_version,
				IscCodes.isc_info_end
			};

			return GetDatabaseInfo(items, IscCodes.BUFFER_SIZE_128)[0].ToString();
		}

		public ArrayList GetDatabaseInfo(byte[] items)
		{
			return GetDatabaseInfo(items, IscCodes.DEFAULT_MAX_BUFFER_SIZE);
		}

		public ArrayList GetDatabaseInfo(byte[] items, int bufferLength)
		{
			byte[] buffer = new byte[bufferLength];

			DatabaseInfo(items, buffer, buffer.Length);

			return IscHelper.ParseDatabaseInfo(buffer);
		}

		#endregion

		#region Trigger Context Methods

		public ITriggerContext GetTriggerContext()
		{
			throw new NotSupportedException();
		}

		#endregion

		#region Internal Methods

		internal void ParseStatusVector(IntPtr[] statusVector)
		{
			IscException ex = FesConnection.ParseStatusVector(statusVector, _charset);

			if (ex != null)
			{
				if (ex.IsWarning)
				{
					_warningMessage(ex);
				}
				else
				{
					throw ex;
				}
			}
		}

		#endregion

		#region Private Methods

		private void ClearStatusVector()
		{
			Array.Clear(_statusVector, 0, _statusVector.Length);
		}

		private void DatabaseInfo(byte[] items, byte[] buffer, int bufferLength)
		{
			lock (this)
			{
				int dbHandle = Handle;

				// Clear status vector
				ClearStatusVector();

				_fbClient.isc_database_info(
					_statusVector,
					ref	dbHandle,
					(short)items.Length,
					items,
					(short)bufferLength,
					buffer);

				ParseStatusVector(_statusVector);
			}
		}

		#endregion
	}
}
