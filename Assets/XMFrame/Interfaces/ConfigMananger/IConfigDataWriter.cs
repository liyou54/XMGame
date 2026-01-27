using XMFrame;

namespace XMFrame.Interfaces.ConfigMananger
{
    /// <summary>
    /// 由 ConfigDataCenter 实现，供 ClassHelper 调用以完成 AllocTableMap / AddPrimaryKeyOnly / AddOrUpdateRow，无需反射。
    /// </summary>
    public interface IConfigDataWriter
    {
        void AllocTableMap<TUnmanaged>(TableHandle tableHandle, int capacity)
            where TUnmanaged : unmanaged, IConfigUnManaged<TUnmanaged>;

        void AddPrimaryKeyOnly<TUnmanaged>(TableHandle tableHandle, CfgId cfgId)
            where TUnmanaged : unmanaged, IConfigUnManaged<TUnmanaged>;

        void AddOrUpdateRow<TUnmanaged>(TableHandle tableHandle, CfgId cfgId, TUnmanaged value)
            where TUnmanaged : unmanaged, IConfigUnManaged<TUnmanaged>;
    }
}
