/*
 *  Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.ibphoenix.com/main.nfs?a=ibphoenix&l=;PAGES;NAME='ibp_idpl'
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2002, 2004 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using System;
using System.Runtime.InteropServices;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Embedded
{
#if (SINGLE_DLL)
	internal
#else
	public
#endif
	abstract class FesAttachment : IAttachment
	{
		#region Fields

		private AttachmentParams	parameters;
		private int					handle;

		#endregion

		#region Properties

		public AttachmentParams Parameters
		{
			get { return this.parameters; }
		}

		public int Handle
		{
			get { return this.handle; }
			set { this.handle = value; }
		}

		public FactoryBase Factory
		{
			get { return FesFactory.Instance; }
		}

		public bool IsLittleEndian
		{
			get { return true; }
		}

		#endregion

		#region Constructors

		protected FesAttachment(AttachmentParams parameters)
		{
			this.parameters = parameters;
		}

		#endregion

		#region Static Methods

		public static int[] GetNewStatusVector()
		{
			return new int[IscCodes.ISC_STATUS_LENGTH];
		}

		#endregion

		#region Methods

		public int VaxInteger(byte[] buffer, int index, int length) 
		{
			return IscHelper.VaxInteger(buffer, index, length);

			/*
			byte[] innerBuffer = new byte[length];

			Buffer.BlockCopy(buffer, pos, innerBuffer, 0, length);

			return FbClient.isc_vax_integer(innerBuffer, (short)length);
			*/
		}

		#endregion

		#region Abstract Methods

		public abstract void SendWarning(IscException ex);

		#endregion

		#region Internal Methods

		internal void ParseStatusVector(int[] statusVector)
		{
			IscException exception = new IscException();

			for (int i = 0; i < statusVector.Length;)
			{
				int arg = statusVector[i++];
				switch (arg) 
				{
					case IscCodes.isc_arg_gds: 
						int er = statusVector[i++];
						if (er != 0) 
						{
							exception.Errors.Add(arg, er);
						}
						break;

					case IscCodes.isc_arg_end:
					{		
						if (exception.Errors.Count != 0 && !exception.IsWarning()) 
						{
							exception.BuildExceptionMessage();
							throw exception;
						}
						else
						{
							if (exception.Errors.Count != 0 && exception.IsWarning())
							{
								exception.BuildExceptionMessage();
								this.SendWarning(exception);
							}
						}
					}
					return;
					
					case IscCodes.isc_arg_interpreted:						
					case IscCodes.isc_arg_string:
					{
						IntPtr ptr = new IntPtr(statusVector[i++]);
						string arg_value = Marshal.PtrToStringAnsi(ptr);
						exception.Errors.Add(arg, arg_value);
					}
					break;

					case IscCodes.isc_arg_cstring:
					{
						int count = statusVector[i++];

						IntPtr ptr = new IntPtr(statusVector[i++]);
						string arg_value = Marshal.PtrToStringAnsi(ptr);
						exception.Errors.Add(arg, arg_value);
					}
					break;
					
					case IscCodes.isc_arg_win32:
					case IscCodes.isc_arg_number:
					{
						int arg_value = statusVector[i++];
						exception.Errors.Add(arg, arg_value);
					}
					break;
					
					default:
					{
						int e = statusVector[i++];
						if (e != 0) 
						{
							exception.Errors.Add(arg, e);
						}
					}
					break;
				}
			}
		}

		#endregion
	}
}
