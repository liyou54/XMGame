using System.Collections.Generic;
using XM;
using XM.Contracts.Config;
using XM.Utils.Attribute;

namespace TestConfigLargenum
{
    [XmlDefined]
    public partial class PerfConfig6 : IXConfig<PerfConfig6, PerfConfig6UnManaged>
    {
        public CfgI Data { get; set; }
        public CfgS<PerfConfig6UnManaged> Id;
        public string Name;
        public int Level;
        public List<int> Tags;
    }

    public partial struct PerfConfig6UnManaged : IConfigUnManaged<PerfConfig6UnManaged> { }
}
