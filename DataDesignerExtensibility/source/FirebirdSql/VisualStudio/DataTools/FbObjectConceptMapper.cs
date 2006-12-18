using System;
using System.Data;
using Microsoft.VisualStudio.Data.AdoDotNet;

namespace FirebirdSql.VisualStudio.DataTools
{
    public class FbObjectConceptMapper : AdoDotNetObjectConceptMapper
    {
        protected override DbType GetDbTypeFromNativeType(string nativeType)
        {
            DataRow[] rows = this.DataTypes.Select(String.Format("TypeName = '{0}'", nativeType));

            if (rows != null && rows.Length > 0)
            {
                return (DbType)Convert.ToInt32(rows[0]["DbType"]);
            }
            
            return base.GetDbTypeFromNativeType(nativeType);
        }

        protected override int GetProviderTypeFromNativeType(string nativeType)
        {
            DataRow[] rows = this.DataTypes.Select(String.Format("TypeName = '{0}'", nativeType));

            if (rows != null && rows.Length > 0)
            {
                return Convert.ToInt32(rows[0]["ProviderDbType"]);
            }
            
            return base.GetProviderTypeFromNativeType(nativeType);
        }

        protected override Type GetFrameworkTypeFromNativeType(string nativeType)
        {
            DataRow[] rows = this.DataTypes.Select(String.Format("TypeName = '{0}'", nativeType));

            if (rows != null && rows.Length > 0)
            {
                return Type.GetType(rows[0]["DataType"].ToString());
            }
            
            return base.GetFrameworkTypeFromNativeType(nativeType);
        }

        /*
        public static string GetDataTypeName(DbDataType dataType)
        {
            switch (dataType)
            {
                case DbDataType.Array:
                    return "ARRAY";

                case DbDataType.Binary:
                    return "BLOB";

                case DbDataType.Text:
                    return "BLOB SUB_TYPE 1";

                case DbDataType.Char:
                case DbDataType.Guid:
                    return "CHAR";

                case DbDataType.VarChar:
                    return "VARCHAR";

                case DbDataType.SmallInt:
                    return "SMALLINT";

                case DbDataType.Integer:
                    return "INTEGER";

                case DbDataType.Float:
                    return "FLOAT";

                case DbDataType.Double:
                    return "DOUBLE PRECISION";

                case DbDataType.BigInt:
                    return "BIGINT";

                case DbDataType.Numeric:
                    return "NUMERIC";

                case DbDataType.Decimal:
                    return "DECIMAL";

                case DbDataType.Date:
                    return "DATE";

                case DbDataType.Time:
                    return "TIME";

                case DbDataType.TimeStamp:
                    return "TIMESTAMP";

                default:
                    return null;
            }
        }
        */
    }
}
