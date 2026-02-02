# 快速开始：MyMod配置测试

## 🚀 5分钟快速验证

### 步骤1：检查测试文件
确认以下文件存在：

**XModTest工程（测试数据）：**
```
XModTest/Assets/Mods/MyMod/Xml/
  ├─ MyModConfig_Basic.xml        (10条基础配置)
  ├─ MyModConfig_TestConfig.xml   (10条复杂配置)
  ├─ MyModConfig_Nested.xml       (10条嵌套配置)
  ├─ MyModConfig_Link.xml         (10条链接配置)
  ├─ MyModConfig_Edge.xml         (10条边界测试)
  └─ TEST_XML_README.md           (测试说明)
```

**主工程（测试管理器）：**
```
Assets/XMFrame/Implementation/XConfigManager/
  ├─ ConfigTestManager.cs         (测试管理器)
  └─ CONFIG_TEST_README.md        (使用说明)
```

### 步骤2：运行游戏
1. 在Unity中打开项目
2. 按下播放键 ▶️
3. 等待GameMain初始化完成

### 步骤3：查看结果
打开Console窗口，查找以下输出：

**成功示例：**
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

## ❓ 常见问题

### Q: 没有看到测试输出？
**A:** 检查以下几点：
1. `ConfigTestManager.cs`是否有`[AutoCreate]`标记
2. 日志级别是否设置为Debug：`XLog.CurrentLogLevel = LogLevel.Debug`
3. Unity Console的Filter是否开启

### Q: 测试失败怎么办？
**A:** 根据错误类型处理：

**"表 XXX 未注册"**
- 检查配置类是否有`[XmlDefined]`标记
- 运行代码生成器重新生成ClassHelper

**"配置不存在"**
- 检查XML文件是否在正确路径
- 验证XML格式是否正确
- 确认配置ID与cls属性匹配

**"异常 - XXX"**
- 查看完整错误信息
- 检查相关配置类定义
- 查阅详细文档

### Q: 如何禁用测试？
**A:** 编辑`ConfigTestManager.cs`：
```csharp
// 方法1：注释掉AutoCreate
// [AutoCreate(priority: 1000)]

// 方法2：在OnInit中注释掉测试
public override async UniTask OnInit()
{
    // await TestBasicConfigs();
    // await TestComplexConfigs();
    // ...
}
```

### Q: 如何添加新的测试？
**A:** 在对应的测试方法中添加：
```csharp
private async UniTask TestBasicConfigs()
{
    StartTestSection("MyModConfig_Basic.xml - 基础配置测试");
    
    var modS = new ModS("MyMod");
    
    // 添加新测试
    TestConfigExists("my_new_config", "MyItemConfig", modS, "my_config_id");
    
    await UniTask.Yield();
}
```

## 📋 测试清单

使用此清单验证系统是否正常工作：

- [ ] XModTest工程包含5个XML文件
- [ ] 每个XML文件包含10条数据
- [ ] ConfigTestManager.cs存在于主工程
- [ ] 运行游戏能看到测试输出
- [ ] 所有50个测试通过
- [ ] Console没有错误信息

## 🎯 下一步

完成基础测试后，可以：

1. **添加字段值验证**
   - 等待ConfigDataCenter查询API完善
   - 验证配置字段的具体值

2. **添加性能测试**
   - 测量配置加载时间
   - 测试大量配置查询性能

3. **添加压力测试**
   - 创建包含1000+条配置的XML
   - 验证系统在大数据量下的表现

4. **集成到CI/CD**
   - 在自动化构建中运行测试
   - 自动生成测试报告

## 📚 详细文档

- **测试系统完整文档**：`MYMOD_TEST_SYSTEM.md`
- **测试XML说明**：`XModTest/.../TEST_XML_README.md`
- **测试管理器说明**：`Assets/.../CONFIG_TEST_README.md`

## 💡 提示

- 测试会在**每次启动**时自动运行
- 测试结果会输出到**Unity Console**
- 如果测试失败，**不会阻止游戏运行**
- 在**正式发布**时建议禁用测试管理器

---

**开始测试** → 按下Unity播放键 ▶️ → 查看Console输出 ✅
