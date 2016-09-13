/*
 *	Firebird ADO.NET Data provider for .NET	and	Mono
 *
 *	   The contents	of this	file are subject to	the	Initial
 *	   Developer's Public License Version 1.0 (the "License");
 *	   you may not use this	file except	in compliance with the
 *	   License.	You	may	obtain a copy of the License at
 *	   http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *	   Software	distributed	under the License is distributed on
 *	   an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *	   express or implied.	See	the	License	for	the	specific
 *	   language	governing rights and limitations under the License.
 *
 *	Copyright (c) 2002, 2007 Carlos Guzman Alvarez
 *	All	Rights Reserved.
 */

using System;
using System.Collections;
using System.Security.Cryptography;

using FirebirdSql.Data.FirebirdClient;
using NUnit.Framework;

namespace FirebirdSql.Data.UnitTests
{
	[FbTestFixture(FbServerType.Default, false)]
	[FbTestFixture(FbServerType.Default, true)]
	[FbTestFixture(FbServerType.Embedded, default(bool))]
	public class FbArrayTests : TestsBase
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

			int id_value = GetId();

			string selectText = "SELECT	iarray_field FROM TEST WHERE int_field = " + id_value.ToString();
			string insertText = "INSERT	INTO TEST (int_field, iarray_field)	values(@int_field, @array_field)";

			// Insert new Record
			int[] insert_values = new int[4];

			insert_values[0] = 10;
			insert_values[1] = 20;
			insert_values[2] = 30;
			insert_values[3] = 40;

			FbCommand insert = new FbCommand(insertText, Connection, Transaction);
			insert.Parameters.Add("@int_field", FbDbType.Integer).Value = id_value;
			insert.Parameters.Add("@array_field", FbDbType.Array).Value = insert_values;
			insert.ExecuteNonQuery();
			insert.Dispose();

			Transaction.Commit();

