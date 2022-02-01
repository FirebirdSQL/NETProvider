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

//$Authors = Jiri Cincura (jiri@cincura.net)

using System;
using System.IO;
using System.Text;

namespace EntityFramework.Firebird.SqlGen;

internal class SqlWriter : StringWriter
{
	#region Fields

	// We start at -1, since the first select statement will increment it to 0.
	private int _indent = -1;
	private bool _atBeginningOfLine = true;

	#endregion

	#region Properties

	/// <summary>
	/// The number of tabs to be added at the beginning of each new line.
	/// </summary>
	internal int Indent
	{
		get { return _indent; }
		set { _indent = value; }
	}

	#endregion

	#region Constructors

	/// <summary>
	///
	/// </summary>
	/// <param name="b"></param>
	public SqlWriter(StringBuilder b)
		: base(b, System.Globalization.CultureInfo.InvariantCulture)
	{
	}

	#endregion

	#region Methods

	/// <summary>
	/// Reset atBeginningofLine if we detect the newline string.
	/// <see cref="SqlBuilder.AppendLine"/>
	/// Add as many tabs as the value of indent if we are at the
	/// beginning of a line.
	/// </summary>
	/// <param name="value"></param>
	public override void Write(string value)
	{
		if (value == Environment.NewLine)
		{
			base.WriteLine();
			_atBeginningOfLine = true;
		}
		else
		{
			if (_atBeginningOfLine)
			{
				if (_indent > 0)
				{
					base.Write(new string('\t', _indent));
				}
				_atBeginningOfLine = false;
			}
			base.Write(value);
		}
	}

	public override void WriteLine()
	{
		base.WriteLine();
		_atBeginningOfLine = true;
	}

	public override void WriteLine(string value)
	{
		Write(value);
		WriteLine();
	}

	#endregion
}
