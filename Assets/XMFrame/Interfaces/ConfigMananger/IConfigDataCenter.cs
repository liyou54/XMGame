using XMFrame.Implementation;

namespace XMFrame.Interfaces
{
    public interface IConfigDataCenter : IManager<IConfigDataCenter>
    {
        /// <summary>
        /// 注册配置表
        /// </summary>
        void RegisterConfigTable();

        
        bool TryGetConfigBySingleIndex<TData ,TIndex>(in TIndex index, out TData data)
            where TIndex : IConfigIndexGroup<TData>
            where TData : unmanaged, IConfigUnManaged<TData>;
    }
}