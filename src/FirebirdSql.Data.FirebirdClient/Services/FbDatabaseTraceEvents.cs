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

namespace FirebirdSql.Data.Services;

[Flags]
public enum FbDatabaseTraceEvents
{
	Connections = 0x00_00_01,
	Transactions = 0x00_00_02,
	StatementPrepare = 0x00_00_04,
	StatementFree = 0x00_00_08,
	StatementStart = 0x00_00_10,
	StatementFinish = 0x00_00_20,
	ProcedureStart = 0x00_00_40,
	ProcedureFinish = 0x00_00_80,
	FunctionStart = 0x00_01_00,
	FunctionFinish = 0x00_02_00,
	TriggerStart = 0x00_04_00,
	TriggerFinish = 0x00_08_00,
	Context = 0x00_10_00,
	Errors = 0x00_20_00,
	Warnings = 0x00_40_00,
	InitFini = 0x00_80_00,
	Sweep = 0x01_00_00,
	PrintPlan = 0x02_00_00,
	ExplainPlan = 0x04_00_00,
	PrintPerf = 0x08_00_00,
	BLRRequests = 0x10_00_00,
	PrintBLR = 0x20_00_00,
	DYNRequests = 0x40_00_00,
	PrintDYN = 0x80_00_00,
}
