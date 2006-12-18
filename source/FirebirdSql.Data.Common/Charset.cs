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
using System.Text;

namespace FirebirdSql.Data.Common
{
#if (SINGLE_DLL)
	internal
#else
	public
#endif
	sealed class Charset
	{
		#region Static Fields

		private static readonly CharsetCollection supportedCharsets = InitializeSupportedCharsets();

		#endregion

		#region Static Properties

		public static CharsetCollection SupportedCharsets
		{
			get { return supportedCharsets; }
		}

		public static Charset DefaultCharset
		{
			get { return Charset.SupportedCharsets[0]; }
		}

		#endregion

		#region Fields

		private int			id;
		private string		name;
		private Encoding	encoding;
		private int			bytesPerCharacter;

		#endregion

		#region Properties

		public int ID
		{
			get { return this.id; }
		}

		public string Name
		{
			get { return this.name; }
		}

		public Encoding Encoding
		{
			get { return this.encoding; }
		}

		public int BytesPerCharacter
		{
			get { return this.bytesPerCharacter; }
		}

		#endregion

		#region Constructors

		public Charset()
		{
		}

		public Charset(
			int id, 
			string name, 
			int bytesPerCharacter, 
			string systemCharset)
		{
			this.id					= id;
			this.name				= name;
			this.bytesPerCharacter	= bytesPerCharacter;
			this.encoding			= Encoding.GetEncoding(systemCharset);
		}

		public Charset(
			int id, 
			string name,
			int bytesPerCharacter, 
			int cp)
		{
			this.id					= id;
			this.name				= name;
			this.bytesPerCharacter	= bytesPerCharacter;
			this.encoding			= Encoding.GetEncoding(cp);
		}

		public Charset(
			int id, 
			string name, 
			int bytesPerCharacter,
			Encoding encoding)
		{
			this.id					= id;
			this.name				= name;
			this.bytesPerCharacter	= bytesPerCharacter;
			this.encoding			= encoding;
		}

		#endregion

		#region Static Methods

		public static CharsetCollection InitializeSupportedCharsets()
		{
            CharsetCollection charsets = new CharsetCollection();

			// NONE
			Charset.addCharset(charsets, 0, "NONE"		, 1, Encoding.Default);
			// American Standard Code for Information Interchange	
			Charset.addCharset(charsets, 2, "ASCII"		, 1, "ascii");
			// Eight-bit Unicode Transformation Format
			Charset.addCharset(charsets, 3, "UNICODE_FSS"	, 3, "UTF-8");
			// Shift-JIS, Japanese
			Charset.addCharset(charsets, 5, "SJIS_0208"	, 2, "shift_jis");
			// JIS X 0201, 0208, 0212, EUC encoding, Japanese
			Charset.addCharset(charsets, 6, "EUCJ_0208"	, 2, "euc-jp");
			// Windows Japanese	
			Charset.addCharset(charsets, 7, "ISO2022-JP"	, 2, "iso-2022-jp");
			// MS-DOS United States, Australia, New Zealand, South Africa	
			Charset.addCharset(charsets, 10, "DOS437"		, 1, "IBM437");
			// MS-DOS Latin-1				
			Charset.addCharset(charsets, 11, "DOS850"		, 1, "ibm850");
			// MS-DOS Nordic	
			Charset.addCharset(charsets, 12, "DOS865"		, 1, "IBM865");
			// MS-DOS Portuguese	
			Charset.addCharset(charsets, 13, "DOS860"		, 1, "IBM860");
			// MS-DOS Canadian French	
			Charset.addCharset(charsets, 14, "DOS863"		, 1, "IBM863");
			// ISO 8859-1, Latin alphabet No. 1
			Charset.addCharset(charsets, 21, "ISO8859_1"	, 1, "iso-8859-1");
			// ISO 8859-2, Latin alphabet No. 2
			Charset.addCharset(charsets, 22, "ISO8859_2"	, 1, "iso-8859-2");		
			// Windows Korean	
			Charset.addCharset(charsets, 44, "KSC_5601"	, 2, "ks_c_5601-1987");
			// MS-DOS Icelandic	
			Charset.addCharset(charsets, 47, "DOS861"		, 1, "ibm861");
			// Windows Eastern European
			Charset.addCharset(charsets, 51, "WIN1250"	, 1, "windows-1250");
			// Windows Cyrillic
			Charset.addCharset(charsets, 52, "WIN1251"	, 1, "windows-1251");
			// Windows Latin-1
			Charset.addCharset(charsets, 53, "WIN1252"	, 1, "windows-1252");
			// Windows Greek
			Charset.addCharset(charsets, 54, "WIN1253"	, 1, "windows-1253");
			// Windows Turkish
			Charset.addCharset(charsets, 55, "WIN1254"	, 1, "windows-1254");
			// Big5, Traditional Chinese
			Charset.addCharset(charsets, 56, "BIG_5"		, 2, "big5");
			// GB2312, EUC encoding, Simplified Chinese	
			Charset.addCharset(charsets, 57, "GB_2312"	, 2, "gb2312");
			// Windows Hebrew
			Charset.addCharset(charsets, 58, "WIN1255"	, 1, "windows-1255");
			// Windows Arabic	
			Charset.addCharset(charsets, 59, "WIN1256"	, 1, "windows-1256");
			// Windows Baltic	
			Charset.addCharset(charsets, 60, "WIN1257"	, 1, "windows-1257");

            return charsets;
		}

		private static void addCharset(
            CharsetCollection charsets,
			int id, 
			string charset, 
			int bytesPerCharacter, 
			string systemCharset)
		{
			try
			{
				charsets.Add(
					id, 
					charset, 
					bytesPerCharacter, 
					systemCharset);
			}
			catch (Exception)
			{
			}
		}

		private static void addCharset(
            CharsetCollection charsets,
			int id, 
			string charset, 
			int bytesPerCharacter,
			Encoding encoding)
		{
			try
			{
				charsets.Add(
					id, 
					charset, 
					bytesPerCharacter,
					encoding);
			}
			catch (Exception)
			{
			}
		}

		#endregion
	}
}
