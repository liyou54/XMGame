namespace XMFrame.Utils.Attribute
{
    public class XmlIndexAttribute:System.Attribute
    {
        public string IndexName;
        public int IndexGroupId;

        public XmlIndexAttribute(string indexName, int groupId = 0)
        {
            IndexName = indexName;
            IndexGroupId = groupId;
        }
    }
}