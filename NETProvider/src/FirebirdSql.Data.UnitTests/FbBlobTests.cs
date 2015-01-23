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
 *  Copyright (c) 2002, 2007 Carlos Guzman Alvarez
 *  All Rights Reserved.
 *   
 *  Contributors:
 *    Jiri Cincura (jiri@cincura.net)
 */

using System;
using System.Collections;
using System.Security.Cryptography;

using FirebirdSql.Data.FirebirdClient;
using NUnit.Framework;

namespace FirebirdSql.Data.UnitTests
{
	[TestFixture]
	public class FbBlobTests : TestsBase
	{
		#region Constructors

		public FbBlobTests()
			: base(false)
		{
		}

		#endregion

		#region Unit Tests

		[Test]
		public void BinaryBlobTest()
		{
			int id_value = GetId();

			string selectText = "SELECT blob_field FROM TEST WHERE int_field = " + id_value.ToString();
			string insertText = "INSERT INTO TEST (int_field, blob_field) values(@int_field, @blob_field)";

			Console.WriteLine("\r\n\r\nBinary Blob Test");

			Console.WriteLine("Generating an array of temp data");
			// Generate an array of temp data
			byte[] insert_values = new byte[100000 * 4];
			RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
			rng.GetBytes(insert_values);

			Console.WriteLine("Executing insert command");

			// Execute insert command
			FbTransaction transaction = Connection.BeginTransaction();

			FbCommand insert = new FbCommand(insertText, Connection, transaction);
			insert.Parameters.Add("@int_field", FbDbType.Integer).Value = id_value;
			insert.Parameters.Add("@blob_field", FbDbType.Binary).Value = insert_values;
			insert.ExecuteNonQuery();

			transaction.Commit();

			Console.WriteLine("Checking inserted values");

			// Check that inserted values are correct
			FbCommand select = new FbCommand(selectText, Connection);
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
		public void ReaderGetBytes()
		{
			int id_value = GetId();

			string selectText = "SELECT blob_field FROM TEST WHERE int_field = " + id_value.ToString();
			string insertText = "INSERT INTO TEST (int_field, blob_field) values(@int_field, @blob_field)";

			Console.WriteLine("\r\n\r\nFbDataReader.GetBytes with Binary Blob Test");

			Console.WriteLine("Generating an array of temp data");
			// Generate an array of temp data
			byte[] insert_values = new byte[100000 * 4];
			RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
			rng.GetBytes(insert_values);

			Console.WriteLine("Executing insert command");

			// Execute insert command
			FbTransaction transaction = Connection.BeginTransaction();

			FbCommand insert = new FbCommand(insertText, Connection, transaction);
			insert.Parameters.Add("@int_field", FbDbType.Integer).Value = id_value;
			insert.Parameters.Add("@blob_field", FbDbType.Binary).Value = insert_values;
			insert.ExecuteNonQuery();

			transaction.Commit();

			Console.WriteLine("Checking inserted values");

			// Check that inserted values are correct
			FbCommand select = new FbCommand(selectText, Connection);

			byte[] select_values = new byte[100000 * 4];

			using (FbDataReader reader = select.ExecuteReader())
			{
				int index = 0;
				int segmentSize = 1000;
				while (reader.Read())
				{
					while (index < 400000)
					{
						reader.GetBytes(0, index, select_values, index, segmentSize);

						index += segmentSize;
					}
				}
			}

			for (int i = 0; i < insert_values.Length; i++)
			{
				if (insert_values[i] != select_values[i])
				{
					throw new Exception("differences at index " + i.ToString());
				}
			}
		}

		#endregion
	}
}
