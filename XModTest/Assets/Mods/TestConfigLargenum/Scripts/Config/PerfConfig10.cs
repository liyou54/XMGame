using System.Collections.Generic;
using XM;
using XM.Contracts.Config;
using XM.Utils.Attribute;

namespace TestConfigLargenum
{
    [XmlDefined]
    public partial class PerfConfig10 : IXConfig<PerfConfig10, PerfConfig10UnManaged>
    {
        public CfgI Data { get; set; }
        public CfgS<PerfConfig10UnManaged> Id;
        public string Name;
        public int Level;
        public List<int> Tags;
    }

    public partial struct PerfConfig10UnManaged : IConfigUnManaged<PerfConfig10UnManaged> { }
}
