using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit)]
public readonly struct ModHandle
{
    [FieldOffset(0)] public readonly short ModId;

    public ModHandle(short modId)
    {
        ModId = modId;
    }
}

[StructLayout(LayoutKind.Explicit)]
public readonly struct TableHandle
{
    [FieldOffset(0)] public readonly short TableId;

    [FieldOffset(2)] public readonly ModHandle Mod;

    public TableHandle(short tableId, ModHandle mod)
    {
        TableId = tableId;
        Mod = mod;
    }

    public TableHandle<T> As<T>() where T : unmanaged
    {
        return new TableHandle<T>(TableId, Mod);
    }
}


[StructLayout(LayoutKind.Explicit)]

public readonly struct TableHandle<T> where T : unmanaged
{
    [FieldOffset(0)] public readonly short TableId;

    [FieldOffset(2)] public readonly ModHandle Mod;

    public TableHandle(short tableId, ModHandle mod)
    {
        TableId = tableId;
        Mod = mod;
    }

    public TableHandle As()
    {
        return new TableHandle(TableId, Mod);
    }
}


[StructLayout(LayoutKind.Explicit)]
public readonly struct ConfigHandle
{
    [FieldOffset(0)] public readonly short ConfigId;

    [FieldOffset(2)] public readonly ModHandle Mod;
    [FieldOffset(4)] public readonly TableHandle Table;

    public ConfigHandle(short configId, ModHandle mod, TableHandle table)
    {
        ConfigId = configId;
        Mod = mod;
        Table = table;
    }

    public ConfigHandle<T> As<T>() where T : unmanaged
    {
        return new ConfigHandle<T>(ConfigId, Mod, Table.As<T>());
    }
}

[StructLayout(LayoutKind.Explicit)]
public readonly struct ConfigHandle<T> where T : unmanaged
{
    [FieldOffset(0)] public readonly short ConfigId;
    [FieldOffset(2)] public readonly ModHandle Mod;
    [FieldOffset(4)] public readonly TableHandle<T> Table;

    public ConfigHandle(short configId, ModHandle mod, TableHandle<T> table)
    {
        ConfigId = configId;
        Mod = mod;
        Table = table;
    }
}