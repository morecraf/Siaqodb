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
        class SqoQuery<T>: ISqoQuery<T>
    {
        Siaqodb siaqodb;
        Expression expression;
        public Expression Expression { get { return expression; } set { expression = value; } }
        public SqoQuery(Siaqodb siaqodb)
        {
            this.siaqodb = siaqodb;
            
        }
        public Siaqodb Siaqodb { get { return siaqodb; } }
        private IObjectList<T> oList;
        private List<int> oidsList;
        public List<int> GetFilteredOids()
        {
            if (expression == null)
                return null;
            else
            {
                return siaqodb.LoadOids<T>(this.expression);
            }
        }
#if ASYNC
        public async Task<List<int>> GetFilteredOidsAsync()
        {
            if (expression == null)
                return null;
            else
            {
                return await siaqodb.LoadOidsAsync<T>(this.expression);
            }
        }
#endif
        public int CountOids()
        {
            if (expression == null)
            {
                return siaqodb.Count<T>();
            }
            else
            {
                return siaqodb.LoadOids<T>(this.expression).Count;
            }
        }
#if ASYNC
        public async Task<int> CountOidsAsync()
        {
            if (expression == null)
            {
                return await siaqodb.CountAsync<T>();
            }
            else
            {
                List<int> list = await siaqodb.LoadOidsAsync<T>(this.expression);
                return list.Count;
            }
        }
        public async Task<IList<T>> ToListAsync()
        {
            if (oList == null)
            {
                if (expression == null)
                {
                    oList = await siaqodb.LoadAllAsync<T>();

                }
                else
                {
                    oList = await siaqodb.LoadAsync<T>(this.expression);

                }
            }
            return oList;
        }
#endif

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
           
           
                if (oList == null)
                {
                    if (expression == null)
                    {
                        oList = siaqodb.LoadAll<T>();
                        
                    }
                    else
                    {
                        oList = siaqodb.Load<T>(this.expression);
                    }
                }
            
            return oList.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
			return (IEnumerator < T > )this.GetEnumerator();
        }

        #endregion

        public LazyEnumerator<T> GetLazyEnumerator()
        {
            if (oidsList == null)
            {
                if (expression == null)
                {
                    oidsList = siaqodb.LoadAllOIDs<T>();

                }
                else
                {
                    oidsList = siaqodb.LoadOids<T>(this.expression);
                }
            }
            return new LazyEnumerator<T>(this.siaqodb, oidsList);
            
        }
#if ASYNC
        public async Task<LazyEnumerator<T>> GetLazyEnumeratorAsync()
        {
            if (oidsList == null)
            {
                if (expression == null)
                {
                    oidsList = await siaqodb.LoadAllOIDsAsync<T>();

                }
                else
                {
                    oidsList = await siaqodb.LoadOidsAsync<T>(this.expression);
                }
            }
            return new LazyEnumerator<T>(this.siaqodb, oidsList);

        }
#endif
        public T GetLast(bool throwExce)
        {
            if (oidsList == null)
            {
                if (expression == null)
                {
                    oidsList = siaqodb.LoadAllOIDs<T>();

                }
                else
                {
                    oidsList = siaqodb.LoadOids<T>(this.expression);
                }
            }
            if (oidsList.Count > 0)
            {
                return siaqodb.LoadObjectByOID<T>(oidsList[oidsList.Count - 1]);
            }
            else
            {
                if (throwExce)
                {
                    throw new InvalidOperationException("no match found");
                }
                else
                {
                    return default(T);
                }
            }
        }
#if ASYNC
        public async Task<T> GetLastAsync(bool throwExce)
        {
            if (oidsList == null)
            {
                if (expression == null)
                {
                    oidsList = await siaqodb.LoadAllOIDsAsync<T>();

                }
                else
                {
                    oidsList = await siaqodb.LoadOidsAsync<T>(this.expression);
                }
            }
            if (oidsList.Count > 0)
            {
                return await siaqodb.LoadObjectByOIDAsync<T>(oidsList[oidsList.Count - 1]);
            }
            else
            {
                if (throwExce)
                {
                    throw new InvalidOperationException("no match found");
                }
                else
                {
                    return default(T);
                }
            }
        }
#endif
        public List<int> GetOids()
        {
            if (expression == null)
                return siaqodb.LoadAllOIDs<T>();
            else
            {
                return siaqodb.LoadOids<T>(this.expression);
            }
        }
#if ASYNC
        public async Task<List<int>> GetOidsAsync()
        {
            if (expression == null)
                return await siaqodb.LoadAllOIDsAsync<T>();
            else
            {
                return await siaqodb.LoadOidsAsync<T>(this.expression);
            }
        }
#endif
       
    }
    
}
