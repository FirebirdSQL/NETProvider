using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirebirdSql.Data.Client.Native.Handle
{
	// public visibility added, because auto-generated assembly can't work with internal types
	public interface IFirebirdHandle
	{
		void SetClient(IFbClient fbClient);
	}
}
