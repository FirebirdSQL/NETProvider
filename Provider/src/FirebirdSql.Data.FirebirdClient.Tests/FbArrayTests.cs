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
using System.Linq;
using System.Security.Cryptography;
using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.TestsBase;
using NUnit.Framework;

namespace FirebirdSql.Data.FirebirdClient.Tests
{
	[FbTestFixture(FbServerType.Default, false)]
	[FbTestFixture(FbServerType.Default, true)]
	[FbTestFixture(FbServerType.Embedded, default)]
	public class FbArrayTests : FbTestsBase
	{
		#region Constructors

		public FbArrayTests(FbServerType serverType, bool compression)
			: base(serverType, compression)
		{ }

		#endregion

		#region Unit Tests

		[Test]
		public void IntegerArrayTest()
		{
			Transaction = Connection.BeginTransaction();

			var id_value = GetId();

			var selectText = "SELECT	iarray_field FROM TEST WHERE int_field = " + id_value.ToString();
			var insertText = "INSERT	INTO TEST (int_field, iarray_field)	values(@int_field, @array_field)";

			var insert_values = new int[4];

			insert_values[0] = 10;
			insert_values[1] = 20;
			insert_values[2] = 30;
			insert_values[3] = 40;

			var insert = new FbCommand(insertText, Connection, Transaction);
			insert.Parameters.Add("@int_field", FbDbType.Integer).Value = id_value;
			insert.Parameters.Add("@array_field", FbDbType.Array).Value = insert_values;
			insert.ExecuteNonQuery();
			insert.Dispose();

			Transaction.Commit();

			var select = new FbCommand(selectText, Connection);
			var reader = select.ExecuteReader();
			if (reader.Read())
			{
				if (!reader.IsDBNull(0))
				{
					var select_values = new int[insert_values.Length];
					Array.Copy((Array)reader.GetValue(0), select_values, select_values.Length);
					CollectionAssert.AreEqual(insert_values, select_values);
				}
			}
			reader.Close();
			select.Dispose();
		}

		[Test]
		public void ShortArrayTest()
		{
			Transaction = Connection.BeginTransaction();

			var id_value = GetId();

			var selectText = "SELECT	sarray_field FROM TEST WHERE int_field = " + id_value.ToString();
			var insertText = "INSERT	INTO TEST (int_field, sarray_field)	values(@int_field, @array_field)";

			var insert_values = new short[4];

			insert_values[0] = 50;
			insert_values[1] = 60;
			insert_values[2] = 70;
			insert_values[3] = 80;

			var insert = new FbCommand(insertText, Connection, Transaction);
			insert.Parameters.Add("@int_field", FbDbType.Integer).Value = id_value;
			insert.Parameters.Add("@array_field", FbDbType.Array).Value = insert_values;
			insert.ExecuteNonQuery();
			insert.Dispose();

			Transaction.Commit();

			var select = new FbCommand(selectText, Connection);
			var reader = select.ExecuteReader();
			if (reader.Read())
			{
				if (!reader.IsDBNull(0))
				{
					var select_values = new short[insert_values.Length];
					Array.Copy((Array)reader.GetValue(0), select_values, select_values.Length);
					CollectionAssert.AreEqual(insert_values, select_values);
				}
			}
			reader.Close();
			select.Dispose();
		}

		[Test]
		public void BigIntArrayTest()
		{
			Transaction = Connection.BeginTransaction();

			var id_value = GetId();

			var selectText = "SELECT	larray_field FROM TEST WHERE int_field = " + id_value.ToString();
			var insertText = "INSERT	INTO TEST (int_field, larray_field)	values(@int_field, @array_field)";

			var insert_values = new long[4];

			insert_values[0] = 50;
			insert_values[1] = 60;
			insert_values[2] = 70;
			insert_values[3] = 80;

			var insert = new FbCommand(insertText, Connection, Transaction);
			insert.Parameters.Add("@int_field", FbDbType.Integer).Value = id_value;
			insert.Parameters.Add("@array_field", FbDbType.Array).Value = insert_values;
			insert.ExecuteNonQuery();
			insert.Dispose();

			Transaction.Commit();

			var select = new FbCommand(selectText, Connection);
			var reader = select.ExecuteReader();
			if (reader.Read())
			{
				if (!reader.IsDBNull(0))
				{
					var select_values = new long[insert_values.Length];
					Array.Copy((Array)reader.GetValue(0), select_values, select_values.Length);
					CollectionAssert.AreEqual(insert_values, select_values);
				}
			}
			reader.Close();
			select.Dispose();
		}

