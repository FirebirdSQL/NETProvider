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

//$Authors = Jiri Cincura (jiri@cincura.net)

using System;
using System.Threading.Tasks;

namespace FirebirdSql.Data.Common
{
	internal abstract class ServiceManagerBase
	{
		public Action<IscException> WarningMessage { get; set; }

		public int Handle { get; protected set; }

		public abstract ValueTask Attach(ServiceParameterBufferBase spb, string dataSource, int port, string service, byte[] cryptKey, AsyncWrappingCommonArgs async);
		public abstract ValueTask Detach(AsyncWrappingCommonArgs async);
		public abstract ValueTask Start(ServiceParameterBufferBase spb, AsyncWrappingCommonArgs async);
		public abstract ValueTask Query(ServiceParameterBufferBase spb, int requestLength, byte[] requestBuffer, int bufferLength, byte[] buffer, AsyncWrappingCommonArgs async);

		public abstract ServiceParameterBufferBase CreateServiceParameterBuffer();
	}
}
