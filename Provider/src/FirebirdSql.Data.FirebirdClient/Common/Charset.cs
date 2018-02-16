﻿/*
 *  Firebird ADO.NET Data provider for .NET and Mono
 *
 *     The contents of this file are subject to the Initial
 *     Developer's Public License Version 1.0 (the "License");
 *     you may not use this file except in compliance with the
 *     License. You may obtain a copy of the License at
 *     http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *     Software distributed under the License is distributed on
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *     express or implied.  See the License for the specific
 *     language governing rights and limitations under the License.
 *
 *  Copyright (c) 2002, 2007 Carlos Guzman Alvarez
 *  All Rights Reserved.
 *
 *  Contributors:
 *    Jiri Cincura (jiri@cincura.net)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FirebirdSql.Data.Common
{
	internal sealed class Charset
	{
		#region Constants

		internal const string Octets = "OCTETS";
		internal const string None = "NONE";

		#endregion

		#region Statics

		private readonly static Dictionary<int, Charset> charsetsById;
		private readonly static Dictionary<string, Charset> charsetsByName;

		static Charset()
		{
			var charsets = GetSupportedCharsets();
			charsetsById = charsets.ToDictionary(x => x.Identifier);
			charsetsByName = charsets.ToDictionary(x => x.Name, StringComparer.CurrentCultureIgnoreCase);
		}

		public static Charset DefaultCharset
		{
			get { return charsetsById.First().Value; }
		}

		public static Charset GetCharset(int charsetId)
		{
			if (charsetsById.TryGetValue(charsetId, out var value))
				return value;
			return null;
		}

		public static Charset GetCharset(string charsetName)
		{
			if (charsetsByName.TryGetValue(charsetName, out var value))
				return value;
			return null;
		}

		private static List<Charset> GetSupportedCharsets()
		{
			List<Charset> charsets = new List<Charset>();

			// NONE
			charsets.Add(new Charset(0, None, 1, None));
			// OCTETS
			charsets.Add(new Charset(1, Octets, 1, Octets));
			// American Standard Code for Information Interchange
			charsets.Add(new Charset(2, "ASCII", 1, "ascii"));
			// Eight-bit Unicode Transformation Format
			charsets.Add(new Charset(3, "UNICODE_FSS", 3, "UTF-8"));
			// UTF8
			charsets.Add(new Charset(4, "UTF8", 4, "UTF-8"));

			// Shift-JIS, Japanese
			TryAddCharset(charsets, () => new Charset(5, "SJIS_0208", 2, "shift_jis"));
			// JIS X 0201, 0208, 0212, EUC encoding, Japanese
			TryAddCharset(charsets, () => new Charset(6, "EUCJ_0208", 2, "euc-jp"));
			// Windows Japanese
			TryAddCharset(charsets, () => new Charset(7, "ISO2022-JP", 2, "iso-2022-jp"));
			// MS-DOS United States, Australia, New Zealand, South Africa
			TryAddCharset(charsets, () => new Charset(10, "DOS437", 1, "IBM437"));
			// MS-DOS Latin-1
			TryAddCharset(charsets, () => new Charset(11, "DOS850", 1, "ibm850"));
			// MS-DOS Nordic
			TryAddCharset(charsets, () => new Charset(12, "DOS865", 1, "IBM865"));
			// MS-DOS Portuguese
			TryAddCharset(charsets, () => new Charset(13, "DOS860", 1, "IBM860"));
			// MS-DOS Canadian French
			TryAddCharset(charsets, () => new Charset(14, "DOS863", 1, "IBM863"));
			// ISO 8859-1, Latin alphabet No. 1
			TryAddCharset(charsets, () => new Charset(21, "ISO8859_1", 1, "iso-8859-1"));
			// ISO 8859-2, Latin alphabet No. 2
			TryAddCharset(charsets, () => new Charset(22, "ISO8859_2", 1, "iso-8859-2"));
			// Windows Korean
			TryAddCharset(charsets, () => new Charset(44, "KSC_5601", 2, "ks_c_5601-1987"));
			// MS-DOS Icelandic
			TryAddCharset(charsets, () => new Charset(47, "DOS861", 1, "ibm861"));
			// Windows Eastern European
			TryAddCharset(charsets, () => new Charset(51, "WIN1250", 1, "windows-1250"));
			// Windows Cyrillic
			TryAddCharset(charsets, () => new Charset(52, "WIN1251", 1, "windows-1251"));
			// Windows Latin-1
			TryAddCharset(charsets, () => new Charset(53, "WIN1252", 1, "windows-1252"));
			// Windows Greek
			TryAddCharset(charsets, () => new Charset(54, "WIN1253", 1, "windows-1253"));
			// Windows Turkish
			TryAddCharset(charsets, () => new Charset(55, "WIN1254", 1, "windows-1254"));
			// Big5, Traditional Chinese
			TryAddCharset(charsets, () => new Charset(56, "BIG_5", 2, "big5"));
			// GB2312, EUC encoding, Simplified Chinese
			TryAddCharset(charsets, () => new Charset(57, "GB_2312", 2, "gb2312"));
			// Windows Hebrew
			TryAddCharset(charsets, () => new Charset(58, "WIN1255", 1, "windows-1255"));
			// Windows Arabic
			TryAddCharset(charsets, () => new Charset(59, "WIN1256", 1, "windows-1256"));
			// Windows Baltic
			TryAddCharset(charsets, () => new Charset(60, "WIN1257", 1, "windows-1257"));
			// UTF-16
			//TryAddCharset(charsets, () => new Charset(61, "UTF16", 4, "utf-16"));
			// UTF-32
			//TryAddCharset(charsets, () => new Charset(62, "UTF32", 4, "utf-32"));
			// Russian KOI8R
			TryAddCharset(charsets, () => new Charset(63, "KOI8R", 2, "koi8-r"));
			// Ukrainian KOI8U
			TryAddCharset(charsets, () => new Charset(64, "KOI8U", 2, "koi8-u"));
			// TIS-620 Thai character set, single byte (since Firebird 2.1)
			TryAddCharset(charsets, () => new Charset(65, "TIS620", 1, "tis-620"));
			return charsets;
		}

		private static void TryAddCharset(List<Charset> charsets, Func<Charset> charsetCreator)
		{
			// mainly because of Mono dropping supported charsets
			try
			{
				charsets.Add(charsetCreator());
			}
			catch
			{ }
		}

		#endregion

		#region Fields

		private int _id;
		private int _bytesPerCharacter;
		private string _name;
		private string _systemName;
		private Encoding _encoding;
		private bool _isNone;
		private bool _isOctets;

		#endregion

		#region Properties

		public int Identifier
		{
			get { return _id; }
		}

		public string Name
		{
			get { return _name; }
		}

		public int BytesPerCharacter
		{
			get { return _bytesPerCharacter; }
		}

		public bool IsOctetsCharset
		{
			get { return _isOctets; }
		}

		public bool IsNoneCharset
		{
			get { return _isNone; }
		}

		#endregion

		#region Constructors

		public Charset(int id, string name, int bytesPerCharacter, string systemName)
		{
			_id = id;
			_name = name;
			_bytesPerCharacter = bytesPerCharacter;
			_systemName = systemName;
			_isNone = false;
			_isOctets = false;
			switch (_systemName)
			{
				case None:
					_encoding = Encoding2.Default;
					_isNone = true;
					break;
				case Octets:
					_encoding = new BinaryEncoding();
					_isOctets = true;
					break;
				default:
					_encoding = Encoding.GetEncoding(_systemName);
					break;
			}
		}

		#endregion

		#region Methods

		public byte[] GetBytes(string s)
		{
			return _encoding.GetBytes(s);
		}

		public int GetBytes(string s, int charIndex, int charCount, byte[] bytes, int byteIndex)
		{
			return _encoding.GetBytes(s, charIndex, charCount, bytes, byteIndex);
		}

		public string GetString(byte[] buffer)
		{
			return GetString(buffer, 0, buffer.Length);
		}

		public string GetString(byte[] buffer, int index, int count)
		{
			return _encoding.GetString(buffer, index, count);
		}

		#endregion
	}
}
