using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using FirebirdSql.EntityFrameworkCore.Firebird.Storage.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Query.ExpressionTranslators.Internal
{
	public class FbConvertTranslator : IMethodCallTranslator
	{
		static readonly Dictionary<string, string> TypeMapping = new Dictionary<string, string>
		{
			[nameof(Convert.ToByte)] = "SMALLINT",
			[nameof(Convert.ToDecimal)] = $"DECIMAL({FbTypeMapper.DefaultDecimalPrecision},{FbTypeMapper.DefaultDecimalScale})",
			[nameof(Convert.ToDouble)] = "DOUBLE PRECISION",
			[nameof(Convert.ToInt16)] = "SMALLINT",
			[nameof(Convert.ToInt32)] = "INTEGER",
			[nameof(Convert.ToInt64)] = "BIGINT",
			[nameof(Convert.ToString)] = $"VARCHAR({FbTypeMapper.VarcharMaxSize})"
		};

		static readonly HashSet<Type> SuportedTypes = new HashSet<Type>
		{
			typeof(bool),
			typeof(byte),
			typeof(decimal),
			typeof(double),
			typeof(float),
			typeof(int),
			typeof(long),
			typeof(short),
			typeof(string)
		};

		static readonly IEnumerable<MethodInfo> SupportedMethods
			= TypeMapping.Keys
				.SelectMany(t => typeof(Convert).GetTypeInfo().GetDeclaredMethods(t)
					.Where(m => m.GetParameters().Length == 1 && SuportedTypes.Contains(m.GetParameters().First().ParameterType)));

		public virtual Expression Translate(MethodCallExpression methodCallExpression)
			=> SupportedMethods.Contains(methodCallExpression.Method)
				? new ExplicitCastExpression(methodCallExpression.Arguments[0], methodCallExpression.Type)
				: null;
	}
}
