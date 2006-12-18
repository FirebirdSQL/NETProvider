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
using System.Data;
using System.Data.Common;
using System.Text;
using System.Configuration;
using System.Collections;
using System.Security.Cryptography;

using FirebirdSql.Data.Common;
using FirebirdSql.Data.Embedded;
using FirebirdSql.Data.Gds;

namespace FirebirdSql.Data.Client.UnitTest
{
	public class BaseTest
	{
		#region Static Fields

		private	static FactoryBase factory;

		#endregion

		#region Fields

		private IDbAttachment	attachment;
		
		#endregion

		#region Properties

		public IDbAttachment Attachment
		{
			get { return this.attachment; }
		}

		#endregion

		#region Static Properties

		public static FactoryBase Factory
		{
			get 
			{ 
				if (factory == null)
				{
					switch (ConfigurationSettings.AppSettings["ServerType"])
					{
						case "0":
							factory = GdsFactory.Instance;
							break;

						case "1":
							factory = FesFactory.Instance;
							break;
					}					
				}

				return factory;
			}
		}

		#endregion

		#region Constructors
				
		public BaseTest()
		{
		}

		#endregion

		#region NUnit Methods

		[SetUp]
		public virtual void SetUp()
		{
			AttachmentParams p = new AttachmentParams();

			p.DataSource	= ConfigurationSettings.AppSettings["DataSource"];
			p.Database		= ConfigurationSettings.AppSettings["Database"];
			p.Port			= Convert.ToInt32(ConfigurationSettings.AppSettings["Port"]);
			p.Dialect		= Convert.ToByte(ConfigurationSettings.AppSettings["Dialect"]);
			p.Charset		= Charset.SupportedCharsets[ConfigurationSettings.AppSettings["Charset"]];
			p.UserName		= ConfigurationSettings.AppSettings["User"];
			p.UserPassword	= ConfigurationSettings.AppSettings["Password"];

			BaseTest.createDatabase(p);
			BaseTest.createTables(p);
			BaseTest.createProcedures(p);
			BaseTest.createTriggers(p);
			BaseTest.insertTestData(p);
						
			this.attachment = BaseTest.Factory.CreateDbConnection(p);
			this.attachment.Attach();
		}

		[TearDown]
		public virtual void TearDown()
		{			
			this.attachment.Detach();
		}

		#endregion

		#region Database Creation Methods

		private static void createDatabase(AttachmentParams p)
		{
			IDbAttachment attachment = BaseTest.Factory.CreateDbConnection(p);

			// DPB configuration
			DpbBuffer dpb = new DpbBuffer();
				
			// Dpb version
			dpb.Append(IscCodes.isc_dpb_version1);

			// Dummy packet interval
			dpb.Append(IscCodes.isc_dpb_dummy_packet_interval, 
				new byte[] {120, 10, 0, 0});

			// User name
			dpb.Append(IscCodes.isc_dpb_user_name, p.UserName);

			// User password
			dpb.Append(IscCodes.isc_dpb_password, p.UserPassword);

			// Database dialect
			dpb.Append(IscCodes.isc_dpb_sql_dialect, 
				new byte[] {p.Dialect, 0, 0, 0});

			// Page Size
			if (ConfigurationSettings.AppSettings["PageSize"] != null)
			{
				dpb.Append(IscCodes.isc_dpb_page_size, 
					Convert.ToInt32(ConfigurationSettings.AppSettings["PageSize"]));
			}

			// Forced writes
			if (ConfigurationSettings.AppSettings["ForcedWrite"] != null)
			{
				dpb.Append(IscCodes.isc_dpb_force_write, 
					(short)(Boolean.Parse(ConfigurationSettings.AppSettings["ForcedWrite"]) ? 1 : 0));
			}

			// Character set
			dpb.Append(IscCodes.isc_dpb_set_db_charset, p.Charset.Name);

			// Create the new database
			attachment.CreateDatabase(p, dpb);
		}