		[Test]
		public void FloatArrayTest()
		{
			Transaction = Connection.BeginTransaction();

			var id_value = GetId();

			var selectText = "SELECT	farray_field FROM TEST WHERE int_field = " + id_value.ToString();
			var insertText = "INSERT	INTO TEST (int_field, farray_field)	values(@int_field, @array_field)";

			var insert_values = new float[4];

			insert_values[0] = 130.10F;
			insert_values[1] = 140.20F;
			insert_values[2] = 150.30F;
			insert_values[3] = 160.40F;

			var insert = new FbCommand(insertText, Connection, Transaction);
			insert.Parameters.Add("@int_field", FbDbType.Integer).Value = id_value;
			insert.Parameters.Add("@array_field", FbDbType.Array).Value = insert_values;
			insert.ExecuteNonQuery();
			insert.Dispose();

			Transaction.Commit();

			var select = new FbCommand(selectText, Connection);
			var reader = select.ExecuteReader();
			if (reader.Read())
			{
				if (!reader.IsDBNull(0))
				{
					var select_values = new float[insert_values.Length];
					Array.Copy((Array)reader.GetValue(0), select_values, select_values.Length);
					CollectionAssert.AreEqual(insert_values, select_values);
				}
			}
			reader.Close();
			select.Dispose();
		}

		[Test]
		public void DoubleArrayTest()
		{
			Transaction = Connection.BeginTransaction();

			var id_value = GetId();

			var selectText = "SELECT	barray_field FROM TEST WHERE int_field = " + id_value.ToString();
			var insertText = "INSERT	INTO TEST (int_field, barray_field)	values(@int_field, @array_field)";

			var insert_values = new double[4];

			insert_values[0] = 170.10;
			insert_values[1] = 180.20;
			insert_values[2] = 190.30;
			insert_values[3] = 200.40;

			var insert = new FbCommand(insertText, Connection, Transaction);
			insert.Parameters.Add("@int_field", FbDbType.Integer).Value = id_value;
			insert.Parameters.Add("@array_field", FbDbType.Array).Value = insert_values;
			insert.ExecuteNonQuery();
			insert.Dispose();

			Transaction.Commit();

			var select = new FbCommand(selectText, Connection);
			var reader = select.ExecuteReader();
			if (reader.Read())
			{
				if (!reader.IsDBNull(0))
				{
					var select_values = new double[insert_values.Length];
					Array.Copy((Array)reader.GetValue(0), select_values, select_values.Length);
					CollectionAssert.AreEqual(insert_values, select_values);
				}
			}
			reader.Close();
			select.Dispose();
		}

		[Test]
		public void NumericArrayTest()
		{
			Transaction = Connection.BeginTransaction();

			var id_value = GetId();

			var selectText = "SELECT	narray_field FROM TEST WHERE int_field = " + id_value.ToString();
			var insertText = "INSERT	INTO TEST (int_field, narray_field)	values(@int_field, @array_field)";

			var insert_values = new decimal[4];

			insert_values[0] = 210.10M;
			insert_values[1] = 220.20M;
			insert_values[2] = 230.30M;
			insert_values[3] = 240.40M;

			var insert = new FbCommand(insertText, Connection, Transaction);
			insert.Parameters.Add("@int_field", FbDbType.Integer).Value = id_value;
			insert.Parameters.Add("@array_field", FbDbType.Array).Value = insert_values;
			insert.ExecuteNonQuery();
			insert.Dispose();

			Transaction.Commit();

			var select = new FbCommand(selectText, Connection);
			var reader = select.ExecuteReader();
			if (reader.Read())
			{
				if (!reader.IsDBNull(0))
				{
					var select_values = new decimal[insert_values.Length];
					Array.Copy((Array)reader.GetValue(0), select_values, select_values.Length);
					CollectionAssert.AreEqual(insert_values, select_values);
				}
			}
			reader.Close();
			select.Dispose();
		}

		[Test]
		public void DateArrayTest()
		{
			Transaction = Connection.BeginTransaction();

			var id_value = GetId();

			var selectText = "SELECT	darray_field FROM TEST WHERE int_field = " + id_value.ToString();
			var insertText = "INSERT	INTO TEST (int_field, darray_field)	values(@int_field, @array_field)";

			var insert_values = new DateTime[4];

			insert_values[0] = DateTime.Today.AddDays(10);
			insert_values[1] = DateTime.Today.AddDays(20);
			insert_values[2] = DateTime.Today.AddDays(30);
			insert_values[3] = DateTime.Today.AddDays(40);

			var insert = new FbCommand(insertText, Connection, Transaction);
			insert.Parameters.Add("@int_field", FbDbType.Integer).Value = id_value;
			insert.Parameters.Add("@array_field", FbDbType.Array).Value = insert_values;
			insert.ExecuteNonQuery();
			insert.Dispose();

			Transaction.Commit();

			var select = new FbCommand(selectText, Connection);
			var reader = select.ExecuteReader();
			if (reader.Read())
			{
				if (!reader.IsDBNull(0))
				{
					var select_values = new DateTime[insert_values.Length];
					Array.Copy((Array)reader.GetValue(0), select_values, select_values.Length);
					CollectionAssert.AreEqual(insert_values, select_values);
				}
			}
			reader.Close();
			select.Dispose();
		}

