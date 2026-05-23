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

using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;

namespace FirebirdSql.Data.FirebirdClient.Benchmarks;

[Config(typeof(BenchmarkConfig))]
public partial class CommandBenchmark : BenchmarkBase
{
	const int Count = 100;

	[Params("BIGINT", "VARCHAR(10) CHARACTER SET UTF8", "CHAR(100) CHARACTER SET UTF8")]
	public string DataType { get; set; }

	// A parameter value matched to the column type: a string sized to the declared
	// length for CHAR/VARCHAR (so the rune count/validate write path is exercised),
	// or an integer otherwise. The length must not exceed the column or the server
	// raises isc_string_truncation.
	private object SampleValue()
	{
		if (DataType.Contains("CHAR"))
		{
			var match = Regex.Match(DataType, @"\((\d+)\)");
			var length = match.Success ? int.Parse(match.Groups[1].Value) : 1;
			return new string('x', length);
		}
		return 1;
	}
}
