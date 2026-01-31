using System.Collections.Generic;
using XM;
using XM.Contracts.Config;
using XM.Utils.Attribute;

namespace TestConfigLargenum
{
    [XmlDefined]
    public partial class PerfConfig2 : IXConfig<PerfConfig2, PerfConfig2UnManaged>
    {
        public CfgI Data { get; set; }
        public CfgS<PerfConfig2UnManaged> Id;
        public string Name;
        public int Level;
        public List<int> Tags;
    }

    public partial struct PerfConfig2UnManaged : IConfigUnManaged<PerfConfig2UnManaged> { }
}
