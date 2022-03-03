/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/raw/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$Authors = Hennadii Zabula

using System;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;

namespace FirebirdSql.Data.Client.Native.Handles;

// public visibility added, because auto-generated assembly can't work with internal types
public abstract class FirebirdHandle : SafeHandle, IFirebirdHandle
{
	private IFbClient _fbClient;

	protected FirebirdHandle()
		: base(IntPtr.Zero, true)
	{ }

	// Method added because we can't inject IFbClient in ctor
	public void SetClient(IFbClient fbClient)
	{
		Contract.Requires(_fbClient == null);
		Contract.Requires(fbClient != null);
		Contract.Ensures(_fbClient != null);

		_fbClient = fbClient;
	}

	public IFbClient FbClient => _fbClient;

	public override bool IsInvalid => handle == IntPtr.Zero;
}
