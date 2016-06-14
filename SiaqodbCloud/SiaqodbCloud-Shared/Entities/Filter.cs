

namespace SiaqodbCloud
{
    
    public class Filter
    {
        public Filter(string tagOrKey)
        {
            this.TagName = tagOrKey;

        }
        public string TagName { get; set; }
        public object Value { get; set; }
        public object Start { get; set; }
        public object End { get; set; }
      
        public object[] In { get; set; }

       
    }
}
