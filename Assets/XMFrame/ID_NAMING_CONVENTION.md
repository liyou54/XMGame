# XMFrame 标识符命名规范（ID / Key 两套体系）

**约定：托管侧类型名统一加后缀 S（String），非托管侧类型名统一加后缀 I（Int/Id）。**

## 修改统计表（原 str ↔ 原 int → 现在 str(S) ↔ 现在 int(I)）

| 原 str 类型名称 | 原 int 类型名称 | 现在 str 名称（+S） | 现在 int 名称（+I） | 注释说明 |
|-----------------|-----------------|---------------------|---------------------|----------|
| ModKey | ModHandle | ModS | ModI | Mod 逻辑名 ↔ 运行时句柄(short) |
| TableDefine | TableHandle | TblS | TblI | 表键 ↔ 表句柄；泛型 TblI\<T\> |
| ConfigKey / ConfigKey\<T\> | CfgId / CfgId\<T\> | CfgS / CfgS\<T\> | CfgI / CfgI\<T\> | 配置键 ↔ 配置 ID；内部主字段统一 Id |
| （资源路径/Address） | XAssetId | 无统一类型时可称 PathS/AddrS | AssetI | 资源：str 侧 path/Address，int 侧 AssetI |
| StrLabel | StrLabelHandle | LabelS | LabelI | 字符串标签 Key ↔ 标签句柄(ModI+LabelId) |
| （UI 类型用 CfgS） | UIHandle | CfgS | UII | UI 实例：类型侧 CfgI，实例句柄 UII |
| — | StrHandle | — | StrI | 仅 int 侧；字符串句柄(Id) |
| — | TypeId | — | TypeI | 仅 int 侧；类型 ID |
| — | XAssetHandle(class) | — | AssetHI 或保留 XAssetHandle | 仅 int 侧；可释放句柄(class)，持有一份 AssetI |

---

## 一、两套体系

| 体系 | 基础类型 | 用途 | 后缀 |
|------|----------|------|------|
| **托管（+S）** | string | 逻辑键、可读、可序列化、跨进程一致 | `S` |
| **非托管（+I）** | int/short 等 | 运行时数值标识、unmanaged、可做 Map 键 | `I` |

- **S**：托管侧，string 标识（ModS, TblS, CfgS, LabelS）。
- **I**：非托管侧，数值句柄/ID（ModI, TblI, CfgI, AssetI, UII, StrI, TypeI）。

---

## 二、统一命名规则（托管 +S，非托管 +I）

### 1. 托管（string 为基础）→ 类型名加后缀 `S`

| 新类型名 | 含义 | 原名称 |
|----------|------|--------|
| `ModS` | Mod 逻辑名（string） | ModKey |
| `TblS` | 表逻辑名（Mod + 表名） | TableDefine / TableKey |
| `CfgS` / `CfgS<T>` | 配置逻辑键（Mod + 表 + 配置名） | ConfigKey / ConfigKey\<T\> |
| `LabelS` | 字符串标签（ModName + LabelName） | StrLabel |

**规则**：托管、string 相关、用于“逻辑标识”的类型名一律以 **S** 结尾。

---

### 2. 非托管（int/short 为基础）→ 类型名加后缀 `I`

| 新类型名 | 含义 | 原名称 | 内部字段建议 |
|----------|------|--------|----------------|
| `ModI` | Mod 运行时句柄 | ModHandle | `ModId` (short) 或 `V` |
| `TblI` / `TblI<T>` | 表运行时句柄 | TableHandle / TableHandle\<T\> | `TableId` + `Mod` |
| `CfgI` / `CfgI<T>` | 配置运行时 ID | CfgId / CfgId\<T\> | 统一 `Id` |
| `AssetI` | 资源 ID | XAssetId | `Mod` + `Id` |
| `UII` | UI 实例句柄 | UIHandle | `TypeI`(CfgI) + `Id`(实例号) |
| `StrI` | 字符串句柄 | StrHandle | `Id` |
| `LabelI` | 字符串标签句柄 | StrLabelHandle | `Mod` + `LabelId` |
| `TypeI` | 类型 ID | TypeId | — |

**规则**：非托管、数值型、用于“运行时标识”的类型名一律以 **I** 结尾；同一概念成对：**ModS↔ModI, TblS↔TblI, CfgS↔CfgI**。

---

### 3. 引用类型“可释放句柄”（class）→ 保留 Handle 或 `AssetHI`

| 类型名 | 含义 |
|--------|------|
| `XAssetHandle` 或 `AssetHI` | 持有一份 AssetI，引用计数、需释放 |

规则：**class、可释放、持有一个 I** 的可保留 Handle 或命名为 xxxHI，与值类型 AssetI 区分。

---

## 三、内部字段命名（短且一致）

非托管 **I** 系 struct 内部尽量短、一致：

| 类型（+I） | 主键字段建议 | 说明 |
|------------|----------------|------|
| `ModI` | `ModId` 或 `V` | short |
| `TblI` | `TableId` + `Mod` | Mod 即 ModI |
| `CfgI` / `CfgI<T>` | **统一 `Id`** | 与 CfgId 一致，避免 ConfigId/Id 混用 |
| `AssetI` | `Mod` + `Id` | Mod 即 ModI |

---

## 四、对照表（当前 → 建议 +S / +I）

| 当前 | 建议（托管 +S / 非托管 +I） | 说明 |
|------|----------------------------|------|
| ModKey | **ModS** | 托管 |
| TableDefine / TableKey | **TblS** | 托管 |
| ConfigKey / ConfigKey\<T\> | **CfgS** / **CfgS\<T\>** | 托管 |
| StrLabel | **LabelS** | 托管 |
| ModHandle | **ModI** | 非托管 |
| TableHandle / TableHandle\<T\> | **TblI** / **TblI\<T\>** | 非托管 |
| CfgId / CfgId\<T\> | **CfgI** / **CfgI\<T\>** | 非托管 |
| XAssetId | **AssetI** | 非托管 |
| UIHandle | **UII** | 非托管 |
| StrHandle | **StrI** | 非托管 |
| StrLabelHandle | **LabelI** | 非托管 |
| TypeId | **TypeI** | 非托管 |
| XAssetHandle | 保持 或 **AssetHI** | class 可释放句柄 |

---

## 五、简短记忆

- **S** = 托管、string（ModS, TblS, CfgS, LabelS）。  
- **I** = 非托管、数值（ModI, TblI, CfgI, AssetI, UII, StrI, LabelI, TypeI）。  
- 同一概念成对：**ModS↔ModI, TblS↔TblI, CfgS↔CfgI, LabelS↔LabelI**。  
- 内部字段：**I** 系里主 ID 统一用 `Id`，Mod/Table 用 `Mod`、`TableId`。

按上述规则，托管侧加 **S**、非托管侧加 **I**，两套不混、对应清晰。

---

## 六、最短缩写速查（代码/注释可用）

| 新类型名（+S / +I） | 原名称 | 体系 |
|--------------------|--------|------|
| ModS | ModKey | S |
| TblS | TableDefine / TableKey | S |
| CfgS | ConfigKey / CfgKey\<T\> | S |
| LabelS | StrLabel | S |
| ModI | ModHandle | I |
| TblI | TableHandle | I |
| CfgI | CfgId | I |
| AssetI | XAssetId | I |
| UII | UIHandle | I |
| StrI | StrHandle | I |
| LabelI | StrLabelHandle | I |
| TypeI | TypeId | I |

成对记忆：**ModS↔ModI, TblS↔TblI, CfgS↔CfgI, LabelS↔LabelI**。
