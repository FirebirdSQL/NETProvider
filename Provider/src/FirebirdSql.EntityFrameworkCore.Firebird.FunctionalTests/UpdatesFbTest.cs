/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/blob/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$Authors = Jiri Cincura (jiri@cincura.net)

using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestModels.UpdatesModel;
using Xunit;

namespace FirebirdSql.EntityFrameworkCore.Firebird.FunctionalTests
{
	public class UpdatesFbTest : UpdatesRelationalTestBase<UpdatesFbFixture>
	{
		public UpdatesFbTest(UpdatesFbFixture fixture)
			: base(fixture)
		{ }

		public override void Identifiers_are_generated_correctly()
		{
			//using (var context = CreateContext())
			//{
			//	var entityType = context.Model.FindEntityType(typeof(LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorkingCorrectly));
			//	Assert.Equal(
			//		"LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorkingCorrectly",
			//		entityType.Relational().TableName);
			//	Assert.Equal(
			//		"PK_LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorkingCorrectly",
			//		entityType.GetKeys().Single().Relational().Name);
			//	Assert.Equal(
			//		"FK_LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorkingCorrectly_Profile_ProfileId_ProfileId1_ProfileId3_ProfileId4_ProfileId5_ProfileId6_ProfileId7_ProfileId8_ProfileId9_ProfileId10_ProfileId11_ProfileId12_ProfileId13_ProfileId14",
			//		entityType.GetForeignKeys().Single().Relational().Name);
			//	Assert.Equal(
			//		"IX_LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorkingCorrectly_ProfileId_ProfileId1_ProfileId3_ProfileId4_ProfileId5_ProfileId6_ProfileId7_ProfileId8_ProfileId9_ProfileId10_ProfileId11_ProfileId12_ProfileId13_ProfileId14_ExtraProperty",
			//		entityType.GetIndexes().Single().Relational().Name);
			//}
		}
	}
}
