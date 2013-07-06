using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Linq.Expressions;
#if ASYNC
using System.Threading.Tasks;
using Sqo.Exceptions;
#endif

namespace Sqo
{
	class ProjectionSelectReader<T,TSource> : ISqoQuery<T>
	{

		EnumeratorSelect<T> enumerator;
		SqoQuery<TSource> query;
		public ProjectionSelectReader(List<SqoColumn> columns, Func<ProjectionRow, T> projector, SqoQuery<TSource> query )
		{
			this.enumerator = new EnumeratorSelect<T>(columns, projector);
			this.query = query;
		}
#if ASYNC
        public async Task<IList<T>> ToListAsync()
        {
            EnumeratorSelect<T> e = this.enumerator;
            e.siaqodb = query.Siaqodb;
            List<int> oids = await this.query.GetFilteredOidsAsync();

            if (oids == null)
            {
                oids = await e.siaqodb.LoadAllOIDsAsync<TSource>();

            }
            e.oids = oids;

            if (e == null)
            {
                throw new InvalidOperationException("Cannot enumerate more than once");
            }
            List<T> list = new List<T>();
            while (await e.MoveNextAsync())
            {
                list.Add(e.Current);
            }
            return list;

        }
#endif
		public IEnumerator<T> GetEnumerator()
		{
			EnumeratorSelect<T> e = this.enumerator;
			e.siaqodb = query.Siaqodb;
			List<int> oids = this.query.GetFilteredOids();

            if (oids == null)
            {
                oids = e.siaqodb.LoadAllOIDs<TSource>();

            }
			e.oids = oids;

			if (e == null)
			{
				throw new InvalidOperationException("Cannot enumerate more than once");
			}

			//this.enumerator = null;
			return e;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
        
		


    }
    class EnumeratorSelect<T> : ProjectionRow,  IEnumerator<T>, IEnumerator, IDisposable
#if ASYNC
        ,ISqoAsyncEnumerator<T>
#endif
    {
        List<SqoColumn> columns;
        T current;
        internal Siaqodb siaqodb;
        Func<ProjectionRow, T> projector;
        internal List<int> oids;
        int currentIndex = 0;
#if ASYNC
        int currentColumnIndex = 0;
#endif
        internal EnumeratorSelect(List<SqoColumn> columns, Func<ProjectionRow, T> projector)
        {
            this.columns = columns;
            this.projector = projector;
        }
        public override object GetValue(int index)
        {

            SqoColumn col = columns[index];
            if (col.IsFullObject)
            {
                return siaqodb.LoadObjectByOID(col.SourceType, oids[currentIndex]);
            }
            else
            {
                return siaqodb.LoadValue(oids[currentIndex], col.SourcePropName, col.SourceType);
            }



        }
#if ASYNC
        public async Task<object> GetValueAsync(int index)
        {

            SqoColumn col = columns[index];
            if (col.IsFullObject)
            {
                return await siaqodb.LoadObjectByOIDAsync(col.SourceType, oids[currentIndex]);
            }
            else
            {
                return await siaqodb.LoadValueAsync(oids[currentIndex], col.SourcePropName, col.SourceType);
            }
        }
#endif
        public T Current
        {
            get { return this.current; }
        }
        object IEnumerator.Current
        {
            get { return this.current; }
        }

        public bool MoveNext()
        {
            //if (this.reader.Read())
            if (oids.Count > currentIndex)
            {
                this.current = this.projector(this);
                currentIndex++;
                return true;
            }
            else
            {
                this.Reset();
            }
            return false;
        }
#if ASYNC
       
        public async Task<bool> MoveNextAsync()
        {
            throw new SiaqodbException("Not supported async operation");
        }
#endif
        public void Reset()
        {
            this.currentIndex = 0;
        }
        public void Dispose()
        {

        }

    }

}
