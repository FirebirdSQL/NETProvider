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

//$Authors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;

namespace FirebirdSql.Data.Services;

public class FbServerConfig
{
	public int LockMemSize { get; internal set; }
	public int LockSemCount { get; internal set; }
	public int LockSignal { get; internal set; }
	public int EventMemorySize { get; internal set; }
	public int PrioritySwitchDelay { get; internal set; }
	public int MinMemory { get; internal set; }
	public int MaxMemory { get; internal set; }
	public int LockGrantOrder { get; internal set; }
	public int AnyLockMemory { get; internal set; }
	public int AnyLockSemaphore { get; internal set; }
	public int AnyLockSignal { get; internal set; }
	public int AnyEventMemory { get; internal set; }
	public int LockHashSlots { get; internal set; }
	public int DeadlockTimeout { get; internal set; }
	public int LockRequireSpins { get; internal set; }
	public int ConnectionTimeout { get; internal set; }
	public int DummyPacketInterval { get; internal set; }
	public int IpcMapSize { get; internal set; }
	public int DefaultDbCachePages { get; internal set; }

	internal FbServerConfig()
	{ }
}
