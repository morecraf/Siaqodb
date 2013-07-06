using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
#if ASYNC
using System.Threading.Tasks;
#endif

namespace Sqo
{
    #if KEVAST
    internal
#else
        public
#endif
        class IncludeSqoQuery<T> : ISqoQuery<T>
    {
        internal List<string> includes=new List<string>();
        SqoQuery<T> originalQuery;

#if ASYNC
            ISqoAsyncEnumerator<T> enumerator;
#else
            IEnumerator<T> enumerator;
#endif
            public IncludeSqoQuery(SqoQuery<T> query, params string[] properties)
        {
            this.originalQuery = query;
            includes.AddRange(properties);
        }
#if ASYNC
        public async Task<IList<T>> ToListAsync()
        {
            IObjectList<T> list = new ObjectList<T>();
            ISqoAsyncEnumerator<T> asyncEnum = await this.GetEnumeratorAsync();
            while (await asyncEnum.MoveNextAsync())
            {
                list.Add(asyncEnum.Current);
            }
            return list;

        }
        public async Task<ISqoAsyncEnumerator<T>> GetEnumeratorAsync()
        {
            if (this.enumerator == null)
            {
                List<int> oids = await originalQuery.GetOidsAsync();
                this.enumerator = new LazyEnumerator<T>(originalQuery.Siaqodb, oids, includes);
            }
            return this.enumerator;
        }
#endif
        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            if (this.enumerator == null)
            {
                List<int> oids=originalQuery.GetOids();
                this.enumerator = new LazyEnumerator<T>(originalQuery.Siaqodb, oids,includes);
            }
            return this.enumerator;
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.enumerator;
        }

        #endregion
    }
}
