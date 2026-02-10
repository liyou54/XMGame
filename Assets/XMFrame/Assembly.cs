using XM;
using XM.Contracts;
using XM.Utils.Attribute;

[assembly: ModName("Core")]
[assembly: XmlTypeConverter(typeof(TypeConvert), true)]
[assembly: XmlTypeConverter(typeof(TypeConvertI), true)]
[assembly: XmlTypeConverter(typeof(XAssetPathConvert), true)]
[assembly: XmlTypeConverter(typeof(XAssetPathToIConvert), true)]