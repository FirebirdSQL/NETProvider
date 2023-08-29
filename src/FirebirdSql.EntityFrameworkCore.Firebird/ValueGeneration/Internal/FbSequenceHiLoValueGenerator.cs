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
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using FirebirdSql.EntityFrameworkCore.Firebird.Storage.Internal;
using FirebirdSql.EntityFrameworkCore.Firebird.Update.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace FirebirdSql.EntityFrameworkCore.Firebird.ValueGeneration.Internal;

public class FbSequenceHiLoValueGenerator<TValue> : HiLoValueGenerator<TValue>
{
	readonly IRawSqlCommandBuilder _rawSqlCommandBuilder;
	readonly IFbUpdateSqlGenerator _sqlGenerator;
	readonly IFbRelationalConnection _connection;
	readonly ISequence _sequence;
	readonly IRelationalCommandDiagnosticsLogger _commandLogger;

	public FbSequenceHiLoValueGenerator(IRawSqlCommandBuilder rawSqlCommandBuilder, IFbUpdateSqlGenerator sqlGenerator, FbSequenceValueGeneratorState generatorState, IFbRelationalConnection connection, IRelationalCommandDiagnosticsLogger commandLogger)
		: base(generatorState)
	{
		_sequence = generatorState.Sequence;
		_rawSqlCommandBuilder = rawSqlCommandBuilder;
		_sqlGenerator = sqlGenerator;
		_connection = connection;
		_commandLogger = commandLogger;
	}

	protected override long GetNewLowValue()
		=> (long)Convert.ChangeType(
			_rawSqlCommandBuilder
				.Build(_sqlGenerator.GenerateNextSequenceValueOperation(_sequence.Name, _sequence.Schema))
				.ExecuteScalar(
					new RelationalCommandParameterObject(
						_connection,
						parameterValues: null,
						readerColumns: null,
						context: null,
						_commandLogger, CommandSource.ValueGenerator)),
			typeof(long),
			CultureInfo.InvariantCulture)!;

	protected override async Task<long> GetNewLowValueAsync(CancellationToken cancellationToken = default)
		=> (long)Convert.ChangeType(
			await _rawSqlCommandBuilder
				.Build(_sqlGenerator.GenerateNextSequenceValueOperation(_sequence.Name, _sequence.Schema))
				.ExecuteScalarAsync(
					new RelationalCommandParameterObject(
						_connection,
						parameterValues: null,
						readerColumns: null,
						context: null,
						_commandLogger, CommandSource.ValueGenerator),
					cancellationToken)
				.ConfigureAwait(false),
			typeof(long),
			CultureInfo.InvariantCulture)!;

	public override bool GeneratesTemporaryValues
		=> false;
}
