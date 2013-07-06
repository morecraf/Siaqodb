using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if ASYNC
using System.Threading.Tasks;
#endif

namespace Sqo.Queries
{
    class Or:ICriteria
    {
        public Or()
        {

        }
        ICriteria criteria1;
        ICriteria criteria2;
        public void Add(ICriteria criteria1, ICriteria criteria2)
        {
            this.criteria1 = criteria1;
            this.criteria2 = criteria2;
        }


        #region ICriteria Members

        public List<int> GetOIDs()
        {
            List<int> list = new List<int>();
            List<int> unu = criteria1.GetOIDs();
            List<int> doi = criteria2.GetOIDs(); 

            #region old version
            /*foreach (int oid in unu)
            {

                list.Add(oid);

            }
            foreach (int oid in doi)
            {
                if (!list.Contains(oid))
                {
                    list.Add(oid);
                }

            }*/
            #endregion

            if (unu.Count < doi.Count)
            {
                

                foreach (int oid in doi)
                {
                    list.Add(oid);
                }
                doi.Sort();
                foreach (int oid in unu)
                {
                    int index = doi.BinarySearch(oid);
                    if (index < 0)
                    {
                        list.Add(oid);
                    }
                }
                return list;
            }
            else
            {
                foreach (int oid in unu)
                {
                    list.Add(oid);
                }
                unu.Sort();
                foreach (int oid in doi)
                {
                    int index = unu.BinarySearch(oid);
                    if (index < 0)
                    {
                        list.Add(oid);
                    }
                }
                return list;
            }
           

           
        }

        

#if ASYNC
        public async Task<List<int>> GetOIDsAsync()
        {
            List<int> list = new List<int>();
            List<int> unu = await criteria1.GetOIDsAsync().ConfigureAwait(false);
            List<int> doi = await criteria2.GetOIDsAsync().ConfigureAwait(false);
            if (unu.Count < doi.Count)
            {


                foreach (int oid in doi)
                {
                    list.Add(oid);
                }
                doi.Sort();
                foreach (int oid in unu)
                {
                    int index = doi.BinarySearch(oid);
                    if (index < 0)
                    {
                        list.Add(oid);
                    }
                }
                return list;
            }
            else
            {
                foreach (int oid in unu)
                {
                    list.Add(oid);
                }
                unu.Sort();
                foreach (int oid in doi)
                {
                    int index = unu.BinarySearch(oid);
                    if (index < 0)
                    {
                        list.Add(oid);
                    }
                }
                return list;
            }

        }
#endif
        #endregion
    }
}
