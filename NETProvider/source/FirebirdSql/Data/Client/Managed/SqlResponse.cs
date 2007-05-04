using System;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed
{
    internal class SqlResponse : IResponse
    {
        #region · Fields ·

        private int count;

        #endregion

        #region · Properties ·

        public int Count
        {
            get { return this.count; }
        }

        #endregion

        #region · Constructors ·

        public SqlResponse(int count)
        {
            this.count = count;
        }

        #endregion
    }
}
