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
using System.Collections.Concurrent;

namespace FirebirdSql.Data.Common;

internal static class NativeHelpers
{
	private static readonly ConcurrentDictionary<string, bool> _cache = new ConcurrentDictionary<string, bool>(StringComparer.Ordinal);

	public static void CallIfExists(string actionId, Action action)
	{
		if (!_cache.TryGetValue(actionId, out var executionAllowed))
		{
			try
			{
				action();
				_cache.TryAdd(actionId, true);
			}
			catch (EntryPointNotFoundException)
			{
				_cache.TryAdd(actionId, false);
			}
		}
		else if (executionAllowed)
		{
			action();
		}
	}
}
