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
using System.IO;
using System.Text;

namespace FirebirdSql.Data.Firebird.Gds
{
	#region STRUCTS
	
	internal struct GdsArrayBound
	{
		public int LowerBound;
		public int UpperBound;
	}

	internal struct GdsArrayDesc
	{
		public byte		DataType;
		public short	Scale;			// Scale for numeric datatypes
		public short	Length;			// Legth in bytes of each array element
		public string	FieldName;		// Column name - 32
		public string	RelationName;	// Table name -32
		public short	Dimensions;		// Number of array dimensions 
		public short	Flags;			// Specifies wheter array is to be accesed in
		// row mayor or column-mayor order
		public GdsArrayBound[] Bounds; // Lower and upper bounds for each dimension - 16
	}

	#endregion

	internal abstract class GdsSlice
	{
		#region CONSTANTS

		public const int ARRAY_DESC_COLUMN_MAJOR = 1;	/* Set for FORTRAN */

		#endregion

		#region FIELDS

		private GdsArrayDesc	description;
		private long			handle;
		private GdsDbAttachment	db;
		private GdsTransaction	transaction;
		private string			tableName;
		private string			fieldName;
		private string			rdbFieldName;

		#endregion

		#region PROTECTED_PROPERTIES

		protected GdsArrayDesc Description
		{
			get { return this.description; }
		}

		#endregion

		#region PROPERTIES

		public long Handle
		{
			get { return this.handle; }
			set { this.handle = value; }
		}

		public GdsDbAttachment DB
		{
			get { return this.db; }
			set { this.db = value; }
		}

		public GdsTransaction Transaction
		{
			get { return this.transaction; }
			set { this.transaction = value; }
		}

		#endregion

		#region CONSTRUCTORS

		public GdsSlice(GdsDbAttachment db,
						GdsTransaction transaction,
						long handle,
						string tableName,
						string fieldName)
		{
			this.db				= db;
			this.transaction	= transaction;
			this.handle			= handle;
			this.tableName		= tableName;
			this.fieldName		= fieldName;
		}

		#endregion

		#region METHODS

		protected void LookupBounds()
		{
			this.LookupDesc();

			GdsStatement lookup = new GdsStatement(
				getArrayBounds(), db, this.transaction);

			lookup.Allocate();
			lookup.Prepare();
			lookup.Execute();

			int i = 0;
			this.description.Bounds = new GdsArrayBound[16];
			GdsValue[] values;
			while((values = lookup.Fetch()) != null)
			{
				this.description.Bounds[i].LowerBound = (int)values[0].Value;
				this.description.Bounds[i].UpperBound = (int)values[1].Value;

				i++;
			}
			
			lookup.Drop();
			lookup = null;
		}
	
		protected void LookupDesc()
		{
			// Initializa array description information
			this.description = new GdsArrayDesc();
			
			// Create statement for retrieve information
			GdsStatement lookup = new GdsStatement(
				getArrayDesc(), db, this.transaction);

			lookup.Allocate();
			lookup.Prepare();
			lookup.Execute();

			GdsValue[] values;
			if((values = lookup.Fetch()) != null)
			{								
				this.description.RelationName	= tableName;
				this.description.FieldName		= fieldName;
				this.description.DataType		= Convert.ToByte(values[0].Value);
				this.description.Scale			= Convert.ToInt16(values[1].Value);
				this.description.Length			= Convert.ToInt16(values[2].Value);
				this.description.Dimensions		= Convert.ToInt16(values[3].Value);
				this.description.Flags			= 0;

				rdbFieldName = values[4].Value.ToString().Trim();
			}			
			else
			{
				throw new InvalidOperationException();
			}
			
			lookup.Drop();
			lookup = null;
		}

		protected void SetDesc(System.Array source_array)
		{
			this.description.Dimensions	= (short)source_array.Rank;
			this.description.Bounds		= new GdsArrayBound[16];
			for (int i = 0; i < source_array.Rank; i++)
			{
				if (source_array.GetLowerBound(i) == 0)
				{
					this.description.Bounds[i].LowerBound = source_array.GetLowerBound(i) + 1;
					this.description.Bounds[i].UpperBound = source_array.GetUpperBound(i) + 1;
				}
				else
				{
					this.description.Bounds[i].LowerBound = source_array.GetLowerBound(i);
					this.description.Bounds[i].UpperBound = source_array.GetUpperBound(i);
				}
			}
		}

