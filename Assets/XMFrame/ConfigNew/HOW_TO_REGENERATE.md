# 如何重新生成配置代码

## 问题说明

旧的生成文件已被删除，现在需要使用修复后的生成器重新生成代码。

## 已修复的问题

✅ **可空类型转换** - `int?` → `int` (使用 `.GetValueOrDefault()`)
✅ **继承字段支持** - 包含父类所有字段
✅ **全局限定名** - 使用 `global::` 避免命名冲突
✅ **静态构造函数** - 移除错误的 `public` 修饰符
✅ **类型参数正确** - `ActiveSkillConfigUnmanaged` 而不是 `BaseSkillConfigUnmanaged`

## 重新生成方法

### 方法1: 使用 Unity 菜单（推荐）

在 Unity 编辑器菜单栏选择：

```
XMFrame > ConfigNew > 生成三个技能配置
```

这会重新生成：
- `BaseSkillConfig` + Unmanaged + Index + ClassHelper
- `ActiveSkillConfig` + Unmanaged + Index + ClassHelper
- `PassiveSkillConfig` + Unmanaged + Index + ClassHelper

### 方法2: 使用代码生成器窗口

1. 打开窗口：`XMFrame > ConfigNew > 代码生成器(测试)`
2. 切换到"按类型选择"标签
3. 勾选以下类型：
   - BaseSkillConfig
   - ActiveSkillConfig
   - PassiveSkillConfig
4. 点击"生成 Unmanaged 代码"按钮

### 方法3: 测试当前生成器输出

```
XMFrame > ConfigNew > 测试当前生成器输出
```

这会：
- 分析 `ActiveSkillConfig` 元数据
- 生成代码到临时文件
- 用记事本打开，可以查看生成的内容
- **不会保存到项目中**，只是预览

### 方法4: 重新生成所有测试配置

```
XMFrame > ConfigNew > 重新生成所有测试配置
```

这会生成所有测试配置类，包括：
- AttributeConfig
- PriceConfig
- EffectConfig
- BaseItemConfig
- BaseSkillConfig
- ActiveSkillConfig
- PassiveSkillConfig
- ComplexItemConfig
- QuestConfig
- LinkParentConfig
- SingleLinkChildConfig
- ListLinkChildConfig
- ContainerConverterConfig

## 验证生成结果

生成后应该看到以下文件：

### BaseSkillConfig
```
Generated/
  ├── BaseSkillConfigUnmanaged.Gen.cs
  ├── BaseSkillConfigUnmanaged.SkillIdIndex.Gen.cs
  └── BaseSkillConfigClassHelper.Gen.cs
```

### ActiveSkillConfig
```
Generated/
  ├── ActiveSkillConfigUnmanaged.Gen.cs          ← 包含父类字段
  ├── ActiveSkillConfigUnmanaged.SkillIdIndex.Gen.cs
  └── ActiveSkillConfigClassHelper.Gen.cs        ← 使用正确的泛型参数
```

### 检查生成的代码

打开 `ActiveSkillConfigClassHelper.Gen.cs`，检查：

1. **类声明应该使用 global::**
```csharp
public class ActiveSkillConfigClassHelper : 
    ConfigClassHelper<
        global::XM.ConfigNew.TestConfigs.ActiveSkillConfig, 
        global::XM.ConfigNew.TestConfigs.ActiveSkillConfigUnmanaged>
```

2. **AllocContainerWithFillImpl 应该使用正确的类型**
```csharp
public override void AllocContainerWithFillImpl(
    IXConfig value,
    TblI tbli,
    CfgI cfgi,
    ref global::XM.ConfigNew.TestConfigs.ActiveSkillConfigUnmanaged data,  // ✅ 正确
    ...)
{
    var config = (global::XM.ConfigNew.TestConfigs.ActiveSkillConfig)value;
    
    // ✅ 应该能访问这些字段
    data.ManaCost = config.ManaCost;
    data.Cooldown = config.Cooldown;
    data.SkillId = config.SkillId;    // 继承自 BaseSkillConfig
    data.MaxLevel = config.MaxLevel;  // 继承自 BaseSkillConfig
}
```

3. **Unmanaged 结构体应该包含所有字段**
```csharp
public partial struct ActiveSkillConfigUnmanaged : 
    IConfigUnManaged<global::XM.ConfigNew.TestConfigs.ActiveSkillConfigUnmanaged>
{
    // 继承自 BaseSkillConfig 的字段
    public int SkillId;
    public FixedString32Bytes SkillName;
    public int MaxLevel;
    
    // ActiveSkillConfig 自己的字段
    public int ManaCost;      // ✅ 应该有这个字段
    public float Cooldown;    // ✅ 应该有这个字段
    public XBlobArray<global::XM.ConfigNew.TestConfigs.EffectConfigUnmanaged> SkillEffects;
}
```

## 如果还有问题

1. **清理 Library 文件夹**
   - 关闭 Unity
   - 删除 `Library/` 文件夹
   - 重新打开 Unity

2. **手动删除所有生成文件**
   ```
   删除 Assets/XMFrame/ConfigNew/Generated/ 下的所有 .Gen.cs 和 .Gen.cs.meta 文件
   ```

3. **重新编译**
   - 点击 Unity 菜单：`Assets > Refresh`
   - 或者按 Ctrl+R

## 调试工具

如果生成仍然有问题，使用调试工具：

### 查看元数据
```
XMFrame > ConfigNew > 调试: 检查继承字段
```
这会显示 ActiveSkillConfig 包含了哪些字段。

### 测试生成器输出
```
XMFrame > ConfigNew > 测试当前生成器输出
```
这会生成代码到临时文件并用记事本打开，可以查看当前生成器会生成什么内容。

## 预期结果

生成后，编译应该没有错误：
- ✅ 无 "Cannot implicitly convert type 'int?' to 'int'" 错误
- ✅ 无 "does not contain a definition for 'ManaCost'" 错误
- ✅ 无 "Access modifier is not allowed" 错误
- ✅ 无类型冲突警告（因为使用了 global::）

## 技术细节

生成器现在会：
1. 分析配置类型时包含父类字段
2. 生成 Unmanaged 结构体时包含所有字段（父类+子类）
3. 生成 ClassHelper 时使用正确的 Unmanaged 类型
4. 可空类型自动转换为非空类型
5. 所有类型名都使用 global:: 前缀
