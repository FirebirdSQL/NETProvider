/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/raw/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$Authors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.Data.Client.Native.Handle;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Native
{
	internal sealed class FesDatabase : DatabaseBase
	{
		#region Fields

		private DatabaseHandle _handle;
		private IntPtr[] _statusVector;
		private IFbClient _fbClient;

		#endregion

		#region Properties

		public override int Handle
		{
			get { return _handle.DangerousGetHandle().AsInt(); }
		}

		public DatabaseHandle HandlePtr
		{
			get { return _handle; }
		}

		public override bool HasRemoteEventSupport
		{
			get { return false; }
		}

		public override bool ConnectionBroken
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
			_fbClient = FbClientFactory.Create(dllName);
			_handle = new DatabaseHandle();
			Charset = charset ?? Charset.DefaultCharset;
			Dialect = 3;
			PacketSize = 8192;
			_statusVector = new IntPtr[IscCodes.ISC_STATUS_LENGTH];
		}

		#endregion

		#region Database Methods

		public override void CreateDatabase(DatabaseParameterBufferBase dpb, string database, byte[] cryptKey)
		{
			CheckCryptKeyForSupport(cryptKey);

			var databaseBuffer = Encoding2.Default.GetBytes(database);

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
		}
		public override ValueTask CreateDatabaseAsync(DatabaseParameterBufferBase dpb, string database, byte[] cryptKey, CancellationToken cancellationToken = default)
		{
			CheckCryptKeyForSupport(cryptKey);

			var databaseBuffer = Encoding2.Default.GetBytes(database);

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

			return ValueTask2.CompletedTask;
		}

		public override void CreateDatabaseWithTrustedAuth(DatabaseParameterBufferBase dpb, string database, byte[] cryptKey)
		{
			throw new NotSupportedException("Trusted Auth isn't supported on Firebird Embedded.");
		}
		public override ValueTask CreateDatabaseWithTrustedAuthAsync(DatabaseParameterBufferBase dpb, string database, byte[] cryptKey, CancellationToken cancellationToken = default)
		{
			throw new NotSupportedException("Trusted Auth isn't supported on Firebird Embedded.");
		}

		public override void DropDatabase()
		{
			ClearStatusVector();

			_fbClient.isc_drop_database(_statusVector, ref _handle);

			ProcessStatusVector(_statusVector);

			_handle.Dispose();
		}
		public override ValueTask DropDatabaseAsync(CancellationToken cancellationToken = default)
		{
			ClearStatusVector();

			_fbClient.isc_drop_database(_statusVector, ref _handle);

			ProcessStatusVector(_statusVector);

			_handle.Dispose();

			return ValueTask2.CompletedTask;
		}

		#endregion

		#region Remote Events Methods

		public override void CloseEventManager()
		{
			throw new NotSupportedException();
		}
		public override ValueTask CloseEventManagerAsync(CancellationToken cancellationToken = default)
		{
			throw new NotSupportedException();
		}

		public override void QueueEvents(RemoteEvent events)
		{
			throw new NotSupportedException();
		}
		public override ValueTask QueueEventsAsync(RemoteEvent events, CancellationToken cancellationToken = default)
		{
			throw new NotSupportedException();
		}

		public override void CancelEvents(RemoteEvent events)
		{
			throw new NotSupportedException();
		}
		public override ValueTask CancelEventsAsync(RemoteEvent events, CancellationToken cancellationToken = default)
		{
			throw new NotSupportedException();
		}

		#endregion

		#region Methods

		public override void Attach(DatabaseParameterBufferBase dpb, string database, byte[] cryptKey)
		{
			CheckCryptKeyForSupport(cryptKey);

			var databaseBuffer = Encoding2.Default.GetBytes(database);

			ClearStatusVector();

			_fbClient.isc_attach_database(
				_statusVector,
				(short)databaseBuffer.Length,
				databaseBuffer,
				ref _handle,
				dpb.Length,
				dpb.ToArray());

			ProcessStatusVector(_statusVector);

			ServerVersion = GetServerVersion();
		}
		public override async ValueTask AttachAsync(DatabaseParameterBufferBase dpb, string database, byte[] cryptKey, CancellationToken cancellationToken = default)
		{
			CheckCryptKeyForSupport(cryptKey);

			var databaseBuffer = Encoding2.Default.GetBytes(database);

			ClearStatusVector();

			_fbClient.isc_attach_database(
				_statusVector,
				(short)databaseBuffer.Length,
				databaseBuffer,
				ref _handle,
				dpb.Length,
				dpb.ToArray());

			ProcessStatusVector(_statusVector);

			ServerVersion = await GetServerVersionAsync(cancellationToken).ConfigureAwait(false);
		}

		public override void AttachWithTrustedAuth(DatabaseParameterBufferBase dpb, string database, byte[] cryptKey)
		{
			throw new NotSupportedException("Trusted Auth isn't supported on Firebird Embedded.");
		}
		public override ValueTask AttachWithTrustedAuthAsync(DatabaseParameterBufferBase dpb, string database, byte[] cryptKey, CancellationToken cancellationToken = default)
		{
			throw new NotSupportedException("Trusted Auth isn't supported on Firebird Embedded.");
		}

		public override void Detach()
		{
			if (TransactionCount > 0)
			{
				throw IscException.ForErrorCodeIntParam(IscCodes.isc_open_trans, TransactionCount);
			}

			if (!_handle.IsInvalid)
			{
				ClearStatusVector();

				_fbClient.isc_detach_database(_statusVector, ref _handle);

				ProcessStatusVector(_statusVector);

				_handle.Dispose();
			}

			WarningMessage = null;
			Charset = null;
			ServerVersion = null;
			_statusVector = null;
			TransactionCount = 0;
			Dialect = 0;
			PacketSize = 0;
		}
		public override ValueTask DetachAsync(CancellationToken cancellationToken = default)
		{
			if (TransactionCount > 0)
			{
				throw IscException.ForErrorCodeIntParam(IscCodes.isc_open_trans, TransactionCount);
			}

			if (!_handle.IsInvalid)
			{
				ClearStatusVector();

				_fbClient.isc_detach_database(_statusVector, ref _handle);

				ProcessStatusVector(_statusVector);

				_handle.Dispose();
			}

			WarningMessage = null;
			Charset = null;
			ServerVersion = null;
			_statusVector = null;
			TransactionCount = 0;
			Dialect = 0;
			PacketSize = 0;

			return ValueTask2.CompletedTask;
		}

		#endregion

		#region Transaction Methods

		public override TransactionBase BeginTransaction(TransactionParameterBuffer tpb)
		{
			var transaction = new FesTransaction(this);
			transaction.BeginTransaction(tpb);
			return transaction;
		}
		public override async ValueTask<TransactionBase> BeginTransactionAsync(TransactionParameterBuffer tpb, CancellationToken cancellationToken = default)
		{
			var transaction = new FesTransaction(this);
			await transaction.BeginTransactionAsync(tpb, cancellationToken).ConfigureAwait(false);
			return transaction;
		}

		#endregion

		#region Cancel Methods

		public override void CancelOperation(short kind)
		{
			var localStatusVector = new IntPtr[IscCodes.ISC_STATUS_LENGTH];

			_fbClient.fb_cancel_operation(localStatusVector, ref _handle, (ushort)kind);

			try
			{
				ProcessStatusVector(localStatusVector);
			}
			catch (IscException ex) when (ex.ErrorCode == IscCodes.isc_nothing_to_cancel)
			{ }
		}
		public override ValueTask CancelOperationAsync(short kind, CancellationToken cancellationToken = default)
		{
			var localStatusVector = new IntPtr[IscCodes.ISC_STATUS_LENGTH];

			_fbClient.fb_cancel_operation(localStatusVector, ref _handle, (ushort)kind);

			try
			{
				ProcessStatusVector(localStatusVector);
			}
			catch (IscException ex) when (ex.ErrorCode == IscCodes.isc_nothing_to_cancel)
			{ }

			return ValueTask2.CompletedTask;
		}

		#endregion

		#region Statement Creation Methods

		public override StatementBase CreateStatement()
		{
			return new FesStatement(this);
		}

		public override StatementBase CreateStatement(TransactionBase transaction)
		{
			return new FesStatement(this, transaction as FesTransaction);
		}

		#endregion

		#region DPB

		public override DatabaseParameterBufferBase CreateDatabaseParameterBuffer()
		{
			return new DatabaseParameterBuffer1();
		}

		#endregion

		#region Database Information Methods

		public override List<object> GetDatabaseInfo(byte[] items)
		{
			return GetDatabaseInfo(items, IscCodes.DEFAULT_MAX_BUFFER_SIZE);
		}
		public override ValueTask<List<object>> GetDatabaseInfoAsync(byte[] items, CancellationToken cancellationToken = default)
		{
			return GetDatabaseInfoAsync(items, IscCodes.DEFAULT_MAX_BUFFER_SIZE, cancellationToken);
		}

		public override List<object> GetDatabaseInfo(byte[] items, int bufferLength)
		{
			var buffer = new byte[bufferLength];

			DatabaseInfo(items, buffer, buffer.Length);

			return IscHelper.ParseDatabaseInfo(buffer);
		}
		public override ValueTask<List<object>> GetDatabaseInfoAsync(byte[] items, int bufferLength, CancellationToken cancellationToken = default)
		{
			var buffer = new byte[bufferLength];

			DatabaseInfo(items, buffer, buffer.Length);

			return ValueTask2.FromResult(IscHelper.ParseDatabaseInfo(buffer));
		}

		#endregion

		#region Internal Methods

		internal void ProcessStatusVector(IntPtr[] statusVector)
		{
			var ex = FesConnection.ParseStatusVector(statusVector, Charset);

			if (ex != null)
			{
				if (ex.IsWarning)
				{
					WarningMessage?.Invoke(ex);
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

		#region Internal Static Methods

		internal static void CheckCryptKeyForSupport(byte[] cryptKey)
		{
			// ICryptKeyCallbackImpl would have to be passed from C# for 'cryptKey' passing
			if (cryptKey?.Length > 0)
				throw new NotSupportedException("Passing Encryption Key isn't, yet, supported on Firebird Embedded.");
		}

		#endregion
	}
}
