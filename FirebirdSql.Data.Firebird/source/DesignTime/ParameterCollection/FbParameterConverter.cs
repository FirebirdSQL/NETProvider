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
using System.Data;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;

namespace FirebirdSql.Data.Firebird.Design
{
	internal class FbParameterConverter : TypeConverter
	{
		#region Methods

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) 
		{
			if (destinationType == typeof(InstanceDescriptor)) 
			{
				return true;
			}

			return base.CanConvertTo(context, destinationType);
		}
		
		public override object ConvertTo(ITypeDescriptorContext context, 
			CultureInfo culture, object value, Type destinationType) 
		{
			bool parameterNameChanged	= false;
			bool fbDbTypeChanged		= false;
			bool sizeChanged			= false;
			bool valueChanged			= false;
			bool sourceColumnChanged	= false;
			bool miscChanged			= false;
			
			Type[]			ctorTypes	= null;
			object[]		ctorValues	= null;

			if (destinationType == typeof(InstanceDescriptor) && 
				value is FbParameter) 
			{
				FbParameter param = (FbParameter)value;

				if (param.ParameterName != String.Empty)
				{
					parameterNameChanged = true;
				}
				if (param.FbDbType != FbDbType.VarChar)
				{
					fbDbTypeChanged = true;
				}
				if (param.Size != 0)
				{
					sizeChanged = true;
				}
				if (param.Value != null)
				{
					valueChanged = true;
				}
				if (param.SourceColumn != String.Empty)
				{
					sourceColumnChanged = true;
				}
				if (param.Direction != ParameterDirection.Input		||
					param.Precision != 0							||
					param.Scale != 0								||
					param.IsNullable != false						||
					param.SourceVersion	!= DataRowVersion.Current)
				{
					miscChanged = true;
				}
				
				if (parameterNameChanged	&& 
					valueChanged			&&
					!sizeChanged			&&
					!fbDbTypeChanged		&&
					!sourceColumnChanged	&&
					!miscChanged)
				{
					ctorTypes = new Type[] {
											   typeof(string)	, 
											   typeof(object) 
										   };

					ctorValues = new object[] {
												  param.ParameterName, 
												  param.Value
											  };
				}
				else if (
					parameterNameChanged	&& 
					!sizeChanged			&&
					!sourceColumnChanged	&&
					!valueChanged			&&
					!miscChanged)				
				{
					ctorTypes = new Type[] {
											   typeof(string)	, 
											   typeof(FbDbType)
										   };

					ctorValues = new object[] {
												  param.ParameterName, 
												  param.FbDbType
											  };
				}
				else if (
					parameterNameChanged	&& 
					sizeChanged				&&
					!sourceColumnChanged	&&
					!valueChanged			&&
					!miscChanged)				
				{
					ctorTypes = new Type[] {
											   typeof(string)	, 
											   typeof(FbDbType)	, 
											   typeof(int)		, 
					};

					ctorValues = new object[] {
												  param.ParameterName	, 
												  param.FbDbType		,
												  param.Size
											  };
				} 
				else if (
					parameterNameChanged	&& 
					sizeChanged				&&
					sourceColumnChanged		&&
					!valueChanged			&&
					!miscChanged)				
				{
					ctorTypes = new Type[] {
											   typeof(string)	, 
											   typeof(FbDbType)	, 
											   typeof(int)		, 
											   typeof(string)
										   };

					ctorValues = new object[] {
												  param.ParameterName	, 
												  param.FbDbType		,
												  param.Size			,
												  param.SourceColumn
											  };
				} 
				else
				{
					ctorTypes = new Type[] {
											   typeof(string)				,
											   typeof(FbDbType)				, 
											   typeof(int)					, 
											   typeof(ParameterDirection)	,
											   typeof(bool)					, 
											   typeof(byte)					,
											   typeof(byte)					, 
											   typeof(string)				,
											   typeof(DataRowVersion)		, 
											   typeof(object)				
										   };

					ctorValues = new object[] {
												  param.ParameterName	, 
												  param.FbDbType		,
												  param.Size			, 
												  param.Direction		,
												  param.IsNullable		, 
												  param.Precision		,
												  param.Scale			, 
												  param.SourceColumn	,
												  param.SourceVersion	, 
												  param.Value
											  };
				}
			

				ConstructorInfo ctor = typeof(FbParameter).GetConstructor(
									ctorTypes);

				if (ctor != null)
				{
					return new InstanceDescriptor(ctor, ctorValues);
				}
			}

			return base.ConvertTo(context, culture, value, destinationType);      
		}

		#endregion
	}
}
