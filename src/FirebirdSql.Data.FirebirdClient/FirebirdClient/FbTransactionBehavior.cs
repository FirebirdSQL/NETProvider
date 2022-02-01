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

namespace FirebirdSql.Data.FirebirdClient;

[Serializable]
[Flags]
public enum FbTransactionBehavior
{
	Consistency = 1 << 0,
	Concurrency = 1 << 1,
	Shared = 1 << 2,
	Protected = 1 << 3,
	Exclusive = 1 << 4,
	Wait = 1 << 5,
	NoWait = 1 << 6,
	Read = 1 << 7,
	Write = 1 << 8,
	LockRead = 1 << 9,
	LockWrite = 1 << 10,
	ReadCommitted = 1 << 11,
	Autocommit = 1 << 12,
	RecVersion = 1 << 13,
	NoRecVersion = 1 << 14,
	RestartRequests = 1 << 15,
	NoAutoUndo = 1 << 16,
	ReadConsistency = 1 << 17,
}
