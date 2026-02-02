# MyMod 测试XML文件说明

本目录包含5个全面的测试XML文件，每个文件10条数据，用于验证配置系统的各种功能。

## 文件列表

### 1. MyModConfig_Basic.xml
**测试目标：** MyItemConfig 基础配置类型

**覆盖场景：**
- ✅ 简单字段类型（int, string）
- ✅ List<int> 集合（逗号分隔、分号分隔、多行标签）
- ✅ 边界值测试（0, 最大int值 2147483647）
- ✅ 空标签列表
- ✅ 中文字符串
- ✅ 特殊字符（!@#$%）
- ✅ 重复元素
- ✅ 极长字符串和列表
- ✅ 负数值

**数据ID范围：** basic_001 ~ basic_010

---

### 2. MyModConfig_TestConfig.xml
**测试目标：** TestConfig 复杂配置类型

**覆盖场景：**
- ✅ 基础字段 TestInt
- ✅ List<int> 集合（TestSample）
- ✅ Dictionary<int, int> 字典（TestDictSample）
- ✅ List<CfgS> 配置键列表（TestKeyList）
- ✅ HashSet<int> 哈希集合（TestKeyHashSet, TestSetSample）
- ✅ Dictionary<CfgS, CfgS> 配置键字典（TestKeyDict）
- ✅ HashSet<CfgS> 配置键集合（TestSetKey）
- ✅ CfgS 外键引用（Foreign）
- ✅ XmlIndex 索引字段（TestIndex1, TestIndex2, TestIndex3）
- ✅ 空集合和空字典
- ✅ 负数和极限值
- ✅ 自引用和循环引用

**数据ID范围：** test_001 ~ test_010

---

### 3. MyModConfig_Nested.xml
**测试目标：** 嵌套配置结构（NestedConfig）

**覆盖场景：**
- ✅ 单层嵌套对象（TestNested）
- ✅ 嵌套对象列表（TestNestedConfig）
- ✅ 嵌套字典（ConfigDict）
- ✅ XmlNotNull 必填字段（RequiredId）
- ✅ XmlDefault 默认值字段（OptionalWithDefault）
- ✅ 自定义类型字段（TestCustom: int2, TestGlobalConvert: int2）
- ✅ XmlStringMode 字符串模式
  - ELabelI (StrIndex)
  - EFix32 (Str32)
  - EFix64 (Str64)
  - EStrI (Str)
- ✅ 嵌套配置中的配置键列表（TestKeyList）
- ✅ 多层嵌套结构
- ✅ 嵌套中的负数和极限值
- ✅ 空嵌套列表

**数据ID范围：** nested_001 ~ nested_010

---

### 4. MyModConfig_Link.xml
**测试目标：** XMLLink 链接继承机制（TestInhert）

**覆盖场景：**
- ✅ XMLLink 基础引用
- ✅ 链接到不同的TestConfig配置项
- ✅ 自有字段（xxxx）
- ✅ 零值、负值、极限值测试
- ✅ 验证链接后的数据继承

**数据ID范围：** link_001 ~ link_010

**依赖关系：** 需要先加载 MyModConfig_TestConfig.xml 中的配置项

---

### 5. MyModConfig_Edge.xml
**测试目标：** 边界情况和异常场景

**覆盖场景：**
- ✅ 空字符串和空集合
- ✅ XML特殊字符转义（&lt; &gt; &amp; &quot; &apos;）
- ✅ Unicode字符和Emoji表情
- ✅ 极长列表（50个元素）
- ✅ 负数极限值（-2147483648）
- ✅ 换行符和多余空格
- ✅ 中英文混合
- ✅ 引号测试（单引号、双引号）
- ✅ int边界值（0, 2147483647, -2147483648）
- ✅ 所有字段为空/零的配置项

**数据ID范围：** 
- edge_001 ~ edge_009 (MyItemConfig)
- edge_010 (TestConfig)

---

## 测试建议

### 加载顺序
1. MyModConfig_Basic.xml
2. MyModConfig_TestConfig.xml
3. MyModConfig_Nested.xml
4. MyModConfig_Link.xml（依赖TestConfig）
5. MyModConfig_Edge.xml

### 验证要点

**基础验证：**
- 所有配置项是否成功加载
- 字段值是否正确解析
- 集合和字典的元素数量是否正确

**引用验证：**
- CfgS 外键引用是否正确指向目标配置
- XMLLink 链接是否正确继承父配置数据
- 嵌套配置中的引用是否正确

**边界验证：**
- 空值和null处理是否正确
- 极限值（int.MaxValue, int.MinValue）是否正确
- 特殊字符和Unicode字符是否正确保存

**性能验证：**
- 总计50条配置数据的加载时间
- 内存占用情况
- 查询效率（通过索引查询）

### 常见问题检查清单
- [ ] 是否正确处理分隔符（逗号、分号）
- [ ] 是否正确处理空集合和空字典
- [ ] XMLLink 引用的配置项是否存在
- [ ] XmlNotNull 字段缺失时是否给出警告
- [ ] XmlDefault 默认值是否生效
- [ ] 不同 XmlStringMode 的字符串是否正确存储
- [ ] int2 等自定义类型是否正确转换
- [ ] 嵌套结构的层级是否正确
- [ ] 索引字段是否正确建立索引

---

## 数据统计

| 文件 | 配置类型 | 数据量 | 复杂度 |
|------|---------|--------|--------|
| MyModConfig_Basic.xml | MyItemConfig | 10 | ⭐ |
| MyModConfig_TestConfig.xml | TestConfig | 10 | ⭐⭐⭐ |
| MyModConfig_Nested.xml | TestConfig | 10 | ⭐⭐⭐⭐ |
| MyModConfig_Link.xml | TestInhert | 10 | ⭐⭐ |
| MyModConfig_Edge.xml | Mixed | 10 | ⭐⭐⭐⭐⭐ |
| **总计** | - | **50** | - |

---

## 更新日志

### 2026-02-02
- ✅ 创建5个测试XML文件
- ✅ 每个文件包含10条测试数据
- ✅ 覆盖所有配置类型和边界场景
- ✅ 添加详细的测试说明文档
