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

//$Authors = Carlos Guzman Alvarez

using System;

namespace FirebirdSql.Data.FirebirdClient
{
	[Serializable]
	[Flags]
	public enum FbTransactionBehavior : int
	{
		Consistency = 1,
		Concurrency = 2,
		Shared = 4,
		Protected = 8,
		Exclusive = 16,
		Wait = 32,
		NoWait = 64,
		Read = 128,
		Write = 256,
		LockRead = 512,
		LockWrite = 1024,
		ReadCommitted = 2048,
		Autocommit = 4096,
		RecVersion = 8192,
		NoRecVersion = 16384,
		RestartRequests = 32768,
		NoAutoUndo = 65536
	}
}
