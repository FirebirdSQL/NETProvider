/*
 * This example show how to update a text blob field
 * using FbCommand class of the Firebird .NET Data Provider.
 * 
 * Added:
 * 
 *		28/02/2003
 */

using System;
using System.IO;
using System.Data;
using System.Xml;

using FirebirdSql.Data.Firebird;

namespace FirebirdSql.Data.Firebird.Examples
{
	public class UpdateClobFieldExample
	{   
		public static void Main(string[] args)
		{
			// Set the ServerType to 1 for connect to the embedded server
			string connectionString =
				"User=SYSDBA;"					+
				"Password=masterkey;"			+
				"Database=SampleDatabase.fdb;"	+
				"DataSource=localhost;"			+
				"Port=3050;"					+
				"Dialect=3;"					+
				"Charset=NONE;"					+
				"Role=;"						+
				"Connection lifetime=15;"		+
				"Pooling=true;"					+
				"Packet Size=8192;"				+
				"ServerType=0";

			FbConnection	myConnection    = new FbConnection(connectionString);

			myConnection.Open();

			FbTransaction	myTransaction	= myConnection.BeginTransaction();

			FbCommand myCommand = new FbCommand();

			myCommand.CommandText	= "UPDATE TEST_TABLE_01 SET CLOB_FIELD = @CLOB_FIELD WHERE INT_FIELD = @INT_FIELD";
			myCommand.Connection	= myConnection;
			myCommand.Transaction	= myTransaction;

			myCommand.Parameters.Add("@INT_FIELD", FbType.Integer, "INT_FIELD");
			myCommand.Parameters.Add("@CLOB_FIELD", FbType.Text, "CLOB_FIELD");           

			myCommand.Parameters[0].Value = 1;
			myCommand.Parameters[1].Value = GetFileContents(@"GDS.CS");

			// Execute Update
			myCommand.ExecuteNonQuery();

			// Commit changes
			myTransaction.Commit();

			// Free command resources in Firebird Server
			myCommand.Dispose();

			// Close connection
			myConnection.Close();
		}

		public static string GetFileContents(string fileName)
		{
			StreamReader reader = new StreamReader(new FileStream(fileName, FileMode.Open));

			string contents = reader.ReadToEnd();

			reader.Close();

			return contents;
		}
	}
}