		[Test]
		public void TimeArrayTest()
		{
			Transaction = Connection.BeginTransaction();

			var id_value = GetId();

			var selectText = "SELECT	tarray_field FROM TEST WHERE int_field = " + id_value.ToString();
			var insertText = "INSERT	INTO TEST (int_field, tarray_field)	values(@int_field, @array_field)";

			var insert_values = new TimeSpan[4];

			insert_values[0] = new TimeSpan(3, 9, 10);
			insert_values[1] = new TimeSpan(4, 11, 12);
			insert_values[2] = new TimeSpan(6, 13, 14);
			insert_values[3] = new TimeSpan(8, 15, 16);

			var insert = new FbCommand(insertText, Connection, Transaction);
			insert.Parameters.Add("@int_field", FbDbType.Integer).Value = id_value;
			insert.Parameters.Add("@array_field", FbDbType.Array).Value = insert_values;
			insert.ExecuteNonQuery();
			insert.Dispose();

			Transaction.Commit();

			var select = new FbCommand(selectText, Connection);
			var reader = select.ExecuteReader();
			if (reader.Read())
			{
				if (!reader.IsDBNull(0))
				{
					var select_values = new TimeSpan[insert_values.Length];
					Array.Copy((Array)reader.GetValue(0), select_values, select_values.Length);
					CollectionAssert.AreEqual(insert_values, select_values);
				}
			}
			reader.Close();
			select.Dispose();
		}

		[Test]
		public void TimeStampArrayTest()
		{
			Transaction = Connection.BeginTransaction();

			var id_value = GetId();

			var selectText = "SELECT	tsarray_field FROM TEST	WHERE int_field	= " + id_value.ToString();
			var insertText = "INSERT	INTO TEST (int_field, tsarray_field) values(@int_field,	@array_field)";

			var insert_values = new DateTime[4];

			insert_values[0] = DateTime.Now.AddSeconds(10);
			insert_values[1] = DateTime.Now.AddSeconds(20);
			insert_values[2] = DateTime.Now.AddSeconds(30);
			insert_values[3] = DateTime.Now.AddSeconds(40);

			var insert = new FbCommand(insertText, Connection, Transaction);
			insert.Parameters.Add("@int_field", FbDbType.Integer).Value = id_value;
			insert.Parameters.Add("@array_field", FbDbType.Array).Value = insert_values;
			insert.ExecuteNonQuery();
			insert.Dispose();

			Transaction.Commit();

			var select = new FbCommand(selectText, Connection);
			var reader = select.ExecuteReader();
			if (reader.Read())
			{
				if (!reader.IsDBNull(0))
				{
					var select_values = new DateTime[insert_values.Length];
					Array.Copy((Array)reader.GetValue(0), select_values, select_values.Length);
					insert_values = insert_values.Select(x => new DateTime(x.Ticks / 1000 * 1000)).ToArray();
					CollectionAssert.AreEqual(insert_values, select_values);
				}
			}
			reader.Close();
			select.Dispose();
		}

		[Test]
		public void CharArrayTest()
		{
			Transaction = Connection.BeginTransaction();

			var id_value = GetId();

			var selectText = "SELECT	carray_field FROM TEST WHERE int_field = " + id_value.ToString();
			var insertText = "INSERT	INTO TEST (int_field, carray_field)	values(@int_field, @array_field)";

			var insert_values = new string[4];

			insert_values[0] = "abc";
			insert_values[1] = "abcdef";
			insert_values[2] = "abcdefghi";
			insert_values[3] = "abcdefghijkl";

			var insert = new FbCommand(insertText, Connection, Transaction);
			insert.Parameters.Add("@int_field", FbDbType.Integer).Value = id_value;
			insert.Parameters.Add("@array_field", FbDbType.Array).Value = insert_values;
			insert.ExecuteNonQuery();
			insert.Dispose();

			Transaction.Commit();

			var select = new FbCommand(selectText, Connection);
			var reader = select.ExecuteReader();
			if (reader.Read())
			{
				if (!reader.IsDBNull(0))
				{
					var select_values = new string[insert_values.Length];
					Array.Copy((Array)reader.GetValue(0), select_values, select_values.Length);
					select_values = select_values.Select(x => x.TrimEnd(' ')).ToArray();
					CollectionAssert.AreEqual(insert_values, select_values);
				}
			}
			reader.Close();
			select.Dispose();
		}

