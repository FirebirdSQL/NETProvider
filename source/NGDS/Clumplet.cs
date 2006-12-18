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
 * 
 *  This file was originally ported from JayBird <http://firebird.sourceforge.net/>
 */

using System;
using System.Runtime.Serialization;
using FirebirdSql.Data.INGDS;

namespace FirebirdSql.Data.NGDS
{
	/// <include file='xmldoc/clumplet.xml' path='doc/member[@name="T:Clumplet"]/*'/>
	[Serializable()]
	internal class Clumplet : IClumplet, IXdrable 
	{
		#region FIELDS

		private int			type;
		private byte[]		content;
		private Clumplet	next;

		#endregion

		#region PROPERTIES

		/// <include file='xmldoc/clumplet.xml' path='doc/member[@name="P:Type"]/*'/>
		public int Type
		{
			get { return type; }
		}

		/// <include file='xmldoc/clumplet.xml' path='doc/member[@name="P:Type"]/*'/>
		public byte[] Content
		{
			get { return content; }
		}

		/// <include file='xmldoc/clumplet.xml' path='doc/member[@name="P:Next"]/*'/>
		public Clumplet Next
		{
			get { return next; }
		}

		/// <include file='xmldoc/clumplet.xml' path='doc/member[@name="P:Length"]/*'/>
		public int Length
		{
			get
			{
				if (next == null)
				{
					return content.Length + 2;
				}
				else
				{
					return content.Length + 2 + next.Length;
				}
			}
		}

		#endregion

		#region CONSTRUCTORS

		/// <include file='xmldoc/clumplet.xml' path='doc/member[@name="M:#ctor(System.Array,System.Int32)"]/*'/>
		internal Clumplet(int type, byte[] content)
		{
			this.type = type;
			this.content = content;
		}

		/// <include file='xmldoc/clumplet.xml' path='doc/member[@name="M:#ctor(FirebirdSql.Data.NGDS.Clumplet)"]/*'/>
		internal Clumplet(Clumplet c)
		{
			this.type = c.type;
			this.content = c.content;
			if (c.next != null) 
			{
				this.next = new Clumplet(c.next);
			}
		}

		#endregion

		#region METHODS
		
		/// <include file='xmldoc/clumplet.xml' path='doc/member[@name="M:Append(FirebirdSql.Data.NGDS.Clumplet)"]/*'/>
		public void Append(IClumplet c) 
		{
			Clumplet ci = (Clumplet)c;
			if (this.type == ci.type) 
			{
				this.content = ci.content;
			}
			else if (next == null) 
			{
				next = ci;
			}
			else 
			{
				next.Append(c);
			}
		}

		/// <include file='xmldoc/clumplet.xml' path='doc/member[@name="M:Find(System.Int32)"]/*'/>
		public byte[] Find(int type)
		{
			if (type == this.type) 
			{
				return content;        
			}
			if (next == null) 
			{
				return null;        
			}

			return next.Find(type);
		}
		
		/// <include file='xmldoc/clumplet.xml' path='doc/member[@name="M:write(FirebirdSql.Data.NGDS.XdrOutputStream)"]/*'/>
		public void Write(XdrOutputStream output)
		{
			output.Write((byte)type);
			output.Write((byte)content.Length);
			output.Write(content);
			if (next != null) 
			{
				next.Write(output);
			}
		}
		
		/// <include file='xmldoc/clumplet.xml' path='doc/member[@name="M:Read(FirebirdSql.Data.NGDS.XdrInputStream,System.Int32)"]/*'/>
		public void Read(XdrInputStream input, int length) 
		{
		}

		/// <include file='xmldoc/clumplet.xml' path='doc/member[@name="M:Equals(System.Object)"]/*'/>
		public override bool Equals(object o) 
		{
			if ((o == null) || !(o is Clumplet))
			{
				return false;
			}
			Clumplet c = (Clumplet)o;
			if (type != c.type || !Array.Equals(content, c.content)) 
			{				
				return false;	// these have different contents
			}
			if (next != null) 
			{
				return next.Equals(c.next);	// we have next, compare with c.next
			}
			return (c.next == null);	// contents the same, we have no next, == if c has no next.
		}

		/// <summary>
		/// Gets the hash of the Clumplet
		/// </summary>
		/// <returns>The hash code</returns>
		public override int GetHashCode() 
		{
			int arrayhash = type;
			
			for (int i = 0; i < content.Length; i++) 
			{
				arrayhash ^= ((int)content[i]) << (8 * (i % 4));
			}

			if (next != null) 
			{
				arrayhash ^= next.GetHashCode();
			}

			return arrayhash;
		}

		#endregion
	}
}
