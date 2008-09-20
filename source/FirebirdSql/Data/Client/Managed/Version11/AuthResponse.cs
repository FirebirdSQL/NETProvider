#if (TRUSTED_AUTH)

using System;
using FirebirdSql.Data.Client.Managed;

namespace FirebirdSql.Data.Client.Managed.Version11
{
    class AuthResponse : IResponse
    {
        #region · Fields ·

        private byte[] data;

        #endregion

        #region · Properties ·

        public byte[] Data
        {
            get { return this.data; }
        }

        #endregion

        #region · Constructors ·

        public AuthResponse(byte[] data)
        {
            this.data = data;
        }

        #endregion
    }
}

#endif
