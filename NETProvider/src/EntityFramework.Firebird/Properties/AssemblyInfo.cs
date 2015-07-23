/*
 *  Firebird ADO.NET Data provider for .NET and Mono
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
 *  Copyright (c) 2014 Jiri Cincura (jiri@cincura.net)
 *  All Rights Reserved.
 */

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]
[assembly: AssemblyCompany("FirebirdSQL")]
[assembly: AssemblyProduct("FirebirdClient")]
[assembly: AssemblyDelaySign(false)]
[assembly: InternalsVisibleTo("FirebirdSql.Data.UnitTests, PublicKey=00240000048000009400000006020000002400005253413100040000010001002f636382c6d70ed5596f3db517cf3bf37950ee9ee86340d32d6f98143f0a4fdf0e934d361de0a6ce63c61e0a0dddc5f66d8ec752306b94931241061817f3c203e1105da8958ca9a889af83083bbb53dfdfee2d028d554bef2ce8a577816202a7bb38885e2dc74695d2a0fecfef259a34860a8faf54ce49a0cd5b5fdfa90f4bb7")]
[assembly: AssemblyCopyright("(c) 2014-2015")]
[assembly: AssemblyTitle("FirebirdClient - Entity Framework Provider")]
[assembly: AssemblyDescription("FirebirdClient - Entity Framework Provider")]
[assembly: AssemblyVersion(FirebirdSql.Data.EntityFramework6.Properties.VersionInfo.Version)]
[assembly: AssemblyFileVersion(FirebirdSql.Data.EntityFramework6.Properties.VersionInfo.Version)]