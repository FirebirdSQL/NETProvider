/*
 *	Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *	   The contents of this file are subject to the Initial 
 *	   Developer's Public License Version 1.0 (the "License"); 
 *	   you may not use this file except in compliance with the 
 *	   License. You may obtain a copy of the License at 
 *	   http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *	   Software distributed under the License is distributed on 
 *	   an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *	   express or implied. See the License for the specific 
 *	   language governing rights and limitations under the License.
 * 
 *	Copyright (c) 2010-2012 Jiri Cincura (jiri@cincura.net)
 *	All Rights Reserved.
 */

using System;

namespace FirebirdSql.Data.Services
{
	[Flags]
	public enum FbDatabaseTraceEvents
	{
		Connections			= 0x00001,
		Transactions		= 0x00002,
		StatementPrepare	= 0x00004,
		StatementFree		= 0x00008,
		StatementStart		= 0x00010,
		StatementFinish		= 0x00020,
		ProcedureStart		= 0x00040,
		ProcedureFinish		= 0x00080,
		TriggerStart		= 0x00100,
		TriggerFinish		= 0x00200,
		Context				= 0x00400,
		PrintPlan			= 0x00800,
		PrintPerf			= 0x01000,
		BLRRequests			= 0x02000,
		PrintBLR			= 0x04000,
		DYNRequests			= 0x08000,
		PrintDYN			= 0x10000,
		Errors				= 0x20000,
		Sweep				= 0x40000,
	}
}
