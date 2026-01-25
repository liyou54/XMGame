
public abstract class XmlConvertBase<T,This> where This : XmlConvertBase<T,This>, new()
{
    private static This instance;
    
    public static This Instance => instance ??= new This();
    public abstract bool TryGetData(string str, out T data);
}