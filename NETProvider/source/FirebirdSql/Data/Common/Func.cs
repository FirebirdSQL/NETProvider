using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FirebirdSql.Data.Common
{
#if (NET_20)
		delegate TResult Func<TResult>();
		delegate TResult Func<T, TResult>();
#endif
}
