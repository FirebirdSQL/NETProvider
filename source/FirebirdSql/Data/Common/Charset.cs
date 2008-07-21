/*
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
using System.Text;

namespace FirebirdSql.Data.Common
{
	internal sealed class Charset
	{
		#region · Static Fields ·

		private readonly static List<Charset> supportedCharsets = Charset.InitializeSupportedCharsets();

		#endregion

		#region · Static Properties ·

		public static Charset DefaultCharset
		{
			get { return Charset.supportedCharsets[0]; }
		}

		#endregion

        #region · Static Methods ·

        public static Charset GetCharset(int charsetId)
        {
            foreach (Charset charset in supportedCharsets)
            {
                if (charset.Identifier == charsetId)
                    return charset;
            }
            return null;
        }

        public static Charset GetCharset(string charsetName)
        {
            foreach (Charset charset in supportedCharsets)
            {
                if (GlobalizationHelper.CultureAwareCompare(charset.Name, charsetName))
                    return charset;
            }
            return null;
        }

        private static List<Charset> InitializeSupportedCharsets()
        {
            List<Charset> charsets = new List<Charset>();

            // NONE
            charsets.Add(new Charset(0, "NONE", 1, "NONE"));
            // OCTETS
            charsets.Add(new Charset(1, "OCTETS", 1, "OCTETS"));
            // American Standard Code for Information Interchange	
            charsets.Add(new Charset(2, "ASCII", 1, "ascii"));
            // Eight-bit Unicode Transformation Format
            charsets.Add(new Charset(3, "UNICODE_FSS", 3, "UTF-8"));
            // UTF8
            charsets.Add(new Charset(4, "UTF8", 4, "UTF-8"));
            // Shift-JIS, Japanese
            charsets.Add(new Charset(5, "SJIS_0208", 2, "shift_jis"));
            // JIS X 0201, 0208, 0212, EUC encoding, Japanese
            charsets.Add(new Charset(6, "EUCJ_0208", 2, "euc-jp"));
            // Windows Japanese	
            charsets.Add(new Charset(7, "ISO2022-JP", 2, "iso-2022-jp"));
            // MS-DOS United States, Australia, New Zealand, South Africa	
            charsets.Add(new Charset(10, "DOS437", 1, "IBM437"));
            // MS-DOS Latin-1				
            charsets.Add(new Charset(11, "DOS850", 1, "ibm850"));
            // MS-DOS Nordic	
            charsets.Add(new Charset(12, "DOS865", 1, "IBM865"));
            // MS-DOS Portuguese	
            charsets.Add(new Charset(13, "DOS860", 1, "IBM860"));
            // MS-DOS Canadian French	
            charsets.Add(new Charset(14, "DOS863", 1, "IBM863"));
            // ISO 8859-1, Latin alphabet No. 1
            charsets.Add(new Charset(21, "ISO8859_1", 1, "iso-8859-1"));
            // ISO 8859-2, Latin alphabet No. 2
            charsets.Add(new Charset(22, "ISO8859_2", 1, "iso-8859-2"));
#if (NET)
            // Windows Korean	
            charsets.Add(new Charset(44, "KSC_5601", 2, "ks_c_5601-1987"));
#endif
            // MS-DOS Icelandic	
            charsets.Add(new Charset(47, "DOS861", 1, "ibm861"));
            // Windows Eastern European
            charsets.Add(new Charset(51, "WIN1250", 1, "windows-1250"));
            // Windows Cyrillic
            charsets.Add(new Charset(52, "WIN1251", 1, "windows-1251"));
            // Windows Latin-1
            charsets.Add(new Charset(53, "WIN1252", 1, "windows-1252"));
            // Windows Greek
            charsets.Add(new Charset(54, "WIN1253", 1, "windows-1253"));
            // Windows Turkish
            charsets.Add(new Charset(55, "WIN1254", 1, "windows-1254"));
            // Big5, Traditional Chinese
            charsets.Add(new Charset(56, "BIG_5", 2, "big5"));
            // GB2312, EUC encoding, Simplified Chinese	
            charsets.Add(new Charset(57, "GB_2312", 2, "gb2312"));
            // Windows Hebrew
            charsets.Add(new Charset(58, "WIN1255", 1, "windows-1255"));
            // Windows Arabic	
            charsets.Add(new Charset(59, "WIN1256", 1, "windows-1256"));
            // Windows Baltic	
            charsets.Add(new Charset(60, "WIN1257", 1, "windows-1257"));
            // UTF-16
            // Charset.Add(new Charset(61, "UTF16", 4, "utf-16"));
            // UTF-32
            // Charset.Add(new Charset(62, "UTF32", 4, "utf-32"));
            // Russian KOI8R
            charsets.Add(new Charset(63, "KOI8R", 2, "koi8-r"));
            // Ukrainian KOI8U
            charsets.Add(new Charset(64, "KOI8U", 2, "koi8-u"));

            return charsets;
        }

        #endregion

		#region · Fields ·

		private int		    id;
		private int		    bytesPerCharacter;
		private string	    name;
		private string	    systemName;
        private Encoding    encoding;
        private object      syncObject;

		#endregion

		#region · Properties ·

		public int Identifier
		{
			get { return this.id; }
		}

		public string Name
		{
			get { return this.name; }
		}

		public int BytesPerCharacter
		{
			get { return this.bytesPerCharacter; }
		}

        public bool IsOctetsCharset
        {
            get { return (this.id == Charset.GetCharset("OCTETS").Identifier); }
        }

		#endregion

		#region · Constructors ·

		public Charset(int id, string name, int bytesPerCharacter, string systemName)
		{
			this.id					= id;
			this.name				= name;
			this.bytesPerCharacter	= bytesPerCharacter;
			this.systemName			= systemName;
            this.syncObject         = new object();

#if (!NETCF)
            this.GetEncoding();
#endif
		}

		#endregion

		#region · Methods ·

		public byte[] GetBytes(string s)
		{
#if (NETCF)
			return this.GetEncoding().GetBytes(s);
#else
            return this.encoding.GetBytes(s);
#endif
		}

		public int GetBytes(string s, int charIndex, int charCount, byte[] bytes, int byteIndex)
		{
#if (NETCF)
			return this.GetEncoding().GetBytes(s, charIndex, charCount, bytes, byteIndex);
#else
            return this.encoding.GetBytes(s, charIndex, charCount, bytes, byteIndex);
#endif
		}

		public string GetString(byte[] buffer)
		{
			return this.GetString(buffer, 0, buffer.Length);
		}

		public string GetString(byte[] buffer, int index, int count)
		{
#if (NETCF)
			return this.GetEncoding().GetString(buffer, index, count);
#else
            return this.encoding.GetString(buffer, index, count);
#endif
		}

		#endregion

		#region · Private Methods ·

		private Encoding GetEncoding()
		{
            lock (this.syncObject)
            {
                if (this.encoding == null)
                {
                    switch (this.systemName)
                    {
                        case "NONE":
                            this.encoding = Encoding.Default;
                            break;

                        case "OCTETS":
                            this.encoding = new BinaryEncoding();
                            break;

                        default:
                            this.encoding = Encoding.GetEncoding(this.systemName);
                            break;
                    }
                }
            }

            return this.encoding;
		}

		#endregion
	}
}
