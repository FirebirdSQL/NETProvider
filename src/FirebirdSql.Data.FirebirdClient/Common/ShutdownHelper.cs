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

internal static class ShutdownHelper
{
	static ConcurrentBag<Action> _pools;
	static ConcurrentBag<Action> _fbClients;

	static ShutdownHelper()
	{
		_pools = new ConcurrentBag<Action>();
		_fbClients = new ConcurrentBag<Action>();
		AppDomain.CurrentDomain.DomainUnload += (sender, e) => HandleDomainUnload();
		AppDomain.CurrentDomain.ProcessExit += (sender, e) => HandleProcessShutdown();
	}

	internal static void RegisterPoolCleanup(Action item)
	{
		_pools.Add(item);
	}

	internal static void RegisterFbClientShutdown(Action item)
	{
		_fbClients.Add(item);
	}

	static void HandleDomainUnload()
	{
		while (_pools.TryTake(out var item))
			item();
	}

	static void HandleProcessShutdown()
	{
		HandleDomainUnload();
		while (_fbClients.TryTake(out var item))
			item();
	}
}