		protected byte[] GenerateSDL(GdsArrayDesc desc)
		{
			int 			n;
			int 			from;
			int 			to;
			int 			increment;
			int 			dimensions;
			GdsArrayBound	tail;
			BinaryWriter	sdl;
				
			dimensions = desc.Dimensions;
		
			if (dimensions > 16)
			{
				throw new GdsException(GdsCodes.isc_invalid_dimension);
			}
		
			sdl = new BinaryWriter(new MemoryStream());
			stuff(sdl,
						4, 
						GdsCodes.isc_sdl_version1, 
						GdsCodes.isc_sdl_struct, 
						1,
						desc.DataType);
			
			switch (desc.DataType) 
			{
				case GdsCodes.blr_short:
				case GdsCodes.blr_long:
				case GdsCodes.blr_int64:
				case GdsCodes.blr_quad:
					stuffSdl(sdl, (byte)desc.Scale);
					break;
			
				case GdsCodes.blr_text:
				case GdsCodes.blr_cstring:
				case GdsCodes.blr_varying:
					stuffWord(sdl, desc.Length);
					break;

				default:
					break;
			}
		
			stuffString(sdl, GdsCodes.isc_sdl_relation, desc.RelationName);
			stuffString(sdl, GdsCodes.isc_sdl_field, desc.FieldName);
					
			if ((desc.Flags & ARRAY_DESC_COLUMN_MAJOR) == ARRAY_DESC_COLUMN_MAJOR)
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
					stuff(sdl, 2, GdsCodes.isc_sdl_do1, n);
				}
				else 
				{
					stuff(sdl, 2, GdsCodes.isc_sdl_do2, n);
					
					stuffLiteral(sdl, tail.LowerBound);
				}
				stuffLiteral(sdl, tail.UpperBound);
			}
		
			stuff(sdl, 5, GdsCodes.isc_sdl_element, 1, GdsCodes.isc_sdl_scalar, 0, dimensions);
		
			for (n = 0; n < dimensions; n++)
			{
				stuff(sdl, 2, GdsCodes.isc_sdl_variable, n);
			}
		
			stuffSdl(sdl, GdsCodes.isc_sdl_eoc);
			
