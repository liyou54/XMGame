using System;
using System.Runtime.InteropServices;
using XM;
using XM.Contracts;

/// <summary>


/// <summary>
/// Mod 运行时句柄，满足 unmanaged 约束
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public readonly struct ModI : IEquatable<ModI>
{
    [FieldOffset(0)] public readonly short ModId;

    public ModI(short modId)
    {
        ModId = modId;
    }

    public bool Valid => ModId > 0;

    public bool Equals(ModI other)
    {
        return ModId == other.ModId;
    }

    public override bool Equals(object obj)
    {
        return obj is ModI other && Equals(other);
    }

    public override int GetHashCode()
    {
        return ModId.GetHashCode();
    }

    public static bool operator ==(ModI left, ModI right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ModI left, ModI right)
    {
        return !(left == right);
    }

    /// <summary>方便调试：返回 ModI(Id)，解析名称由 ModS.ToString 提供。</summary>
    public override string ToString() => $"ModI({ModId})";
}

/// <summary>
/// Table 运行时句柄，满足 unmanaged 约束
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public readonly struct TblI : IEquatable<TblI>
{
    
    [FieldOffset(0)] public readonly short TableId;

    [FieldOffset(2)] public readonly ModI DefinedMod;

    public TblI(short tableId, ModI definedMod)
    {
        TableId = tableId;
        DefinedMod = definedMod;
    }

    public bool Valid => DefinedMod.Valid && TableId > 0;

    public TblI<T> As<T>() where T : unmanaged
    {
        return new TblI<T>(TableId, DefinedMod);
    }

    public bool Equals(TblI other)
    {
        return TableId == other.TableId && DefinedMod.Equals(other.DefinedMod);
    }

    public override bool Equals(object obj)
    {
        return obj is TblI other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (TableId.GetHashCode() * 397) ^ DefinedMod.GetHashCode();
        }
    }

    public static bool operator ==(TblI left, TblI right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(TblI left, TblI right)
    {
        return !(left == right);
    }

    /// <summary>方便调试：返回 TblI(Id)，解析名称由 TblS.ToString 提供。</summary>
    public override string ToString() => $"TblI({TableId})";
}


/// <summary>
/// Table 泛型运行时句柄，满足 unmanaged 约束
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct TblI<T> : IEquatable<TblI<T>> where T : unmanaged
{
    public static readonly ModI DefinedInMod;
    public readonly short TableId;
    public readonly ModI Mod;

    public TblI(short tableId, ModI mod)
    {
        TableId = tableId;
        Mod = mod;
    }

    public TblI As()
    {
        return new TblI(TableId, Mod);
    }

    public bool Equals(TblI<T> other)
    {
        return TableId == other.TableId && Mod.Equals(other.Mod);
    }

    public override bool Equals(object obj)
    {
        return obj is TblI<T> other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (TableId.GetHashCode() * 397) ^ Mod.GetHashCode();
        }
    }

    public static bool operator ==(TblI<T> left, TblI<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(TblI<T> left, TblI<T> right)
    {
        return !(left == right);
    }

    /// <summary>方便调试：返回 TblI(Id)，解析名称由 TblS.ToString 提供。</summary>
    public override string ToString() => $"TblI({TableId})";
}


/// <summary>
/// Config 运行时 ID，满足 unmanaged 约束
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public readonly struct CfgI : IEquatable<CfgI>
{
    [FieldOffset(0)] public readonly short Id;

    [FieldOffset(2)] public readonly ModI Mod;
    [FieldOffset(4)] public readonly TblI Table;

    public CfgI(short id, ModI mod, TblI table)
    {
        Id = id;
        Mod = mod;
        Table = table;
    }

    public bool Valid => Id > 0 && Mod.Valid && Table.Valid;

    public CfgI<T> As<T>() where T : unmanaged, IConfigUnManaged<T>
    {
        return new CfgI<T>(Id, Mod, Table.As<T>());
    }

    public bool Equals(CfgI other)
    {
        return Id == other.Id && Mod.Equals(other.Mod) && Table.Equals(other.Table);
    }

    public override bool Equals(object obj)
    {
        return obj is CfgI other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = Id.GetHashCode();
            hash = (hash * 397) ^ Mod.GetHashCode();
            hash = (hash * 397) ^ Table.GetHashCode();
            return hash;
        }
    }

    public static bool operator ==(CfgI left, CfgI right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(CfgI left, CfgI right)
    {
        return !(left == right);
    }

    /// <summary>方便调试：返回 CfgI(Id)，解析名称由 CfgS.ToString 提供。</summary>
    public override string ToString() => $"CfgI({Id})";

    public bool TryGetData<T>(out T data) where T : unmanaged, IConfigUnManaged<T>
    {
        return IConfigDataCenter.I.TryGetConfig(out data);
    }
}

/// <summary>
/// Config 泛型运行时 ID，满足 unmanaged 约束
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct CfgI<T> : 
    IEquatable<CfgI<T>> where T : unmanaged, IConfigUnManaged<T>
{
    public static TblI<T> TableStatic;
    
    public readonly short Id;
    public readonly ModI Mod;
    public readonly TblI<T> Table;

    public CfgI(short id, ModI mod, TblI<T> table)
    {
        Id = id;
        Mod = mod;
        Table = table;
    }

    public bool Equals(CfgI<T> other)
    {
        return Id == other.Id && Mod.Equals(other.Mod) && Table.Equals(other.Table);
    }

    public override bool Equals(object obj)
    {
        return obj is CfgI<T> other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = Id.GetHashCode();
            hash = (hash * 397) ^ Mod.GetHashCode();
            hash = (hash * 397) ^ Table.GetHashCode();
            return hash;
        }
    }

    public static bool operator ==(CfgI<T> left, CfgI<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(CfgI<T> left, CfgI<T> right)
    {
        return !(left == right);
    }

    /// <summary>方便调试：返回 CfgI(Id)，解析名称由 CfgS.ToString 提供。</summary>
    public override string ToString() => $"CfgI({Id})";

    public bool TryGetData(out T data)
    {
        return IConfigDataCenter.I.TryGetConfig(out data);
    }
}