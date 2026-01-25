using System;

public readonly struct ModKey : IEquatable<ModKey>
{
    public readonly string Name;

    public ModKey(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public override bool Equals(object obj) => obj is ModKey other && Equals(other);

    public bool Equals(ModKey other) => string.Equals(Name, other.Name, StringComparison.Ordinal);

    public override int GetHashCode() => Name != null ? Name.GetHashCode(StringComparison.Ordinal) : 0;

    public static bool operator ==(ModKey left, ModKey right) => left.Equals(right);

    public static bool operator !=(ModKey left, ModKey right) => !(left == right);

    public override string ToString() => Name ?? string.Empty;
}

public readonly struct TableDefine : IEquatable<TableDefine>
{
    public readonly ModKey DefinedInMod;
    public readonly string TableName;

    public TableDefine(ModKey definedInMod, string tableName)
    {
        DefinedInMod = definedInMod;
        TableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
    }

    public override bool Equals(object obj) => obj is TableDefine other && Equals(other);

    public bool Equals(TableDefine other)
        => DefinedInMod.Equals(other.DefinedInMod) &&
           string.Equals(TableName, other.TableName, StringComparison.Ordinal);

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + DefinedInMod.GetHashCode();
            hash = hash * 23 + (TableName != null ? TableName.GetHashCode(StringComparison.Ordinal) : 0);
            return hash;
        }
    }

    public static bool operator ==(TableDefine left, TableDefine right) => left.Equals(right);

    public static bool operator !=(TableDefine left, TableDefine right) => !(left == right);

    public override string ToString() => $"{DefinedInMod}.{TableName}";
}


public readonly struct ConfigKey<T> : IEquatable<ConfigKey<T>>
{
    /// <summary>
    /// 定义此配置所属的表名。
    /// </summary>
    public static string TableName { get; set; }

    /// <summary>
    /// 所属Mod Key。
    /// </summary>
    public ModKey ModKey { get; }

    /// <summary>
    /// 配置名称。
    /// </summary>
    public string ConfigName { get; }

    public ConfigKey(ModKey modKey, string configName)
    {
        ModKey = modKey;
        ConfigName = configName ?? throw new ArgumentNullException(nameof(configName));
    }

    public override bool Equals(object obj) 
        => obj is ConfigKey<T> other && Equals(other);

    public bool Equals(ConfigKey<T> other) 
        => ModKey.Equals(other.ModKey) &&
           string.Equals(ConfigName, other.ConfigName, StringComparison.Ordinal);

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + ModKey.GetHashCode();
            hash = hash * 23 + (ConfigName != null ? ConfigName.GetHashCode(StringComparison.Ordinal) : 0);
            return hash;
        }
    }

    public static bool operator ==(ConfigKey<T> left, ConfigKey<T> right) => left.Equals(right);

    public static bool operator !=(ConfigKey<T> left, ConfigKey<T> right) => !(left == right);

    public override string ToString() 
        => $"{ModKey}.{TableName}::{ConfigName}";
}