			// Check that inserted values are correct
			FbCommand select = new FbCommand(selectText, Connection);
			FbDataReader reader = select.ExecuteReader();
			if (reader.Read())
			{
				if (!reader.IsDBNull(0))
				{
					int[] select_values = new int[insert_values.Length];
					System.Array.Copy((System.Array)reader.GetValue(0), select_values, select_values.Length);

					for (int i = 0; i < insert_values.Length; i++)
					{
						if (insert_values[i] != select_values[i])
						{
							throw new Exception("differences at	index " + i.ToString());
						}
					}
				}
			}
			reader.Close();
			select.Dispose();
		}

		[Test]
		public void ShortArrayTest()
		{
			Transaction = Connection.BeginTransaction();

			int id_value = GetId();

			string selectText = "SELECT	sarray_field FROM TEST WHERE int_field = " + id_value.ToString();
			string insertText = "INSERT	INTO TEST (int_field, sarray_field)	values(@int_field, @array_field)";

			// Insert new Record
			short[] insert_values = new short[4];

			insert_values[0] = 50;
			insert_values[1] = 60;
			insert_values[2] = 70;
			insert_values[3] = 80;

			FbCommand insert = new FbCommand(insertText, Connection, Transaction);
			insert.Parameters.Add("@int_field", FbDbType.Integer).Value = id_value;
			insert.Parameters.Add("@array_field", FbDbType.Array).Value = insert_values;
			insert.ExecuteNonQuery();
			insert.Dispose();

			Transaction.Commit();

			// Check that inserted values are correct
			FbCommand select = new FbCommand(selectText, Connection);
			FbDataReader reader = select.ExecuteReader();
			if (reader.Read())
			{
				if (!reader.IsDBNull(0))
				{
					short[] select_values = new short[insert_values.Length];
					System.Array.Copy((System.Array)reader.GetValue(0), select_values, select_values.Length);

					for (int i = 0; i < insert_values.Length; i++)
					{
						if (insert_values[i] != select_values[i])
						{
							throw new Exception("differences at	index " + i.ToString());
						}
					}
				}
			}
			reader.Close();
			select.Dispose();
		}

		[Test]
		public void BigIntArrayTest()
		{
			Transaction = Connection.BeginTransaction();

			int id_value = GetId();

			string selectText = "SELECT	larray_field FROM TEST WHERE int_field = " + id_value.ToString();
			string insertText = "INSERT	INTO TEST (int_field, larray_field)	values(@int_field, @array_field)";

			// Insert new Record
			long[] insert_values = new long[4];

			insert_values[0] = 50;
			insert_values[1] = 60;
			insert_values[2] = 70;
			insert_values[3] = 80;

			FbCommand insert = new FbCommand(insertText, Connection, Transaction);
			insert.Parameters.Add("@int_field", FbDbType.Integer).Value = id_value;
			insert.Parameters.Add("@array_field", FbDbType.Array).Value = insert_values;
			insert.ExecuteNonQuery();
			insert.Dispose();

			Transaction.Commit();

			// Check that inserted values are correct
			FbCommand select = new FbCommand(selectText, Connection);
			FbDataReader reader = select.ExecuteReader();
			if (reader.Read())
			{
				if (!reader.IsDBNull(0))
				{
					long[] select_values = new long[insert_values.Length];
					System.Array.Copy((System.Array)reader.GetValue(0), select_values, select_values.Length);

					for (int i = 0; i < insert_values.Length; i++)
					{
						if (insert_values[i] != select_values[i])
						{
							throw new Exception("differences at	index " + i.ToString());
						}
					}
				}
			}
			reader.Close();
			select.Dispose();
		}

		[Test]
		public void FloatArrayTest()
		{
			Transaction = Connection.BeginTransaction();

			int id_value = GetId();

			string selectText = "SELECT	farray_field FROM TEST WHERE int_field = " + id_value.ToString();
			string insertText = "INSERT	INTO TEST (int_field, farray_field)	values(@int_field, @array_field)";

			// Insert new Record
			float[] insert_values = new float[4];

			insert_values[0] = 130.10F;
			insert_values[1] = 140.20F;
			insert_values[2] = 150.30F;
			insert_values[3] = 160.40F;

			FbCommand insert = new FbCommand(insertText, Connection, Transaction);
			insert.Parameters.Add("@int_field", FbDbType.Integer).Value = id_value;
			insert.Parameters.Add("@array_field", FbDbType.Array).Value = insert_values;
			insert.ExecuteNonQuery();
			insert.Dispose();

			Transaction.Commit();

			// Check that inserted values are correct
			FbCommand select = new FbCommand(selectText, Connection);
			FbDataReader reader = select.ExecuteReader();
			if (reader.Read())
			{
				if (!reader.IsDBNull(0))
				{
					float[] select_values = new float[insert_values.Length];
					System.Array.Copy((System.Array)reader.GetValue(0), select_values, select_values.Length);

					for (int i = 0; i < insert_values.Length; i++)
					{
						if (insert_values[i] != select_values[i])
						{
							throw new Exception("differences at	index " + i.ToString());
						}
					}
				}
			}
			reader.Close();
			select.Dispose();
		}

		[Test]
		public void DoubleArrayTest()
		{
			Transaction = Connection.BeginTransaction();

			int id_value = GetId();

			string selectText = "SELECT	barray_field FROM TEST WHERE int_field = " + id_value.ToString();
			string insertText = "INSERT	INTO TEST (int_field, barray_field)	values(@int_field, @array_field)";

			// Insert new Record
			double[] insert_values = new double[4];

			insert_values[0] = 170.10;
			insert_values[1] = 180.20;
			insert_values[2] = 190.30;
			insert_values[3] = 200.40;

			FbCommand insert = new FbCommand(insertText, Connection, Transaction);
			insert.Parameters.Add("@int_field", FbDbType.Integer).Value = id_value;
			insert.Parameters.Add("@array_field", FbDbType.Array).Value = insert_values;
			insert.ExecuteNonQuery();
			insert.Dispose();

			Transaction.Commit();

			// Check that inserted values are correct
			FbCommand select = new FbCommand(selectText, Connection);
			FbDataReader reader = select.ExecuteReader();
			if (reader.Read())
			{
				if (!reader.IsDBNull(0))
				{
					double[] select_values = new double[insert_values.Length];
					System.Array.Copy((System.Array)reader.GetValue(0), select_values, select_values.Length);

					for (int i = 0; i < insert_values.Length; i++)
					{
						if (insert_values[i] != select_values[i])
						{
							throw new Exception("differences at	index " + i.ToString());
						}
					}
				}
			}
			reader.Close();
			select.Dispose();
		}

		[Test]
		public void NumericArrayTest()
		{
			Transaction = Connection.BeginTransaction();

			int id_value = GetId();

			string selectText = "SELECT	narray_field FROM TEST WHERE int_field = " + id_value.ToString();
			string insertText = "INSERT	INTO TEST (int_field, narray_field)	values(@int_field, @array_field)";

			// Insert new Record
			decimal[] insert_values = new decimal[4];

			insert_values[0] = 210.10M;
			insert_values[1] = 220.20M;
			insert_values[2] = 230.30M;
			insert_values[3] = 240.40M;

			FbCommand insert = new FbCommand(insertText, Connection, Transaction);
			insert.Parameters.Add("@int_field", FbDbType.Integer).Value = id_value;
			insert.Parameters.Add("@array_field", FbDbType.Array).Value = insert_values;
			insert.ExecuteNonQuery();
			insert.Dispose();

			Transaction.Commit();

			// Check that inserted values are correct
			FbCommand select = new FbCommand(selectText, Connection);
			FbDataReader reader = select.ExecuteReader();
			if (reader.Read())
			{
				if (!reader.IsDBNull(0))
				{
					decimal[] select_values = new decimal[insert_values.Length];
					System.Array.Copy((System.Array)reader.GetValue(0), select_values, select_values.Length);

					for (int i = 0; i < insert_values.Length; i++)
					{
						if (insert_values[i] != select_values[i])
						{
							throw new Exception("differences at	index " + i.ToString());
						}
					}
				}
			}
			reader.Close();
			select.Dispose();
		}

		[Test]
		public void DateArrayTest()
		{
			Transaction = Connection.BeginTransaction();

			int id_value = GetId();

			string selectText = "SELECT	darray_field FROM TEST WHERE int_field = " + id_value.ToString();
			string insertText = "INSERT	INTO TEST (int_field, darray_field)	values(@int_field, @array_field)";

			// Insert new Record
			DateTime[] insert_values = new DateTime[4];

			insert_values[0] = DateTime.Now.AddDays(10);
			insert_values[1] = DateTime.Now.AddDays(20);
			insert_values[2] = DateTime.Now.AddDays(30);
			insert_values[3] = DateTime.Now.AddDays(40);

			FbCommand insert = new FbCommand(insertText, Connection, Transaction);
			insert.Parameters.Add("@int_field", FbDbType.Integer).Value = id_value;
			insert.Parameters.Add("@array_field", FbDbType.Array).Value = insert_values;
			insert.ExecuteNonQuery();
			insert.Dispose();

			Transaction.Commit();

			// Check that inserted values are correct
			FbCommand select = new FbCommand(selectText, Connection);
			FbDataReader reader = select.ExecuteReader();
			if (reader.Read())
			{
				if (!reader.IsDBNull(0))
				{
					DateTime[] select_values = new DateTime[insert_values.Length];
					System.Array.Copy((System.Array)reader.GetValue(0), select_values, select_values.Length);

					for (int i = 0; i < insert_values.Length; i++)
					{
						if (insert_values[i].ToString("dd/MM/yyy") != select_values[i].ToString("dd/MM/yyy"))
						{
							throw new Exception("differences at	index " + i.ToString());
						}
					}
				}
			}
			reader.Close();
			select.Dispose();
		}

		[Test]
		public void TimeArrayTest()
		{
			Transaction = Connection.BeginTransaction();

			int id_value = GetId();

			string selectText = "SELECT	tarray_field FROM TEST WHERE int_field = " + id_value.ToString();
			string insertText = "INSERT	INTO TEST (int_field, tarray_field)	values(@int_field, @array_field)";

			// Insert new Record
			TimeSpan[] insert_values = new TimeSpan[4];

			insert_values[0] = new TimeSpan(3, 9, 10);
			insert_values[1] = new TimeSpan(4, 11, 12);
			insert_values[2] = new TimeSpan(6, 13, 14);
			insert_values[3] = new TimeSpan(8, 15, 16);

			FbCommand insert = new FbCommand(insertText, Connection, Transaction);
			insert.Parameters.Add("@int_field", FbDbType.Integer).Value = id_value;
			insert.Parameters.Add("@array_field", FbDbType.Array).Value = insert_values;
			insert.ExecuteNonQuery();
			insert.Dispose();

			Transaction.Commit();

			// Check that inserted values are correct
			FbCommand select = new FbCommand(selectText, Connection);
			FbDataReader reader = select.ExecuteReader();
			if (reader.Read())
			{
				if (!reader.IsDBNull(0))
				{
					TimeSpan[] select_values = new TimeSpan[insert_values.Length];
					System.Array.Copy((System.Array)reader.GetValue(0), select_values, select_values.Length);

					for (int i = 0; i < insert_values.Length; i++)
					{
						if (insert_values[i].ToString() != select_values[i].ToString())
						{
							throw new Exception("differences at	index " + i.ToString());
						}
					}
				}
			}
			reader.Close();
			select.Dispose();
		}

		[Test]
		public void TimeStampArrayTest()
		{
			Transaction = Connection.BeginTransaction();

			int id_value = GetId();

			string selectText = "SELECT	tsarray_field FROM TEST	WHERE int_field	= " + id_value.ToString();
			string insertText = "INSERT	INTO TEST (int_field, tsarray_field) values(@int_field,	@array_field)";

			// Insert new Record
			DateTime[] insert_values = new DateTime[4];

			insert_values[0] = DateTime.Now.AddSeconds(10);
			insert_values[1] = DateTime.Now.AddSeconds(20);
			insert_values[2] = DateTime.Now.AddSeconds(30);
			insert_values[3] = DateTime.Now.AddSeconds(40);

			FbCommand insert = new FbCommand(insertText, Connection, Transaction);
			insert.Parameters.Add("@int_field", FbDbType.Integer).Value = id_value;
			insert.Parameters.Add("@array_field", FbDbType.Array).Value = insert_values;
			insert.ExecuteNonQuery();
			insert.Dispose();

			Transaction.Commit();

			// Check that inserted values are correct
			FbCommand select = new FbCommand(selectText, Connection);
			FbDataReader reader = select.ExecuteReader();
			if (reader.Read())
			{
				if (!reader.IsDBNull(0))
				{
					DateTime[] select_values = new DateTime[insert_values.Length];
					System.Array.Copy((System.Array)reader.GetValue(0), select_values, select_values.Length);

					for (int i = 0; i < insert_values.Length; i++)
					{
						if (insert_values[i].ToString("dd/MM/yyyy HH:mm:ss") != select_values[i].ToString("dd/MM/yyyy HH:mm:ss"))
						{
							throw new Exception("differences at	index " + i.ToString());
						}
					}
				}
			}
			reader.Close();
			select.Dispose();
		}

		[Test]
		public void CharArrayTest()
		{
			Transaction = Connection.BeginTransaction();

			int id_value = GetId();

			string selectText = "SELECT	carray_field FROM TEST WHERE int_field = " + id_value.ToString();
			string insertText = "INSERT	INTO TEST (int_field, carray_field)	values(@int_field, @array_field)";

			// Insert new Record
			string[] insert_values = new string[4];

			insert_values[0] = "abc";
			insert_values[1] = "abcdef";
			insert_values[2] = "abcdefghi";
			insert_values[3] = "abcdefghijkl";

			FbCommand insert = new FbCommand(insertText, Connection, Transaction);
			insert.Parameters.Add("@int_field", FbDbType.Integer).Value = id_value;
			insert.Parameters.Add("@array_field", FbDbType.Array).Value = insert_values;
			insert.ExecuteNonQuery();
			insert.Dispose();

			Transaction.Commit();

			// Check that inserted values are correct
			FbCommand select = new FbCommand(selectText, Connection);
			FbDataReader reader = select.ExecuteReader();
			if (reader.Read())
			{
				if (!reader.IsDBNull(0))
				{
					string[] select_values = new string[insert_values.Length];
					System.Array.Copy((System.Array)reader.GetValue(0), select_values, select_values.Length);

					for (int i = 0; i < insert_values.Length; i++)
					{
						if (insert_values[i].Trim() != select_values[i].Trim())
						{
							throw new Exception("differences at	index " + i.ToString());
						}
					}
				}
			}
			reader.Close();
			select.Dispose();
		}

		[Test]
		public void VarCharArrayTest()
		{
			Transaction = Connection.BeginTransaction();

			int id_value = GetId();

			string selectText = "SELECT	varray_field FROM TEST WHERE int_field = " + id_value.ToString();
			string insertText = "INSERT	INTO TEST (int_field, varray_field)	values(@int_field, @array_field)";

			// Insert new Record
			string[] insert_values = new string[4];

			insert_values[0] = "abc";
			insert_values[1] = "abcdef";
			insert_values[2] = "abcdefghi";
			insert_values[3] = "abcdefghijkl";

			FbCommand insert = new FbCommand(insertText, Connection, Transaction);
			insert.Parameters.Add("@int_field", FbDbType.Integer).Value = id_value;
			insert.Parameters.Add("@array_field", FbDbType.Array).Value = insert_values;
			insert.ExecuteNonQuery();
			insert.Dispose();

			Transaction.Commit();

			// Check that inserted values are correct
			FbCommand select = new FbCommand(selectText, Connection);
			FbDataReader reader = select.ExecuteReader();
			if (reader.Read())
			{
				if (!reader.IsDBNull(0))
				{
					string[] select_values = new string[insert_values.Length];
					System.Array.Copy((System.Array)reader.GetValue(0), select_values, select_values.Length);

					for (int i = 0; i < insert_values.Length; i++)
					{
						if (insert_values[i].Trim() != select_values[i].Trim())
						{
							throw new Exception("differences at	index " + i.ToString());
						}
					}
				}
			}
			reader.Close();
			select.Dispose();
		}

		[Test]
		public void IntegerArrayPartialUpdateTest()
		{
			string updateText = "update	TEST set iarray_field =	@array_field " +
								"WHERE int_field = 1";

			int[] new_values = new int[2];

			new_values[0] = 100;
			new_values[1] = 200;

			FbCommand update = new FbCommand(updateText, Connection);

			update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;

			update.ExecuteNonQuery();
			update.Dispose();

			PrintArrayValues(new_values, false);
		}

		[Test]
		public void ShortArrayPartialUpdateTest()
		{
			string updateText = "update	TEST set sarray_field =	@array_field " +
								"WHERE int_field = 1";

			short[] new_values = new short[3];

			new_values[0] = 500;
			new_values[1] = 600;

			FbCommand update = new FbCommand(updateText, Connection);

			update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;

			update.ExecuteNonQuery();
			update.Dispose();

			PrintArrayValues(new_values, false);
		}

		[Test]
		public void BigIntArrayPartialUpdateTest()
		{
			string updateText = "update	TEST set larray_field =	@array_field " +
								"WHERE int_field = 1";

			long[] new_values = new long[4];

			new_values[0] = 900;
			new_values[1] = 1000;
			new_values[2] = 1100;
			new_values[3] = 1200;

			FbCommand update = new FbCommand(updateText, Connection);

			update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;

			update.ExecuteNonQuery();
			update.Dispose();

			PrintArrayValues(new_values, false);
		}

		[Test]
		public void FloatArrayPartialUpdateTest()
		{
			string updateText = "update	TEST set farray_field =	@array_field " +
								"WHERE int_field = 1";

			float[] new_values = new float[4];

			new_values[0] = 1300.10F;
			new_values[1] = 1400.20F;

			FbCommand update = new FbCommand(updateText, Connection);

			update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;

			update.ExecuteNonQuery();
			update.Dispose();

			PrintArrayValues(new_values, false);
		}

		[Test]
		public void DoubleArrayPartialUpdateTest()
		{
			string updateText = "update	TEST set barray_field =	@array_field " +
								"WHERE int_field = 1";

			double[] new_values = new double[2];

			new_values[0] = 1700.10;
			new_values[1] = 1800.20;

			FbCommand update = new FbCommand(updateText, Connection);

			update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;

			update.ExecuteNonQuery();
			update.Dispose();

			PrintArrayValues(new_values, false);
		}

		[Test]
		public void NumericArrayPartialUpdateTest()
		{
			string updateText = "update	TEST set narray_field =	@array_field " +
								"WHERE int_field = 1";

			decimal[] new_values = new decimal[2];

			new_values[0] = 2100.10M;
			new_values[1] = 2200.20M;

			FbCommand update = new FbCommand(updateText, Connection);

			update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;

			update.ExecuteNonQuery();
			update.Dispose();

			PrintArrayValues(new_values, false);
		}

		[Test]
		public void DateArrayPartialUpdateTest()
		{
			string updateText = "update	TEST set darray_field =	@array_field " +
								"WHERE int_field = 1";

			DateTime[] new_values = new DateTime[4];

			new_values[0] = DateTime.Now.AddDays(100);
			new_values[1] = DateTime.Now.AddDays(200);

			FbCommand update = new FbCommand(updateText, Connection);

			update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;

			update.ExecuteNonQuery();
			update.Dispose();

			PrintArrayValues(new_values, false);
		}

		[Test]
		public void TimeArrayPartialUpdateTest()
		{
			string updateText = "update	TEST set tarray_field =	@array_field " +
								"WHERE int_field = 1";

			TimeSpan[] new_values = new TimeSpan[2];

			new_values[0] = new TimeSpan(11, 13, 14);
			new_values[1] = new TimeSpan(12, 15, 16);

			FbCommand update = new FbCommand(updateText, Connection);

			update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;

			update.ExecuteNonQuery();
			update.Dispose();

			PrintArrayValues(new_values, false);
		}

		[Test]
		public void TimeStampArrayPartialUpdateTest()
		{
			string updateText = "update	TEST set tsarray_field = @array_field " +
								"WHERE int_field = 1";

			DateTime[] new_values = new DateTime[2];

			new_values[0] = DateTime.Now.AddSeconds(100);
			new_values[1] = DateTime.Now.AddSeconds(200);

			FbCommand update = new FbCommand(updateText, Connection);

			update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;

			update.ExecuteNonQuery();
			update.Dispose();

			PrintArrayValues(new_values, false);
		}

		[Test]
		public void CharArrayPartialUpdateTest()
		{
			string updateText = "update	TEST set carray_field =	@array_field " +
								"WHERE int_field = 1";

			string[] new_values = new string[2];

			new_values[0] = "abc";
			new_values[1] = "abcdef";

			FbCommand update = new FbCommand(updateText, Connection);

			update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;

			update.ExecuteNonQuery();
			update.Dispose();

			PrintArrayValues(new_values, false);
		}

		[Test]
		public void VarCharArrayPartialUpdateTest()
		{
			string updateText = "update	TEST set varray_field =	@array_field " +
								"WHERE int_field = 1";

			string[] new_values = new string[2];

			new_values[0] = "abc";
			new_values[1] = "abcdef";

			FbCommand update = new FbCommand(updateText, Connection);

			update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;

			update.ExecuteNonQuery();
			update.Dispose();

			PrintArrayValues(new_values, false);
		}

		[Test]
		public void BigArrayTest()
		{
			Transaction = Connection.BeginTransaction();

			int id_value = GetId();
			int elements = short.MaxValue;

			string selectText = "SELECT	big_array FROM TEST	WHERE int_field	= " + id_value.ToString();
			string insertText = "INSERT	INTO TEST (int_field, big_array) values(@int_field,	@array_field)";

			// Generate	an array of	temp data
			byte[] bytes = new byte[elements * 4];
			RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
			rng.GetBytes(bytes);

			int[] insert_values = new int[elements];
			Buffer.BlockCopy(bytes, 0, insert_values, 0, bytes.Length);

			// Execute insert command
			FbCommand insert = new FbCommand(insertText, Connection, Transaction);
			insert.Parameters.Add("@int_field", FbDbType.Integer).Value = id_value;
			insert.Parameters.Add("@array_field", FbDbType.Array).Value = insert_values;
			insert.ExecuteNonQuery();
			insert.Dispose();

			Transaction.Commit();

			// Check that inserted values are correct
			FbCommand select = new FbCommand(selectText, Connection);
			FbDataReader reader = select.ExecuteReader();
			if (reader.Read())
			{
				if (!reader.IsDBNull(0))
				{
					int[] select_values = new int[insert_values.Length];
					System.Array.Copy((System.Array)reader.GetValue(0), select_values, select_values.Length);

					for (int i = 0; i < insert_values.Length; i++)
					{
						if (insert_values[i] != select_values[i])
						{
							throw new Exception("differences at	index " + i.ToString());
						}
					}
				}
			}

			reader.Close();
			select.Dispose();

			// Start a new Transaction
			Transaction = Connection.BeginTransaction();
		}

		[Test]
		public void PartialUpdatesTest()
		{
			Transaction = Connection.BeginTransaction();

			int id_value = GetId();
			int elements = 16384;

			string selectText = "SELECT	big_array FROM TEST	WHERE int_field	= " + id_value.ToString();
			string insertText = "INSERT	INTO TEST (int_field, big_array) values(@int_field,	@array_field)";

			// Generate	an array of	temp data
			byte[] bytes = new byte[elements * 4];
			RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
			rng.GetBytes(bytes);

			int[] insert_values = new int[elements];
			Buffer.BlockCopy(bytes, 0, insert_values, 0, bytes.Length);

			// Execute insert command
			FbCommand insert = new FbCommand(insertText, Connection, Transaction);
			insert.Parameters.Add("@int_field", FbDbType.Integer).Value = id_value;
			insert.Parameters.Add("@array_field", FbDbType.Array).Value = insert_values;
			insert.ExecuteNonQuery();
			insert.Dispose();

			Transaction.Commit();

			// Check that inserted values are correct
			FbCommand select = new FbCommand(selectText, Connection);
			FbDataReader reader = select.ExecuteReader();
			if (reader.Read())
			{
				if (!reader.IsDBNull(0))
				{
					int[] select_values = new int[insert_values.Length];
					System.Array.Copy((System.Array)reader.GetValue(0), select_values, select_values.Length);

					for (int i = 0; i < insert_values.Length; i++)
					{
						if (insert_values[i] != select_values[i])
						{
							throw new Exception("differences at	index " + i.ToString());
						}
					}
				}
			}

			reader.Close();
			select.Dispose();

			// Start a new Transaction
			Transaction = Connection.BeginTransaction();
		}

		#endregion

		#region Private Methods

		private void PrintArrayValues(Array array, bool original)
		{
			IEnumerator i = array.GetEnumerator();
			TestContext.WriteLine($"{(original ? "Original" : "New")} field values:");
			foreach (var item in array)
			{
				TestContext.WriteLine(item);
			}
		}

		#endregion
	}
}
