using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sqo.Documents
{
    public interface IDocQuery<T>:IEnumerable<T>
    {
        Query InnerQuery { get;  }
        IBucket Bucket { get;  }
        List<V> ToObjects<V>();
    }
}
