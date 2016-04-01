package dotissi.sqo;

class Where
{
    public Where(String tagOrKey)
    {

        this.TagName = tagOrKey.toLowerCase();

    }
    public WhereOp Operator;
    public String TagName ;
    public Object Value;
    public Integer Skip ;
    public Integer Limit;
    public boolean Descending ;
    public Object[] In ;
    public Object[] Between;

}
enum WhereOp { Equal, NotEqual, LessThan, LessThanOrEqual, GreaterThan, GreaterThanOrEqual, StartWith, EndWith, Contains,In, Between, BetweenExceptStart, BetweenExceptEnd, BetweenExceptStartEnd }
