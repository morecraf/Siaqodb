package dotissi.sqo;

import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.HashMap;
import java.util.Objects;


public class Document {
    public String key;
    public byte[] content;
    public String version;
    public HashMap<String, Object> tags;
    public HashMap<String, Object> getTags() {
        return tags;
    }
    public void setTags(HashMap<String, Object> tags) {
        this.tags = tags;
    }
    public String getKey() {
        return key;
    }
    public void setKey(String key) {
        this.key = key;
    }
    public String getVersion() {
        return version;
    }
    public void setVersion(String version) {
        this.version = version;
    }
    public byte[] getRawContent() {
        return content;
    }
    public void setRawContent(byte[] content) {
        this.content = content;
    }
    public Object getContent(Class<? extends Object> type) {
        if (this.content == null || this.content.length == 0)
            return null;
        Object obj = SiaqodbConfigurator.getSerializer().deserialize(type, this.content);
        //TODO
        /*IVersionConvention versionConvention=CryptonorConfigurator.getVersionConvention();
        if(versionConvention!=null)
        {
            versionConvention.setVersion(this.getVersion(),obj);
        }*/
        return obj;
    }
    public void setContent(Object content) {
          if (content == null)
            throw new IllegalArgumentException("content");

       //TODO key conventions
        /*IKeyConvention keyConvention=CryptonorConfigurator.getKeyConvention();
        if(this.key==null && keyConvention!=null)
        {
            this.key=keyConvention.getKey(objValue);
        }
        IVersionConvention versionConvention=CryptonorConfigurator.getVersionConvention();
        if(this.version==null && versionConvention!=null)
        {
            this.version= versionConvention.getVersion(objValue);
        }*/
        byte[] serializedObj = SiaqodbConfigurator.getSerializer().serialize(content);
        this.content = serializedObj;

    }
    public void setTag(String tagName, Object value) throws SiaqodbException {
            tagName = tagName.toLowerCase();
        Class<? extends Object> type = value.getClass();
        if (tags == null)
            tags = new HashMap<String, Object>();
        if (type == int.class || type == long.class || type == Long.class || type == Integer.class)
        {
            tags.put(tagName,Long.parseLong(value.toString()));
        }
        else if ( type == float.class|| type == Float.class || type == double.class ||type == Double.class)
        {
            tags.put(tagName,Double.parseDouble(value.toString()));
        }

        else if ( type == String.class
                || type == boolean.class|| type == Boolean.class || type==Date.class)
        {
            tags.put(tagName,value);
        }

        else
        {
            throw new SiaqodbException("Tag type:" + type.toString() + " not supported.");
        }
    }
    public Object getTag(String tagName,Class<? extends Object> type) throws ParseException
    {
        if (tags != null)
        {
            tagName = tagName.toLowerCase();
            if (tags.containsKey(tagName))
            {
                if (tags.get(tagName).getClass() != type)
                {
                    if(type == Date.class){
                        SimpleDateFormat myDateFormat= new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss");

                        return  myDateFormat.parse(tags.get(tagName).toString());
                    }
                    else if(type == int.class || type == Integer.class ){
                        return type.cast(Integer.parseInt( tags.get(tagName).toString()));
                    }
                    else if(type == float.class || type == Float.class ){
                        return type.cast(Float.parseFloat( tags.get(tagName).toString()));
                    }
                    return type.cast(tags.get(tagName));
                }
                else
                {
                    return tags.get(tagName);
                }
            }
        }
        return null;
    }
}
