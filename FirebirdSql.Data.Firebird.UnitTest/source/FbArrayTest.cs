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
using System.Collections;
using System.Security.Cryptography;
using FirebirdSql.Data.Firebird;

namespace FirebirdSql.Data.Firebird.Tests
{
	[TestFixture]
	public class FbArrayTest : BaseTest 
	{
		public FbArrayTest() : base(true)
		{		
		}
		
		[Test]
		public void IntergerArrayTest()
		{
			Console.WriteLine("\r\n");
			Console.WriteLine("Integer Array Test");
			Console.WriteLine("------- ----- ----");
			
			string selectText = "SELECT iarray_field FROM TEST " +
							    "WHERE int_field = 1";
			
			string updateText = "update TEST set iarray_field = @array_field " +
							    "WHERE int_field = 1";

			int[] original_values = new int[4];
			
			FbCommand select = new FbCommand(selectText, Connection, Transaction);
			
			FbDataReader reader = select.ExecuteReader();
			
			if (reader.Read())
			{
				if (!reader.IsDBNull(0))
				{
					System.Array.Copy((System.Array)reader.GetValue(0), original_values, 4);
										
					PrintArrayValues(original_values, true);
				}
				else
				{
					original_values = null;
				}				
			}

			reader.Close();

			int[] new_values = new int[4];

			new_values[0] = 10;
			new_values[1] = 20;
			new_values[2] = 30;
			new_values[3] = 40;
			
			FbCommand update = new FbCommand(updateText, Connection, Transaction);
			
			update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;
						
			update.ExecuteNonQuery();
			
			PrintArrayValues(new_values, false);
		}

		[Test]
		public void ShortArrayTest()
		{
			Console.WriteLine("\r\n");
			Console.WriteLine("Short Array Test");
			Console.WriteLine("----- ----- ----");

			string selectText = "SELECT sarray_field FROM TEST " +
							    "WHERE int_field = 1";
			
			string updateText = "update TEST set sarray_field = @array_field " +
							    "WHERE int_field = 1";


			short[] original_values = new short[5];
			
			FbCommand select = new FbCommand(selectText, Connection, Transaction);
			
			FbDataReader reader = select.ExecuteReader();
			
			if (reader.Read())
			{
				if (!reader.IsDBNull(0))
				{
					System.Array.Copy((System.Array)reader.GetValue(0), original_values, 4);
					PrintArrayValues(original_values, true);
				}
				else
				{
					original_values = null;
				}				
			}

			reader.Close();
			
			short[] new_values = new short[5];

			new_values[0] = 50;
			new_values[1] = 60;
			new_values[2] = 70;
			new_values[3] = 80;

			FbCommand update = new FbCommand(updateText, Connection, Transaction);
			
			update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;
						
			update.ExecuteNonQuery();
			
			PrintArrayValues(new_values, false);
		}
		
		[Test]
		public void BigIntArrayTest()
		{
			Console.WriteLine("\r\n");
			Console.WriteLine("BigInt Array Test");
			Console.WriteLine("------ ----- ----");
			
			string selectText = "SELECT larray_field FROM TEST " +
							    "WHERE int_field = 1";
			
			string updateText = "update TEST set larray_field = @array_field " +
							    "WHERE int_field = 1";
			

			long[] original_values = new long[6];
			
			FbCommand select = new FbCommand(selectText, Connection, Transaction);
			
			FbDataReader reader = select.ExecuteReader();
			
			if (reader.Read())
			{
				if (!reader.IsDBNull(0))
				{
					System.Array.Copy((System.Array)reader.GetValue(0), original_values, 4);
					PrintArrayValues(original_values, true);
				}
				else
				{
					original_values = null;
				}				
			}
			
			reader.Close();
			
			long[] new_values = new long[6];

			new_values[0] = 90;
			new_values[1] = 100;
			new_values[2] = 110;
			new_values[3] = 120;

			FbCommand update = new FbCommand(updateText, Connection, Transaction);
			
			update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;
						
			update.ExecuteNonQuery();
			
			PrintArrayValues(new_values, false);
		}

