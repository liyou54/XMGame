using System.Collections.Generic;
using XM;
using XM.Contracts.Config;
using XM.Utils.Attribute;

namespace TestConfigLargenum
{
    [XmlDefined]
    public partial class PerfConfig4 : IXConfig<PerfConfig4, PerfConfig4UnManaged>
    {
        public CfgI Data { get; set; }
        public CfgS<PerfConfig4UnManaged> Id;
        public string Name;
        public int Level;
        public List<int> Tags;
    }

    public partial struct PerfConfig4UnManaged : IConfigUnManaged<PerfConfig4UnManaged> { }
}
