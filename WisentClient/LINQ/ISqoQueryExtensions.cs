using Sqo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptonorClient
{
    public static class ISqoQueryExtensions
    {
        public static async Task<CryptonorResultSet> GetResultSetAsync<T>(this ISqoQuery<T> query)
        {
            CryptonorResultSet res= null;
            CNQuery<CryptonorObject> cnquery = query as CNQuery<CryptonorObject>;
            if (cnquery != null)
            {
                return await cnquery.GetResultSetAsync<T>();
            }
            SqoQuery<CryptonorObject> sqoQuery = query as SqoQuery<CryptonorObject>;
            if (sqoQuery != null)
            {
               
                var all = await sqoQuery.ToListAsync();
                var lastOne = all.OrderBy(a=>a.OID).LastOrDefault();
                long contiToken = 0;
                if (lastOne != null)
                    contiToken = lastOne.OID;
                return new CryptonorResultSet { Objects = all, Count = all.Count, ContinuationToken = contiToken };
            }
            return res;
        }
      
    }
    public class ResultInfo
    {
        public long ContinuationToken{get;set;}
        public int Count{get;set;}
    }
}
