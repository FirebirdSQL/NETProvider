using System;
using System.Data;
using Borland.Data.Common;
using Borland.Data.Schema;

namespace FirebirdSql.Data.Bdp
{
	public class FbSQLSchemaCreate : ISQLSchemaCreate
	{
		#region Fields

		private FbConnection connection;
		private string schemaName;
		private string dbName;

		#endregion

		#region Properties

		public bool AutoCommitDDL 
		{
			get { return true; }
		}

		public bool BatchExecute 
		{
			get { return false; }
		}

		public string SchemaName 
		{
			get { return this.schemaName; }
			set { this.schemaName = value; }
		}

		#endregion

		#region Constructors

		public FbSQLSchemaCreate(FbConnection connection)
		{
			this.connection = connection;
			this.dbName = "firebird";
		}

		#endregion

		#region Methods

		public void CreateObject(ObjectType objectType, string objectName, string baseName, BdpColumnCollection columns)
		{
		}

		public void CreateObject(ObjectType objectType, string objectName, string baseName, DataTable table)
		{			
		}

		public void DropObject(ObjectType objectType, string objectName, string baseName)
		{
			switch (objectType)
			{
				case ObjectType.Table:
					this.ExecuteDDL(String.Format("DROP TABLE {0}", objectName));
					break;

				case ObjectType.View:
					this.ExecuteDDL(String.Format("DROP VIEW {0}", objectName));
					break;

				case ObjectType.Procedure:
					this.ExecuteDDL(String.Format("DROP PROCEDURE {0}", objectName));
					break;

				case ObjectType.Index:
					this.ExecuteDDL(String.Format("DROP INDEX {0}", objectName));
					break;
			}
		}

		public DataTable FillSQLTypeMapping(string destination, bool isDefault)
		{
			switch (destination.ToLower())
			{
				case "interbase":
					return this.GetInterbaseMappings(destination);
			}

			return null;
		}

		public DataTable GetDataTypes()
		{
			DataTable dataTypes = BdpMetaDataHelper.GetDataTypes();

			DataRowCollection r = dataTypes.Rows;

			/* Columns:
				RecNo
				TypeName
				BdpType
				BdpSubType
				MaxPrecision
				MaxScale
				NeedPrecision
				NeedScale
				Searchable
			*/
			r.Add(new object[] { 1, "array", BdpType.Array, 0, 0, 0, false, false, true });
			r.Add(new object[] { 2, "bigint", BdpType.Int64, 0, 0, 0, false, false, true });
			r.Add(new object[] { 3, "blob", BdpType.Blob, BdpType.stBinary, 0, 0, false, false, true });
			r.Add(new object[] { 4, "blob sub_type 1", BdpType.Blob, BdpType.stMemo, 0, 0, false, false, true });
			r.Add(new object[] { 5, "char", BdpType.String, BdpType.stFixed, Int16.MaxValue, 0, false, false, true });
			r.Add(new object[] { 6, "date", BdpType.Date, 0, 0, 0, false, false, true });
			r.Add(new object[] { 7, "decimal", BdpType.Decimal, null, 18, 18, true, true, true });
			r.Add(new object[] { 8, "double precision", BdpType.Double, 0, 0, 0, false, false, true });
			r.Add(new object[] { 9, "float", BdpType.Float, 0, 0, 0, false, false, true });
			r.Add(new object[] { 10, "integer", BdpType.Int32, 0, 0, 0, false, false, true });
			r.Add(new object[] { 11, "numeric", BdpType.Decimal, null, 18, 18, true, true, true });
			r.Add(new object[] { 12, "smallint", BdpType.Int16, 0, 0, 0, false, false, true });
			r.Add(new object[] { 13, "time", BdpType.Time, 0, 0, 0, false, false, true });
			r.Add(new object[] { 14, "timestamp", BdpType.DateTime, 0, 0, 0, false, false, true });
			r.Add(new object[] { 15, "varchar", BdpType.String, null, Int16.MaxValue, 0, false, false, true });

			return dataTypes;
		}

		public string GetDDL(ObjectType objectType, string objectName, string baseName, BdpColumnCollection columns)
		{
			return null;
		}

		public string[] GetDDL(ObjectType objectType, string objectName, string baseName, DataTable table)
		{
			return null;
		}

		#endregion

		#region Private Methods

		private void ExecuteDDL(string sql)
		{
			FbCommand command = new FbCommand(this.connection);
			ISQLCursor cursor = null;
			short numCols = 0;

			command.Prepare(sql, 0);
			command.Execute(out cursor, ref numCols);
			command.Release();
		}

		private DataTable GetInterbaseMappings(string destination)
		{
			/* Columns:
				SourceDB
				DestinationDB
				SourceSQLType
				DestinationSQLType
				DestinationPrecision
				DestinationScale
			*/

			DataTable typeMapping = BdpMetaDataHelper.GetSQLTypeMapping();

			foreach (DataRow row in this.GetDataTypes().Rows)
			{
				DataRow newRow = typeMapping.NewRow();

				newRow["Sourcedb"]				= this.dbName;
				newRow["DestinationDB"]			= destination;
				newRow["SourceSQLType"]			= row["TypeName"];
				newRow["DestinationSQLType"]	= row["TypeName"];
				newRow["DestinationPrecision"]	= newRow["MaxPrecision"];
				newRow["DestinationScale"]		= newRow["MaxScale"];

				typeMapping.Rows.Add(newRow);
			}

			return typeMapping;
		}

		#endregion
	}
}