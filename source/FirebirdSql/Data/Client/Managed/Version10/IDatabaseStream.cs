using System;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.Managed.Version10
{
    internal interface IDatabaseStream
    {
        #region · Read Methods ·

        byte[] ReadBytes(int count);

        byte[] ReadOpaque(int length);

        byte[] ReadBuffer();

        string ReadString();

        string ReadString(int length);

        string ReadString(Charset charset);

        string ReadString(Charset charset, int length);

        short ReadInt16();

        int ReadInt32();

        long ReadInt64();

        Guid ReadGuid(int length);

        float ReadSingle();

        double ReadDouble();

        DateTime ReadDateTime();

        DateTime ReadDate();

        DateTime ReadTime();

        decimal ReadDecimal(int type, int scale);

        object ReadValue(DbField field);

        #endregion

        #region · Write Methods ·

        void WriteOpaque(byte[] buffer);

        void WriteOpaque(byte[] buffer, int length);

        void WriteBuffer(byte[] buffer);

        void WriteBuffer(byte[] buffer, int length);

        void WriteBlobBuffer(byte[] buffer);

        void WriteTyped(int type, byte[] buffer);

        void Write(string value);

        void Write(short value);

        void Write(int value);

        void Write(long value);

        void Write(float value);

        void Write(double value);

        void Write(decimal value, int type, int scale);

        void Write(bool value);

        void Write(DateTime value);

        void WriteDate(DateTime value);

        void WriteTime(DateTime value);

        void Write(Descriptor descriptor);

        void Write(DbField param);

        void Write(byte[] buffer, int offset, int count);

        void Flush();

        #endregion

        #region · Reponse Methods ·

        int ReadOperation();

        int NextOperation();

        IResponse ReadResponse();

        IscException ReadStatusVector();

        void SetOperation(int operation);

        void ReleaseObject(int op, int id);

        #endregion
    }
}