		[Test]
		public void VarCharArrayTest()
		{
			Transaction = Connection.BeginTransaction();

			var id_value = GetId();

			var selectText = "SELECT	varray_field FROM TEST WHERE int_field = " + id_value.ToString();
			var insertText = "INSERT	INTO TEST (int_field, varray_field)	values(@int_field, @array_field)";

			var insert_values = new string[4];

			insert_values[0] = "abc";
			insert_values[1] = "abcdef";
			insert_values[2] = "abcdefghi";
			insert_values[3] = "abcdefghijkl";

			var insert = new FbCommand(insertText, Connection, Transaction);
			insert.Parameters.Add("@int_field", FbDbType.Integer).Value = id_value;
			insert.Parameters.Add("@array_field", FbDbType.Array).Value = insert_values;
			insert.ExecuteNonQuery();
			insert.Dispose();

			Transaction.Commit();

			var select = new FbCommand(selectText, Connection);
			var reader = select.ExecuteReader();
			if (reader.Read())
			{
				if (!reader.IsDBNull(0))
				{
					var select_values = new string[insert_values.Length];
					Array.Copy((Array)reader.GetValue(0), select_values, select_values.Length);
					CollectionAssert.AreEqual(insert_values, select_values);
				}
			}
			reader.Close();
			select.Dispose();
		}

		[Test]
		public void IntegerArrayPartialUpdateTest()
		{
			var updateText = "update	TEST set iarray_field =	@array_field " +
								"WHERE int_field = 1";

			var new_values = new int[2];

			new_values[0] = 100;
			new_values[1] = 200;

			var update = new FbCommand(updateText, Connection);

			update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;

			update.ExecuteNonQuery();
			update.Dispose();
		}

		[Test]
		public void ShortArrayPartialUpdateTest()
		{
			var updateText = "update	TEST set sarray_field =	@array_field " +
								"WHERE int_field = 1";

			var new_values = new short[3];

			new_values[0] = 500;
			new_values[1] = 600;

			var update = new FbCommand(updateText, Connection);

			update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;

			update.ExecuteNonQuery();
			update.Dispose();
		}

		[Test]
		public void BigIntArrayPartialUpdateTest()
		{
			var updateText = "update	TEST set larray_field =	@array_field " +
								"WHERE int_field = 1";

			var new_values = new long[4];

			new_values[0] = 900;
			new_values[1] = 1000;
			new_values[2] = 1100;
			new_values[3] = 1200;

			var update = new FbCommand(updateText, Connection);

			update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;

			update.ExecuteNonQuery();
			update.Dispose();
		}

		[Test]
		public void FloatArrayPartialUpdateTest()
		{
			var updateText = "update	TEST set farray_field =	@array_field " +
								"WHERE int_field = 1";

			var new_values = new float[4];

			new_values[0] = 1300.10F;
			new_values[1] = 1400.20F;

			var update = new FbCommand(updateText, Connection);

			update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;

			update.ExecuteNonQuery();
			update.Dispose();
		}

		[Test]
		public void DoubleArrayPartialUpdateTest()
		{
			var updateText = "update	TEST set barray_field =	@array_field " +
								"WHERE int_field = 1";

			var new_values = new double[2];

			new_values[0] = 1700.10;
			new_values[1] = 1800.20;

			var update = new FbCommand(updateText, Connection);

			update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;

			update.ExecuteNonQuery();
			update.Dispose();
		}

		[Test]
		public void NumericArrayPartialUpdateTest()
		{
			var updateText = "update	TEST set narray_field =	@array_field " +
								"WHERE int_field = 1";

			var new_values = new decimal[2];

			new_values[0] = 2100.10M;
			new_values[1] = 2200.20M;

			var update = new FbCommand(updateText, Connection);

			update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;

			update.ExecuteNonQuery();
			update.Dispose();
		}

		[Test]
		public void DateArrayPartialUpdateTest()
		{
			var updateText = "update	TEST set darray_field =	@array_field " +
								"WHERE int_field = 1";

			var new_values = new DateTime[4];

			new_values[0] = DateTime.Now.AddDays(100);
			new_values[1] = DateTime.Now.AddDays(200);

			var update = new FbCommand(updateText, Connection);

			update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;

			update.ExecuteNonQuery();
			update.Dispose();
		}

