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
 *	Copyright (c) 2016 Jiri Cincura (jiri@cincura.net)
 *	All Rights Reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed
{
	internal static class ProtocolsSupported
	{
		internal class Protocol
		{
			public int Version { get; }
			public int MinPType { get; }
			public int MaxPType { get; }

			public Protocol(int version, int minPType, int maxPType)
			{
				Version = version;
				MinPType = minPType;
				MaxPType = maxPType;
			}
		}

		public static ICollection<Protocol> Protocols
		{
			get
			{
				return new[]
				{
					new Protocol(IscCodes.PROTOCOL_VERSION10, IscCodes.ptype_rpc, IscCodes.ptype_batch_send),
					new Protocol(IscCodes.PROTOCOL_VERSION11, IscCodes.ptype_rpc, IscCodes.ptype_lazy_send),
					new Protocol(IscCodes.PROTOCOL_VERSION12, IscCodes.ptype_rpc, IscCodes.ptype_lazy_send),
					new Protocol(IscCodes.PROTOCOL_VERSION13, IscCodes.ptype_rpc, IscCodes.ptype_lazy_send | IscCodes.pflag_compress),
				};
			}
		}
	}
}
