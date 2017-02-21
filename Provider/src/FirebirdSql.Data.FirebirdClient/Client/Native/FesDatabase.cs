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
using FirebirdSql.Data.Client.Native.Handle;

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

		private DatabaseHandle _handle;
		private int _transactionCount;
		private string _serverVersion;
		private Charset _charset;
		private short _packetSize;
		private short _dialect;
		private bool _disposed;
		private IntPtr[] _statusVector;

		private IFbClient _fbClient;

		#endregion

		#region Properties

		public int Handle
		{
			get { return _handle.DangerousGetHandle().AsInt(); }
		}

		public DatabaseHandle HandlePtr
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

		#endregion

		#region Constructors

		public FesDatabase(string dllName, Charset charset)
		{
			_fbClient = FbClientFactory.GetFbClient(dllName);
			_handle = new DatabaseHandle();
			_charset = (charset != null ? charset : Charset.DefaultCharset);
			_dialect = 3;
			_packetSize = 8192;
			_statusVector = new IntPtr[IscCodes.ISC_STATUS_LENGTH];
		}

		#endregion

		#region IDisposable methods

		public void Dispose()
		{
			if (!_disposed)
			{
				_disposed = true;
				Detach();
				_warningMessage = null;
				_charset = null;
				_serverVersion = null;
				_statusVector = null;
				_transactionCount = 0;
				_dialect = 0;
				_handle.Dispose();
				_packetSize = 0;
			}
		}

		#endregion

		#region Database Methods

		public void CreateDatabase(DatabaseParameterBuffer dpb, string dataSource, int port, string database)
		{
			byte[] databaseBuffer = Encoding.UTF8.GetBytes(database);

			ClearStatusVector();

			_fbClient.isc_create_database(
				_statusVector,
				(short)databaseBuffer.Length,
				databaseBuffer,
				ref _handle,
				dpb.Length,
				dpb.ToArray(),
				0);

			ProcessStatusVector(_statusVector);

			Detach();
		}

		public void DropDatabase()
		{
			ClearStatusVector();

			_fbClient.isc_drop_database(_statusVector, ref _handle);

			_handle.Dispose();

			ProcessStatusVector(_statusVector);
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
			byte[] databaseBuffer = Encoding.UTF8.GetBytes(database);

			ClearStatusVector();

			_fbClient.isc_attach_database(
				_statusVector,
				(short)databaseBuffer.Length,
				databaseBuffer,
				ref _handle,
				dpb.Length,
				dpb.ToArray());

			ProcessStatusVector(_statusVector);

			_serverVersion = GetServerVersion();
		}

		public void AttachWithTrustedAuth(DatabaseParameterBuffer dpb, string dataSource, int port, string database)
		{
			throw new NotSupportedException("Trusted Auth isn't supported on Embedded Firebird.");
		}

		public void Detach()
		{
			if (TransactionCount > 0)
			{
				throw IscException.ForErrorCodeIntParam(IscCodes.isc_open_trans, TransactionCount);
			}

			ClearStatusVector();

			_fbClient.isc_detach_database(_statusVector, ref _handle);

			ProcessStatusVector(_statusVector);
		}

		#endregion

		#region Transaction Methods

		public TransactionBase BeginTransaction(TransactionParameterBuffer tpb)
		{
			FesTransaction transaction = new FesTransaction(this);
			transaction.BeginTransaction(tpb);

			return transaction;
		}

		#endregion

		#region Cancel Methods

		public void CancelOperation(int kind)
		{
			IntPtr[] localStatusVector = new IntPtr[IscCodes.ISC_STATUS_LENGTH];

			_fbClient.fb_cancel_operation(localStatusVector, ref _handle, kind);

			ProcessStatusVector(localStatusVector);
		}

		#endregion

		#region Statement Creation Methods

		public StatementBase CreateStatement()
		{
			return new FesStatement(this);
		}

		public StatementBase CreateStatement(TransactionBase transaction)
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

		#region Internal Methods

		internal void ProcessStatusVector(IntPtr[] statusVector)
		{
			IscException ex = FesConnection.ParseStatusVector(statusVector, _charset);

			if (ex != null)
			{
				if (ex.IsWarning)
				{
					_warningMessage?.Invoke(ex);
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
			ClearStatusVector();

			_fbClient.isc_database_info(
				_statusVector,
				ref _handle,
				(short)items.Length,
				items,
				(short)bufferLength,
				buffer);

			ProcessStatusVector(_statusVector);
		}

		#endregion
	}
}
