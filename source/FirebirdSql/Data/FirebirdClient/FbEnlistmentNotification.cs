/*
 *	Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *	   The contents of this file are subject to the Initial 
 *	   Developer's Public License Version 1.0 (the "License"); 
 *	   you may not use this file except in compliance with the 
 *	   License. You may obtain a copy of the License at 
 *	   http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *	   Software distributed under the License is distributed on 
 *	   an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *	   express or implied. See the License for the specific 
 *	   language governing rights and limitations under the License.
 * 
 *	Copyright (c) 2006 Carlos Guzman Alvarez
 *	All Rights Reserved.
 */

#if (NET)

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Transactions;

namespace FirebirdSql.Data.FirebirdClient
{
    internal sealed class FbEnlistmentNotification : IEnlistmentNotification
    {
        #region · Fields ·

        private Dictionary<FbConnectionInternal, Transaction> transactions;

        #endregion

        #region · Properties ·

        public Dictionary<FbConnectionInternal, Transaction> Transactions
        {
            get
            {
                if (this.transactions == null)
                {
                    this.transactions = new Dictionary<FbConnectionInternal, Transaction>();
                }

                return this.transactions;
            }
        }

        #endregion

        #region · Constructors ·

        public FbEnlistmentNotification()
        {
        }

        #endregion

        #region · IEnlistmentNotification Members ·

        public void Commit(Enlistment enlistment)
        {
            Console.WriteLine("Commit notification received");

            //Do any work necessary when commit notification is received

            //Declare done on the enlistment
            enlistment.Done();
        }

        public void InDoubt(Enlistment enlistment)
        {
            Console.WriteLine("In doubt notification received");

            //Do any work necessary when indout notification is received

            //Declare done on the enlistment
            enlistment.Done();
        }

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            Console.WriteLine("Prepare notification received");

            //Perform transactional work

            //If work finished correctly, reply prepared
            preparingEnlistment.Prepared();

            // otherwise, do a ForceRollback
            // preparingEnlistment.ForceRollback();
        }

        public void Rollback(Enlistment enlistment)
        {
            Console.WriteLine("Rollback notification received");

            //Do any work necessary when commit notification is received

            //Declare done on the enlistment
            enlistment.Done();
        }

        #endregion

        #region · Internal Methods ·

        public void Add(FbConnectionInternal connection, Transaction transaction)
        {
            this.Transactions.Add(connection, transaction);

            transaction.EnlistVolatile(this, System.Transactions.EnlistmentOptions.None);
        }

        #endregion
    }
}

#endif