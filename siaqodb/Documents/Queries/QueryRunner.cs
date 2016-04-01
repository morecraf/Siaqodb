using LightningDB;
using Sqo.Documents.Indexes;
using Sqo.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Sqo.Documents.Queries
{
    class QueryRunner
    {
        TagsIndexManager indexManag;
        LightningTransaction lmdbTransaction;
        string BucketName;
        public QueryRunner(TagsIndexManager indexManag,LightningTransaction lmdbTransaction, string bucketName)
        {
            this.indexManag = indexManag;
            this.lmdbTransaction = lmdbTransaction;
            this.BucketName = bucketName;
        }
        public List<string> Run(Query query)
        {
            if (query.wheres.Count == 0)
            {
                throw new ArgumentException("Query does not have defined any filtering");
            }
            List<Where> uniqueWheres = new List<Where>();
            uniqueWheres.AddRange(query.wheres);
            this.TryOptimizeBetween(query, uniqueWheres);
            this.MakeSorting(query, uniqueWheres);

            //intersection
            var keys1 = GetBySingleWhere(uniqueWheres[0]);
            for (int i = 1; i < uniqueWheres.Count; i++)
            {
                if (keys1.Count == 0)
                    break;
                var keys2 = GetBySingleWhere(uniqueWheres[i]);
                keys1 = keys1.Intersect(keys2).ToList();
            }
            //union
            foreach(Query or in query.ors)
            {
                var keys2 = this.Run(or);
                var dict = keys1.ToDictionary(a => a);
                foreach (var key in keys2)
                {
                    dict[key] = key;
                }
                keys1 = dict.Values.ToList();
            }
            return keys1;

        }

        private void TryOptimizeBetween(Query query, List<Where> uniqueWheres)
        {
            var dup = query.wheres.GroupBy(x => x.TagName)
                  .Where(g => g.Count() > 1)
                  .Select(y => new { TagName = y.Key, Counter = y.Count() })
                  .ToList();
            foreach (var groupItem in dup)
            {
                var startWhere = query.wheres.Find(a => a.TagName == groupItem.TagName && (a.Operator == WhereOp.GreaterThan || a.Operator == WhereOp.GreaterThanOrEqual));
                var endWhere = query.wheres.Find(a => a.TagName == groupItem.TagName && (a.Operator == WhereOp.LessThan || a.Operator == WhereOp.LessThanOrEqual));
                if (startWhere != null && endWhere != null)
                {
                    Where wNew = new Where(groupItem.TagName);
                    wNew.Between = new object[] { startWhere.Value, endWhere.Value };
                    wNew.Operator = this.GetBetweenOperator(startWhere.Operator, endWhere.Operator);
                    uniqueWheres.Add(wNew);
                    uniqueWheres.Remove(startWhere);
                    uniqueWheres.Remove(endWhere);
                }
            }
        }

        private void MakeSorting(Query query, List<Where> uniqueWheres)
        {
            for (int i = 0; i < query.orderby.Count; i++)
            {
                int index = uniqueWheres.FindIndex(a => a.TagName == query.orderby[i].tagName);
                if (index >= 0)
                {
                    uniqueWheres[index].Descending = query.orderby[i].desc;
                    if (index != i)
                    {
                        Where w = uniqueWheres[index];
                        uniqueWheres.RemoveAt(index);
                        uniqueWheres.Insert(i, w);
                    }

                }
            }
        }

        private List<string> GetBySingleWhere(Where where)
        {
            if (string.Compare(where.TagName, "key", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return this.LoadByKey(where, lmdbTransaction);
            }
            else //by tags
            {
                return this.indexManag.LoadKeysByIndex(where, this.BucketName, lmdbTransaction);
            }
        }

        private WhereOp GetBetweenOperator(WhereOp startOp, WhereOp endOp)
        {
            if (startOp == WhereOp.GreaterThan && endOp == WhereOp.LessThan)
                return WhereOp.BetweenExceptStartEnd;
            else if(startOp == WhereOp.GreaterThanOrEqual && endOp == WhereOp.LessThan)
                return WhereOp.BetweenExceptEnd;
            else if (startOp == WhereOp.GreaterThan && endOp == WhereOp.LessThanOrEqual)
                return WhereOp.BetweenExceptStart;
            else if (startOp == WhereOp.GreaterThanOrEqual && endOp == WhereOp.LessThanOrEqual)
                return WhereOp.Between;

            return WhereOp.Between;
        }

        private List<string> LoadByKey(Where where, LightningTransaction transaction)
        {
            var db = transaction.OpenDatabase(BucketName, DatabaseOpenFlags.Create);
            {
                IIndex index = new IndexKey(db, transaction);
                return IndexQueryFinder.FindKeys(index, where);
            }
        }
    }
}
