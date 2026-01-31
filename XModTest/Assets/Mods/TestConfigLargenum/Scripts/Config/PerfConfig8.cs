using System.Collections.Generic;
using XM;
using XM.Contracts.Config;
using XM.Utils.Attribute;

namespace TestConfigLargenum
{
    [XmlDefined]
    public partial class PerfConfig8 : IXConfig<PerfConfig8, PerfConfig8UnManaged>
    {
        public CfgI Data { get; set; }
        public CfgS<PerfConfig8UnManaged> Id;
        public string Name;
        public int Level;
        public List<int> Tags;
    }

    public partial struct PerfConfig8UnManaged : IConfigUnManaged<PerfConfig8UnManaged> { }
}