		private static void createTables(AttachmentParams p)
		{
			IDbAttachment attachment = BaseTest.Factory.CreateDbConnection(p);
			attachment.Attach();

			ITransaction transaction = attachment.BeginTransaction(
				BaseTest.BuildTpb(IsolationLevel.ReadCommitted));

			StringBuilder commandText = new StringBuilder();

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
			commandText.Append("IARRAY_FIELD     INTEGER [1:4],");
			commandText.Append("SARRAY_FIELD     SMALLINT [1:5],");
			commandText.Append("LARRAY_FIELD     BIGINT [1:6],");
			commandText.Append("FARRAY_FIELD     FLOAT [1:4],");
			commandText.Append("BARRAY_FIELD     DOUBLE PRECISION [1:4],");
			commandText.Append("NARRAY_FIELD     NUMERIC(10,6) [1:4],");
			commandText.Append("DARRAY_FIELD     DATE [1:4],");
			commandText.Append("TARRAY_FIELD     TIME [1:4],");
			commandText.Append("TSARRAY_FIELD    TIMESTAMP [1:4],");
			commandText.Append("CARRAY_FIELD     CHAR(21) [1:4],");
			commandText.Append("VARRAY_FIELD     VARCHAR(30) [1:4],");
			commandText.Append("BIG_ARRAY        INTEGER [1:32767],");
			commandText.Append("EXPR_FIELD       COMPUTED BY (smallint_field * 1000),");
			commandText.Append("CS_FIELD         CHAR(1) CHARACTER SET UNICODE_FSS,");
			commandText.Append("UCCHAR_ARRAY     CHAR(10) [1:10] CHARACTER SET UNICODE_FSS);");
   
			StatementBase stmt = attachment.CreateStatement(transaction);

			stmt.Prepare(commandText.ToString());
			stmt.Execute();
			stmt.Release();

			transaction.Commit();			
			attachment.Detach();
		}

		private static void createProcedures(AttachmentParams p)
		{
			IDbAttachment attachment = BaseTest.Factory.CreateDbConnection(p);
			attachment.Attach();

			ITransaction transaction = attachment.BeginTransaction(BaseTest.BuildTpb(IsolationLevel.ReadCommitted));

			StringBuilder commandText = new StringBuilder();

			// SELECT_DATA
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

			StatementBase stmt = attachment.CreateStatement(transaction);
				
			stmt.Prepare(commandText.ToString());
			stmt.Execute();
			stmt.Release();

			// GETRECORDCOUNT
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

			stmt = attachment.CreateStatement(transaction);
			
			stmt.Prepare(commandText.ToString());
			stmt.Execute();
			stmt.Release();

			// GETVARCHARFIELD
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

			stmt = attachment.CreateStatement(transaction);

			stmt.Prepare(commandText.ToString());
			stmt.Execute();
			stmt.Release();
						
			// DELETERECORD
			commandText = new StringBuilder();

			commandText.Append("CREATE PROCEDURE DELETERECORD (\r\n");
			commandText.Append("ID INTEGER)\r\n");
			commandText.Append("AS\r\n");
			commandText.Append("begin\r\n");
			commandText.Append("delete from test where int_field = :id;\r\n");
			commandText.Append("end\r\n");

			stmt = attachment.CreateStatement(transaction);
				
			stmt.Prepare(commandText.ToString());
			stmt.Execute();
			stmt.Release();

			transaction.Commit();
			attachment.Detach();
		}