		[Test]
		public void FloatArrayTest()
		{
			Console.WriteLine("\r\n");
			Console.WriteLine("Float Array Test");
			Console.WriteLine("----- ----- ----");

			string selectText = "SELECT farray_field FROM TEST " +
							    "WHERE int_field = 1";
			
			string updateText = "update TEST set farray_field = @array_field " +
							    "WHERE int_field = 1";
			

			float[] original_values = new float[4];
			
			FbCommand select = new FbCommand(selectText, Connection, Transaction);
			
			FbDataReader reader = select.ExecuteReader();
			
			if (reader.Read())
			{
				if (!reader.IsDBNull(0))
				{
					System.Array.Copy((System.Array)reader.GetValue(0), original_values, 4);
					PrintArrayValues(original_values, true);
				}
				else
				{
					original_values = null;
				}				
			}
			
			reader.Close();
			
			float[] new_values = new float[4];

			new_values[0] = 130.10F;
			new_values[1] = 140.20F;
			new_values[2] = 150.30F;
			new_values[3] = 160.40F;
			
			FbCommand update = new FbCommand(updateText, Connection, Transaction);
			
			update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;
						
			update.ExecuteNonQuery();
			
			PrintArrayValues(new_values, false);
		}

		[Test]
		public void DoubleArrayTest()
		{
			Console.WriteLine("\r\n");
			Console.WriteLine("Double Array Test");
			Console.WriteLine("------ ----- ----");

			string selectText = "SELECT barray_field FROM TEST " +
							    "WHERE int_field = 1";
			
			string updateText = "update TEST set barray_field = @array_field " +
							    "WHERE int_field = 1";
			

			double[] original_values = new double[4];
			
			FbCommand select = new FbCommand(selectText, Connection, Transaction);
			
			FbDataReader reader = select.ExecuteReader();
			
			if (reader.Read())
			{
				if (!reader.IsDBNull(0))
				{
					System.Array.Copy((System.Array)reader.GetValue(0), original_values, 4);
					PrintArrayValues(original_values, true);
				}
				else
				{
					original_values = null;
				}				
			}
			
			reader.Close();
			
			double[] new_values = new double[4];

			new_values[0] = 170.10;
			new_values[1] = 180.20;
			new_values[2] = 190.30;
			new_values[3] = 200.40;
			
			FbCommand update = new FbCommand(updateText, Connection, Transaction);
			
			update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;
						
			update.ExecuteNonQuery();
			
			PrintArrayValues(new_values, false);
		}
		
		[Test]
		public void NumericArrayTest()
		{
			Console.WriteLine("\r\n");
			Console.WriteLine("Numeric/Decimal Array Test");
			Console.WriteLine("--------------- ----- ----");

			string selectText = "SELECT narray_field FROM TEST " +
							    "WHERE int_field = 1";
			
			string updateText = "update TEST set narray_field = @array_field " +
							    "WHERE int_field = 1";
			

			decimal[] original_values = new decimal[4];
			
			FbCommand select = new FbCommand(selectText, Connection, Transaction);
			
			FbDataReader reader = select.ExecuteReader();
			
			if (reader.Read())
			{
				if (!reader.IsDBNull(0))
				{
					System.Array.Copy((System.Array)reader.GetValue(0), original_values, 4);
					PrintArrayValues(original_values, true);
				}
				else
				{
					original_values = null;
				}				
			}
			
			reader.Close();
			
			decimal[] new_values = new decimal[4];

			new_values[0] = 210.10M;
			new_values[1] = 220.20M;
			new_values[2] = 230.30M;
			new_values[3] = 240.40M;
			
			FbCommand update = new FbCommand(updateText, Connection, Transaction);
			
			update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;
						
			update.ExecuteNonQuery();
			
			PrintArrayValues(new_values, false);
		}

