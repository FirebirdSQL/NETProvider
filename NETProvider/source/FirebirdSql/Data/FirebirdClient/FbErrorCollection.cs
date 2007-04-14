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
 *   Jiri Cincura (jiri@cincura.net)
 */

using System;
using System.Collections;
using System.ComponentModel;
using System.Collections.Generic;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.FirebirdClient
{
    /// <include file='Doc/en_EN/FbErrorCollection.xml' path='doc/class[@name="FbErrorCollection"]/overview/*'/>
#if (!NETCF)
    [Serializable, ListBindable(false)]
#endif
    public sealed class FbErrorCollection : IEnumerable, ICollection
    {
        #region · Fields ·

        private List<FbError> errors;

        #endregion

        #region · Indexers ·

        /// <include file='Doc/en_EN/FbErrorCollection.xml' path='doc/class[@name="FbErrorCollection"]/indexer[@name="Item(System.Int32)"]/*'/>
        public FbError this[int index]
        {
            get { return this.errors[index]; }
        }

        #endregion

        #region · Constructors ·

        internal FbErrorCollection()
        {
            this.errors = new List<FbError>();
        }

        #endregion

        #region · ICollection Properties ·

        /// <include file='Doc/en_EN/FbErrorCollection.xml' path='doc/class[@name="FbErrorCollection"]/property[@name="Count"]/*'/>
        public int Count
        {
            get { return this.errors.Count; }
        }

        // implemented explicitly like in SqlErrorCollection
        bool ICollection.IsSynchronized
        {
            get { return ((ICollection)this.errors).IsSynchronized; }
        }

        // implemented explicitly like in SqlErrorCollection
        object ICollection.SyncRoot
        {
            get { return ((ICollection)this.errors).SyncRoot; }
        }

        #endregion

        #region · ICollection Methods ·

        /// <include file='Doc/en_EN/FbErrorCollection.xml' path='doc/class[@name="FbErrorCollection"]/method[@name="CopyTo(System.Array,System.Int32)"]/*'/>	
        public void CopyTo(Array array, int index)
        {
            ((ICollection)this.errors).CopyTo(array, index);
        }

        #endregion

        #region · IEnumerable Methods ·

        public IEnumerator GetEnumerator()
        {
            return this.errors.GetEnumerator();
        }

        #endregion

        #region · Internal Methods ·

        /// <include file='Doc/en_EN/FbErrorCollection.xml' path='doc/class[@name="FbErrorCollection"]/method[@name="IndexOf(System.String)"]/*'/>		
        internal int IndexOf(string errorMessage)
        {
            int index = 0;
            foreach (FbError item in this)
            {
                if (GlobalizationHelper.CultureAwareCompare(item.Message, errorMessage))
                {
                    return index;
                }
                index++;
            }

            return -1;
        }

        /// <include file='Doc/en_EN/FbErrorCollection.xml' path='doc/class[@name="FbErrorCollection"]/method[@name="Add(FbError)"]/*'/>
        internal FbError Add(FbError error)
        {
            this.errors.Add(error);

            return error;
        }

        /// <include file='Doc/en_EN/FbErrorCollection.xml' path='doc/class[@name="FbErrorCollection"]/method[@name="Add(System.String,System.Int32)"]/*'/>
        internal FbError Add(string errorMessage, int number)
        {
            return this.Add(new FbError(errorMessage, number));
        }

        #endregion
    }
}