			return ((MemoryStream)sdl.BaseStream).ToArray();
		}

		protected byte[] GetSlice(int slice_length)
		{
			lock (db) 
			{
				try 
				{					
					db.Send.WriteInt(GdsCodes.op_get_slice);	// Op code
					db.Send.WriteInt(this.transaction.Handle); 	// Transaction
					db.Send.WriteLong(this.handle);				// Array id
					db.Send.WriteInt(slice_length);				// Slice length
					db.Send.WriteBuffer(
						GenerateSDL(this.description));			// Slice description language
					db.Send.WriteString("");					// Slice parameters					
					db.Send.WriteInt(0);						// Slice proper
					db.Send.Flush();

					return receiveSliceResponse(this.Description);
				}
				catch (IOException) 
				{
					throw new GdsException(GdsCodes.isc_net_read_err);
				}
			}
		}

		protected void PutSlice(System.Array source_array, int slice_length)
		{
			lock (db) 
			{
				try 
				{					
					db.Send.WriteInt(GdsCodes.op_put_slice);				// Op code
					db.Send.WriteInt(this.transaction.Handle);				// Transaction
					db.Send.WriteLong(this.handle);							// Array Handle
					db.Send.WriteInt(slice_length);							// Slice length
					db.Send.WriteBuffer(
						GenerateSDL(this.Description));						// Slice description language
					db.Send.WriteString("");								// Slice parameters
					db.Send.WriteSlice(
						this.Description, source_array, slice_length);		// Slice proper
					db.Send.Flush();

					GdsResponse r = db.ReceiveResponse();
					
					handle = r.BlobHandle;
				}
				catch (IOException) 
				{
					throw new GdsException(GdsCodes.isc_net_read_err);
				}
			}			
		}
	
		#endregion

		#region PRIVATE_METHODS

		private string getArrayDesc()
		{
			StringBuilder sql = new StringBuilder();

			sql.Append(
				"SELECT Y.RDB$FIELD_TYPE, Y.RDB$FIELD_SCALE, Y.RDB$FIELD_LENGTH, Y.RDB$DIMENSIONS, X.RDB$FIELD_SOURCE " +
				"FROM RDB$RELATION_FIELDS X, RDB$FIELDS Y " +
				"WHERE X.RDB$FIELD_SOURCE = Y.RDB$FIELD_NAME ");

			if (tableName != null)
			{
				if (tableName.Length != 0)
				{
					sql.AppendFormat(" AND X.RDB$RELATION_NAME = '{0}'", tableName);
				}
			}
					
			if (fieldName != null)
			{
				if (fieldName.Length != 0)
				{
					sql.AppendFormat(" AND X.RDB$FIELD_NAME = '{0}'", fieldName);					
				}
			}
									
			return sql.ToString();
		}

		private string getArrayBounds()
		{
			StringBuilder sql = new StringBuilder();

			sql.Append("SELECT X.RDB$LOWER_BOUND, X.RDB$UPPER_BOUND FROM RDB$FIELD_DIMENSIONS X ");				
					
			if (fieldName != null)
			{
				if (fieldName.Length != 0)
				{
					sql.AppendFormat("WHERE X.RDB$FIELD_NAME = '{0}'", rdbFieldName);
				}
			}

			sql.Append(" ORDER BY X.RDB$DIMENSION");

			return sql.ToString();
		}

		private byte[] receiveSliceResponse(GdsArrayDesc desc)
		{
			try 
			{
				int op = db.ReadOperation();
				if (op == GdsCodes.op_slice)
				{
					return db.Receive.ReadSlice(desc);
				}
				else
				{
					db.OP = op;
					
					// Receive standard response
					db.ReceiveResponse();

					return null;
				}
			} 
			catch (IOException ex) 
			{
				// ex.getMessage() makes little sense here, it will not be displayed
				// because error message for isc_net_read_err does not accept params
				throw new GdsException(GdsCodes.isc_arg_gds, 
										GdsCodes.isc_net_read_err, 
										ex.Message);
			}
		}

		private void stuff(BinaryWriter sdl, short count, params object[] va_arg)
		{	
			for (int i = 0; i < count; i++) 
			{
				sdl.Write(Convert.ToByte(va_arg[i]));
			}		
		}
		
		private void stuff(BinaryWriter sdl, byte[] args)
		{	
			sdl.Write(args);
		}

		private void stuffSdl(BinaryWriter sdl, byte sdl_byte)
		{
			stuff(sdl, 1, sdl_byte);
		}

		private void stuffWord(BinaryWriter sdl, short word)
		{
			stuff(sdl, BitConverter.GetBytes(word));
		}

		private void stuffLong(BinaryWriter sdl, int word)
		{
			stuff(sdl, BitConverter.GetBytes(word));
		}
	
		private void stuffLiteral(BinaryWriter sdl, int literal)
		{
			if (literal >= -128 && literal <= 127)
			{
				stuff(sdl, 2, GdsCodes.isc_sdl_tiny_integer, literal);
				
				return;
			}
		
			if (literal >= -32768 && literal <= 32767)
			{
				stuffSdl(sdl, GdsCodes.isc_sdl_short_integer);
				stuffWord(sdl, (short)literal);

				return;
			}
			
			stuffSdl(sdl, GdsCodes.isc_sdl_long_integer);

			stuffLong(sdl, literal);
		}
		
		private void stuffString(BinaryWriter sdl, int sdl_constant, string sdlValue)
		{
			stuffSdl(sdl, (byte)sdl_constant);
			stuffSdl(sdl, (byte)sdlValue.Length);
		
			for(int i = 0; i < sdlValue.Length; i++)
			{
				stuffSdl(sdl, (byte)sdlValue[i]);
			}
		}

		#endregion
	}
}