		[Test]
		public void DateArrayTest()
		{
			Console.WriteLine("\r\n");
			Console.WriteLine("Date Array Test");
			Console.WriteLine("---- ----- ----");

			string selectText = "SELECT darray_field FROM TEST " +
							    "WHERE int_field = 1";
			
			string updateText = "update TEST set darray_field = @array_field " +
							    "WHERE int_field = 1";
			

			DateTime[] original_values = new DateTime[4];
			
			FbCommand select = new FbCommand(selectText, Connection, Transaction);
			
			FbDataReader reader = select.ExecuteReader();
			
			if (reader.Read())
			{
				if (!reader.IsDBNull(0))
				{
					System.Array.Copy((System.Array)reader.GetValue(0), original_values, 4);
					PrintArrayValues(original_values, true);
				}
				else
				{
					original_values = null;
				}				
			}
			
			reader.Close();
			
			DateTime[] new_values = new DateTime[4];

			new_values[0] = DateTime.Now.AddDays(10);
			new_values[1] = DateTime.Now.AddDays(20);
			new_values[2] = DateTime.Now.AddDays(30);
			new_values[3] = DateTime.Now.AddDays(40);
			
			FbCommand update = new FbCommand(updateText, Connection, Transaction);
			
			update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;
						
			update.ExecuteNonQuery();
			
			PrintArrayValues(new_values, false);
		}
			
		[Test]
		public void TimeArrayTest()
		{
			Console.WriteLine("\r\n");
			Console.WriteLine("Time Array Test");
			Console.WriteLine("---- ----- ----");

			string selectText = "SELECT tarray_field FROM TEST " +
							    "WHERE int_field = 1";
			
			string updateText = "update TEST set tarray_field = @array_field " +
							    "WHERE int_field = 1";

			DateTime[] original_values = new DateTime[4];
			
			FbCommand select = new FbCommand(selectText, Connection, Transaction);
			
			FbDataReader reader = select.ExecuteReader();
			
			if (reader.Read())
			{
				if (!reader.IsDBNull(0))
				{
					System.Array.Copy((System.Array)reader.GetValue(0), original_values, 4);
					PrintArrayValues(original_values, true);
				}
				else
				{
					original_values = null;
				}				
			}			

			reader.Close();
			
			DateTime[] new_values = new DateTime[4];

			new_values[0] = DateTime.Now.AddHours(10);
			new_values[1] = DateTime.Now.AddHours(20);
			new_values[2] = DateTime.Now.AddHours(30);
			new_values[3] = DateTime.Now.AddHours(40);
			
			FbCommand update = new FbCommand(updateText, Connection, Transaction);
			
			update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;
						
			update.ExecuteNonQuery();
			
			PrintArrayValues(new_values, false);
		}
		
		[Test]
		public void TimeStampArrayTest()
		{
			Console.WriteLine("\r\n");
			Console.WriteLine("TimeStamp Array Test");
			Console.WriteLine("--------- ----- ----");

			string selectText = "SELECT tsarray_field FROM TEST " +
							    "WHERE int_field = 1";
			
			string updateText = "update TEST set tsarray_field = @array_field " +
							    "WHERE int_field = 1";

			DateTime[] original_values = new DateTime[4];
			
			FbCommand select = new FbCommand(selectText, Connection, Transaction);
			
			FbDataReader reader = select.ExecuteReader();
			
			if (reader.Read())
			{
				if (!reader.IsDBNull(0))
				{
					System.Array.Copy((System.Array)reader.GetValue(0), original_values, 4);
					PrintArrayValues(original_values, true);
				}
				else
				{
					original_values = null;
				}				
			}
			
			reader.Close();
			
			DateTime[] new_values = new DateTime[4];

			new_values[0] = DateTime.Now.AddSeconds(10);
			new_values[1] = DateTime.Now.AddSeconds(20);
			new_values[2] = DateTime.Now.AddSeconds(30);
			new_values[3] = DateTime.Now.AddSeconds(40);
			
			FbCommand update = new FbCommand(updateText, Connection, Transaction);
			
			update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;
						
			update.ExecuteNonQuery();
			
			PrintArrayValues(new_values, false);
		}
				
