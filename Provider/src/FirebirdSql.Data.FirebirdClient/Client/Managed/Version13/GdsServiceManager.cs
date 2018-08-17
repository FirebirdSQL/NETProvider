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

using System.IO;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed.Version13
{
	internal class GdsServiceManager : Version12.GdsServiceManager
	{
		public GdsServiceManager(GdsConnection connection)
			: base(connection)
		{ }

		public override void Attach(ServiceParameterBuffer spb, string dataSource, int port, string service, byte[] cryptKey)
		{
			try
			{
				SendAttachToBuffer(spb, service);
				Database.XdrStream.Flush();
				var response = Database.ReadResponse();
				response = (Database as GdsDatabase).ProcessCryptCallbackResponseIfNeeded(response, cryptKey);
				ProcessAttachResponse(response as GenericResponse);
			}
			catch (IOException ex)
			{
				Database.Detach();
				throw IscException.ForErrorCode(IscCodes.isc_net_write_err, ex);
			}
		}

		protected override Version10.GdsDatabase CreateDatabase(GdsConnection connection)
		{
			return new GdsDatabase(connection);
		}
	}
}
