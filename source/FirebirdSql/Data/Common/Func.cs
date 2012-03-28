using System;
using System.Collections.Generic;
using System.Text;

namespace FirebirdSql.Data.Common
{
#if (NET_20)
		delegate TResult Func<TResult>();
		delegate TResult Func<T, TResult>(T arg);
#endif
}
