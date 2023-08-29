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
using FirebirdSql.EntityFrameworkCore.Firebird.Storage.Internal;
using FirebirdSql.EntityFrameworkCore.Firebird.Update.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace FirebirdSql.EntityFrameworkCore.Firebird.ValueGeneration.Internal;

public class FbSequenceValueGeneratorFactory : IFbSequenceValueGeneratorFactory
{
	readonly IFbUpdateSqlGenerator _sqlGenerator;

	public FbSequenceValueGeneratorFactory(IFbUpdateSqlGenerator sqlGenerator)
	{
		_sqlGenerator = sqlGenerator;
	}

	public virtual ValueGenerator TryCreate(IProperty property, Type type, FbSequenceValueGeneratorState generatorState, IFbRelationalConnection connection, IRawSqlCommandBuilder rawSqlCommandBuilder, IRelationalCommandDiagnosticsLogger commandLogger)
	{
		if (type == typeof(long))
		{
			return new FbSequenceHiLoValueGenerator<long>(rawSqlCommandBuilder, _sqlGenerator, generatorState, connection, commandLogger);
		}

		if (type == typeof(int))
		{
			return new FbSequenceHiLoValueGenerator<int>(rawSqlCommandBuilder, _sqlGenerator, generatorState, connection, commandLogger);
		}

		if (type == typeof(decimal))
		{
			return new FbSequenceHiLoValueGenerator<decimal>(rawSqlCommandBuilder, _sqlGenerator, generatorState, connection, commandLogger);
		}

		if (type == typeof(short))
		{
			return new FbSequenceHiLoValueGenerator<short>(rawSqlCommandBuilder, _sqlGenerator, generatorState, connection, commandLogger);
		}

		if (type == typeof(byte))
		{
			return new FbSequenceHiLoValueGenerator<byte>(rawSqlCommandBuilder, _sqlGenerator, generatorState, connection, commandLogger);
		}

		if (type == typeof(char))
		{
			return new FbSequenceHiLoValueGenerator<char>(rawSqlCommandBuilder, _sqlGenerator, generatorState, connection, commandLogger);
		}

		if (type == typeof(ulong))
		{
			return new FbSequenceHiLoValueGenerator<ulong>(rawSqlCommandBuilder, _sqlGenerator, generatorState, connection, commandLogger);
		}

		if (type == typeof(uint))
		{
			return new FbSequenceHiLoValueGenerator<uint>(rawSqlCommandBuilder, _sqlGenerator, generatorState, connection, commandLogger);
		}

		if (type == typeof(ushort))
		{
			return new FbSequenceHiLoValueGenerator<ushort>(rawSqlCommandBuilder, _sqlGenerator, generatorState, connection, commandLogger);
		}

		if (type == typeof(sbyte))
		{
			return new FbSequenceHiLoValueGenerator<sbyte>(rawSqlCommandBuilder, _sqlGenerator, generatorState, connection, commandLogger);
		}

		return null;
	}
}
