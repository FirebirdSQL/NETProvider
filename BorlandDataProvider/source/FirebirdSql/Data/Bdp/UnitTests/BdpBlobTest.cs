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
 *  Copyright (c) 2002, 2004 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using System;
using System.Collections;
using System.Security.Cryptography;

using NUnit.Framework;
using Borland.Data.Provider;
using Borland.Data.Common;

namespace FirebirdSql.Data.Bdp.Tests
{
    [TestFixture]
	public class BdpBlobTest : BaseTest 
	{
        public BdpBlobTest() : base(false)
        {		
		}
		
		[Test]
		public void BinaryBlobTest()
		{
			int id_value = this.GetId();
			
			string selectText = "SELECT blob_field FROM TEST WHERE int_field = " + id_value.ToString();
			string insertText = "INSERT INTO TEST (int_field, blob_field) values(?, ?)";
			
			Console.WriteLine("\r\n\r\nBinary Blob Test");
			
			Console.WriteLine("Generating an array of temp data");
			// Generate an array of temp data
			byte[] insert_values = new byte[100000*4];
			RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
			rng.GetBytes(insert_values);
			
			Console.WriteLine("Executing insert command");

			// Execute insert command
			BdpTransaction transaction = Connection.BeginTransaction();

            BdpCommand insert = new BdpCommand(insertText, Connection, transaction);
            insert.Parameters.Add("@int_field", BdpType.Int32).Value = id_value;
            insert.Parameters.Add("@blob_field", BdpType.Blob, BdpType.stHBinary).Value = insert_values;
            insert.ExecuteNonQuery();

			transaction.Commit();

			Console.WriteLine("Checking inserted values");

			// Check that inserted values are correct
            BdpCommand select = new BdpCommand(selectText, Connection);
            byte[] select_values = (byte[])select.ExecuteScalar();			

			for (int i = 0; i < insert_values.Length; i++)
			{
				if (insert_values[i] != select_values[i])
				{
					throw new Exception("differences at index " + i.ToString());
				}
			}

			Console.WriteLine("Finishing test");
		}

        [Test]
        public void TextBlobTest()
        {
            int    id_value = this.GetId();
            string text     = "Borland Data Provider Clob Field test";

            string selectText = "SELECT clob_field FROM TEST WHERE int_field = " + id_value.ToString();
            string insertText = "INSERT INTO TEST (int_field, clob_field) values(?, ?)";

            Console.WriteLine("\r\n\r\nBinary Blob Test");

            Console.WriteLine("Executing insert command");

            // Execute insert command
            BdpTransaction transaction = Connection.BeginTransaction();

            BdpCommand insert = new BdpCommand(insertText, Connection, transaction);
            insert.Parameters.Add("@int_field", BdpType.Int32).Value = id_value;
            insert.Parameters.Add("@clob_field", BdpType.Blob, BdpType.stHMemo).Value = text;
            insert.ExecuteNonQuery();

            transaction.Commit();

            Console.WriteLine("Checking inserted values");

            // Check that inserted values are correct
            BdpCommand select = new BdpCommand(selectText, Connection);
            string result = new String((char[])select.ExecuteScalar());

            Assert.AreEqual(text, result);

            Console.WriteLine("Finishing test");
        }
    }
}
