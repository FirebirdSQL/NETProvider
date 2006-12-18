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
using System.Net;
using System.Text;
using System.IO;
using System.Collections;
using System.Globalization;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Gds
{
#if (SINGLE_DLL)
	internal
#else
	public
#endif
	sealed class GdsArray : ArrayBase
	{
		#region Fields

		private long			handle;
		private GdsDbAttachment	db;
		private GdsTransaction	transaction;

		#endregion

		#region Properties

		public override long Handle
		{
			get { return this.handle; }
			set { this.handle = value; }
		}

		public override IDbAttachment DB
		{
			get { return this.db; }
			set { this.db = (GdsDbAttachment)value; }
		}

		public override ITransaction Transaction
		{
			get { return this.transaction; }
			set { this.transaction = (GdsTransaction)value; }
		}

		#endregion

		#region Constructors

		public GdsArray(
			IDbAttachment	db,
			ITransaction	transaction,
			string			tableName, 
			string			fieldName) : this(db, transaction, -1, tableName, fieldName)
		{
		}

		public GdsArray(
			IDbAttachment	db,
			ITransaction	transaction,
			long			handle, 
			string			tableName, 
			string			fieldName) : base(tableName, fieldName)
		{
			if (!(db is GdsDbAttachment))
			{
				throw new ArgumentException("Specified argument is not of GdsDbAttachment type.");
			}
			
			if (!(transaction is GdsTransaction))
			{
				throw new ArgumentException("Specified argument is not of GdsTransaction type.");
			}

			this.db				= (GdsDbAttachment)db;
			this.transaction	= (GdsTransaction)transaction;
			this.handle			= handle;

			if (handle == -1)
			{
				// Lookup array information
				this.LookupDesc();
			}
			else
			{
				// Lookup array information
				this.LookupBounds();
			}
		}

		#endregion

		#region Methods

		public override byte[] GetSlice(int sliceLength)
		{
			lock (this.db) 
			{
				try
				{
					this.db.Send.Write(IscCodes.op_get_slice);	// Op code
					this.db.Send.Write(this.transaction.Handle);// Transaction
					this.db.Send.Write(this.handle);			// Array id
					this.db.Send.Write(sliceLength);			// Slice length
					this.db.Send.WriteBuffer(
						this.generateSDL(this.Descriptor));		// Slice descriptor language
					this.db.Send.Write(String.Empty);			// Slice parameters					
					this.db.Send.Write(0);						// Slice proper
					this.db.Send.Flush();

					return this.receiveSliceResponse(this.Descriptor);
				}
				catch (IOException) 
				{
					throw new IscException(IscCodes.isc_net_read_err);
				}
			}
		}

		public override void PutSlice(System.Array sourceArray, int sliceLength)
		{
			lock (this.db) 
			{
				try 
				{
					byte[] sdl		= this.generateSDL(this.Descriptor);
					byte[] slice	= this.encodeSliceArray(sourceArray);

					this.db.Send.Write(IscCodes.op_put_slice);	// Op code
					this.db.Send.Write(this.transaction.Handle);// Transaction
					this.db.Send.Write(this.handle);			// Array Handle
					this.db.Send.Write(sliceLength);			// Slice length
					this.db.Send.WriteBuffer(sdl);				// Slice descriptor language
					this.db.Send.Write(String.Empty);			// Slice parameters
					this.db.Send.Write(sliceLength);			// Slice length
					this.db.Send.Write(slice, 0, slice.Length);	// Slice proper
					this.db.Send.Flush();

					GdsResponse r = this.db.ReceiveResponse();
					
					this.handle = r.BlobId;
				}
				catch (IOException) 
				{
					throw new IscException(IscCodes.isc_net_read_err);
				}
			}			
		}
	
		#endregion

		#region Protected Methods

		protected override System.Array DecodeSlice(byte[] slice)
		{
			System.Array 	sliceData		= null;			
			int				type			= 0;
			DbDataType		dbType			= DbDataType.Array;
			Type			systemType		= this.GetSystemType();
			Charset			charset			= this.db.Parameters.Charset;
			int[]			lengths 		= new int[this.Descriptor.Dimensions];
			int[]			lowerBounds		= new int[this.Descriptor.Dimensions];
			int				index			= 0;

			// Get upper and lower bounds of each dimension
			for (int i = 0; i < this.Descriptor.Dimensions; i++)
			{
				lowerBounds[i] 	= this.Descriptor.Bounds[i].LowerBound;
				lengths[i] 		= this.Descriptor.Bounds[i].UpperBound;
			}

			// Create arrays
			sliceData = Array.CreateInstance(systemType, lengths, lowerBounds);
			
			System.Array tempData =Array.CreateInstance(systemType, sliceData.Length);

			// Infer Firebird and Db datatypes
			type	= TypeHelper.GetFbType(this.Descriptor.DataType);
			dbType	= TypeHelper.GetDbDataType(this.Descriptor.DataType, 0, this.Descriptor.Scale);

			// Decode slice data
			XdrStream xdr = new XdrStream(slice, this.db.Parameters.Charset);

			while (xdr.Position < xdr.Length)
			{
				switch (dbType)
				{
					case DbDataType.Char:
						tempData.SetValue(xdr.ReadString(this.Descriptor.Length), index);
						break;
					
					case DbDataType.VarChar:
						tempData.SetValue(xdr.ReadString(), index);
						break;
					
					case DbDataType.SmallInt:
						tempData.SetValue(xdr.ReadInt16(), index);
						break;
					
					case DbDataType.Integer:
						tempData.SetValue(xdr.ReadInt32(), index);
						break;
					
					case DbDataType.BigInt:
						tempData.SetValue(xdr.ReadInt64(), index);
						break;

					case DbDataType.Numeric:
					case DbDataType.Decimal:
						tempData.SetValue(xdr.ReadDecimal(type, this.Descriptor.Scale), index);
						break;

					case DbDataType.Float:
						tempData.SetValue(xdr.ReadSingle(), index);
						break;
					
					case DbDataType.Double:
						tempData.SetValue(xdr.ReadDouble(), index);
						break;
		
					case DbDataType.Date:
						tempData.SetValue(xdr.ReadDate(), index);
						break;					
			
					case DbDataType.Time:
						tempData.SetValue(xdr.ReadTime(), index);
						break;					

					case DbDataType.TimeStamp:
						tempData.SetValue(xdr.ReadDateTime(), index);
						break;
				}

				index++;
			}
			
			if (systemType.IsPrimitive)
			{
				// For primitive types we can use System.Buffer to copy generated data to destination array
				System.Buffer.BlockCopy(
					tempData, 0, sliceData, 0, System.Buffer.ByteLength(tempData));
			}
			else
			{
				sliceData = tempData;	
			}

			// Close XDR stream
			xdr.Close();
			
			return sliceData;
		}

		#endregion

		#region Private Methods

		private byte[] receiveSliceResponse(ArrayDesc desc)
		{
			try 
			{
				int op = this.db.ReadOperation();
				if (op == IscCodes.op_slice)
				{
					// Read slice length
					bool	isVariying	= false;
					int		elements	= 0;		
					int		length		= this.db.Receive.ReadInt32();
					length				= this.db.Receive.ReadInt32();
					
					switch (desc.DataType)
					{
						case IscCodes.blr_text: 
						case IscCodes.blr_text2: 
						case IscCodes.blr_cstring: 
						case IscCodes.blr_cstring2:
							elements = length / desc.Length;
							length += elements * ((4 - desc.Length) & 3);
							break;

						case IscCodes.blr_varying: 
						case IscCodes.blr_varying2:
							elements	= length / desc.Length;
							isVariying	= true;
							break;

						case IscCodes.blr_short: 
							length = length * desc.Length;
							break;
					}

					if (isVariying)
					{
						XdrStream xdr = new XdrStream();
												
						for (int i = 0; i < elements; i ++)
						{
							byte[]	buffer = this.db.Receive.ReadOpaque(
								this.db.Receive.ReadInt32());
                            
							xdr.WriteBuffer(buffer, buffer.Length);
						}

						return xdr.ToArray();
					}
					else
					{
						return this.db.Receive.ReadOpaque(length);
					}
				}
				else
				{
					this.db.OP = op;
					
					// Receive standard response
					this.db.ReceiveResponse();

					return null;
				}
			} 
			catch (IOException) 
			{
				throw new IscException(IscCodes.isc_net_read_err);
			}
		}

		private byte[] encodeSliceArray(Array sourceArray)
		{
			IEnumerator i 			= sourceArray.GetEnumerator();
			int			type		= 0;
			DbDataType	dbType		= DbDataType.Array;
			Encoding	encoding	= this.DB.Parameters.Charset.Encoding;
			XdrStream	xdr			= new XdrStream(this.DB.Parameters.Charset);

			type	= TypeHelper.GetFbType(this.Descriptor.DataType);
			dbType	= TypeHelper.GetDbDataType(this.Descriptor.DataType, 0, this.Descriptor.Scale);
			
			while (i.MoveNext())
			{
				switch (dbType)
				{
					case DbDataType.Char:
						byte[] buffer = encoding.GetBytes(i.Current.ToString());
						xdr.WriteOpaque(buffer, this.Descriptor.Length);
						break;

					case DbDataType.VarChar:
						xdr.Write((string)i.Current);
						break;

					case DbDataType.SmallInt:
						xdr.Write((short)i.Current);
						break;

					case DbDataType.Integer:
						xdr.Write((int)i.Current);
						break;

					case DbDataType.BigInt:
						xdr.Write((long)i.Current);
						break;

					case DbDataType.Decimal:
					case DbDataType.Numeric:
						xdr.Write((decimal)i.Current, type, this.Descriptor.Scale);
						break;

					case DbDataType.Float:
						xdr.Write((float)i.Current);
						break;

					case DbDataType.Double:
						xdr.Write((double)i.Current);
						break;

					case DbDataType.Date:
						xdr.WriteDate(Convert.ToDateTime(i.Current, CultureInfo.InvariantCulture.DateTimeFormat));
						break;

					case DbDataType.Time:
						xdr.WriteTime(Convert.ToDateTime(i.Current, CultureInfo.InvariantCulture.DateTimeFormat));
						break;

					case DbDataType.TimeStamp:
						xdr.Write(Convert.ToDateTime(i.Current, CultureInfo.InvariantCulture.DateTimeFormat));
						break;
					
					default:
						throw new NotSupportedException("Unknown data type");
				}
			}

			return xdr.ToArray();
		}

		private byte[] generateSDL(ArrayDesc desc)
		{
			int 			n;
			int 			from;
			int 			to;
			int 			increment;
			int 			dimensions;
			ArrayBound		tail;
			BinaryWriter	sdl;
				
			dimensions = desc.Dimensions;
		
			if (dimensions > 16)
			{
				throw new IscException(IscCodes.isc_invalid_dimension);
			}
		
			sdl = new BinaryWriter(new MemoryStream());
			this.stuff(
				sdl,
				4, 
				IscCodes.isc_sdl_version1, 
				IscCodes.isc_sdl_struct, 
				1,
				desc.DataType);
			
			switch (desc.DataType) 
			{
				case IscCodes.blr_short:
				case IscCodes.blr_long:
				case IscCodes.blr_int64:
				case IscCodes.blr_quad:
					this.stuffSdl(sdl, (byte)desc.Scale);
					break;
			
				case IscCodes.blr_text:
				case IscCodes.blr_cstring:
				case IscCodes.blr_varying:
					this.stuffWord(sdl, desc.Length);
					break;

				default:
					break;
			}
		
			this.stuffString(sdl, IscCodes.isc_sdl_relation, desc.RelationName);
			this.stuffString(sdl, IscCodes.isc_sdl_field, desc.FieldName);
					
			if ((desc.Flags & IscCodes.ARRAY_DESC_COLUMN_MAJOR) == IscCodes.ARRAY_DESC_COLUMN_MAJOR)
			{
				from		= dimensions - 1;
				to 			= -1;
				increment 	= -1;
			}
			else 
			{
				from 		= 0;
				to 			= dimensions;
				increment 	= 1;
			}
		
			for (n = from; n != to; n += increment) 
			{
				tail = desc.Bounds[n];
				if (tail.LowerBound == 1) 
				{
					this.stuff(sdl, 2, IscCodes.isc_sdl_do1, n);
				}
				else 
				{
					this.stuff(sdl, 2, IscCodes.isc_sdl_do2, n);
					
					this.stuffLiteral(sdl, tail.LowerBound);
				}
				this.stuffLiteral(sdl, tail.UpperBound);
			}
		
			this.stuff(
				sdl, 
				5, IscCodes.isc_sdl_element, 
				1, IscCodes.isc_sdl_scalar, 
				0, dimensions);
		
			for (n = 0; n < dimensions; n++)
			{
				this.stuff(sdl, 2, IscCodes.isc_sdl_variable, n);
			}
		
			this.stuffSdl(sdl, IscCodes.isc_sdl_eoc);
			
			return ((MemoryStream)sdl.BaseStream).ToArray();
		}

		private void stuff(
			BinaryWriter	sdl, 
			short			count, 
			params object[] va_arg)
		{	
			for (int i = 0; i < count; i++) 
			{
				sdl.Write(Convert.ToByte(va_arg[i], CultureInfo.InvariantCulture.NumberFormat));
			}		
		}
		
		private void stuff(BinaryWriter sdl, byte[] args)
		{	
			sdl.Write(args);
		}

		private void stuffSdl(BinaryWriter sdl, byte sdl_byte)
		{
			this.stuff(sdl, 1, sdl_byte);
		}

		private void stuffWord(BinaryWriter sdl, short word)
		{
			this.stuff(sdl, BitConverter.GetBytes(word));
		}

		private void stuffLong(BinaryWriter sdl, int word)
		{
			this.stuff(sdl, BitConverter.GetBytes(word));
		}
	
		private void stuffLiteral(BinaryWriter sdl, int literal)
		{
			if (literal >= -128 && literal <= 127)
			{
				this.stuff(sdl, 2, IscCodes.isc_sdl_tiny_integer, literal);
				
				return;
			}
		
			if (literal >= -32768 && literal <= 32767)
			{
				this.stuffSdl(sdl, IscCodes.isc_sdl_short_integer);
				this.stuffWord(sdl, (short)literal);

				return;
			}
			
			this.stuffSdl(sdl, IscCodes.isc_sdl_long_integer);
			this.stuffLong(sdl, literal);
		}
		
		private void stuffString(
			BinaryWriter	sdl, 
			int				sdl_constant, 
			string			sdlValue)
		{
			this.stuffSdl(sdl, (byte)sdl_constant);
			this.stuffSdl(sdl, (byte)sdlValue.Length);
		
			for (int i = 0; i < sdlValue.Length; i++)
			{
				this.stuffSdl(sdl, (byte)sdlValue[i]);
			}
		}

		#endregion
	}	
}
