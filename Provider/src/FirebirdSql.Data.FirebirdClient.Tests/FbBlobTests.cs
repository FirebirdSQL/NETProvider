/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/blob/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$Authors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;
using System.Security.Cryptography;
using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.TestsBase;
using NUnit.Framework;

namespace FirebirdSql.Data.FirebirdClient.Tests
{
	[FbTestFixture(FbServerType.Default, false)]
	[FbTestFixture(FbServerType.Default, true)]
	[FbTestFixture(FbServerType.Embedded, default)]
	public class FbBlobTests : FbTestsBase
	{
		#region Constructors

		public FbBlobTests(FbServerType serverType, bool compression)
			: base(serverType, compression)
		{ }

		#endregion

		#region Unit Tests

		[Test]
		public void BinaryBlobTest()
		{
			var id_value = GetId();

			var selectText = "SELECT blob_field FROM TEST WHERE int_field = " + id_value.ToString();
			var insertText = "INSERT INTO TEST (int_field, blob_field) values(@int_field, @blob_field)";

			// Generate an array of temp data
			var insert_values = new byte[100000 * 4];
			var rng = new RNGCryptoServiceProvider();
			rng.GetBytes(insert_values);

			// Execute insert command
			var transaction = Connection.BeginTransaction();

			var insert = new FbCommand(insertText, Connection, transaction);
			insert.Parameters.Add("@int_field", FbDbType.Integer).Value = id_value;
			insert.Parameters.Add("@blob_field", FbDbType.Binary).Value = insert_values;
			insert.ExecuteNonQuery();

			transaction.Commit();

			// Check that inserted values are correct
			var select = new FbCommand(selectText, Connection);
			var select_values = (byte[])select.ExecuteScalar();

			for (var i = 0; i < insert_values.Length; i++)
			{
				if (insert_values[i] != select_values[i])
				{
					throw new Exception("differences at index " + i.ToString());
				}
			}
		}

		[Test]
		public void ReaderGetBytes()
		{
			var id_value = GetId();

			var selectText = "SELECT blob_field FROM TEST WHERE int_field = " + id_value.ToString();
			var insertText = "INSERT INTO TEST (int_field, blob_field) values(@int_field, @blob_field)";

			// Generate an array of temp data
			var insert_values = new byte[100000 * 4];
			var rng = new RNGCryptoServiceProvider();
			rng.GetBytes(insert_values);

			// Execute insert command
			var transaction = Connection.BeginTransaction();

			var insert = new FbCommand(insertText, Connection, transaction);
			insert.Parameters.Add("@int_field", FbDbType.Integer).Value = id_value;
			insert.Parameters.Add("@blob_field", FbDbType.Binary).Value = insert_values;
			insert.ExecuteNonQuery();

			transaction.Commit();

			// Check that inserted values are correct
			var select = new FbCommand(selectText, Connection);

			var select_values = new byte[100000 * 4];

			using (var reader = select.ExecuteReader())
			{
				var index = 0;
				var segmentSize = 1000;
				while (reader.Read())
				{
					while (index < 400000)
					{
						reader.GetBytes(0, index, select_values, index, segmentSize);

						index += segmentSize;
					}
				}
			}

			for (var i = 0; i < insert_values.Length; i++)
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
