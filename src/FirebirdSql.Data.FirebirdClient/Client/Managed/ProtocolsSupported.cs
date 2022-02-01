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

using System.Collections.Generic;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed;

internal static class ProtocolsSupported
{
	internal class Protocol
	{
		public int Version { get; }
		public int MaxPType { get; }

		public Protocol(int version, int maxPType)
		{
			Version = version;
			MaxPType = maxPType;
		}
	}

	public static ICollection<Protocol> Get(bool compression)
	{
		return new[]
		{
				new Protocol(IscCodes.PROTOCOL_VERSION10, IscCodes.ptype_batch_send),
				new Protocol(IscCodes.PROTOCOL_VERSION11, IscCodes.ptype_lazy_send),
				new Protocol(IscCodes.PROTOCOL_VERSION12, IscCodes.ptype_lazy_send),
				new Protocol(IscCodes.PROTOCOL_VERSION13, IscCodes.ptype_lazy_send | (compression ? IscCodes.pflag_compress : 0)),
				new Protocol(IscCodes.PROTOCOL_VERSION15, IscCodes.ptype_lazy_send | (compression ? IscCodes.pflag_compress : 0)),
				new Protocol(IscCodes.PROTOCOL_VERSION16, IscCodes.ptype_lazy_send | (compression ? IscCodes.pflag_compress : 0)),
			};
	}
}
