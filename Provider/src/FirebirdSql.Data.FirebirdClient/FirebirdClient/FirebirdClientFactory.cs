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

using System.Data.Common;

namespace FirebirdSql.Data.FirebirdClient
{
	public class FirebirdClientFactory : DbProviderFactory
	{
		#region Static Properties

		public static readonly FirebirdClientFactory Instance = new FirebirdClientFactory();

		#endregion

		#region Properties

#if !NETSTANDARD1_6
		public override bool CanCreateDataSourceEnumerator
		{
			get { return false; }
		}
#endif

		#endregion

		#region Constructors

		private FirebirdClientFactory()
			: base()
		{ }

		#endregion

		#region Methods

		public override DbCommand CreateCommand()
		{
			return new FbCommand();
		}

#if !NETSTANDARD1_6
		public override DbCommandBuilder CreateCommandBuilder()
		{
			return new FbCommandBuilder();
		}
#endif

		public override DbConnection CreateConnection()
		{
			return new FbConnection();
		}

		public override DbConnectionStringBuilder CreateConnectionStringBuilder()
		{
			return new FbConnectionStringBuilder();
		}

#if !NETSTANDARD1_6
		public override DbDataAdapter CreateDataAdapter()
		{
			return new FbDataAdapter();
		}
#endif

		public override DbParameter CreateParameter()
		{
			return new FbParameter();
		}

		#endregion
	}
}
