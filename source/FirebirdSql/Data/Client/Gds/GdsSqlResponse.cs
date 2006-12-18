using System;

namespace FirebirdSql.Data.Client.Gds
{
    class GdsSqlResponse : IResponse
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

        public GdsSqlResponse(int count)
        {
            this.count = count;
        }

        #endregion
    }
}
