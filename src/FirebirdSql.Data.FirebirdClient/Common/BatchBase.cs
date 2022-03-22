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

//$Authors = Jiri Cincura (jiri@cincura.net)

using System.Threading;
using System.Threading.Tasks;

namespace FirebirdSql.Data.Common;

internal abstract class BatchBase
{
	public abstract StatementBase Statement { get; }
	public bool MultiError { get; set; }
	public int BatchBufferSize { get; set; }

	public class ExecuteResultItem
	{
		public int RecordsAffected { get; set; }
		public bool IsError { get; set; }
		public IscException Exception { get; set; }
	}
	public abstract ExecuteResultItem[] Execute(int count, IDescriptorFiller descriptorFiller);
	public abstract ValueTask<ExecuteResultItem[]> ExecuteAsync(int count, IDescriptorFiller descriptorFiller, CancellationToken cancellationToken = default);

	public abstract int ComputeBatchSize(int count, IDescriptorFiller descriptorFiller);
	public abstract ValueTask<int> ComputeBatchSizeAsync(int count, IDescriptorFiller descriptorFiller, CancellationToken cancellationToken = default);

	public abstract void Release();
	public abstract ValueTask ReleaseAsync(CancellationToken cancellationToken = default);
}
