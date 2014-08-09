using System;
namespace Cryptonor.Queries
{
    public interface ICryptonorQuery
    {
         string TagName { get; set; }
         object Value { get; set; }
         object Start { get; set; }
         object End { get; set; }
         int? Skip { get; set; }
         int? Limit { get; set; }
         bool? Descending { get; set; }
         string TagType { get; set; }
         object[] In { get; set; }
    }
}
