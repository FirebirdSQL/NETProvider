/*
 *  Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.ibphoenix.com/main.nfs?a=ibphoenix&l=;PAGES;NAME='ibp_idpl'
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2002, 2004 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using NUnit.Framework;
using System;
using FirebirdSql.Data.Firebird;

namespace FirebirdSql.Data.Firebird.Tests
{
	[TestFixture]
	public class FbTypeConversion : BaseTest 
	{
		public FbTypeConversion() : base(false)
		{		
		}
				
		[Test]
		public void InsertDateAsString()
		{			
			FbTransaction	transaction = this.Connection.BeginTransaction();
			DateTime		dValue		= DateTime.Parse("01.01.1999");

			string sql = "insert into test (int_field, date_field) values (@int_field, @date_field)";

			try
			{
				FbCommand cmd = new FbCommand(
					sql, 
					this.Connection, 
					transaction);
				cmd.Parameters.Add("@int_Field", FbDbType.Integer).Value = 20000;
				cmd.Parameters.Add("@date_field", FbDbType.Date).Value = dValue;

				int result = cmd.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				transaction.Commit();
			}

			sql = "select date_field from test where int_field = @int_field";
			
			FbCommand check = new FbCommand(sql, this.Connection);
			check.Parameters.Add("@int_Field", FbDbType.Integer).Value = 20000;

			DateTime date = Convert.ToDateTime(check.ExecuteScalar());

			check.Dispose();

			Assertion.AssertEquals("Incorrect date value", dValue, date);
		}		

		[Test]
		public void InsertTimeAsString()
		{			
			FbTransaction	transaction = this.Connection.BeginTransaction();
			DateTime		tValue		= DateTime.Parse("10:20:30");

			string sql = "insert into test (int_field, time_field) values (@int_field, @time_field)";

			try
			{
				FbCommand cmd = new FbCommand(
					sql, 
					this.Connection, 
					transaction);
				cmd.Parameters.Add("@int_Field", FbDbType.Integer).Value = 20000;
				cmd.Parameters.Add("@time_field", FbDbType.Time).Value = tValue;

				int result = cmd.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				transaction.Commit();
			}

			sql = "select time_field from test where int_field = @int_field";
			
			FbCommand check = new FbCommand(sql, this.Connection);
			check.Parameters.Add("@int_Field", FbDbType.Integer).Value = 20000;

			DateTime time = Convert.ToDateTime(check.ExecuteScalar());

			check.Dispose();

			Assertion.AssertEquals("Incorrect time value", 
				tValue.ToString("HH:mm:ss"), time.ToString("HH:mm:ss"));
		}		

		[Test]
		public void InsertTimeStampAsString()
		{			
			FbTransaction	transaction = this.Connection.BeginTransaction();
			DateTime		dtValue		= DateTime.Parse("01.01.1999 10:20:30");

			string sql = "insert into test (int_field, timestamp_field) values (@int_field, @timestamp_field)";

			try
			{
				FbCommand cmd = new FbCommand(
					sql, 
					this.Connection, 
					transaction);
				cmd.Parameters.Add("@int_Field", FbDbType.Integer).Value = 20000;
				cmd.Parameters.Add("@timestamp_field", FbDbType.TimeStamp).Value = dtValue;

				int result = cmd.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				transaction.Commit();
			}

			sql = "select timestamp_field from test where int_field = @int_field";
			
			FbCommand check = new FbCommand(sql, this.Connection);
			check.Parameters.Add("@int_Field", FbDbType.Integer).Value = 20000;

			DateTime timeStamp = Convert.ToDateTime(check.ExecuteScalar());

			check.Dispose();

			Assertion.AssertEquals("Incorrect timestamp value", dtValue, timeStamp);
		}
	}
}