		[Test]
		public void CharArrayTest()
		{
			Console.WriteLine("\r\n");
			Console.WriteLine("Char Array Test");
			Console.WriteLine("---- ----- ----");

			string selectText = "SELECT carray_field FROM TEST " +
							    "WHERE int_field = 1";
			
			string updateText = "update TEST set carray_field = @array_field " +
							    "WHERE int_field = 1";

			string[] original_values = new string[4];
			
			FbCommand select = new FbCommand(selectText, Connection, Transaction);
			
			FbDataReader reader = select.ExecuteReader();
			
			if (reader.Read())
			{
				if (!reader.IsDBNull(0))
				{
					System.Array.Copy((System.Array)reader.GetValue(0), original_values, 4);
					PrintArrayValues(original_values, true);
				}
				else
				{
					original_values = null;
				}				
			}
			
			reader.Close();
			
			string[] new_values = new string[4];

			new_values[0] = "abc";
			new_values[1] = "abcdef";
			new_values[2] = "abcdefghi";
			new_values[3] = "abcdefghijkl";
			
			FbCommand update = new FbCommand(updateText, Connection, Transaction);
			
			update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;
						
			update.ExecuteNonQuery();
			
			PrintArrayValues(new_values, false);
		}
	
		[Test]
		public void VarCharArrayTest()
		{
			Console.WriteLine("\r\n");
			Console.WriteLine("VarChar Array Test");
			Console.WriteLine("------- ----- ----");

			string selectText = "SELECT varray_field FROM TEST " +
							    "WHERE int_field = 1";
			
			string updateText = "update TEST set varray_field = @array_field " +
							    "WHERE int_field = 1";

			string[] original_values = new string[4];
			
			FbCommand select = new FbCommand(selectText, Connection, Transaction);
			
			FbDataReader reader = select.ExecuteReader();
			
			if (reader.Read())
			{
				if (!reader.IsDBNull(0))
				{
					System.Array.Copy((System.Array)reader.GetValue(0), original_values, 4);
					PrintArrayValues(original_values, true);
				}
				else
				{
					original_values = null;
				}				
			}
			
			reader.Close();
			
			string[] new_values = new string[4];

			new_values[0] = "abc";
			new_values[1] = "abcdef";
			new_values[2] = "abcdefghi";
			new_values[3] = "abcdefghijkl";
			
			FbCommand update = new FbCommand(updateText, Connection, Transaction);
			
			update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;
						
			update.ExecuteNonQuery();
			
			PrintArrayValues(new_values, false);
		}

		[Test]
		public void IntergerArrayPartialUpdateTest()
		{
			Console.WriteLine("\r\n");
			Console.WriteLine("Integer Array Test");
			Console.WriteLine("------- ----- ----");
						
			string updateText = "update TEST set iarray_field = @array_field " +
							    "WHERE int_field = 1";

			int[] new_values = new int[2];

			new_values[0] = 100;
			new_values[1] = 200;
			
			FbCommand update = new FbCommand(updateText, Connection, Transaction);
			
			update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;
						
			update.ExecuteNonQuery();
			
			PrintArrayValues(new_values, false);
		}

		[Test]
		public void ShortArrayPartialUpdateTest()
		{
			Console.WriteLine("\r\n");
			Console.WriteLine("Short Array Test");
			Console.WriteLine("----- ----- ----");
			
			string updateText = "update TEST set sarray_field = @array_field " +
							    "WHERE int_field = 1";
			
			short[] new_values = new short[3];

			new_values[0] = 500;
			new_values[1] = 600;

			FbCommand update = new FbCommand(updateText, Connection, Transaction);
			
			update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;
						
			update.ExecuteNonQuery();
			
			PrintArrayValues(new_values, false);
		}
		
		[Test]
		public void BigIntArrayPartialUpdateTest()
		{
			Console.WriteLine("\r\n");
			Console.WriteLine("BigInt Array Test");
			Console.WriteLine("------ ----- ----");
						
			string updateText = "update TEST set larray_field = @array_field " +
							    "WHERE int_field = 1";
						
			long[] new_values = new long[4];

			new_values[0] = 900;
			new_values[1] = 1000;
			new_values[2] = 1100;
			new_values[3] = 1200;

			FbCommand update = new FbCommand(updateText, Connection, Transaction);
			
			update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;
						
			update.ExecuteNonQuery();
			
			PrintArrayValues(new_values, false);
		}

