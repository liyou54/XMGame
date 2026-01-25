public abstract class XmlConvertBase<T>
{
    public abstract bool TryGetData(string str, out T data);
}