		[Test]
		public void TimeArrayPartialUpdateTest()
		{
			var updateText = "update	TEST set tarray_field =	@array_field " +
								"WHERE int_field = 1";

			var new_values = new TimeSpan[2];

			new_values[0] = new TimeSpan(11, 13, 14);
			new_values[1] = new TimeSpan(12, 15, 16);

			var update = new FbCommand(updateText, Connection);

			update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;

			update.ExecuteNonQuery();
			update.Dispose();
		}

		[Test]
		public void TimeStampArrayPartialUpdateTest()
		{
			var updateText = "update	TEST set tsarray_field = @array_field " +
								"WHERE int_field = 1";

			var new_values = new DateTime[2];

			new_values[0] = DateTime.Now.AddSeconds(100);
			new_values[1] = DateTime.Now.AddSeconds(200);

			var update = new FbCommand(updateText, Connection);

			update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;

			update.ExecuteNonQuery();
			update.Dispose();
		}

		[Test]
		public void CharArrayPartialUpdateTest()
		{
			var updateText = "update	TEST set carray_field =	@array_field " +
								"WHERE int_field = 1";

			var new_values = new string[2];

			new_values[0] = "abc";
			new_values[1] = "abcdef";

			var update = new FbCommand(updateText, Connection);

			update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;

			update.ExecuteNonQuery();
			update.Dispose();
		}

		[Test]
		public void VarCharArrayPartialUpdateTest()
		{
			var updateText = "update	TEST set varray_field =	@array_field " +
								"WHERE int_field = 1";

			var new_values = new string[2];

			new_values[0] = "abc";
			new_values[1] = "abcdef";

			var update = new FbCommand(updateText, Connection);

			update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;

			update.ExecuteNonQuery();
			update.Dispose();
		}

		[Test]
		public void BigArrayTest()
		{
			Transaction = Connection.BeginTransaction();

			var id_value = GetId();
			int elements = short.MaxValue;

			var selectText = "SELECT	big_array FROM TEST	WHERE int_field	= " + id_value.ToString();
			var insertText = "INSERT	INTO TEST (int_field, big_array) values(@int_field,	@array_field)";

			var bytes = new byte[elements * 4];
			var rng = new RNGCryptoServiceProvider();
			rng.GetBytes(bytes);

			var insert_values = new int[elements];
			Buffer.BlockCopy(bytes, 0, insert_values, 0, bytes.Length);

			var insert = new FbCommand(insertText, Connection, Transaction);
			insert.Parameters.Add("@int_field", FbDbType.Integer).Value = id_value;
			insert.Parameters.Add("@array_field", FbDbType.Array).Value = insert_values;
			insert.ExecuteNonQuery();
			insert.Dispose();

			Transaction.Commit();

			var select = new FbCommand(selectText, Connection);
			var reader = select.ExecuteReader();
			if (reader.Read())
			{
				if (!reader.IsDBNull(0))
				{
					var select_values = new int[insert_values.Length];
					Array.Copy((Array)reader.GetValue(0), select_values, select_values.Length);
					CollectionAssert.AreEqual(insert_values, select_values);
				}
			}

			reader.Close();
			select.Dispose();
		}

		[Test]
		public void PartialUpdatesTest()
		{
			Transaction = Connection.BeginTransaction();

			var id_value = GetId();
			var elements = 16384;

			var selectText = "SELECT	big_array FROM TEST	WHERE int_field	= " + id_value.ToString();
			var insertText = "INSERT	INTO TEST (int_field, big_array) values(@int_field,	@array_field)";

			var bytes = new byte[elements * 4];
			var rng = new RNGCryptoServiceProvider();
			rng.GetBytes(bytes);

			var insert_values = new int[elements];
			Buffer.BlockCopy(bytes, 0, insert_values, 0, bytes.Length);

			var insert = new FbCommand(insertText, Connection, Transaction);
			insert.Parameters.Add("@int_field", FbDbType.Integer).Value = id_value;
			insert.Parameters.Add("@array_field", FbDbType.Array).Value = insert_values;
			insert.ExecuteNonQuery();
			insert.Dispose();

			Transaction.Commit();

			var select = new FbCommand(selectText, Connection);
			var reader = select.ExecuteReader();
			if (reader.Read())
			{
				if (!reader.IsDBNull(0))
				{
					var select_values = new int[insert_values.Length];
					Array.Copy((Array)reader.GetValue(0), select_values, select_values.Length);
					CollectionAssert.AreEqual(insert_values, select_values);
				}
			}

			reader.Close();
			select.Dispose();
		}

		#endregion
	}
}
