FB.NET - Firebird managed data provider for .Net and Mono
=========================================================

License
-------

Firebird .NET Data Provider - Firebird managed data provider for .NET and Mono
Copyright (C) 2002-2003  Carlos Guzman Alvarez

Distributable under LGPL license.
You may obtain a copy of the License at http://www.gnu.org/copyleft/lesser.html

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.
 
This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
Lesser General Public License for more details.





Requirements for build the sources on Windows
---------------------------------------------

- The Microsoft .NET Framework or Mono:: platform.


Build with Microsoft .Net Framework:

	- You need the Microsoft .NET Platform.

	- The provider sources have a build file for NAnt ( http://nant.sourceforge.net/ ), FirebirdNetProvider.build,
	before build the sources using Nant you need to change two things on it:

		1.- Change the Html Help Compiler Path, 
		2.- Change the NUnit Path.

	For build it you only need to exececute nant on the same directory as the build file.

	- The Nant build file generates:

			1.- FirebirdSql.Data.Firebird.dll ( binary of the ADO .NET data provider )
			2.- FirebirdSql.Data.Firebird.Tests.dll ( binary of the NUnit tests. )
			3.- MSDN style documentation.


Build with Mono:: platform:

	- You nedd Mono:: ( www.go-mono.com ), version > 0.16 ( at this moment CVS Sources ).

	- The provider sources have a build file, makefile, for build the sources ( this script file needs Cygwin ).

	- The makefile build file generates:

		1.- FirebirdSql.Data.Firebird.dll ( binary of the ado .net provider )

	Note : You can build it using NANt too, for this you need to modify the NAnt script changing the build 
	file for allow it.



Requirements for build the sources on Linux
---------------------------------------------

Build with Mono:: platform:

	- You nedd Mono:: ( www.go-mono.com ), version > 0.16 ( at this moment CVS Sources ).

	- The provider sources have a build file, makefile, for build the sources, you only need to execute make on the same
	directory as the script.
	
	- The makefile build file generates:
		1.- FirebirdSql.Data.Firebird.dll ( binary of the ado .net provider  )



Developement list
-----------------

You can subscribe to the developement list at:

	http://lists.sourceforge.net/lists/listinfo/firebird-net-provider


You can access to the lastest developement sources through CVS, see:

	http://sourceforge.net/cvs/?group_id=9028



Reporting Bugs
--------------

Yo can report bugs using two ways:

1. Sending it to the developement list.
2. If you have a Sourceforge ID you can send it using the Bugs section of the Firebird Project web page 
(category .Net Provider):


	http://sourceforge.net/tracker/?group_id=9028&atid=109028
