/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/raw/master/license.txt.
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
using System.Data;
using System.Threading.Tasks;
using FirebirdSql.Data.TestsBase;
using NUnit.Framework;

namespace FirebirdSql.Data.FirebirdClient.Tests
{
	[TestFixtureSource(typeof(FbServerTypeTestFixtureSource), nameof(FbServerTypeTestFixtureSource.Default))]
	[TestFixtureSource(typeof(FbServerTypeTestFixtureSource), nameof(FbServerTypeTestFixtureSource.Embedded))]
	public class FbDataAdapterTests : FbTestsBase
	{
		public FbDataAdapterTests(FbServerType serverType, bool compression, FbWireCrypt wireCrypt)
			: base(serverType, compression, wireCrypt)
		{ }

		[Test]
		public async Task FillTest()
		{
			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var command = new FbCommand("select * from TEST", Connection, transaction))
				{
					using (var adapter = new FbDataAdapter(command))
					{
						adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
						using (var ds = new DataSet())
						{
							adapter.Fill(ds, "TEST");
							Assert.AreEqual(100, ds.Tables["TEST"].Rows.Count, "Incorrect row count");
						}
					}
				}
				await transaction.CommitAsync();
			}
		}

		[Test]
		public async Task FillMultipleTest()
		{
			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var command = new FbCommand("select * from TEST", Connection, transaction))
				{
					using (var adapter = new FbDataAdapter(command))
					{
						adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
						using (var ds1 = new DataSet())
						using (var ds2 = new DataSet())
						{
							adapter.Fill(ds1, "TEST");
							adapter.Fill(ds2, "TEST");

							Assert.AreEqual(100, ds1.Tables["TEST"].Rows.Count, "Incorrect row count (ds1)");
							Assert.AreEqual(100, ds2.Tables["TEST"].Rows.Count, "Incorrect row count (ds2)");
						}
					}
				}
				await transaction.CommitAsync();
			}
		}

		[Test]
		public async Task FillMultipleWithImplicitTransactionTest()
		{
			await using (var command = new FbCommand("select * from TEST", Connection))
			{
				using (var adapter = new FbDataAdapter(command))
				{
					adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
					using (var ds1 = new DataSet())
					using (var ds2 = new DataSet())
					{
						adapter.Fill(ds1, "TEST");
						adapter.Fill(ds2, "TEST");

						Assert.AreEqual(100, ds1.Tables["TEST"].Rows.Count, "Incorrect row count (ds1)");
						Assert.AreEqual(100, ds2.Tables["TEST"].Rows.Count, "Incorrect row count (ds2)");
					}
				}
			}
		}

		[Test]
		public async Task InsertTest()
		{
			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var command = new FbCommand("select * from TEST", Connection, transaction))
				{
					using (var adapter = new FbDataAdapter(command))
					{
						using (var builder = new FbCommandBuilder(adapter))
						{
							adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
							using (var ds = new DataSet())
							{
								adapter.Fill(ds, "TEST");

								Assert.AreEqual(100, ds.Tables["TEST"].Rows.Count, "Incorrect row count");

								var newRow = ds.Tables["TEST"].NewRow();

								newRow["int_field"] = 101;
								newRow["CHAR_FIELD"] = "ONE THOUSAND";
								newRow["VARCHAR_FIELD"] = ":;,.{}`+^*[]\\!|@#$%&/()?_-<>";
								newRow["BIGint_field"] = 100000;
								newRow["SMALLint_field"] = 100;
								newRow["DOUBLE_FIELD"] = 100.01;
								newRow["NUMERIC_FIELD"] = 100.01;
								newRow["DECIMAL_FIELD"] = 100.01;
								newRow["DATE_FIELD"] = new DateTime(100, 10, 10);
								newRow["TIME_FIELD"] = new TimeSpan(10, 10, 10);
								newRow["TIMESTAMP_FIELD"] = new DateTime(100, 10, 10, 10, 10, 10, 10);
								newRow["CLOB_FIELD"] = "ONE THOUSAND";

								ds.Tables["TEST"].Rows.Add(newRow);

								adapter.Update(ds, "TEST");
							}
						}
					}
				}
				await transaction.CommitAsync();
			}
		}

		[Test]
		public async Task UpdateCharTest()
		{
			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var command = new FbCommand("select * from TEST where int_field = @int_field", Connection, transaction))
				{
					using (var adapter = new FbDataAdapter(command))
					{
						using (var builder = new FbCommandBuilder(adapter))
						{
							adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
							adapter.SelectCommand.Parameters.Add("@int_field", FbDbType.Integer).Value = 1;
							using (var ds = new DataSet())
							{
								adapter.Fill(ds, "TEST");

								Assert.AreEqual(1, ds.Tables["TEST"].Rows.Count, "Incorrect row count");

								ds.Tables["TEST"].Rows[0]["CHAR_FIELD"] = "ONE THOUSAND";

								adapter.Update(ds, "TEST");
							}
						}
					}
				}
				await transaction.CommitAsync();
			}

			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var command = new FbCommand("SELECT char_field FROM TEST WHERE int_field = @int_field", Connection, transaction))
				{
					command.Parameters.Add("@int_field", FbDbType.Integer).Value = 1;
					var val = (string)await command.ExecuteScalarAsync();
					Assert.AreEqual("ONE THOUSAND", val.Trim(), "char_field has not correct value");
				}
				await transaction.CommitAsync();
			}
		}

		[Test]
		public async Task UpdateVarCharTest()
		{
			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var command = new FbCommand("select * from TEST where int_field = @int_field", Connection, transaction))
				{
					using (var adapter = new FbDataAdapter(command))
					{
						using (var builder = new FbCommandBuilder(adapter))
						{
							adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
							adapter.SelectCommand.Parameters.Add("@int_field", FbDbType.Integer).Value = 1;
							using (var ds = new DataSet())
							{
								adapter.Fill(ds, "TEST");

								Assert.AreEqual(1, ds.Tables["TEST"].Rows.Count, "Incorrect row count");

								ds.Tables["TEST"].Rows[0]["VARCHAR_FIELD"] = "ONE VAR THOUSAND";

								adapter.Update(ds, "TEST");
							}
						}
					}
				}
				await transaction.CommitAsync();
			}

			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var command = new FbCommand("SELECT varchar_field FROM TEST WHERE int_field = @int_field", Connection, transaction))
				{
					command.Parameters.Add("@int_field", FbDbType.Integer).Value = 1;
					var val = (string)await command.ExecuteScalarAsync();
					Assert.AreEqual("ONE VAR THOUSAND", val.Trim(), "varchar_field has not correct value");
				}
				await transaction.CommitAsync();
			}
		}

		[Test]
		public async Task UpdateSmallIntTest()
		{
			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var command = new FbCommand("select * from TEST where int_field = @int_field", Connection, transaction))
				{
					using (var adapter = new FbDataAdapter(command))
					{
						using (var builder = new FbCommandBuilder(adapter))
						{
							adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
							adapter.SelectCommand.Parameters.Add("@int_field", FbDbType.Integer).Value = 1;
							using (var ds = new DataSet())
							{
								adapter.Fill(ds, "TEST");

								Assert.AreEqual(1, ds.Tables["TEST"].Rows.Count, "Incorrect row count");

								ds.Tables["TEST"].Rows[0]["SMALLint_field"] = short.MaxValue;

								adapter.Update(ds, "TEST");
							}
						}
					}
				}
				await transaction.CommitAsync();
			}

			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var command = new FbCommand("SELECT smallint_field FROM TEST WHERE int_field = @int_field", Connection, transaction))
				{
					command.Parameters.Add("@int_field", FbDbType.Integer).Value = 1;
					var val = (short)await command.ExecuteScalarAsync();
					Assert.AreEqual(short.MaxValue, val, "smallint_field has not correct value");
				}
				await transaction.CommitAsync();
			}
		}

		[Test]
		public async Task UpdateBigIntTest()
		{
			await using (var transaction = Connection.BeginTransaction())
			{
				await using (var command = new FbCommand("select * from TEST where int_field = @int_field", Connection, transaction))
				{
					using (var adapter = new FbDataAdapter(command))
					{
						using (var builder = new FbCommandBuilder(adapter))
						{
							adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
							adapter.SelectCommand.Parameters.Add("@int_field", FbDbType.Integer).Value = 1;
							using (var ds = new DataSet())
							{
								adapter.Fill(ds, "TEST");

								Assert.AreEqual(1, ds.Tables["TEST"].Rows.Count, "Incorrect row count");

								ds.Tables["TEST"].Rows[0]["BIGINT_FIELD"] = int.MaxValue;

								adapter.Update(ds, "TEST");
							}
						}
					}
				}
				transaction.Commit();
			}

			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var command = new FbCommand("SELECT bigint_field FROM TEST WHERE int_field = @int_field", Connection, transaction))
				{
					command.Parameters.Add("@int_field", FbDbType.Integer).Value = 1;
					var val = (long)await command.ExecuteScalarAsync();
					Assert.AreEqual(int.MaxValue, val, "bigint_field has not correct value");
				}
				await transaction.CommitAsync();
			}
		}

		[Test]
		public async Task UpdateDoubleTest()
		{
			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var command = new FbCommand("select * from TEST where int_field = @int_field", Connection, transaction))
				{
					using (var adapter = new FbDataAdapter(command))
					{
						using (var builder = new FbCommandBuilder(adapter))
						{
							adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
							adapter.SelectCommand.Parameters.Add("@int_field", FbDbType.Integer).Value = 1;
							using (var ds = new DataSet())
							{
								adapter.Fill(ds, "TEST");

								Assert.AreEqual(1, ds.Tables["TEST"].Rows.Count, "Incorrect row count");

								ds.Tables["TEST"].Rows[0]["DOUBLE_FIELD"] = int.MaxValue;

								adapter.Update(ds, "TEST");
							}
						}
					}
				}
				await transaction.CommitAsync();
			}

			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var command = new FbCommand("SELECT double_field FROM TEST WHERE int_field = @int_field", Connection, transaction))
				{
					command.Parameters.Add("@int_field", FbDbType.Integer).Value = 1;
					var val = (double)await command.ExecuteScalarAsync();
					Assert.AreEqual(int.MaxValue, val, "double_field has not correct value");
				}
				await transaction.CommitAsync();
			}
		}

		[Test]
		public async Task UpdateFloatTest()
		{
			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var command = new FbCommand("select * from TEST where int_field = @int_field", Connection, transaction))
				{
					using (var adapter = new FbDataAdapter(command))
					{
						using (var builder = new FbCommandBuilder(adapter))
						{
							adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
							adapter.SelectCommand.Parameters.Add("@int_field", FbDbType.Integer).Value = 1;
							using (var ds = new DataSet())
							{
								adapter.Fill(ds, "TEST");

								Assert.AreEqual(1, ds.Tables["TEST"].Rows.Count, "Incorrect row count");

								ds.Tables["TEST"].Rows[0]["FLOAT_FIELD"] = (float)100.20;

								adapter.Update(ds, "TEST");
							}
						}
					}
				}
				await transaction.CommitAsync();
			}

			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var command = new FbCommand("SELECT float_field FROM TEST WHERE int_field = @int_field", Connection, transaction))
				{
					command.Parameters.Add("@int_field", FbDbType.Integer).Value = 1;
					var val = (float)await command.ExecuteScalarAsync();
					Assert.AreEqual((float)100.20, val, "double_field has not correct value");
				}
				await transaction.CommitAsync();
			}
		}

		[Test]
		public async Task UpdateNumericTest()
		{
			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var command = new FbCommand("select * from TEST where int_field = @int_field", Connection, transaction))
				{
					using (var adapter = new FbDataAdapter(command))
					{
						using (var builder = new FbCommandBuilder(adapter))
						{
							adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
							adapter.SelectCommand.Parameters.Add("@int_field", FbDbType.Integer).Value = 1;
							using (var ds = new DataSet())
							{
								adapter.Fill(ds, "TEST");

								Assert.AreEqual(1, ds.Tables["TEST"].Rows.Count, "Incorrect row count");

								ds.Tables["TEST"].Rows[0]["NUMERIC_FIELD"] = int.MaxValue;

								adapter.Update(ds, "TEST");
							}
						}
					}
				}
				await transaction.CommitAsync();
			}

			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var command = new FbCommand("SELECT numeric_field FROM TEST WHERE int_field = @int_field", Connection, transaction))
				{
					command.Parameters.Add("@int_field", FbDbType.Integer).Value = 1;
					var val = (decimal)await command.ExecuteScalarAsync();
					Assert.AreEqual(int.MaxValue, val, "numeric_field has not correct value");
				}
				await transaction.CommitAsync();
			}
		}

		[Test]
		public async Task UpdateDecimalTest()
		{
			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var command = new FbCommand("select * from TEST where int_field = @int_field", Connection, transaction))
				{
					using (var adapter = new FbDataAdapter(command))
					{
						using (var builder = new FbCommandBuilder(adapter))
						{
							adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
							adapter.SelectCommand.Parameters.Add("@int_field", FbDbType.Integer).Value = 1;
							using (var ds = new DataSet())
							{
								adapter.Fill(ds, "TEST");

								Assert.AreEqual(1, ds.Tables["TEST"].Rows.Count, "Incorrect row count");

								ds.Tables["TEST"].Rows[0]["DECIMAL_FIELD"] = int.MaxValue;

								adapter.Update(ds, "TEST");
							}
						}
					}
				}
				await transaction.CommitAsync();
			}

			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var command = new FbCommand("SELECT decimal_field FROM TEST WHERE int_field = @int_field", Connection, transaction))
				{
					command.Parameters.Add("@int_field", FbDbType.Integer).Value = 1;
					var val = (decimal)await command.ExecuteScalarAsync();
					Assert.AreEqual(int.MaxValue, val, "decimal_field has not correct value");
				}
				await transaction.CommitAsync();
			}
		}

		[Test]
		public async Task UpdateDateTest()
		{
			var dtValue = DateTime.Now;

			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var command = new FbCommand("select * from TEST where int_field = @int_field", Connection, transaction))
				{
					using (var adapter = new FbDataAdapter(command))
					{
						using (var builder = new FbCommandBuilder(adapter))
						{
							adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
							adapter.SelectCommand.Parameters.Add("@int_field", FbDbType.Integer).Value = 1;
							using (var ds = new DataSet())
							{
								adapter.Fill(ds, "TEST");

								Assert.AreEqual(1, ds.Tables["TEST"].Rows.Count, "Incorrect row count");


								ds.Tables["TEST"].Rows[0]["DATE_FIELD"] = dtValue;

								adapter.Update(ds, "TEST");
							}
						}
					}
				}
				await transaction.CommitAsync();
			}

			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var command = new FbCommand("SELECT date_field FROM TEST WHERE int_field = @int_field", Connection, transaction))
				{
					command.Parameters.Add("@int_field", FbDbType.Integer).Value = 1;
					var val = (DateTime)await command.ExecuteScalarAsync();
					Assert.AreEqual(dtValue.Day, val.Day, "date_field has not correct day");
					Assert.AreEqual(dtValue.Month, val.Month, "date_field has not correct month");
					Assert.AreEqual(dtValue.Year, val.Year, "date_field has not correct year");
				}
				await transaction.CommitAsync();
			}
		}

		[Test]
		public async Task UpdateTimeTest()
		{
			var dtValue = new TimeSpan(5, 6, 7);

			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var command = new FbCommand("select * from TEST where int_field = @int_field", Connection, transaction))
				{
					using (var adapter = new FbDataAdapter(command))
					{
						using (var builder = new FbCommandBuilder(adapter))
						{
							adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
							adapter.SelectCommand.Parameters.Add("@int_field", FbDbType.Integer).Value = 1;
							using (var ds = new DataSet())
							{
								adapter.Fill(ds, "TEST");

								Assert.AreEqual(1, ds.Tables["TEST"].Rows.Count, "Incorrect row count");


								ds.Tables["TEST"].Rows[0]["TIME_FIELD"] = dtValue;

								adapter.Update(ds, "TEST");
							}
						}
					}
				}
				await transaction.CommitAsync();
			}

			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var command = new FbCommand("SELECT time_field FROM TEST WHERE int_field = @int_field", Connection, transaction))
				{
					command.Parameters.Add("@int_field", FbDbType.Integer).Value = 1;
					var val = (TimeSpan)await command.ExecuteScalarAsync();
					Assert.AreEqual(dtValue.Hours, val.Hours, "time_field has not correct hour");
					Assert.AreEqual(dtValue.Minutes, val.Minutes, "time_field has not correct minute");
					Assert.AreEqual(dtValue.Seconds, val.Seconds, "time_field has not correct second");
				}
				await transaction.CommitAsync();
			}
		}

		[Test]
		public async Task UpdateTimeStampTest()
		{
			var dtValue = DateTime.Now;

			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var command = new FbCommand("select * from TEST where int_field = @int_field", Connection, transaction))
				{
					using (var adapter = new FbDataAdapter(command))
					{
						using (var builder = new FbCommandBuilder(adapter))
						{
							adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
							adapter.SelectCommand.Parameters.Add("@int_field", FbDbType.Integer).Value = 1;
							using (var ds = new DataSet())
							{
								adapter.Fill(ds, "TEST");

								Assert.AreEqual(1, ds.Tables["TEST"].Rows.Count, "Incorrect row count");

								ds.Tables["TEST"].Rows[0]["TIMESTAMP_FIELD"] = dtValue;

								adapter.Update(ds, "TEST");
							}
						}
					}
				}
				await transaction.CommitAsync();
			}

			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var command = new FbCommand("SELECT timestamp_field FROM TEST WHERE int_field = @int_field", Connection, transaction))
				{
					command.Parameters.Add("@int_field", FbDbType.Integer).Value = 1;
					var val = (DateTime)await command.ExecuteScalarAsync();
					Assert.AreEqual(dtValue.Day, val.Day, "timestamp_field has not correct day");
					Assert.AreEqual(dtValue.Month, val.Month, "timestamp_field has not correct month");
					Assert.AreEqual(dtValue.Year, val.Year, "timestamp_field has not correct year");
					Assert.AreEqual(dtValue.Hour, val.Hour, "timestamp_field has not correct hour");
					Assert.AreEqual(dtValue.Minute, val.Minute, "timestamp_field has not correct minute");
					Assert.AreEqual(dtValue.Second, val.Second, "timestamp_field has not correct second");
				}
				await transaction.CommitAsync();
			}
		}

		[Test]
		public async Task UpdateClobTest()
		{
			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var command = new FbCommand("select * from TEST where int_field = @int_field", Connection, transaction))
				{
					using (var adapter = new FbDataAdapter(command))
					{
						using (var builder = new FbCommandBuilder(adapter))
						{
							adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
							adapter.SelectCommand.Parameters.Add("@int_field", FbDbType.Integer).Value = 1;
							using (var ds = new DataSet())
							{
								adapter.Fill(ds, "TEST");

								Assert.AreEqual(1, ds.Tables["TEST"].Rows.Count, "Incorrect row count");

								ds.Tables["TEST"].Rows[0]["CLOB_FIELD"] = "ONE THOUSAND";

								adapter.Update(ds, "TEST");
							}
						}
					}
				}
				await transaction.CommitAsync();
			}
		}

		[Test]
		public async Task DeleteTest()
		{
			await using (var transaction = await Connection.BeginTransactionAsync())
			{
				await using (var command = new FbCommand("select * from TEST where int_field = @int_field", Connection, transaction))
				{
					using (var adapter = new FbDataAdapter(command))
					{
						using (var builder = new FbCommandBuilder(adapter))
						{
							adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
							adapter.SelectCommand.Parameters.Add("@int_field", FbDbType.Integer).Value = 10;
							using (var ds = new DataSet())
							{
								adapter.Fill(ds, "TEST");

								Assert.AreEqual(1, ds.Tables["TEST"].Rows.Count, "Incorrect row count");

								ds.Tables["TEST"].Rows[0].Delete();

								adapter.Update(ds, "TEST");
							}
						}
					}
				}
				await transaction.CommitAsync();
			}
		}

		[Test]
		public async Task SubsequentDeletes()
		{
			await using (var select = new FbCommand("SELECT * FROM test", Connection))
			{
				await using (var delete = new FbCommand("DELETE FROM test WHERE int_field = @id", Connection))
				{
					delete.Parameters.Add("@id", FbDbType.Integer);
					delete.Parameters[0].SourceColumn = "INT_FIELD";
					using (var adapter = new FbDataAdapter(select))
					{
						adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
						adapter.DeleteCommand = delete;
						using (var ds = new DataSet())
						{
							adapter.Fill(ds);

							ds.Tables[0].Rows[0].Delete();
							adapter.Update(ds);

							ds.Tables[0].Rows[0].Delete();
							adapter.Update(ds);

							ds.Tables[0].Rows[0].Delete();
							adapter.Update(ds);
						}
					}
				}
			}
		}
	}
}
