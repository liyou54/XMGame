using XM;
using XM.Contracts;

namespace XM.Contracts
{
    /// <summary>
    /// 字符串管理器接口：提供 StrI&lt;-&gt;string、LabelS&lt;-&gt;LabelI&lt;-&gt;string 的注册与查询。
    /// </summary>
    public interface IStringManager : IManager<IStringManager>
    {
        #region StrI 与 string 互转

        /// <summary>
        /// 将字符串注册或查找，返回对应的 StrI。
        /// </summary>
        bool TryGetStrI(string value, out StrI strI);

        /// <summary>
        /// 根据 StrI 查询对应的字符串。
        /// </summary>
        bool TryGetStr(StrI strI, out string value);

        #endregion

        #region LabelS 与 LabelI 互转

        /// <summary>
        /// 将 LabelS 注册或查找，返回对应的 LabelI。
        /// </summary>
        bool TryGetLabelI(LabelS labelS, out LabelI labelI);

        /// <summary>
        /// 根据 LabelI 查询对应的 LabelS。
        /// </summary>
        bool TryGetLabelS(LabelI labelI, out LabelS labelS);

        /// <summary>
        /// 根据 LabelI 查询对应的 "ModName::LabelName" 格式字符串。
        /// </summary>
        bool TryGetLabel(LabelI labelI, out string value);

        #endregion
    }
}
