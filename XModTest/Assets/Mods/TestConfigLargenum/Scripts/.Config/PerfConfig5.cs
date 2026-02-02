using System.Collections.Generic;
using XM;
using XM.Contracts.Config;
using XM.Utils.Attribute;

namespace TestConfigLargenum
{
    [XmlDefined]
    public partial class PerfConfig5 : IXConfig<PerfConfig5, PerfConfig5UnManaged>
    {
        public CfgI Data { get; set; }
        public CfgS<PerfConfig5UnManaged> Id;
        public string Name;
        public int Level;
        public List<int> Tags;
    }

    public partial struct PerfConfig5UnManaged : IConfigUnManaged<PerfConfig5UnManaged> { }
}
