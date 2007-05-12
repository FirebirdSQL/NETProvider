/*
 *  Firebird BDP - Borland Data provider Firebird
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
 *  Copyright (c) 2004 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using NUnit.Framework;
using System;
using System.Text;
using System.Configuration;
using System.Collections;
using System.Security.Cryptography;

using Borland.Data.Provider;
using Borland.Data.Common;

namespace FirebirdSql.Data.Bdp.Tests
{
	public class BaseTest
	{
		#region Fields

		private BdpConnection	connection;
		private BdpTransaction	transaction;
		private bool			withTransaction;

		#endregion

		#region Properties

		public BdpConnection Connection
		{
			get { return connection; }
		}

		public BdpTransaction Transaction
		{
			get { return transaction; }
			set { transaction = value; }
		}

		#endregion

		#region Constructors
				
		public BaseTest()
		{
			this.withTransaction = false;
		}

		public BaseTest(bool withTransaction)
		{
			this.withTransaction = withTransaction;
		}

		#endregion

		#region NUnit Methods

		[SetUp]
		public virtual void SetUp()
		{
			// Build the connection string
			StringBuilder connectionString = new StringBuilder();
			connectionString.AppendFormat(
				"Assembly={0};UserName={1};Password={2};Database={3}",
				ConfigurationSettings.AppSettings["Assembly"],
				ConfigurationSettings.AppSettings["UserName"],
				ConfigurationSettings.AppSettings["Password"],
				ConfigurationSettings.AppSettings["Database"]);

			StringBuilder connOptions = new StringBuilder();
			connOptions.AppendFormat(
				"Charset={0};Dialect={1};Server Type={2};Pooling=false",
				ConfigurationSettings.AppSettings["Charset"],
				ConfigurationSettings.AppSettings["Dialect"],
				ConfigurationSettings.AppSettings["ServerType"]);

			try
			{
				try
				{
					System.IO.File.Delete(ConfigurationSettings.AppSettings["DatabasePath"]);
				}
				catch
				{
				}
				System.IO.File.Copy(
					ConfigurationSettings.AppSettings["BaseDatabase"],
					ConfigurationSettings.AppSettings["DatabasePath"]);
			}
			catch
			{
			}

			this.connection						= new BdpConnection();
			this.connection.ConnectionString	= connectionString.ToString();
			this.connection.ConnectionOptions	= connOptions.ToString();
			this.connection.Open();

			CreateTables(connectionString.ToString());
			CreateProcedures(connectionString.ToString());
			CreateTriggers(connectionString.ToString());
			InsertTestData(connectionString.ToString());

			if (this.withTransaction)
			{
				this.transaction = this.connection.BeginTransaction();
			}
		}

		[TearDown]
		public virtual void TearDown()
		{			
			if (this.withTransaction)
			{
				try
				{
					transaction.Commit();
				}
				catch
				{
				}
			}
			connection.Close();
		}

		#endregion

		#region Database Creation Methods

		private static void CreateTables(string connectionString)
		{
			BdpConnection connection = new BdpConnection(connectionString);
			connection.Open();

			StringBuilder commandText = new StringBuilder();
			commandText.Append("DROP TABLE TEST");

			BdpCommand command = null;

			try
			{
				command = new BdpCommand(commandText.ToString(), connection);
				command.ExecuteNonQuery();
				command.Dispose();
			}
			catch
			{
			}

			commandText = new StringBuilder();

			// Table for general purpouse tests
			commandText.Append("CREATE TABLE TEST (");
			commandText.Append("INT_FIELD        INTEGER DEFAULT 0 NOT NULL PRIMARY KEY,");
			commandText.Append("CHAR_FIELD       CHAR(30),");
			commandText.Append("VARCHAR_FIELD    VARCHAR(100),");
			commandText.Append("BIGINT_FIELD     BIGINT,");
			commandText.Append("SMALLINT_FIELD   SMALLINT,");
			commandText.Append("DOUBLE_FIELD     DOUBLE PRECISION,");
			commandText.Append("FLOAT_FIELD		 FLOAT,");
			commandText.Append("NUMERIC_FIELD    NUMERIC(15,2),");
			commandText.Append("DECIMAL_FIELD    DECIMAL(15,2),");
			commandText.Append("DATE_FIELD       DATE,");
			commandText.Append("TIME_FIELD       TIME,");
			commandText.Append("TIMESTAMP_FIELD  TIMESTAMP,");
			commandText.Append("CLOB_FIELD       BLOB SUB_TYPE 1 SEGMENT SIZE 80,");
			commandText.Append("BLOB_FIELD       BLOB SUB_TYPE 0 SEGMENT SIZE 80,");
			commandText.Append("EXPR_FIELD       COMPUTED BY (smallint_field * 1000));");

			try
			{
				command = new BdpCommand(commandText.ToString(), connection);
				command.ExecuteNonQuery();
				command.Dispose();
			}
			catch
			{
			}

			connection.Close();
		}

		private static void CreateProcedures(string connectionString)
		{
			BdpConnection connection = new BdpConnection(connectionString);
			connection.Open();

			BdpCommand command = null;

			StringBuilder commandText = new StringBuilder();

			// SELECT_DATA
			commandText = new StringBuilder();

			commandText.Append("DROP PROCEDURE SELECT_DATA");

			try
			{
				command = new BdpCommand(commandText.ToString(), connection);
				command.ExecuteNonQuery();
				command.Dispose();
			}
			catch
			{
			}

			commandText = new StringBuilder();

			commandText.Append("CREATE PROCEDURE SELECT_DATA  \r\n");
			commandText.Append("RETURNS ( \r\n");
			commandText.Append("INT_FIELD INTEGER, \r\n");
			commandText.Append("VARCHAR_FIELD VARCHAR(100), \r\n");
			commandText.Append("DECIMAL_FIELD DECIMAL(15,2)) \r\n");
			commandText.Append("AS \r\n");
			commandText.Append("begin \r\n");
			commandText.Append("FOR SELECT INT_FIELD, VARCHAR_FIELD, DECIMAL_FIELD FROM TEST INTO :INT_FIELD, :VARCHAR_FIELD, :DECIMAL_FIELD \r\n");
			commandText.Append("DO \r\n");
			commandText.Append("SUSPEND; \r\n");
			commandText.Append("end;");

			try
			{
				command = new BdpCommand(commandText.ToString(), connection);
				command.ExecuteNonQuery();
				command.Dispose();
			}
			catch
			{
			}

			// GETRECORDCOUNT
			commandText = new StringBuilder();

			commandText.Append("DROP PROCEDURE GETRECORDCOUNT");

			try
			{
				command = new BdpCommand(commandText.ToString(), connection);
				command.ExecuteNonQuery();
				command.Dispose();
			}
			catch
			{
			}

			commandText = new StringBuilder();

			commandText.Append("CREATE PROCEDURE GETRECORDCOUNT \r\n");
			commandText.Append("RETURNS ( \r\n");
			commandText.Append("RECCOUNT SMALLINT) \r\n");
			commandText.Append("AS \r\n");
			commandText.Append("begin \r\n");
			commandText.Append("for select count(*) from test into :reccount \r\n");
			commandText.Append("do \r\n");
			commandText.Append("suspend; \r\n");
			commandText.Append("end\r\n");

			command = new BdpCommand(commandText.ToString(), connection);
			command.ExecuteNonQuery();
			command.Dispose();

			// GETVARCHARFIELD
			commandText = new StringBuilder();

			commandText.Append("DROP PROCEDURE GETVARCHARFIELD");

			try
			{
				command = new BdpCommand(commandText.ToString(), connection);
				command.ExecuteNonQuery();
				command.Dispose();
			}
			catch
			{
			}

			commandText = new StringBuilder();

			commandText.Append("CREATE PROCEDURE GETVARCHARFIELD (\r\n");
			commandText.Append("ID INTEGER)\r\n");
			commandText.Append("RETURNS (\r\n");
			commandText.Append("VARCHAR_FIELD VARCHAR(100))\r\n");
			commandText.Append("AS\r\n");
			commandText.Append("begin\r\n");
			commandText.Append("for select varchar_field from test where int_field = :id into :varchar_field\r\n");
			commandText.Append("do\r\n");
			commandText.Append("suspend;\r\n");
			commandText.Append("end\r\n");

			command = new BdpCommand(commandText.ToString(), connection);
			command.ExecuteNonQuery();
			command.Dispose();

			// GETASCIIBLOB
			commandText = new StringBuilder();

			commandText.Append("DROP PROCEDURE GETASCIIBLOB");

			try
			{
				command = new BdpCommand(commandText.ToString(), connection);
				command.ExecuteNonQuery();
				command.Dispose();
			}
			catch
			{
			}

			commandText = new StringBuilder();

			commandText.Append("CREATE PROCEDURE GETASCIIBLOB (\r\n");
			commandText.Append("ID INTEGER)\r\n");
			commandText.Append("RETURNS (\r\n");
			commandText.Append("ASCII_BLOB BLOB SUB_TYPE 1)\r\n");
			commandText.Append("AS\r\n");
			commandText.Append("begin\r\n");
			commandText.Append("for select clob_field from test where int_field = :id into :ascii_blob\r\n");
			commandText.Append("do\r\n");
			commandText.Append("suspend;\r\n");
			commandText.Append("end\r\n");

			try
			{
				command = new BdpCommand(commandText.ToString(), connection);
				command.ExecuteNonQuery();
				command.Dispose();
			}
			catch
			{
			}

			connection.Close();
		}

		private static void CreateTriggers(string connectionString)
		{
			BdpConnection connection = new BdpConnection(connectionString);
			connection.Open();

			StringBuilder commandText = new StringBuilder();

			BdpCommand command = null;

			// new_row
			commandText = new StringBuilder();

			commandText.Append("DROP TRIGGER new_row");

			try
			{
				command = new BdpCommand(commandText.ToString(), connection);
				command.ExecuteNonQuery();
				command.Dispose();
			}
			catch
			{
			}

			commandText = new StringBuilder();

			commandText.Append("CREATE TRIGGER new_row FOR test ACTIVE\r\n");
			commandText.Append("AFTER INSERT POSITION 0\r\n");
			commandText.Append("AS\r\n");
			commandText.Append("BEGIN\r\n");
			commandText.Append("POST_EVENT 'new row';\r\n");
			commandText.Append("END");

			try
			{
				command = new BdpCommand(commandText.ToString(), connection);
				command.ExecuteNonQuery();
				command.Dispose();
			}
			catch
			{
			}

			// update_row

			commandText = new StringBuilder();

			commandText.Append("DROP TRIGGER update_row");

			try
			{
				command = new BdpCommand(commandText.ToString(), connection);
				command.ExecuteNonQuery();
				command.Dispose();
			}
			catch
			{
			}

			commandText = new StringBuilder();

			commandText.Append("CREATE TRIGGER update_row FOR test ACTIVE\r\n");
			commandText.Append("AFTER UPDATE POSITION 0\r\n");
			commandText.Append("AS\r\n");
			commandText.Append("BEGIN\r\n");
			commandText.Append("POST_EVENT 'updated row';\r\n");
			commandText.Append("END");

			try
			{
				command = new BdpCommand(commandText.ToString(), connection);
				command.ExecuteNonQuery();
				command.Dispose();
			}
			catch
			{
			}

			connection.Close();
		}

		private static void InsertTestData(string connectionString)
		{
			BdpConnection connection = new BdpConnection(connectionString);
			connection.Open();

			StringBuilder commandText = new StringBuilder();

			commandText.Append("DELETE FROM TEST");

			BdpCommand command = new BdpCommand(commandText.ToString(), connection);
			command.ExecuteNonQuery();
			command.Dispose();

			commandText = new StringBuilder();

			commandText.Append("insert into test (int_field, char_field, varchar_field, bigint_field, smallint_field, float_field, double_field, numeric_field, date_field, time_field, timestamp_field, clob_field, blob_field)");
			commandText.Append(" values(?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)");

			BdpTransaction transaction = connection.BeginTransaction();
			command = new BdpCommand(commandText.ToString(), connection, transaction);

			try
			{
				// Add command parameters
				command.Parameters.Add("@int_field", BdpType.Int32);
				command.Parameters.Add("@char_field", BdpType.String, BdpType.stFixed);
				command.Parameters.Add("@varchar_field", BdpType.String);
				command.Parameters.Add("@bigint_field", BdpType.Int64);
				command.Parameters.Add("@smallint_field", BdpType.Int16);
				command.Parameters.Add("@float_field", BdpType.Float);
				command.Parameters.Add("@double_field", BdpType.Double);
				command.Parameters.Add("@numeric_field", BdpType.Decimal);
				command.Parameters.Add("@date_field", BdpType.Date);
				command.Parameters.Add("@time_Field", BdpType.Time);
				command.Parameters.Add("@timestamp_field", BdpType.DateTime);
				command.Parameters.Add("@clob_field", BdpType.Blob, BdpType.stHMemo);
				command.Parameters.Add("@blob_field", BdpType.Blob, BdpType.stHBinary);

				command.Prepare();

				for (int i = 0; i < 100; i++)
				{
					command.Parameters["@int_field"].Value = i;
					command.Parameters["@char_field"].Value = "IRow " + i.ToString();
					command.Parameters["@varchar_field"].Value = "IRow Number " + i.ToString();
					command.Parameters["@bigint_field"].Value = i;
					command.Parameters["@smallint_field"].Value = i;
					command.Parameters["@float_field"].Value = (float)(i + 10) / 5;
					command.Parameters["@double_field"].Value = Math.Log(i, 10);
					command.Parameters["@numeric_field"].Value = (decimal)(i + 10) / 5;
					command.Parameters["@date_field"].Value = DateTime.Now;
					command.Parameters["@time_field"].Value = DateTime.Now;
					command.Parameters["@timestamp_field"].Value = DateTime.Now;
					command.Parameters["@clob_field"].Value = "IRow Number " + i.ToString();
					command.Parameters["@blob_field"].Value = Encoding.Default.GetBytes("IRow Number " + i.ToString());

					command.ExecuteNonQuery();
				}

				// Commit transaction
				transaction.Commit();
			}
			catch (Exception ex)
			{
				transaction.Rollback();
				throw ex;
			}
			finally
			{
				command.Dispose();
				connection.Close();
			}
		}

		#endregion

		#region Methods

		public int GetId()
		{
			RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

			byte[] buffer = new byte[4];
			
			rng.GetBytes(buffer);

			return BitConverter.ToInt32(buffer, 0);
		}

		#endregion
	}
}
