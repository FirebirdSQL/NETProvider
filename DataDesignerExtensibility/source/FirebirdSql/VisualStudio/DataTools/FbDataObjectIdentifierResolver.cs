/*
 *  Visual Studio 2005 DDEX Provider for Firebird
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

using System;
using System.Data;
using System.Diagnostics;
using System.Collections;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services.SupportEntities;

namespace FirebirdSql.VisualStudio.DataTools
{
    internal class FbDataObjectIdentifierResolver : DataObjectIdentifierResolver
    {
		#region Constructors

        public FbDataObjectIdentifierResolver(IVsDataConnection connection)
			: base(connection)
		{
		}

		#endregion

		#region Public Methods

		public override object[] ExpandIdentifier(string typeName, object[] partialIdentifier)
		{
			if (typeName == null)
			{
				throw new ArgumentNullException("typeName");
			}

			// Find the type in the data object support model
			IVsDataObjectType           type                = null;
			IVsDataObjectSupportModel   objectSupportModel  = Site.GetService(typeof(IVsDataObjectSupportModel)) as IVsDataObjectSupportModel;

			Debug.Assert(objectSupportModel != null);

			if (objectSupportModel != null && objectSupportModel.Types.ContainsKey(typeName))
			{
				type = objectSupportModel.Types[typeName];
			}

			if (type == null)
			{
				throw new ArgumentException("Invalid type " + typeName + ".");
			}

			// Create an identifier array of the correct full length
			object[] identifier = new object[type.Identifier.Count];

			// If the input identifier is not null, copy it to the full
			// identifier array.  If the input identifier's length is less
			// than the full length we assume the more specific parts are
			// specified and thus copy into the rightmost portion of the
			// full identifier array.
			if (partialIdentifier != null)
			{
				if (partialIdentifier.Length > type.Identifier.Count)
				{
					throw new ArgumentException("Invalid partial identifier.");
				}
				
                partialIdentifier.CopyTo(identifier, type.Identifier.Count - partialIdentifier.Length);
			}

			// Get the data source information service
			IVsDataSourceInformation sourceInformation = Site.GetService(typeof(IVsDataSourceInformation)) as IVsDataSourceInformation;
			Debug.Assert(sourceInformation != null);
			
            if (sourceInformation == null)
			{
				// This should never occur
				return identifier;
			}

			// Now expand the identifier as required
			if (type.Identifier.Count > 0)
			{
				// Fill in the current database if not specified
				if (!(identifier[0] is string))
				{
					identifier[0] = sourceInformation[DataSourceInformation.DefaultCatalog] as string;
				}
			}
			if (type.Identifier.Count > 1)
			{
				// Fill in the default schema if not specified
				if (!(identifier[1] is string))
				{
					identifier[1] = sourceInformation[DataSourceInformation.DefaultSchema] as string;
				}
			}

			return identifier;
		}

		public override object[] ContractIdentifier(string typeName, object[] fullIdentifier)
		{
			if (typeName == null)
			{
				throw new ArgumentNullException("typeName");
			}
			if (typeName == FbDataObjectTypes.Root)
			{
				// There is no contraction available
				return base.ContractIdentifier(typeName, fullIdentifier);
			}

			// Find the type in the data object support model
			IVsDataObjectType           type                = null;
			IVsDataObjectSupportModel   objectSupportModel  = Site.GetService(typeof(IVsDataObjectSupportModel)) as IVsDataObjectSupportModel;

			Debug.Assert(objectSupportModel != null);

			if (objectSupportModel != null && objectSupportModel.Types.ContainsKey(typeName))
			{
				type = objectSupportModel.Types[typeName];
			}

			if (type == null)
			{
				throw new ArgumentException("Invalid type " + typeName + ".");
			}

			// Create an identifier array of the correct full length
			object[] identifier = new object[type.Identifier.Count];

			// If the input identifier is not null, copy it to the full
			// identifier array.  If the input identifier's length is less
			// than the full length we assume the more specific parts are
			// specified and thus copy into the rightmost portion of the
			// full identifier array.
			if (fullIdentifier != null)
			{
				if (fullIdentifier.Length > type.Identifier.Count)
				{
					throw new ArgumentException("Invalid full identifier.");
				}

				fullIdentifier.CopyTo(identifier, type.Identifier.Count - fullIdentifier.Length);
			}

			// Get the data source information service
			IVsDataSourceInformation sourceInformation = Site.GetService(typeof(IVsDataSourceInformation)) as IVsDataSourceInformation;
			
            Debug.Assert(sourceInformation != null);

			if (sourceInformation == null)
			{
				// This should never occur
				return identifier;
			}

			// Get the data object member comparer service
			IVsDataObjectMemberComparer objectMemberComparer = Site.GetService(typeof(IVsDataObjectMemberComparer)) as IVsDataObjectMemberComparer;

			Debug.Assert(objectMemberComparer != null);
			
            if (objectMemberComparer == null)
			{
				// This should never occur
				return identifier;
			}

			// Now contract the identifier where possible
			if (type.Identifier.Count > 0)
			{
				// Remove the database if equal to the current database
				if (identifier.Length > 0 && identifier[0] != null)
				{
					string database = sourceInformation[DataSourceInformation.DefaultCatalog] as string;

					if (objectMemberComparer.Compare(FbDataObjectTypes.Root, identifier, 0, database) == 0)
					{
						identifier[0] = null;
					}
				}
			}
			if (type.Identifier.Count > 1)
			{
				// Fill in the default schema if not specified
				if (identifier.Length > 1 && identifier[1] != null)
				{
					string schema = sourceInformation[DataSourceInformation.DefaultSchema] as string;

                    if (objectMemberComparer.Compare(FbDataObjectTypes.Root, identifier, 1, schema) == 0)
					{
						identifier[1] = null;
					}
				}
			}

			return identifier;
		}

		#endregion
    }
}
