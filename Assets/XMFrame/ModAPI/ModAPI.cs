// ModAPI 程序集 - 供 mod 开发者使用
// 此程序集只包含接口定义，mod 只需要引用此程序集即可访问管理器接口

// 重新导出 Interfaces 中的关键类型，方便 mod 使用

using XM.Contracts;

namespace XM.ModAPI
{
    // 重新导出接口，方便 mod 使用
    using IManager = XM.Contracts.IManager;
    using ModBase = XM.Contracts.ModBase;
}
