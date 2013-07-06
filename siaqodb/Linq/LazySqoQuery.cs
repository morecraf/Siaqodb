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
    class LazySqoQuery<T> : ISqoQuery<T>
    {
        protected List<int> oids;
        private Siaqodb siaqodb;
#if ASYNC
        ISqoAsyncEnumerator<T> enumerator;
#else
            IEnumerator<T> enumerator;
#endif
        
        Expression expression;
        public LazySqoQuery(Siaqodb siaqodb, List<int> oids)
        {
            this.oids = oids;
            this.siaqodb = siaqodb;
            this.enumerator = new LazyEnumerator<T>(siaqodb, oids);
        }
        public LazySqoQuery(LazyEnumerator<T> enumerator)
        {

            this.enumerator = enumerator;
        }
        public LazySqoQuery(Siaqodb siaqodb,Expression expression)
        {
            this.siaqodb = siaqodb;
            this.expression = expression;
        }
        public Siaqodb Siaqodb { get { return siaqodb; } }
        public List<int> GetOids()
        {
            return this.oids;
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
                if (oids == null)
                {
                    if (expression == null)
                    {
                        oids = await siaqodb.LoadAllOIDsAsync<T>();

                    }
                    else
                    {
                        oids = await siaqodb.LoadOidsAsync<T>(this.expression);
                    }
                }
                this.enumerator = new LazyEnumerator<T>(siaqodb, oids);
            }
            return this.enumerator;
        }
#endif
        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            if (this.enumerator == null)
            {
                if (oids == null)
                {
                    if (expression == null)
                    {
                        oids = siaqodb.LoadAllOIDs<T>();

                    }
                    else
                    {
                        oids = siaqodb.LoadOids<T>(this.expression);
                    }
                }
                this.enumerator = new LazyEnumerator<T>(siaqodb, oids);
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
