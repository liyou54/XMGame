# MyMod测试XML配置加载问题排查

## 错误信息
```
Value cannot be null.
Parameter name: configName
```

## 问题分析

此错误发生在 `CfgS` 构造函数中，当 `configName` 参数为 null 时抛出。

## 已应用的修复

### 1. 增强 ConfigParseHelper.TryParseCfgSString 验证
在 `Assets/XMFrame/Interfaces/ConfigMananger/ConfigParseHelper.cs` 中添加了额外的空值检查：
- 确保解析后的 `modName` 和 `configName` 都不为空
- 如果为空，返回 false 并记录警告日志

## 可能的原因

### 原因1：配置引用格式错误
XML中的配置引用格式不正确，例如：
```xml
<!-- 错误：缺少配置名 -->
<Link>MyMod::</Link>
<Foreign>::config_name</Foreign>
<Id>::</Id>

<!-- 正确格式 -->
<Link>MyMod::test_001</Link>
<Foreign>MyMod::item_001</Foreign>
<Id>MyMod::config_001</Id>
```

### 原因2：空白字符问题
```xml
<!-- 可能有问题 -->
<Link>  </Link>
<Link></Link>

<!-- 正确 -->
<Link>MyMod::test_001</Link>
<!-- 或者完全省略 -->
```

### 原因3：特殊字符或编码问题
如果XML文件编码不是UTF-8，可能导致解析错误。

## 排查步骤

### 步骤1：检查所有XML文件的配置引用
运行以下PowerShell命令检查所有XML文件：

```powershell
# 进入XML目录
cd XModTest\Assets\Mods\MyMod\Xml

# 查找所有可能有问题的配置引用
Select-String -Path "*.xml" -Pattern "<(Link|Foreign|Id|TestIndex\d+|TestKeyList)>.*::\s*</\1>" | ForEach-Object {
    Write-Host "可能有问题: $($_.Filename):$($_.LineNumber) - $($_.Line.Trim())" -ForegroundColor Yellow
}

# 查找只包含 :: 的引用
Select-String -Path "*.xml" -Pattern ">::</" | ForEach-Object {
    Write-Host "空引用: $($_.Filename):$($_.LineNumber) - $($_.Line.Trim())" -ForegroundColor Red
}
```

### 步骤2：逐个启用XML文件
1. 保留原始的 `MyModConfig.xml` 和 `MyModConfig_Basic.xml`
2. 临时重命名其他XML为 `.bak` 后缀
3. 逐个恢复XML文件并测试

```powershell
# 禁用所有新XML
Rename-Item "MyModConfig_TestConfig.xml" "MyModConfig_TestConfig.xml.bak"
Rename-Item "MyModConfig_Nested.xml" "MyModConfig_Nested.xml.bak"
Rename-Item "MyModConfig_Link.xml" "MyModConfig_Link.xml.bak"
Rename-Item "MyModConfig_Edge.xml" "MyModConfig_Edge.xml.bak"

# 逐个启用测试
Rename-Item "MyModConfig_TestConfig.xml.bak" "MyModConfig_TestConfig.xml"
# 测试...如果失败就是这个文件有问题
```

### 步骤3：启用详细日志
在 `GameMain.cs` 的 `OnAwake()` 方法中设置：
```csharp
XLog.CurrentLogLevel = LogLevel.Debug;
```

这会输出更多解析过程的日志，帮助定位具体哪个配置项有问题。

### 步骤4：检查Unity Console的完整错误堆栈
查看Unity Console中的完整错误堆栈，特别注意：
- 错误发生在哪个XML文件
- 错误发生在解析哪个字段
- 前面是否有警告日志

## 已知问题和解决方案

### 问题：TestInhert缺少Id字段
**解决方案**：TestInhert类型的配置不需要 `<Id>` 字段，只需要 `<Link>` 字段。

### 问题：引用不存在的配置
**解决方案**：确保 XML 加载顺序正确：
1. MyModConfig_TestConfig.xml（包含test_001到test_010）
2. MyModConfig_Link.xml（引用上述配置）

### 问题：Nested配置中的空列表
**解决方案**：空列表可以使用以下任一方式：
```xml
<!-- 方式1：省略标签 -->
<!-- 不写 <TestKeyList> 标签 -->

<!-- 方式2：空标签 -->
<TestKeyList></TestKeyList>

<!-- 方式3：自闭合标签 -->
<TestKeyList/>
```

## 临时解决方案

如果无法立即定位问题，可以：

1. **禁用测试管理器**
```csharp
// 在 ConfigTestManager.cs 中注释掉 [AutoCreate]
// [AutoCreate(priority: 1000)]
public class ConfigTestManager : ManagerBase<IConfigTestManager>, IConfigTestManager
```

2. **只使用基础测试XML**
临时删除或重命名 4 个复杂的测试XML文件，只保留：
- MyModConfig.xml（原始文件）
- MyModConfig_Basic.xml（基础测试）

3. **跳过引用验证**（不推荐）
临时修改 `TryParseCfgSString` 允许空的 configName（但这可能导致其他问题）

## 修复验证

修复后，应该看到：
```
=================================================================
开始配置测试：验证MyMod XML配置数据
=================================================================

【MyModConfig_Basic.xml - 基础配置测试】
-----------------------------------------------------------------
✓ basic_001: 配置存在
✓ basic_002: 配置存在
...

【测试总结】
总测试数: 50
通过: 50 (100.0%)
失败: 0
=================================================================
✓ 所有测试通过！配置系统运行正常。
```

## 需要帮助？

如果问题持续存在，请提供：
1. 完整的Unity Console错误堆栈
2. 所有XML文件的内容（或有问题的XML文件）
3. ConfigParseHelper的警告日志（如果有）

---

**更新时间：** 2026-02-02  
**状态：** 已应用修复，等待验证
