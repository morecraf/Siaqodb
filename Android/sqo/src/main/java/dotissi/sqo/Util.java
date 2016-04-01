package dotissi.sqo;

 class Util {
    public static int Compare(Object a, Object b)
    {
        int c = 0;
        if (a == null || b == null)
        {
            if (a == b)
                c = 0;
            else if (a == null)
                c = -1;
            else if (b == null)
                c = 1;
        }
        else
        {
            if (b.getClass() != a.getClass())
            {
                b = ChangeType(b, a.getClass());
            }
            c = ((Comparable)a).compareTo(b);
        }
        return c;
    }

    public static Object ChangeType(Object b, Class<? extends Object> class1) {
        return class1.cast(b);
    }
}