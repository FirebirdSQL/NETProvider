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

namespace FirebirdSql.Data.Common
{
	internal interface IServiceManager
	{
		Action<IscException> WarningMessage { get; set; }

		int Handle { get; }

		void Attach(ServiceParameterBuffer spb, string dataSource, int port, string service, byte[] cryptKey);
		void Detach();
		void Start(ServiceParameterBuffer spb);
		void Query(ServiceParameterBuffer spb, int requestLength, byte[] requestBuffer, int bufferLength, byte[] buffer);
	}
}
