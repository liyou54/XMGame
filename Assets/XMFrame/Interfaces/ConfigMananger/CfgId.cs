using System;
using Unity.Burst;

namespace XM.Contracts.Config
{
    public readonly struct ModS : IEquatable<ModS>
    {
        public readonly string Name;
        public bool Valid => !String.IsNullOrEmpty(Name);

        public ModS(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public override bool Equals(object obj) => obj is ModS other && Equals(other);

        public bool Equals(ModS other) => string.Equals(Name, other.Name, StringComparison.Ordinal);

        public override int GetHashCode() => Name != null ? Name.GetHashCode(StringComparison.Ordinal) : 0;

        public static bool operator ==(ModS left, ModS right) => left.Equals(right);

        public static bool operator !=(ModS left, ModS right) => !(left == right);

        public override string ToString() => Name ?? string.Empty;
    }

    public readonly struct TblS : IEquatable<TblS>
    {
        public readonly ModS DefinedInMod;
        public readonly string TableName;
        
        public bool Valid => !String.IsNullOrEmpty(TableName);

        public TblS(ModS definedInMod, string tableName)
        {
            DefinedInMod = definedInMod;
            TableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
        }

        public TblS(string definedInMod, string tableName)
        {
            DefinedInMod = new ModS(definedInMod);
            TableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
        }

        public override bool Equals(object obj) => obj is TblS other && Equals(other);

        public bool Equals(TblS other)
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

        public static bool operator ==(TblS left, TblS right) => left.Equals(right);

        public static bool operator !=(TblS left, TblS right) => !(left == right);

        /// <summary>方便调试：TableName(DefinedInMod)。</summary>
        public override string ToString() => $"{TableName}({DefinedInMod})";
    }


    /// <summary>
    /// 非泛型配置键（用于跨类型传递和存储）
    /// </summary>
    public readonly struct CfgS : IEquatable<CfgS>
    {
        /// <summary>
        /// 所属Mod。
        /// </summary>
        public ModS ConfigInMod { get; }

        /// <summary>
        /// 表名。
        /// </summary>
        public TblS Table { get; }

        /// <summary>
        /// 配置名称。
        /// </summary>
        public string ConfigName { get; }

        public CfgS(ModS configInMod, TblS tableName, string configName)
        {
            ConfigInMod = configInMod;
            Table = tableName;
            ConfigName = configName ?? throw new ArgumentNullException(nameof(configName));
        }


        public override bool Equals(object obj)
            => obj is CfgS other && Equals(other);

        [BurstDiscard]
        public bool Equals(CfgS other)
            => ConfigInMod.Equals(other.ConfigInMod) &&
               Equals(Table, other.Table) &&
               string.Equals(ConfigName, other.ConfigName, StringComparison.Ordinal);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + ConfigInMod.GetHashCode();
                hash = hash * 23 + Table.GetHashCode();
                hash = hash * 23 + (ConfigName != null ? ConfigName.GetHashCode(StringComparison.Ordinal) : 0);
                return hash;
            }
        }

        public static bool operator ==(CfgS left, CfgS right) => left.Equals(right);

        public static bool operator !=(CfgS left, CfgS right) => !(left == right);

        public override string ToString()
            => $"{ConfigInMod}.{Table}::{ConfigName}";
    }

    public readonly struct CfgS<T> : IEquatable<CfgS<T>>
    {
        /// <summary>
        /// 定义此配置所属的表名。
        /// </summary>
        public static TblS Table;

        /// <summary>
        /// 所属Mod。
        /// </summary>
        public ModS Mod { get; }

        /// <summary>
        /// 配置名称。
        /// </summary>
        public string ConfigName { get; }

        public bool Valid => Table.Valid && Mod.Valid && !String.IsNullOrEmpty(ConfigName);

        public CfgS(ModS mod, string configName)
        {
            Mod = mod;
            ConfigName = configName;
        }

        /// <summary>
        /// 转换为非泛型 CfgS
        /// </summary>
        public CfgS AsNonGeneric()
        {
            if (!Valid)
            {
                return default;
            }
            
            return new CfgS(Mod, Table, ConfigName);
        }

        public override bool Equals(object obj)
            => obj is CfgS<T> other && Equals(other);

        public bool Equals(CfgS<T> other)
            => Mod.Equals(other.Mod) &&
               string.Equals(ConfigName, other.ConfigName, StringComparison.Ordinal);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Mod.GetHashCode();
                hash = hash * 23 + (ConfigName != null ? ConfigName.GetHashCode(StringComparison.Ordinal) : 0);
                return hash;
            }
        }

        public static bool operator ==(CfgS<T> left, CfgS<T> right) => left.Equals(right);

        public static bool operator !=(CfgS<T> left, CfgS<T> right) => !(left == right);

        public override string ToString()
            => $"{Mod}.{Table}::{ConfigName}";
    }
}