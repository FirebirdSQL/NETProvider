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
 *	Copyright (c) 2005 Carlos Guzman Alvarez
 *	All Rights Reserved.
 *
 *  Contributors:
 *      Jiri Cincura (jiri@cincura.net)
 */

using System;
using System.Collections;
using System.Data;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.ExternalEngine
{
	internal sealed class ExtDatabase : IDatabase
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
		private int[] _statusVector;
		private object _syncObject;

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

		public ExtDatabase()
		{
			_charset = Charset.DefaultCharset;
			_dialect = 3;
			_packetSize = 8192;
			_statusVector = new int[IscCodes.ISC_STATUS_LENGTH];
		}

		#endregion

		#region Finalizer

		~ExtDatabase()
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
		}

		public void DropDatabase()
		{
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
			int dbHandle = 0;

			// Clear status vector
			ClearStatusVector();

			lock (this)
			{
				SafeNativeMethods.isc_get_current_database(_statusVector, ref dbHandle);

				_handle = dbHandle;
			}
		}

		public void AttachWithTrustedAuth(DatabaseParameterBuffer dpb, string dataSource, int port, string database)
		{
			throw new NotSupportedException("Trusted Auth isn't supported on External Engine.");
		}

		public void Detach()
		{
		}

		#endregion

		#region Transaction Methods

		public ITransaction BeginTransaction(TransactionParameterBuffer tpb)
		{
			ExtTransaction transaction = new ExtTransaction(this);
			transaction.BeginTransaction(tpb);

			return transaction;
		}

		#endregion

		#region Cancel Methods

		public void CancelOperation(int kind)
		{
			throw new NotSupportedException("Cancel Operation isn't supported on External Engine.");
		}

		#endregion

		#region Statement Creation Methods

		public StatementBase CreateStatement()
		{
			return new ExtStatement(this);
		}

		public StatementBase CreateStatement(ITransaction transaction)
		{
			return new ExtStatement(this, transaction as ExtTransaction);
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

			return GetDatabaseInfo(items, 50)[0].ToString();
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
			return new ExtTriggerContext(this);
		}

		#endregion

		#region Internal Methods

		internal void ParseStatusVector(int[] statusVector)
		{
			IscException ex = ExtConnection.ParseStatusVector(statusVector);

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
				int[] statusVector = ExtConnection.GetNewStatusVector();
				int dbHandle = Handle;

				SafeNativeMethods.isc_database_info(
					statusVector,
					ref dbHandle,
					(short)items.Length,
					items,
					(short)bufferLength,
					buffer);

				ParseStatusVector(statusVector);
			}
		}

		#endregion
	}
}
