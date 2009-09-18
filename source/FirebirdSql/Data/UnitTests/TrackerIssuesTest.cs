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
 *  Copyright (c) 2006 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using System;
using System.Data;
using System.Configuration;
using System.Collections.Specialized;

using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.Isql;
using NUnit.Framework;
using System.Text;

namespace FirebirdSql.Data.UnitTests
{
    [TestFixture]
    public class TrackerIssuesTest : BaseTest
    {
        #region · Constructors ·

        public TrackerIssuesTest()
        {
        }

        #endregion

        #region · Unit Tests ·

        [Test]
        public void DNET60()
        {
            using (FbCommand command = Connection.CreateCommand())
            {
                command.CommandText = "select ' ' AS EmptyColumn from rdb$database";

                using (FbDataReader r = command.ExecuteReader())
                {
                    while (r.Read())
                    {
                    }
                }
            }
		}

        [Test]
        public void DNET183()
        {
            const string value = "foo  ";

            using (FbCommand cmd = Connection.CreateCommand())
            {
                cmd.CommandText = "select cast(@foo as varchar(5)) from rdb$database";
                cmd.Parameters.Add(new FbParameter() { ParameterName = "@foo", FbDbType = FbDbType.VarChar, Size = 5, Value = value });
                using (FbDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Assert.AreEqual(value, (string)reader[0]);
                    }
                }
            }
        }

        [Test]
        public void DNET217()
        {
            StringBuilder cols = new StringBuilder();
            string separator = string.Empty;
            for (int i = 0; i < 1235; i++)
            {
                if (i % 2 == 0)
                    cols.AppendFormat("{0}'r' as col{1}", separator, i);
                else
                    cols.AppendFormat("{0}24 as col{1}", separator, i);

                separator = ",";
            }
            using (FbCommand cmd = Connection.CreateCommand())
            {
                cmd.CommandText = "select " + cols.ToString() + " from rdb$database where 'x' = @x or 'x' = @x and current_timestamp = @y and current_timestamp = @y and current_timestamp = @y and current_timestamp = @y and current_timestamp = @y and current_timestamp = @y and current_timestamp = @y and current_timestamp = @y and current_timestamp = @y and current_timestamp = @y and current_timestamp = @y";
                cmd.Parameters.Add(new FbParameter() { ParameterName = "@x", Value = "-1" });
                cmd.Parameters.Add(new FbParameter() { ParameterName = "@y", Value = DateTime.Now });
                using (FbDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    { }
                }
            }
        }

		[Test]
		public void DNET273()
		{
			using (FbCommand cmd = Connection.CreateCommand())
			{
				cmd.CommandText = "insert into test (INT_FIELD, CLOB_FIELD) values (@INT_FIELD, @CLOB_FIELD)";
				cmd.Parameters.Add("@INT_FIELD", FbDbType.Integer).Value = 100;
				cmd.Parameters.Add("@CLOB_FIELD", FbDbType.Binary).Value = new byte[] { 0x00, 0x001 };
				cmd.ExecuteNonQuery();
			}
		}

        #endregion
    }
}
