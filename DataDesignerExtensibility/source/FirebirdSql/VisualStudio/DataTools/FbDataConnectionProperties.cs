/*
 *  Visual Studio DDEX Provider for Firebird
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
using System.Collections;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Framework.AdoDotNet;

namespace FirebirdSql.VisualStudio.DataTools
{
    internal class FbDataConnectionProperties : AdoDotNetConnectionProperties
    {
        #region · Properties ·

        public override bool IsComplete
        {
            get 
            {
                if (!(this["Data Source"] is string) ||
                    (this["Data Source"] as string).Length == 0)
                {
                    return false;
                }
                if (!(this["User ID"] is string) ||
                    (this["User ID"] as string).Length == 0)
                {
                    return false;
                }
                if (!(this["Initial Catalog"] is string) ||
                    (this["Initial Catalog"] as string).Length == 0)
                {
                    return false;
                }

                return true;
            }
        }

        #endregion
    }
}
