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
 *  Copyright (c) 2008 Jiri Cincura (jiri@cincura.net)
 *  All Rights Reserved.
 *  
 *  Based on the Microsoft Entity Framework Provider Sample SP1 Beta 1
 */

#if (NET_35 && ENTITY_FRAMEWORK)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.Metadata.Edm;
using System.Data.Common.CommandTrees;
using System.Data.Common.Utils;
using System.Data.Mapping.Update.Internal;

using FirebirdSql.Data.FirebirdClient;

namespace FirebirdSql.Data.Entity
{
    /// <summary>
    /// Lightweight expression translator for DML expression trees, which have constrained
    /// scope and support.
    /// </summary>
    class ExpressionTranslator : DbExpressionVisitor
    {
        #region · Fields ·

        private readonly StringBuilder                      commandText;
        private readonly DbModificationCommandTree          commandTree;
        private readonly List<DbParameter>                  parameters;
        private readonly Dictionary<EdmMember, DbParameter> memberValues;
        private int                                         parameterNameCount = 0;

        #endregion

        #region · Internal Properties ·

        internal List<DbParameter> Parameters
        {
            get { return this.parameters; }
        }

        internal Dictionary<EdmMember, DbParameter> MemberValues
        {
            get { return this.memberValues; }
        }

        #endregion

        #region · Unsupported Visit Methods ·

        public override void Visit(DbApplyExpression expression)
        {
            throw new NotSupportedException("Visit(\"ApplyExpression\") is not supported.");
        }

        public override void Visit(DbArithmeticExpression expression)
        {
            throw new NotSupportedException("Visit(\"ArithmeticExpression\") is not supported.");
        }

        public override void Visit(DbCaseExpression expression)
        {
            throw new NotSupportedException("Visit(\"CaseExpression\") is not supported.");
        }

        public override void Visit(DbCastExpression expression)
        {
            throw new NotSupportedException("Visit(\"CastExpression\") is not supported.");
        }

        public override void Visit(DbCrossJoinExpression expression)
        {
            throw new NotSupportedException("Visit(\"CrossJoinExpression\") is not supported.");
        }

        public override void Visit(DbDerefExpression expression)
        {
            throw new NotSupportedException("Visit(\"DerefExpression\") is not supported.");
        }

        public override void Visit(DbDistinctExpression expression)
        {
            throw new NotSupportedException("Visit(\"DistinctExpression\") is not supported.");
        }

        public override void Visit(DbElementExpression expression)
        {
            throw new NotSupportedException("Visit(\"ElementExpression\") is not supported.");
        }

        public override void Visit(DbEntityRefExpression expression)
        {
            throw new NotSupportedException("Visit(\"EntityRefExpression\") is not supported.");
        }

        public override void Visit(DbExceptExpression expression)
        {
            throw new NotSupportedException("Visit(\"ExceptExpression\") is not supported.");
        }

        public override void Visit(DbExpression expression)
        {
            throw new NotSupportedException("Visit(\"Expression\") is not supported.");
        }

        public override void Visit(DbFilterExpression expression)
        {
            throw new NotSupportedException("Visit(\"FilterExpression\") is not supported.");
        }

        public override void Visit(DbFunctionExpression expression)
        {
            throw new NotSupportedException("Visit(\"FunctionExpression\") is not supported.");
        }

        public override void Visit(DbGroupByExpression expression)
        {
            throw new NotSupportedException("Visit(\"GroupByExpression\") is not supported.");
        }

        public override void Visit(DbIntersectExpression expression)
        {
            throw new NotSupportedException("Visit(\"IntersectExpression\") is not supported.");
        }

        public override void Visit(DbIsEmptyExpression expression)
        {
            throw new NotSupportedException("Visit(\"IsEmptyExpression\") is not supported.");
        }

        public override void Visit(DbIsOfExpression expression)
        {
            throw new NotSupportedException("Visit(\"IsOfExpression\") is not supported.");
        }

        public override void Visit(DbJoinExpression expression)
        {
            throw new NotSupportedException("Visit(\"JoinExpression\") is not supported.");
        }

        public override void Visit(DbLikeExpression expression)
        {
            throw new NotSupportedException("Visit(\"LikeExpression\") is not supported.");
        }

        public override void Visit(DbLimitExpression expression)
        {
            throw new NotSupportedException("Visit(\"LimitExpression\") is not supported.");
        }

        public override void Visit(DbOfTypeExpression expression)
        {
            throw new NotSupportedException("Visit(\"OfTypeExpression\") is not supported.");
        }

        public override void Visit(DbParameterReferenceExpression expression)
        {
            throw new NotSupportedException("Visit(\"ParameterReferenceExpression\") is not supported.");
        }

        public override void Visit(DbProjectExpression expression)
        {
            throw new NotSupportedException("Visit(\"ProjectExpression\") is not supported.");
        }

        public override void Visit(DbQuantifierExpression expression)
        {
            throw new NotSupportedException("Visit(\"QuantifierExpression\") is not supported.");
        }

        public override void Visit(DbRefExpression expression)
        {
            throw new NotSupportedException("Visit(\"RefExpression\") is not supported.");
        }

        public override void Visit(DbRefKeyExpression expression)
        {
            throw new NotSupportedException("Visit(\"RefKeyExpression\") is not supported.");
        }

        public override void Visit(DbRelationshipNavigationExpression expression)
        {
            throw new NotSupportedException("Visit(\"RelationshipNavigationExpression\") is not supported.");
        }

        public override void Visit(DbSkipExpression expression)
        {
            throw new NotSupportedException("Visit(\"SkipExpression\") is not supported.");
        }