		private static void createTriggers(AttachmentParams p)
		{
			IDbAttachment attachment = BaseTest.Factory.CreateDbConnection(p);
			attachment.Attach();

			ITransaction transaction = attachment.BeginTransaction(BaseTest.BuildTpb(IsolationLevel.ReadCommitted));

			StringBuilder commandText = new StringBuilder();

			// new_row
			commandText = new StringBuilder();

			commandText.Append("CREATE TRIGGER new_row FOR test ACTIVE\r\n");
			commandText.Append("AFTER INSERT POSITION 0\r\n");
			commandText.Append("AS\r\n");
			commandText.Append("BEGIN\r\n");
			commandText.Append("POST_EVENT 'new row';\r\n");
			commandText.Append("END");

			StatementBase stmt = attachment.CreateStatement(transaction);
				
			stmt.Prepare(commandText.ToString());
			stmt.Execute();
			stmt.Release();

			// update_row
			commandText = new StringBuilder();

			commandText.Append("CREATE TRIGGER update_row FOR test ACTIVE\r\n");
			commandText.Append("AFTER UPDATE POSITION 0\r\n");
			commandText.Append("AS\r\n");
			commandText.Append("BEGIN\r\n");
			commandText.Append("POST_EVENT 'updated row';\r\n");
			commandText.Append("END");

			stmt = attachment.CreateStatement(transaction);
				
			stmt.Prepare(commandText.ToString());
			stmt.Execute();
			stmt.Release();

			transaction.Commit();
			attachment.Detach();
		}

		private static void insertTestData(AttachmentParams p)
		{
			IDbAttachment attachment = BaseTest.Factory.CreateDbConnection(p);
			attachment.Attach();

			StringBuilder commandText = new StringBuilder();

			commandText.Append("insert into test (int_field, char_field, varchar_field, bigint_field, smallint_field, float_field, double_field, numeric_field, date_field, time_field, timestamp_field, clob_field, blob_field)");
			commandText.Append(" values(?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)");

			ITransaction transaction = attachment.BeginTransaction(BaseTest.BuildTpb(IsolationLevel.ReadCommitted));

			StatementBase stmt = attachment.CreateStatement(transaction);

			try
			{
				stmt.Prepare(commandText.ToString());
				stmt.DescribeParameters();

				for (int i = 0; i < 100; i++)
				{
					BlobBase ascii = stmt.CreateBlob(0);
					ascii.Write("IRow Number" + i.ToString());

					BlobBase binary = stmt.CreateBlob(0);
					binary.Write(Encoding.Default.GetBytes("IRow Number" + i.ToString()));

					stmt.Parameters[0].Value	= i;
					stmt.Parameters[1].Value	= "IRow " + i.ToString();
					stmt.Parameters[2].Value	= "IRow Number" + i.ToString();
					stmt.Parameters[3].Value	= i;
					stmt.Parameters[4].Value	= i;
					stmt.Parameters[5].Value	= (float)(i + 10)/5;
					stmt.Parameters[6].Value	= Math.Log(i, 10);
					stmt.Parameters[7].Value	= (decimal)(i + 10)/5;
					stmt.Parameters[8].Value	= DateTime.Now;
					stmt.Parameters[9].Value	= DateTime.Now;
					stmt.Parameters[10].Value	= DateTime.Now;
					stmt.Parameters[11].Value	= ascii.Id;
					stmt.Parameters[12].Value	= binary.Id;

					stmt.Execute();
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
				stmt.Release();
				attachment.Detach();
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

		public static DpbBuffer BuildTpb()
		{
			return BaseTest.BuildTpb(IsolationLevel.ReadCommitted);
		}

		public static DpbBuffer BuildTpb(IsolationLevel isolationLevel)
		{
			DpbBuffer tpb = new DpbBuffer();

			tpb.Append(IscCodes.isc_tpb_version3);
			tpb.Append(IscCodes.isc_tpb_write);
			tpb.Append(IscCodes.isc_tpb_wait);

			/* Isolation level */
			switch (isolationLevel)
			{
				case IsolationLevel.Serializable:
					tpb.Append(IscCodes.isc_tpb_consistency);
					break;

				case IsolationLevel.RepeatableRead:			
					tpb.Append(IscCodes.isc_tpb_concurrency);
					break;

				case IsolationLevel.ReadUncommitted:
					tpb.Append(IscCodes.isc_tpb_read_committed);
					tpb.Append(IscCodes.isc_tpb_rec_version);
					break;

				case IsolationLevel.ReadCommitted:
				default:					
					tpb.Append(IscCodes.isc_tpb_read_committed);
					tpb.Append(IscCodes.isc_tpb_no_rec_version);
					break;
			}

			return tpb;
		}

		#endregion
	}
}
