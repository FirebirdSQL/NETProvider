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
using System.Text.RegularExpressions;
using System.Collections;
	
namespace FirebirdSql.Data.Firebird.Isql
{	
	/// <include file='Doc/en_EN/FbScript.xml' path='doc/class[@name="FbScript"]/overview/*'/>
	public class FbScript
	{	
		#region STRUCTS
			
		private struct isqlCommand
		{
			public string Name;
			public string Pattern;
			public string DDL;
			
			public isqlCommand(string name, string pattern, string ddl)
			{
				Name 	= name;
				Pattern	= pattern;
				DDL		= ddl;
			}
		}

		#endregion

		#region FIELDS
		
		private ArrayList	isqlCommands = new ArrayList();
		private string		term;
		private int			position;
		
		#endregion
				
		#region CONSTRUCTORS

		/// <include file='Doc/en_EN/FbScript.xml' path='doc/class[@name="FbScript"]/constructor[@name="ctor"]/*'/>
		public FbScript()
		{
			initializeIsqlCommands();		
			term = ";";
		}

		#endregion
		
		#region METHODS

		/// <include file='Doc/en_EN/FbScript.xml' path='doc/class[@name="FbScript"]/method[@name="Parse(System.IO.FileInfo)"]/*'/>
		public ArrayList Parse(FileInfo file)
		{
			if (file.Exists)
			{
				try
				{
					StreamReader	reader = new StreamReader(file.FullName);
					string			script = reader.ReadToEnd();
					reader.Close();

					return this.Parse(script);
				}
				catch
				{
					throw;
				}
			}
			else
			{
				throw new FieldAccessException("The given ISQL script file is not valid (doesn't exists)");
			}
		}

		/// <include file='Doc/en_EN/FbScript.xml' path='doc/class[@name="FbScript"]/method[@name="Parse(System.IO.String,System.Text.Encoding)"]/*'/>
		public ArrayList Parse(string path, Encoding encoding)
		{
			FileInfo file = new FileInfo(path);
			if (file.Exists)
			{
				try
				{
					StreamReader	reader = new StreamReader(file.FullName, encoding);
					string			script = reader.ReadToEnd();
					reader.Close();

					return this.Parse(script);
				}
				catch
				{	
					throw;
				}
			}
			else
			{
				throw new FieldAccessException("The given ISQL script file is not valid (doesn't exists)");
			}
		}

		/// <include file='Doc/en_EN/FbScript.xml' path='doc/class[@name="FbScript"]/method[@name="Parse(System.IO.TextReader,System.Text.Encoding)"]/*'/>
		public ArrayList Parse(FileInfo file, Encoding encoding)
		{
			if (file.Exists)
			{
				try
				{
					StreamReader	reader = new StreamReader(file.FullName, encoding);
					string			script = reader.ReadToEnd();
					reader.Close();

					return this.Parse(script);
				}
				catch
				{	
					throw;
				}
			}
			else
			{
				throw new FieldAccessException("The given ISQL script file is not valid (doesn't exists)");
			}
		}

		/// <include file='Doc/en_EN/FbScript.xml' path='doc/class[@name="FbScript"]/method[@name="Parse(System.IO.TextReader)"]/*'/>
		public ArrayList Parse(TextReader input)
		{
			return this.Parse(input.ReadToEnd());
		}

		/// <include file='Doc/en_EN/FbScript.xml' path='doc/class[@name="FbScript"]/method[@name="Parse(System.String)"]/*'/>
		public ArrayList Parse(string script)
		{
			ArrayList commands = new ArrayList();

			position = 0;
						
			while (position <= script.Length)
			{
				// Get the next terminator position
				int termPos = script.IndexOf(term, position);
				// Calculate length
				int len		= termPos - position + 2;
				
				if (len < 0)
				{
					break;
				}
				
				string commandText = script.Substring(position, termPos - position + 2).Trim();				
				
				// Update actual position
				position = ++termPos;
				
			    commandText = processIsqlCommand(commandText);
			    			    
			    if (commandText != null)
			    {
			    	if (commandText.Trim().Length > 0)
			    	{
			    		// Add new command to the commands collection without the terminator
			    		commands.Add(commandText.Replace(term, String.Empty));
			    	}
			    }					    
			}

			return commands;
		}
		
		#endregion

		#region PRIVATE_METHODS