        public override void Visit(DbSortExpression expression)
        {
            throw new NotSupportedException("Visit(\"SortExpression\") is not supported.");
        }

        public override void Visit(DbTreatExpression expression)
        {
            throw new NotSupportedException("Visit(\"TreatExpression\") is not supported.");
        }

        public override void Visit(DbUnionAllExpression expression)
        {
            throw new NotSupportedException("Visit(\"UnionAllExpression\") is not supported.");
        }

        public override void Visit(DbVariableReferenceExpression expression)
        {
            throw new NotSupportedException("Visit(\"VariableReferenceExpression\") is not supported.");
        }

#warning SP1 Beta comment
        //public override void Visit(DbViewExpression expression)
        //{
        //    throw new NotSupportedException("Visit(\"ViewExpression\") is not supported.");
        //}

        #endregion

        #region · Methods ·

        public override void Visit(DbAndExpression expression)
        {
            VisitBinary(expression, " and ");
        }

        public override void Visit(DbOrExpression expression)
        {
            VisitBinary(expression, " or ");
        }

        public override void Visit(DbComparisonExpression expression)
        {
            Debug.Assert(expression.ExpressionKind == DbExpressionKind.Equals,
                "only equals comparison expressions are produced in DML command trees in V1");

            VisitBinary(expression, " = ");

            RegisterMemberValue(expression.Left, expression.Right);
        }

        public override void Visit(DbIsNullExpression expression)
        {
            expression.Argument.Accept(this);
            commandText.Append(" is null");
        }

        public override void Visit(DbNotExpression expression)
        {
            commandText.Append("not (");
            expression.Accept(this);
            commandText.Append(")");
        }

        public override void Visit(DbConstantExpression expression)
        {
            FbParameter parameter = CreateParameter(expression.Value, expression.ResultType);
            commandText.Append(parameter.ParameterName);
        }

        public override void Visit(DbScanExpression expression)
        {
            commandText.Append(SqlGenerator.GetTargetTSql(expression.Target));
        }

        public override void Visit(DbPropertyExpression expression)
        {
            commandText.Append(DmlSqlGenerator.GenerateMemberSql(expression.Property));
        }

        public override void Visit(DbNullExpression expression)
        {
            commandText.Append("null");
        }

        public override void Visit(DbNewInstanceExpression expression)
        {
            // assumes all arguments are self-describing (no need to use aliases
            // because no renames are ever used in the projection)
            bool first = true;

            foreach (DbExpression argument in expression.Arguments)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    commandText.Append(", ");
                }
                argument.Accept(this);
            }
        }

        #endregion

        #region · Internal Methods ·

        /// <summary>
        /// Initialize a new expression translator populating the given string builder
        /// with command text. Command text builder and command tree must not be null.
        /// </summary>
        /// <param name="commandText">Command text with which to populate commands</param>
        /// <param name="commandTree">Command tree generating SQL</param>
        /// <param name="preserveMemberValues">Indicates whether the translator should preserve
        /// member values while compiling t-SQL (only needed for server generation)</param>
        internal ExpressionTranslator(
            StringBuilder               commandText, 
            DbModificationCommandTree   commandTree,
            bool                        preserveMemberValues)
        {
            Debug.Assert(null != commandText);
            Debug.Assert(null != commandTree);

            this.commandText    = commandText;
            this.commandTree    = commandTree;
            this.parameters     = new List<DbParameter>();
            this.memberValues   = preserveMemberValues ? new Dictionary<EdmMember, DbParameter>() : null;
        }

        // generate parameter (name based on parameter ordinal)
        internal FbParameter CreateParameter(object value, TypeUsage type)
        {
            PrimitiveTypeKind   primitiveType   = MetadataHelpers.GetPrimitiveTypeKind(type);
            DbType              dbType          = MetadataHelpers.GetDbType(primitiveType);

            return this.CreateParameter(value, dbType);
        }

        // Creates a new parameter for a value in this expression translator
        internal FbParameter CreateParameter(object value, DbType dbType)
        {
            string parameterName = string.Concat("@p", parameterNameCount.ToString(CultureInfo.InvariantCulture));
            parameterNameCount++;

            FbParameter parameter = new FbParameter(parameterName, value);
            
            parameter.DbType = dbType;
            parameters.Add(parameter);

            return parameter;
        }

        /// <summary>
        /// Call this method to register a property value pair so the translator "remembers"
        /// the values for members of the row being modified. These values can then be used
        /// to form a predicate for server-generation (based on the key of the row)
        /// </summary>
        /// <param name="propertyExpression">Expression containing the column reference (property expression).</param>
        /// <param name="value">Expression containing the value of the column.</param>
        internal void RegisterMemberValue(DbExpression propertyExpression, DbExpression value)
        {
            if (null != memberValues)
            {
                // register the value for this property
                Debug.Assert(propertyExpression.ExpressionKind == DbExpressionKind.Property,
                             "DML predicates and setters must be of the form property = value");

                // get name of left property 
                EdmMember property = ((DbPropertyExpression)propertyExpression).Property;

                // don't track null values
                if (value.ExpressionKind != DbExpressionKind.Null)
                {
                    Debug.Assert(value.ExpressionKind == DbExpressionKind.Constant, "value must either constant or null");

                    // retrieve the last parameter added (which describes the parameter)
                    memberValues[property] = parameters[parameters.Count - 1];
                }
            }
        }

        #endregion

        #region · Private Methods ·

        private void VisitBinary(DbBinaryExpression expression, string separator)
        {
            commandText.Append("(");
            expression.Left.Accept(this);
            commandText.Append(separator);
            expression.Right.Accept(this);
            commandText.Append(")");
        }

        #endregion
    }
}
#endif