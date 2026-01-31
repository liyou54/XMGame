using System.Collections.Generic;
using XM;
using XM.Contracts.Config;
using XM.Utils.Attribute;

namespace TestConfigLargenum
{
    [XmlDefined]
    public partial class PerfConfig1 : IXConfig<PerfConfig1, PerfConfig1UnManaged>
    {
        public CfgI Data { get; set; }
        public CfgS<PerfConfig1UnManaged> Id;
        public string Name;
        public int Level;
        public List<int> Tags;
    }

    public partial struct PerfConfig1UnManaged : IConfigUnManaged<PerfConfig1UnManaged> { }
}
