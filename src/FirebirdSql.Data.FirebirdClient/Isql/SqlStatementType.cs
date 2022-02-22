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

//$Authors = Abel Eduardo Pereira, Jiri Cincura (jiri@cincura.net), Olivier Metod

using System;

namespace FirebirdSql.Data.Isql;

[Serializable]
public enum SqlStatementType
{
	AlterCharacterSet,
	AlterDatabase,
	AlterDomain,
	AlterException,
	AlterExternalFunction,
	AlterFunction,
	AlterIndex,
	AlterPackage,
	AlterProcedure,
	AlterRole,
	AlterSequence,
	AlterTable,
	AlterTrigger,
	AlterView,
	Close,
	CommentOn,
	Commit,
	Connect,
	CreateCollation,
	CreateDatabase,
	CreateDomain,
	CreateException,
	CreateFunction,
	CreateGenerator,
	CreateIndex,
	CreatePackage,
	CreatePackageBody,
	CreateProcedure,
	CreateRole,
	CreateSequence,
	CreateShadow,
	CreateTable,
	CreateTrigger,
	CreateView,
	DeclareCursor,
	DeclareExternalFunction,
	DeclareFilter,
	DeclareStatement,
	DeclareTable,
	Delete,
	Describe,
	Disconnect,
	DropCollation,
	DropDatabase,
	DropDomain,
	DropException,
	DropExternalFunction,
	DropFunction,
	DropFilter,
	DropGenerator,
	DropIndex,
	DropPackage,
	DropPackageBody,
	DropProcedure,
	DropSequence,
	DropRole,
	DropShadow,
	DropTable,
	DropTrigger,
	DropView,
	EndDeclareSection,
	EventInit,
	EventWait,
	Execute,
	ExecuteBlock,
	ExecuteImmediate,
	ExecuteProcedure,
	Fetch,
	Grant,
	Insert,
	InsertCursor,
	Merge,
	Open,
	Prepare,
	RecreateFunction,
	RecreatePackage,
	RecreatePackageBody,
	RecreateProcedure,
	RecreateTable,
	RecreateTrigger,
	RecreateView,
	Revoke,
	Rollback,
	Select,
	SetAutoDDL,
	SetDatabase,
	SetGenerator,
	SetNames,
	SetSQLDialect,
	SetStatistics,
	SetTransaction,
	ShowSQLDialect,
	Update,
	Whenever,
}
