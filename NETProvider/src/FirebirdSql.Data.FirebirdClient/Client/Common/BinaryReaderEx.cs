using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirebirdSql.Data.Client.Common
{
	internal static class BinaryReaderEx
	{
		public static IntPtr ReadIntPtr(this BinaryReader self)
		{
			if(IntPtr.Size == sizeof(int))
			{
				return new IntPtr(self.ReadInt32());
			}
			else if(IntPtr.Size == sizeof(long))
			{
				return new IntPtr(self.ReadInt64());
			}
			throw new NotImplementedException();
		}
	}
}
