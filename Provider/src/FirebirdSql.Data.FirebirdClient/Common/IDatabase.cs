/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/blob/master/license.txt.
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
using System.Threading.Tasks;

namespace FirebirdSql.Data.Common
{
	internal interface IDatabase
	{
		Action<IscException> WarningMessage { get; set; }

		int Handle { get; }
		int TransactionCount { get; set; }
		string ServerVersion { get; }
		Charset Charset { get; set; }
		short PacketSize { get; set; }
		short Dialect { get; set; }
		bool HasRemoteEventSupport { get; }
		bool ConnectionBroken { get; }

		Task Attach(DatabaseParameterBufferBase dpb, string dataSource, int port, string database, byte[] cryptKey, AsyncWrappingCommonArgs async);
		Task AttachWithTrustedAuth(DatabaseParameterBufferBase dpb, string dataSource, int port, string database, byte[] cryptKey, AsyncWrappingCommonArgs async);
		Task Detach(AsyncWrappingCommonArgs async);

		Task CreateDatabase(DatabaseParameterBufferBase dpb, string dataSource, int port, string database, byte[] cryptKey, AsyncWrappingCommonArgs async);
		Task CreateDatabaseWithTrustedAuth(DatabaseParameterBufferBase dpb, string dataSource, int port, string database, byte[] cryptKey, AsyncWrappingCommonArgs async);
		Task DropDatabase(AsyncWrappingCommonArgs async);

		Task<TransactionBase> BeginTransaction(TransactionParameterBuffer tpb, AsyncWrappingCommonArgs async);

		StatementBase CreateStatement();
		StatementBase CreateStatement(TransactionBase transaction);

		DatabaseParameterBufferBase CreateDatabaseParameterBuffer();

		Task<List<object>> GetDatabaseInfo(byte[] items, AsyncWrappingCommonArgs async);
		Task<List<object>> GetDatabaseInfo(byte[] items, int bufferLength, AsyncWrappingCommonArgs async);

		Task CloseEventManager(AsyncWrappingCommonArgs async);
		Task QueueEvents(RemoteEvent events, AsyncWrappingCommonArgs async);
		Task CancelEvents(RemoteEvent events, AsyncWrappingCommonArgs async);

		Task CancelOperation(int kind, AsyncWrappingCommonArgs async);
	}
}
