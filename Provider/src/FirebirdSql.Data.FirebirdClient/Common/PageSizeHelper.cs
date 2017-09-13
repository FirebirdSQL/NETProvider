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

namespace FirebirdSql.Data.Common
{
	internal static class PageSizeHelper
	{
		public static bool IsValidPageSize(int value)
		{
			return value == 1024
				|| value == 2048
				|| value == 4096
				|| value == 8192
				|| value == 16384;
		}
	}
}
