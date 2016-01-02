using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FirebirdSql.Data.Client.Native.Handle
{
	// public visibility added, because auto-generated assembly can't work with internal types
	public abstract class FirebirdHandle : SafeHandle , IFirebirdHandle
	{
		protected IFbClient _fbClient;

		protected FirebirdHandle()
			: base(IntPtr.Zero, true)
		{

		}

		// Method added because we can't inject IFbClient in ctor
		void IFirebirdHandle.SetClient(IFbClient fbClient)
		{
			Contract.Requires(_fbClient == null); // We shouldn't set this if already set
			Contract.Requires(fbClient != null);
			Contract.Ensures(_fbClient != null);

			_fbClient = fbClient;
		}

		public override bool IsInvalid
		{
			get
			{
				return handle == IntPtr.Zero;
			}
		}
	}
}
