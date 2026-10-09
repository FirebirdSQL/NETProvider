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

//$Authors = Jiri Cincura (jiri@cincura.net)

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FirebirdSql.Data.Common;

internal abstract class ServiceManagerBase
{
	public Action<IscException> WarningMessage { get; set; }

	public abstract bool UseUtf8ParameterBuffer { get; }
	public Encoding ParameterBufferEncoding => UseUtf8ParameterBuffer ? Encoding.UTF8 : Encoding.GetANSIEncoding();

	public int Handle { get; protected set; }
	public Charset Charset { get; }

	public ServiceManagerBase(Charset charset)
	{
		Charset = charset;
	}

	public abstract void Attach(ServiceParameterBufferBase spb, string dataSource, int port, string service, byte[] cryptKey);
	public abstract ValueTask AttachAsync(ServiceParameterBufferBase spb, string dataSource, int port, string service, byte[] cryptKey, CancellationToken cancellationToken = default);

	public abstract void Detach();
	public abstract ValueTask DetachAsync(CancellationToken cancellationToken = default);

	public abstract void Start(ServiceParameterBufferBase spb);
	public abstract ValueTask StartAsync(ServiceParameterBufferBase spb, CancellationToken cancellationToken = default);

	public abstract void Query(ServiceParameterBufferBase spb, int requestLength, byte[] requestBuffer, int bufferLength, byte[] buffer);
	public abstract ValueTask QueryAsync(ServiceParameterBufferBase spb, int requestLength, byte[] requestBuffer, int bufferLength, byte[] buffer, CancellationToken cancellationToken = default);

	public abstract ServiceParameterBufferBase CreateServiceParameterBuffer();
}
