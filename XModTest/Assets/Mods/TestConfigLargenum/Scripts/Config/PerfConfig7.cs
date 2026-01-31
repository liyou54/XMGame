using System.Collections.Generic;
using XM;
using XM.Contracts.Config;
using XM.Utils.Attribute;

namespace TestConfigLargenum
{
    [XmlDefined]
    public partial class PerfConfig7 : IXConfig<PerfConfig7, PerfConfig7UnManaged>
    {
        public CfgI Data { get; set; }
        public CfgS<PerfConfig7UnManaged> Id;
        public string Name;
        public int Level;
        public List<int> Tags;
    }

    public partial struct PerfConfig7UnManaged : IConfigUnManaged<PerfConfig7UnManaged> { }
}
