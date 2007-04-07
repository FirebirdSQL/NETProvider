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
    }
}