		private void initializeIsqlCommands()
		{
			isqlCommand command = new isqlCommand();
						
			command.Name	= "COMMIT WORK";
			command.Pattern	= @"COMMIT\s*WORK\s*";
			command.DDL		= "COMMIT WORK";
			
			isqlCommands.Add(command);
						
			command.Name	= "ROLLBACK WORK";
			command.Pattern	= @"ROLLBACK\s*WORK\s*";
			command.DDL		= "ROLLBACK WORK";
			
			isqlCommands.Add(command);
			
			command.Name	= "SET TERM";
			command.Pattern	= @"SET\s*TERM\s*([\w\W\D\d]*)\s*";
			command.DDL		= "";
			
			isqlCommands.Add(command);

			command.Name	= "SET NAMES";
			command.Pattern	= @"SET\s*NAMES\s*([\w\W\D\d]*)\s*";
			command.DDL		= "";
			
			isqlCommands.Add(command);

			command.Name	= "SET SQL DIALECT";
			command.Pattern	= @"SET\s*SQL\s*DIALECT\s*([1-3])\s*";
			command.DDL		= "";
			
			isqlCommands.Add(command);

			command.Name	= "DESCRIBE TABLE";
			command.Pattern	= @"DESCRIBE\s*TABLE\s*([a-zA-Z0-9_$]*)\s*('[a-zA-Z0-9_$\s\S]*')\s*";
			command.DDL		= "update RDB$RELATIONS set RDB$DESCRIPTION = {1} where RDB$RELATION_NAME='{0}'";
			
			isqlCommands.Add(command);

			command.Name	= "DESCRIBE VIEW";
			command.Pattern	= @"DESCRIBE\s*VIEW\s*([a-zA-Z0-9_$]*)\s*('[a-zA-Z0-9_$\s\S]*')\s*";
			command.DDL		= "update RDB$RELATIONS set RDB$DESCRIPTION = {1} where RDB$RELATION_NAME='{0}'";
			
			isqlCommands.Add(command);

			command.Name	= "DESCRIBE FIELD";
			command.Pattern	= @"DESCRIBE\s*FIELD\s*([a-zA-Z0-9_$]*)\s*TABLE\s*([a-zA-Z0-9_$]*)\s*('[a-zA-Z0-9_$\s\S]*')\s*";
			command.DDL		= "update RDB$RELATION_FIELDS set RDB$DESCRIPTION = {2} where (RDB$RELATION_NAME = '{1}') and (RDB$FIELD_NAME = '{0}')";
			
			isqlCommands.Add(command);

			command.Name	= "DESCRIBE DOMAIN";
			command.Pattern	= @"DESCRIBE\s*DOMAIN\s*([a-zA-Z0-9_$]*)\s*('[a-zA-Z0-9_$\s\S]*')\s*";
			command.DDL		= "update RDB$FIELDS set RDB$DESCRIPTION = {1} where RDB$FIELD_NAME='{0}'";
			
			isqlCommands.Add(command);

			command.Name	= "DESCRIBE EXCEPTION";
			command.Pattern	= @"DESCRIBE\s*EXCEPTION\s*([a-zA-Z0-9_$]*)\s*('[a-zA-Z0-9_$\s\S]*')\s*";
			command.DDL		= "update RDB$EXCEPTIONS set RDB$DESCRIPTION = {1} where RDB$EXCEPTION_NAME='{0}'";
			
			isqlCommands.Add(command);

			command.Name	= "DESCRIBE FUNCTION";
			command.Pattern	= @"DESCRIBE\s*FUNCTION\s*([a-zA-Z0-9_$]*)\s*('[a-zA-Z0-9_$\s\S]*')\s*";
			command.DDL		= "update RDB$FUNCTIONS set RDB$DESCRIPTION = {1} where RDB$FUNCTION_NAME='{0}'";
			
			isqlCommands.Add(command);

			command.Name	= "DESCRIBE PROCEDURE";
			command.Pattern	= @"DESCRIBE\s*PROCEDURE\s*([a-zA-Z0-9_$]*)\s*('[a-zA-Z0-9_$\s\S]*')\s*";
			command.DDL		= "update RDB$PROCEDURES set RDB$DESCRIPTION = {1} where RDB$PROCEDURE_NAME='{0}'";
			
			isqlCommands.Add(command);

			command.Name	= "DESCRIBE TRIGGER";
			command.Pattern	= @"DESCRIBE\s*TRIGGER\s*([a-zA-Z0-9_$]*)\s*('[a-zA-Z0-9_$\s\S]*')\s*";
			command.DDL		= "update RDB$TRIGGERS set RDB$DESCRIPTION = {1} where RDB$TRIGGER_NAME='{0}'";
			
			isqlCommands.Add(command);
		}

		private string processIsqlCommand(string commandText)
		{
			StringBuilder 	builder		= new StringBuilder();
			bool 			addCommand 	= true;
			bool			isIsql		= false;
	    	
			foreach (isqlCommand command in isqlCommands)
			{    	
				string pattern = command.Pattern + escapeTerm(term);
	    		
				Regex regEx	= new Regex(pattern, 
					RegexOptions.IgnoreCase|
					RegexOptions.Multiline) ;
				MatchCollection	matches = regEx.Matches(commandText);
				
				if (matches.Count != 0)
				{
					GroupCollection gc = matches[0].Groups;
					
					string[] parameters = new string[gc.Count - 1];
										
					if (gc.Count > 1)
					{
						for (int j = 1; j < gc.Count; j++)
						{
							parameters[j-1] = gc[j].Value.Trim();
						}

						builder.AppendFormat(command.DDL, parameters);
					}
					else
					{
						builder.Append(command.DDL);
					}

					switch (command.Name)
					{
						case "SET TERM":
							addCommand 	= false;
							term 		= gc[1].Value.Trim();
							break;
						
						case "SET SQL DIALECT":
						case "SET NAMES":
							addCommand = false;
							break;
					}
					
					isIsql = true;
					
					break;
				}		    	
			}
			
			if (!addCommand)
			{
				return null;
			}
			else
			{
				if (isIsql)
				{
					return builder.ToString();
				}
				else
				{
					return commandText;		
				}				
			}
		}

		private string escapeTerm(string terminator)
		{
			terminator = terminator.Replace("^", "\\^");
			terminator = terminator.Replace("?", "\\?");
			terminator = terminator.Replace(".", "\\.");
			terminator = terminator.Replace("*", "\\*");
			terminator = terminator.Replace("$", "\\$");			
			
			return terminator;
		}
		
		#endregion
	}
}
