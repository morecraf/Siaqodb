using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
#if ASYNC_LMDB
using System.Threading.Tasks;
#endif

namespace Sqo
{
#if KEVAST
    internal
#else
        public
#endif
    class LazyEnumerator<T> : IEnumerator<T>, IEnumerator
#if ASYNC_LMDB
, ISqoAsyncEnumerator<T>
#endif
    {

        private Siaqodb siaqodb;
        private List<int> oids;
        private List<string> propertiesIncluded;
        T current;
        int currentIndex = 0;
        public LazyEnumerator(Siaqodb siaqodb,List<int> oids)
        {
            this.siaqodb = siaqodb;
            this.oids = oids;
        }
        public LazyEnumerator(Siaqodb siaqodb, List<int> oids,List<string> includes)
        {
            this.siaqodb = siaqodb;
            this.oids = oids;
            this.propertiesIncluded = includes;
        }
        #region IEnumerator<T> Members

        public T Current
        {
            get { return this.current; }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            
        }

        #endregion

        #region IEnumerator Members

        object IEnumerator.Current
        {
            get { return this.current; }
        }

        public bool MoveNext()
        {
            if (oids.Count > currentIndex)
            {
                if (propertiesIncluded == null)
                {
                    this.current = siaqodb.LoadObjectByOID<T>(oids[currentIndex]);
                }
                else
                {
                    this.current = siaqodb.LoadObjectByOID<T>(oids[currentIndex], this.propertiesIncluded);
                }
                currentIndex++;
                return true;
            }
            else
            {
                Reset();
            }
            return false;
        }

        public void Reset()
        {
            this.currentIndex = 0;
        }

        #endregion

#if ASYNC_LMDB
        public async Task<bool> MoveNextAsync()
        {
            if (oids.Count > currentIndex)
            {
                if (propertiesIncluded == null)
                {
                    this.current = await siaqodb.LoadObjectByOIDAsync<T>(oids[currentIndex]);
                }
                else
                {
                    this.current = await siaqodb.LoadObjectByOIDAsync<T>(oids[currentIndex], this.propertiesIncluded);
                }
                currentIndex++;
                return true;
            }
            else
            {
                Reset();
            }
            return false;
        }
#endif
    }
#if ASYNC_LMDB    
        public interface ISqoAsyncEnumerator<T>:IEnumerator<T>
        {
            T Current { get; }

            Task<bool> MoveNextAsync();
            void Reset();
        }
#endif
}
