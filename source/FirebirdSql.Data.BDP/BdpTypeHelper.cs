/*
 *  Firebird BDP - Borland Data provider Firebird
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2004 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using System;

using Borland.Data.Common;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Bdp
{
	internal class BdpTypeHelper
	{
		#region Constructors

		private BdpTypeHelper()
		{
		}

		#endregion

		#region Methods

		public static BdpType GetBdpType(DbDataType type)
		{
			switch (type)
			{
				case DbDataType.Array:
					return BdpType.Array;

				case DbDataType.Binary:
					return BdpType.Blob;

                case DbDataType.SmallInt:
                    return BdpType.Int16;

                case DbDataType.Integer:
                    return BdpType.Int32;

                case DbDataType.BigInt:
					return BdpType.Int64;

				case DbDataType.Char:
				case DbDataType.VarChar:
					return BdpType.String;

				case DbDataType.Date:
					return BdpType.Date;

				case DbDataType.Decimal:
				case DbDataType.Numeric:
					return BdpType.Decimal;

				case DbDataType.Double:
					return BdpType.Double;

				case DbDataType.Float:
					return BdpType.Float;

                case DbDataType.Text:
					return BdpType.Blob;

				case DbDataType.Time:
					return BdpType.Time;

				case DbDataType.TimeStamp:
					return BdpType.DateTime;

				default:
					return BdpType.Unknown;
			}
		}

		public static BdpType GetBdpSubType(DbDataType type)
		{
			switch (type)
			{
				case DbDataType.Binary:
					return BdpType.stHBinary;

				case DbDataType.Char:
					return BdpType.stFixed;

				case DbDataType.Text:
					return BdpType.stHMemo;

				default:
					return BdpType.Unknown;
			}
		}

		#endregion
	}
}
