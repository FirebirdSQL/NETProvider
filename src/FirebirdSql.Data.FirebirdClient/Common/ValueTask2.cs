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

using System.Threading.Tasks;

namespace FirebirdSql.Data.Common;

internal static class ValueTask2
{
	public static ValueTask<TResult> FromResult<TResult>(TResult result) =>
#if NET48 || NETSTANDARD2_0 || NETSTANDARD2_1
			new ValueTask<TResult>(result);
#else
			ValueTask.FromResult(result);
#endif

	public static ValueTask CompletedTask =>
#if NET48 || NETSTANDARD2_0 || NETSTANDARD2_1
			default;
#else
			ValueTask.CompletedTask;
#endif
}
