using System.Collections.Generic;
using XM;
using XM.Contracts.Config;
using XM.Utils.Attribute;

namespace TestConfigLargenum
{
    [XmlDefined]
    public partial class PerfConfig3 : IXConfig<PerfConfig3, PerfConfig3UnManaged>
    {
        public CfgI Data { get; set; }
        public CfgS<PerfConfig3UnManaged> Id;
        public string Name;
        public int Level;
        public List<int> Tags;
    }

    public partial struct PerfConfig3UnManaged : IConfigUnManaged<PerfConfig3UnManaged> { }
}
