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

using NUnit.Framework;
using System;
using System.Data;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Client.UnitTest
{
	[TestFixture]
	public class TransactionTest : BaseTest
	{
		[Test]
		public void BeginTransaction()
		{
			// Read Commited
			ITransaction txnRc = this.Attachment.BeginTransaction(BaseTest.BuildTpb());
			txnRc.Commit();

			// Read Uncommited
			ITransaction txnRu = this.Attachment.BeginTransaction(BaseTest.BuildTpb(IsolationLevel.ReadUncommitted));
			txnRu.Commit();

			// Serializable
			ITransaction txnSe = this.Attachment.BeginTransaction(BaseTest.BuildTpb(IsolationLevel.Serializable));
			txnSe.Commit();
		}

		[Test]
		public void Commit()
		{
			// Read Commited
			ITransaction txnRc = this.Attachment.BeginTransaction(BaseTest.BuildTpb());
			txnRc.Commit();

			// Read Uncommited
			ITransaction txnRu = this.Attachment.BeginTransaction(BaseTest.BuildTpb(IsolationLevel.ReadUncommitted));
			txnRu.Commit();

			// Serializable
			ITransaction txnSe = this.Attachment.BeginTransaction(BaseTest.BuildTpb(IsolationLevel.Serializable));
			txnSe.Commit();
		}

		[Test]
		public void Rollback()
		{
			// Read Commited
			ITransaction txnRc = this.Attachment.BeginTransaction(BaseTest.BuildTpb());
			txnRc.Rollback();

			// Read Uncommited
			ITransaction txnRu = this.Attachment.BeginTransaction(BaseTest.BuildTpb(IsolationLevel.ReadUncommitted));
			txnRu.Rollback();

			// Serializable
			ITransaction txnSe = this.Attachment.BeginTransaction(BaseTest.BuildTpb(IsolationLevel.Serializable));
			txnSe.Rollback();
		}

		[Test]
		public void CommitRetaining()
		{
			// Read Commited
			ITransaction txnRc = this.Attachment.BeginTransaction(BaseTest.BuildTpb());
			txnRc.CommitRetaining();
			txnRc.Commit();

			// Read Uncommited
			ITransaction txnRu = this.Attachment.BeginTransaction(BaseTest.BuildTpb(IsolationLevel.ReadUncommitted));
			txnRu.CommitRetaining();
			txnRu.Commit();

			// Serializable
			ITransaction txnSe = this.Attachment.BeginTransaction(BaseTest.BuildTpb(IsolationLevel.Serializable));
			txnSe.CommitRetaining();
			txnSe.Commit();
		}

		[Test]
		public void RollbackRetaining()
		{
			// Read Commited
			ITransaction txnRc = this.Attachment.BeginTransaction(BaseTest.BuildTpb());
			txnRc.RollbackRetaining();
			txnRc.Rollback();

			// Read Uncommited
			ITransaction txnRu = this.Attachment.BeginTransaction(BaseTest.BuildTpb(IsolationLevel.ReadUncommitted));
			txnRu.RollbackRetaining();
			txnRu.Rollback();

			// Serializable
			ITransaction txnSe = this.Attachment.BeginTransaction(BaseTest.BuildTpb(IsolationLevel.Serializable));
			txnSe.RollbackRetaining();
			txnSe.Rollback();
		}

		[Test]
		[Ignore("Not implemented")]
		public void Prepare()
		{
			/* TODO: Implement prepare test case */
		}
	}
}
