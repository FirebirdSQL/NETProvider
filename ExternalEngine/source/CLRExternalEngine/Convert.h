/*
 *  .NET External Procedure Engine for Firebird
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
 *  Copyright (c) 2005 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

#ifndef CONVERT_H
#define CONVERT_H

using namespace std;

class Convert
{
public:
	static DISPPARAMS ToDispParams(int count, PARAMDSC** source);
	static void ToParamDsc(VARIANT source, PARAMDSC* target);
	static void Copy(PARAMDSC* source, PARAMDSC* target);

private:
	static int EncodeDate(UDATE date);
	static BSTR DecodeDate(int date);
	static int EncodeTime(UDATE time);
	static BSTR DecodeTime(int time);
	Convert();	
};

#endif