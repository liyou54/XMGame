using System;
using System.Runtime.InteropServices;

namespace XM
{
    /// <summary>
    /// 配置覆盖模式。异常策略：None/ReWrite 严格（解析失败打 Error 含文件/行/字段，仍正常序列化返回 obj）；Modify 宽松（仅 Warning）；Delete 不反序列化。
    /// </summary>
    public enum OverrideMode
    {
        /// <summary>无覆盖，新增配置。严格：Error(文件,行,字段) 仍返回 config</summary>
        None,
        /// <summary>追加。严格：Error(文件,行,字段) 仍返回 config</summary>
        ReWrite,
        /// <summary>删除。不反序列化</summary>
        Delete,
        /// <summary>修改。宽松：仅 Warning</summary>
        Modify
    }

    public interface IXConfig
    {
    }

    public interface IXConfig<T, TUnmanaged>
        : IXConfig
        where T : IXConfig<T, TUnmanaged>
        where TUnmanaged : unmanaged, IConfigUnManaged<TUnmanaged>
    {
    }
    
    public interface IConfigUnManaged
    {
    }

    public interface IConfigUnManaged<T>:IConfigUnManaged
        where T : unmanaged, IConfigUnManaged<T>
    {
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct IndexType:IEquatable<IndexType>
    {
        [FieldOffset(0)] public TblI Tbl;
        [FieldOffset(4)] public short Index;
        [FieldOffset(6)] public short padding;

        public bool Equals(IndexType other)
        {
            return Tbl.Equals(other.Tbl) && Index == other.Index ;
        }

        public override bool Equals(object obj)
        {
            return obj is IndexType other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Tbl, Index);
        }
    }

    public interface IConfigIndexGroup<TData>
        where TData : unmanaged, IConfigUnManaged<TData>
    {
        public static IndexType IndexType { get; }
    }

    public struct StrI:IEquatable<StrI>
    {
        public int Id;

        public bool Equals(StrI other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            return obj is StrI other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }

    public struct LabelI
    {
        public ModI DefinedModId;
        public int labelId;
    }

    public struct LabelS
    {
        public string ModName;

        public string LabelName;
    }
}