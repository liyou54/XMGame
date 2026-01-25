using Cysharp.Threading.Tasks;
using XMFrame.Implementation;
using XMFrame.Interfaces;
using System.Xml;
using XMFrame.Utils;

namespace XMFrame
{
    public class ConfigDataCenter : ManagerBase<IConfigDataCenter>
    {
        public override UniTask OnCreate()
        {
            return UniTask.CompletedTask;
        }

        public override UniTask OnInit()
        {
            return UniTask.CompletedTask;
        }

        public void RegisterConfigTable()
        {
            
        }

        public void LoadConfigFromXml<T>(string xmlFilePath) where T : XConfig
        {
            throw new System.NotImplementedException();
        }

        public void LoadConfigFromXmlElement<T>(XmlElement element) where T : XConfig
        {
            throw new System.NotImplementedException();
        }

        public void RegisterConfigTable<T>() where T : XConfig
        {
            throw new System.NotImplementedException();
        }

        public ITypeConverter<TSource, TTarget> GetConverter<TSource, TTarget>(string domain = "")
        {
            return TypeConverterRegistry.GetConverter<TSource, TTarget>(domain);
        }

        public bool HasConverter<TSource, TTarget>(string domain = "")
        {
            return TypeConverterRegistry.HasConverter<TSource, TTarget>(domain);
        }
    }  
}  