		[Test]
		public void FloatArrayPartialUpdateTest()
		{
			Console.WriteLine("\r\n");
			Console.WriteLine("Float Array Test");
			Console.WriteLine("----- ----- ----");
			
			string updateText = "update TEST set farray_field = @array_field " +
							    "WHERE int_field = 1";
						
			float[] new_values = new float[4];

			new_values[0] = 1300.10F;
			new_values[1] = 1400.20F;
			
			FbCommand update = new FbCommand(updateText, Connection, Transaction);
			
			update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;
						
			update.ExecuteNonQuery();
			
			PrintArrayValues(new_values, false);
		}

		[Test]
		public void DoubleArrayPartialUpdateTest()
		{
			Console.WriteLine("\r\n");
			Console.WriteLine("Double Array Test");
			Console.WriteLine("------ ----- ----");
			
			string updateText = "update TEST set barray_field = @array_field " +
							    "WHERE int_field = 1";
			
			double[] new_values = new double[2];

			new_values[0] = 1700.10;
			new_values[1] = 1800.20;
			
			FbCommand update = new FbCommand(updateText, Connection, Transaction);
			
			update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;
						
			update.ExecuteNonQuery();
			
			PrintArrayValues(new_values, false);
		}
		
		[Test]
		public void NumericArrayPartialUpdateTest()
		{
			Console.WriteLine("\r\n");
			Console.WriteLine("Numeric/Decimal Array Test");
			Console.WriteLine("--------------- ----- ----");
			
			string updateText = "update TEST set narray_field = @array_field " +
							    "WHERE int_field = 1";
			
			decimal[] new_values = new decimal[2];

			new_values[0] = 2100.10M;
			new_values[1] = 2200.20M;
			
			FbCommand update = new FbCommand(updateText, Connection, Transaction);
			
			update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;
						
			update.ExecuteNonQuery();
			
			PrintArrayValues(new_values, false);
		}

		[Test]
		public void DateArrayPartialUpdateTest()
		{
			Console.WriteLine("\r\n");
			Console.WriteLine("Date Array Test");
			Console.WriteLine("---- ----- ----");
			
			string updateText = "update TEST set darray_field = @array_field " +
							    "WHERE int_field = 1";
			
			DateTime[] new_values = new DateTime[4];

			new_values[0] = DateTime.Now.AddDays(100);
			new_values[1] = DateTime.Now.AddDays(200);
			
			FbCommand update = new FbCommand(updateText, Connection, Transaction);
			
			update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;
						
			update.ExecuteNonQuery();
			
			PrintArrayValues(new_values, false);
		}
			
		[Test]
		public void TimeArrayPartialUpdateTest()
		{
			Console.WriteLine("\r\n");
			Console.WriteLine("Time Array Test");
			Console.WriteLine("---- ----- ----");
			
			string updateText = "update TEST set tarray_field = @array_field " +
							    "WHERE int_field = 1";

			DateTime[] new_values = new DateTime[2];

			new_values[0] = DateTime.Now.AddHours(100);
			new_values[1] = DateTime.Now.AddHours(200);
			
			FbCommand update = new FbCommand(updateText, Connection, Transaction);
			
			update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;
						
			update.ExecuteNonQuery();
			
			PrintArrayValues(new_values, false);
		}
		
		[Test]
		public void TimeStampArrayPartialUpdateTest()
		{
			Console.WriteLine("\r\n");
			Console.WriteLine("TimeStamp Array Test");
			Console.WriteLine("--------- ----- ----");
			
			string updateText = "update TEST set tsarray_field = @array_field " +
							    "WHERE int_field = 1";
		
			DateTime[] new_values = new DateTime[2];

			new_values[0] = DateTime.Now.AddSeconds(100);
			new_values[1] = DateTime.Now.AddSeconds(200);
			
			FbCommand update = new FbCommand(updateText, Connection, Transaction);
			
			update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;
						
			update.ExecuteNonQuery();
			
			PrintArrayValues(new_values, false);
		}
				
