using System;
using System.Runtime.InteropServices;
using XMFrame;
using XMFrame.Interfaces;

/// <summary>
/// Mod ID 结构体，满足 unmanaged 约束
/// </summary>
public readonly struct ModId : IEquatable<ModId>
{
    public readonly short Value;

    public ModId(short value)
    {
        Value = value;
    }

    public bool Equals(ModId other)
    {
        return Value == other.Value;
    }

    public override bool Equals(object obj)
    {
        return obj is ModId other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public static bool operator ==(ModId left, ModId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ModId left, ModId right)
    {
        return !(left == right);
    }

    public static implicit operator short(ModId modId)
    {
        return modId.Value;
    }

    public static implicit operator ModId(short value)
    {
        return new ModId(value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}

/// <summary>
/// Mod Handle 结构体，满足 unmanaged 约束
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public readonly struct ModHandle : IEquatable<ModHandle>
{
    [FieldOffset(0)] public readonly short ModId;

    public ModHandle(short modId)
    {
        ModId = modId;
    }

    public bool Valid => ModId > 0;

    public bool Equals(ModHandle other)
    {
        return ModId == other.ModId;
    }

    public override bool Equals(object obj)
    {
        return obj is ModHandle other && Equals(other);
    }

    public override int GetHashCode()
    {
        return ModId.GetHashCode();
    }

    public static bool operator ==(ModHandle left, ModHandle right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ModHandle left, ModHandle right)
    {
        return !(left == right);
    }
}

/// <summary>
/// Table Handle 结构体，满足 unmanaged 约束
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public readonly struct TableHandle : IEquatable<TableHandle>
{
    [FieldOffset(0)] public readonly short TableId;

    [FieldOffset(2)] public readonly ModHandle Mod;

    public TableHandle(short tableId, ModHandle mod)
    {
        TableId = tableId;
        Mod = mod;
    }

    public bool Valid => Mod.Valid && TableId > 0;

    public TableHandle<T> As<T>() where T : unmanaged
    {
        return new TableHandle<T>(TableId, Mod);
    }

    public bool Equals(TableHandle other)
    {
        return TableId == other.TableId && Mod.Equals(other.Mod);
    }

    public override bool Equals(object obj)
    {
        return obj is TableHandle other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (TableId.GetHashCode() * 397) ^ Mod.GetHashCode();
        }
    }

    public static bool operator ==(TableHandle left, TableHandle right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(TableHandle left, TableHandle right)
    {
        return !(left == right);
    }
}


/// <summary>
/// Table Handle 泛型结构体，满足 unmanaged 约束
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct TableHandle<T> : IEquatable<TableHandle<T>> where T : unmanaged
{
    public readonly short TableId;
    public readonly ModHandle Mod;

    public TableHandle(short tableId, ModHandle mod)
    {
        TableId = tableId;
        Mod = mod;
    }

    public TableHandle As()
    {
        return new TableHandle(TableId, Mod);
    }

    public bool Equals(TableHandle<T> other)
    {
        return TableId == other.TableId && Mod.Equals(other.Mod);
    }

    public override bool Equals(object obj)
    {
        return obj is TableHandle<T> other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (TableId.GetHashCode() * 397) ^ Mod.GetHashCode();
        }
    }

    public static bool operator ==(TableHandle<T> left, TableHandle<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(TableHandle<T> left, TableHandle<T> right)
    {
        return !(left == right);
    }
}


/// <summary>
/// Config Handle 结构体，满足 unmanaged 约束
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public readonly struct CfgId : IEquatable<CfgId>
{
    [FieldOffset(0)] public readonly short Id;

    [FieldOffset(2)] public readonly ModHandle Mod;
    [FieldOffset(4)] public readonly TableHandle Table;

    public CfgId(short id, ModHandle mod, TableHandle table)
    {
        Id = id;
        Mod = mod;
        Table = table;
    }

    public bool Valid => Id > 0 && Mod.Valid && Table.Valid;

    public CfgId<T> As<T>() where T : unmanaged, IConfigUnManaged<T>
    {
        return new CfgId<T>(Id, Mod, Table.As<T>());
    }

    public bool Equals(CfgId other)
    {
        return Id == other.Id && Mod.Equals(other.Mod) && Table.Equals(other.Table);
    }

    public override bool Equals(object obj)
    {
        return obj is CfgId other && Equals(other);
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

    public static bool operator ==(CfgId left, CfgId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(CfgId left, CfgId right)
    {
        return !(left == right);
    }

    public bool TryGetData<T>(out T data) where T : unmanaged, IConfigUnManaged<T>
    {
        return IConfigDataCenter.I.TryGetConfig(out data);
    }
}

/// <summary>
/// Config Handle 泛型结构体，满足 unmanaged 约束
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct CfgId<T> : 
    IEquatable<CfgId<T>> where T : unmanaged, IConfigUnManaged<T>
{
    public readonly short ConfigId;
    public readonly ModHandle Mod;
    public readonly TableHandle<T> Table;

    public CfgId(short configId, ModHandle mod, TableHandle<T> table)
    {
        ConfigId = configId;
        Mod = mod;
        Table = table;
    }

    public bool Equals(CfgId<T> other)
    {
        return ConfigId == other.ConfigId && Mod.Equals(other.Mod) && Table.Equals(other.Table);
    }

    public override bool Equals(object obj)
    {
        return obj is CfgId<T> other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = ConfigId.GetHashCode();
            hash = (hash * 397) ^ Mod.GetHashCode();
            hash = (hash * 397) ^ Table.GetHashCode();
            return hash;
        }
    }

    public static bool operator ==(CfgId<T> left, CfgId<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(CfgId<T> left, CfgId<T> right)
    {
        return !(left == right);
    }

    public bool TryGetData(out T data)
    {
        return IConfigDataCenter.I.TryGetConfig(out data);
    }
}