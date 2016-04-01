package dotissi.sqo;


public class SiaqodbConfigurator {
    private static IDocumentSerializer DocumentSerializer;
    public static IDocumentSerializer getSerializer()
    {
        return DocumentSerializer;
    }
    public static void setSerializer(IDocumentSerializer serializer)
    {
         DocumentSerializer=serializer;
    }

    public static boolean IsBucketSyncable(String bucketName) {
        return false;
    }
}