		[Test]
		public void CharArrayPartialUpdateTest()
		{
			Console.WriteLine("\r\n");
			Console.WriteLine("Char Array Test");
			Console.WriteLine("---- ----- ----");
			
			string updateText = "update TEST set carray_field = @array_field " +
							    "WHERE int_field = 1";
			
			string[] new_values = new string[2];

			new_values[0] = "abc";
			new_values[1] = "abcdef";
			
			FbCommand update = new FbCommand(updateText, Connection, Transaction);
			
			update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;
						
			update.ExecuteNonQuery();
			
			PrintArrayValues(new_values, false);
		}
	
		[Test]
		public void VarCharArrayPartialUpdateTest()
		{
			Console.WriteLine("\r\n");
			Console.WriteLine("VarChar Array Test");
			Console.WriteLine("------- ----- ----");
			
			string updateText = "update TEST set varray_field = @array_field " +
							    "WHERE int_field = 1";
			
			string[] new_values = new string[2];

			new_values[0] = "abc";
			new_values[1] = "abcdef";
			
			FbCommand update = new FbCommand(updateText, Connection, Transaction);
			
			update.Parameters.Add("@array_field", FbDbType.Array).Value = new_values;
						
			update.ExecuteNonQuery();
			
			PrintArrayValues(new_values, false);
		}

		[Test]
		public void BigArrayTest()
		{
			int id_value = this.GetId();
			
			string selectText = "SELECT big_array FROM TEST WHERE int_field = " + id_value.ToString();
			string insertText = "INSERT INTO TEST (int_field, big_array) values(@int_field, @array_field)";
			
			Console.WriteLine("\r\n\r\nBigArrayTest");
			Console.WriteLine("Generating an array of temp data");
			// Generate an array of temp data
			byte[] bytes = new byte[100000*4];
			RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
			rng.GetBytes(bytes);
			
			int[] insert_values = new int[100000];
			Buffer.BlockCopy(bytes, 0, insert_values, 0, bytes.Length);

			Console.WriteLine("Executing insert command");
			// Execute insert command
			FbCommand insert = new FbCommand(insertText, Connection, Transaction);
			insert.Parameters.Add("@int_field", FbDbType.Integer).Value = id_value;
			insert.Parameters.Add("@array_field", FbDbType.Array).Value = insert_values;
			insert.ExecuteNonQuery();

			Transaction.Commit();

			Console.WriteLine("Checking inserted values");

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
							throw new Exception("differences at index " + i.ToString());
						}
					}
				}
			}

			Console.WriteLine("Finishing test");
			reader.Close();

			// Start a new Transaction
			Transaction = Connection.BeginTransaction();
		}
		
		[Test]
		public void PartialUpdatesTest()
		{
			int id_value = this.GetId();
			
			string selectText = "SELECT big_array FROM TEST WHERE int_field = " + id_value.ToString();
			string insertText = "INSERT INTO TEST (int_field, big_array) values(@int_field, @array_field)";
			
			Console.WriteLine("\r\n\r\nPartialUpdatesTest");
			Console.WriteLine("Generating an array of temp data");
			// Generate an array of temp data
			byte[] bytes = new byte[10000*4];
			RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
			rng.GetBytes(bytes);
			
			int[] insert_values = new int[10000];
			Buffer.BlockCopy(bytes, 0, insert_values, 0, bytes.Length);

			Console.WriteLine("Executing insert command");
			// Execute insert command
			FbCommand insert = new FbCommand(insertText, Connection, Transaction);
			insert.Parameters.Add("@int_field", FbDbType.Integer).Value = id_value;
			insert.Parameters.Add("@array_field", FbDbType.Array).Value = insert_values;
			insert.ExecuteNonQuery();

			Transaction.Commit();

			Console.WriteLine("Checking inserted values");

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
							throw new Exception("differences at index " + i.ToString());
						}
					}
				}
			}

			Console.WriteLine("Finishing test");
			reader.Close();

			// Start a new Transaction
			Transaction = Connection.BeginTransaction();
		}

		private void PrintArrayValues(System.Array array, bool original)
		{
			IEnumerator i = array.GetEnumerator();
			
			if (original)
			{
				Console.WriteLine("Original field values:");
			}
			else
			{
				Console.WriteLine("New field values:");	
			}
			
			while(i.MoveNext())
			{
				Console.WriteLine(i.Current.ToString());
			}
		}
	}
}
