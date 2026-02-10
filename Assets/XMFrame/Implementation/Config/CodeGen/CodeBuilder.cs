using System;
using System.Collections.Generic;
using System.Text;

namespace XM.ConfigNew.CodeGen
{
    /// <summary>
    /// 代码构建器 - 使用栈管理缩进,避免魔法数字
    /// </summary>
    public class CodeBuilder
    {
        private readonly StringBuilder _sb;
        private readonly Stack<int> _indentStack;
        private int _currentIndent;
        private const string IndentUnit = "    "; // 4空格缩进
        
        public CodeBuilder()
        {
            _sb = new StringBuilder();
            _indentStack = new Stack<int>();
            _currentIndent = 0;
        }
        
        #region 缩进管理
        
        /// <summary>
        /// 增加缩进层级
        /// </summary>
        public void PushIndent()
        {
            _indentStack.Push(_currentIndent);
            _currentIndent++;
        }
        
        /// <summary>
        /// 减少缩进层级
        /// </summary>
        public void PopIndent()
        {
            if (_indentStack.Count > 0)
                _currentIndent = _indentStack.Pop();
        }
        
        /// <summary>
        /// 获取当前缩进字符串
        /// </summary>
        private string GetIndent()
        {
            return new string(' ', _currentIndent * IndentUnit.Length);
        }
        
        #endregion
        
        #region 基础写入
        
        /// <summary>
        /// 写入一行代码(自动添加缩进)
        /// </summary>
        public CodeBuilder AppendLine(string line = "")
        {
            if (!string.IsNullOrEmpty(line))
                _sb.Append(GetIndent()).AppendLine(line);
            else
                _sb.AppendLine();
            return this;
        }
        
        /// <summary>
        /// 写入多行代码
        /// </summary>
        public CodeBuilder AppendLines(params string[] lines)
        {
            foreach (var line in lines)
                AppendLine(line);
            return this;
        }
        
        /// <summary>
        /// 写入代码(不换行,不缩进)
        /// </summary>
        public CodeBuilder Append(string code)
        {
            _sb.Append(code);
            return this;
        }
        
        #endregion
        
        #region 代码块
        
        /// <summary>
        /// 开始代码块 {
        /// </summary>
        public CodeBuilder BeginBlock(string header = null)
        {
            if (!string.IsNullOrEmpty(header))
                AppendLine(header);
            AppendLine("{");
            PushIndent();
            return this;
        }
        
        /// <summary>
        /// 结束代码块 }
        /// </summary>
        public CodeBuilder EndBlock(bool semicolon = false)
        {
            PopIndent();
            AppendLine(semicolon ? "};" : "}");
            return this;
        }
        
        #endregion
        
        #region 常用结构
        
        /// <summary>
        /// 写入using语句
        /// </summary>
        public CodeBuilder AppendUsing(string namespaceName)
        {
            AppendLine($"using {namespaceName};");
            return this;
        }
        
        /// <summary>
        /// 开始命名空间
        /// </summary>
        public CodeBuilder BeginNamespace(string namespaceName)
        {
            return BeginBlock($"namespace {namespaceName}");
        }
        
        /// <summary>
        /// 结束命名空间
        /// </summary>
        public CodeBuilder EndNamespace()
        {
            return EndBlock();
        }
        
        /// <summary>
        /// 开始类定义
        /// </summary>
        public CodeBuilder BeginClass(string className, string baseClass = null, bool isPartial = false, bool isStruct = false)
        {
            var keyword = isStruct ? "struct" : "class";
            var partial = isPartial ? "partial " : "";
            var inheritance = !string.IsNullOrEmpty(baseClass) ? $" : {baseClass}" : "";
            
            return BeginBlock($"public {partial}{keyword} {className}{inheritance}");
        }
        
        /// <summary>
        /// 结束类定义
        /// </summary>
        public CodeBuilder EndClass()
        {
            return EndBlock();
        }
        
        /// <summary>
        /// 写入字段
        /// </summary>
        public CodeBuilder AppendField(string type, string name, string comment = null)
        {
            if (!string.IsNullOrEmpty(comment))
                AppendLine($"/// <summary>{comment}</summary>");
            AppendLine($"public {type} {name};");
            return this;
        }
        
        /// <summary>
        /// 写入属性
        /// </summary>
        public CodeBuilder AppendProperty(string type, string name, string getter, string setter = null, string comment = null)
        {
            if (!string.IsNullOrEmpty(comment))
                AppendLine($"/// <summary>{comment}</summary>");
            
            if (string.IsNullOrEmpty(setter))
                AppendLine($"public {type} {name} => {getter};");
            else
                AppendLine($"public {type} {name} {{ get => {getter}; set => {setter}; }}");
            
            return this;
        }
        
        /// <summary>
        /// 开始方法定义
        /// </summary>
        public CodeBuilder BeginMethod(string signature, string comment = null)
        {
            if (!string.IsNullOrEmpty(comment))
                AppendLine($"/// <summary>{comment}</summary>");
            return BeginBlock($"public {signature}");
        }
        
        /// <summary>
        /// 结束方法定义（等同于 EndBlock）
        /// </summary>
        public CodeBuilder EndMethod()
        {
            return EndBlock();
        }
        
        /// <summary>
        /// 写入注释
        /// </summary>
        public CodeBuilder AppendComment(string comment)
        {
            AppendLine($"// {comment}");
            return this;
        }
        
        /// <summary>
        /// 写入XML文档注释
        /// </summary>
        public CodeBuilder AppendXmlComment(string summary, Dictionary<string, string> parameters = null)
        {
            AppendLine("/// <summary>");
            AppendLine($"/// {summary}");
            AppendLine("/// </summary>");
            
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    AppendLine($"/// <param name=\"{param.Key}\">{param.Value}</param>");
                }
            }
            
            return this;
        }
        
        /// <summary>
        /// 开始Region
        /// </summary>
        public CodeBuilder BeginRegion(string name)
        {
            AppendLine($"#region {name}");
            AppendLine();
            return this;
        }
        
        /// <summary>
        /// 结束Region
        /// </summary>
        public CodeBuilder EndRegion()
        {
            AppendLine();
            AppendLine("#endregion");
            AppendLine();
            return this;
        }
        
        /// <summary>
        /// 开始私有方法定义
        /// </summary>
        public CodeBuilder BeginPrivateMethod(string signature, string comment = null)
        {
            if (!string.IsNullOrEmpty(comment))
                AppendLine($"/// <summary>{comment}</summary>");
            return BeginBlock($"private {signature}");
        }
        
        /// <summary>
        /// 开始静态方法定义
        /// </summary>
        public CodeBuilder BeginStaticMethod(string signature, string comment = null)
        {
            if (!string.IsNullOrEmpty(comment))
                AppendLine($"/// <summary>{comment}</summary>");
            return BeginBlock($"public static {signature}");
        }
        
        /// <summary>
        /// 开始私有静态方法定义
        /// </summary>
        public CodeBuilder BeginPrivateStaticMethod(string signature, string comment = null)
        {
            if (!string.IsNullOrEmpty(comment))
                AppendLine($"/// <summary>{comment}</summary>");
            return BeginBlock($"private static {signature}");
        }
        
        /// <summary>
        /// 写入If语句块
        /// </summary>
        public CodeBuilder BeginIfBlock(string condition)
        {
            return BeginBlock($"if ({condition})");
        }
        
        /// <summary>
        /// 写入Else语句块
        /// </summary>
        public CodeBuilder BeginElseBlock()
        {
            PopIndent();
            AppendLine("else");
            PushIndent();
            AppendLine("{");
            PushIndent();
            return this;
        }
        
        /// <summary>
        /// 写入ElseIf语句块
        /// </summary>
        public CodeBuilder BeginElseIfBlock(string condition)
        {
            PopIndent();
            AppendLine($"else if ({condition})");
            PushIndent();
            AppendLine("{");
            PushIndent();
            return this;
        }
        
        /// <summary>
        /// 写入For循环
        /// </summary>
        public CodeBuilder BeginForLoop(string init, string condition, string increment)
        {
            return BeginBlock($"for ({init}; {condition}; {increment})");
        }
        
        /// <summary>
        /// 写入Foreach循环
        /// </summary>
        public CodeBuilder BeginForeachLoop(string itemType, string itemName, string collection)
        {
            return BeginBlock($"foreach (var {itemName} in {collection})");
        }
        
        /// <summary>
        /// 写入Try语句块
        /// </summary>
        public CodeBuilder BeginTryBlock()
        {
            return BeginBlock("try");
        }
        
        /// <summary>
        /// 写入Catch语句块
        /// </summary>
        public CodeBuilder BeginCatchBlock(string exceptionType = "Exception", string varName = "ex")
        {
            PopIndent();
            AppendLine($"catch ({exceptionType} {varName})");
            PushIndent();
            AppendLine("{");
            PushIndent();
            return this;
        }
        
        #endregion
        
        #region 变量声明和赋值
        
        /// <summary>
        /// 生成变量声明: var xxx = yyy;
        /// </summary>
        public CodeBuilder AppendVarDeclaration(string varName, string expression)
        {
            AppendLine($"var {varName} = {expression};");
            return this;
        }
        
        /// <summary>
        /// 生成带类型的变量声明: Type xxx = yyy;
        /// </summary>
        public CodeBuilder AppendTypedVarDeclaration(string typeName, string varName, string expression)
        {
            AppendLine($"{typeName} {varName} = {expression};");
            return this;
        }
        
        /// <summary>
        /// 生成 default 表达式变量声明: var xxx = default(Type);
        /// </summary>
        public CodeBuilder AppendDefaultVarDeclaration(string varName, string typeName)
        {
            AppendLine($"var {varName} = default({typeName});");
            return this;
        }
        
        /// <summary>
        /// 生成 new 表达式变量声明: var xxx = new Type();
        /// </summary>
        public CodeBuilder AppendNewVarDeclaration(string varName, string typeName)
        {
            AppendLine($"var {varName} = new {typeName}();");
            return this;
        }
        
        /// <summary>
        /// 生成赋值语句: target = value;
        /// </summary>
        public CodeBuilder AppendAssignment(string target, string value)
        {
            AppendLine($"{target} = {value};");
            return this;
        }
        
        #endregion
        
        #region 容器分配（使用 CodeGenConstants）
        
        /// <summary>
        /// 生成 BlobContainer.AllocArray 调用
        /// </summary>
        public CodeBuilder AppendAllocArray(string varName, string elementTypeName, string countExpr)
        {
            AppendLine($"var {varName} = {CodeGenConstants.BlobContainerAccess}.{CodeGenConstants.AllocArrayMethod}<{elementTypeName}>({countExpr});");
            return this;
        }
        
        /// <summary>
        /// 生成 BlobContainer.AllocMap 调用
        /// </summary>
        public CodeBuilder AppendAllocMap(string varName, string keyTypeName, string valueTypeName, string countExpr)
        {
            AppendLine($"var {varName} = {CodeGenConstants.BlobContainerAccess}.{CodeGenConstants.AllocMapMethod}<{keyTypeName}, {valueTypeName}>({countExpr});");
            return this;
        }
        
        /// <summary>
        /// 生成 BlobContainer.AllocSet 调用
        /// </summary>
        public CodeBuilder AppendAllocSet(string varName, string elementTypeName, string countExpr)
        {
            AppendLine($"var {varName} = {CodeGenConstants.BlobContainerAccess}.{CodeGenConstants.AllocSetMethod}<{elementTypeName}>({countExpr});");
            return this;
        }
        
        #endregion
        
        #region Blob 容器赋值
        
        /// <summary>
        /// 生成 Blob 数组索引赋值: container[BlobContainer, index] = value;
        /// </summary>
        public CodeBuilder AppendBlobIndexAssign(string containerVar, string indexExpr, string valueExpr)
        {
            AppendLine($"{containerVar}[{CodeGenConstants.BlobContainerAccess}, {indexExpr}] = {valueExpr};");
            return this;
        }
        
        /// <summary>
        /// 生成 Blob Set 添加: set.Add(BlobContainer, value);
        /// </summary>
        public CodeBuilder AppendBlobSetAdd(string setVar, string valueExpr)
        {
            AppendLine($"{setVar}.{CodeGenConstants.SetAddMethod}({CodeGenConstants.BlobContainerAccess}, {valueExpr});");
            return this;
        }
        
        /// <summary>
        /// 生成 Blob Map 赋值: map[BlobContainer, key] = value;
        /// </summary>
        public CodeBuilder AppendBlobMapAssign(string mapVar, string keyExpr, string valueExpr)
        {
            AppendLine($"{mapVar}[{CodeGenConstants.BlobContainerAccess}, {keyExpr}] = {valueExpr};");
            return this;
        }
        
        #endregion
        
        #region Null/空检查
        
        /// <summary>
        /// 生成 null 或空检查并返回: if (xxx == null || xxx.Count == 0) { return; }
        /// </summary>
        public CodeBuilder AppendNullOrEmptyReturn(string varExpr, string returnStmt = "return;")
        {
            BeginIfBlock($"{varExpr} == null || {varExpr}.{CodeGenConstants.CountProperty} == 0");
            AppendLine(returnStmt);
            EndBlock();
            return this;
        }
        
        /// <summary>
        /// 生成 null 或空检查并 continue: if (xxx == null || xxx.Count == 0) { continue; }
        /// </summary>
        public CodeBuilder AppendNullOrEmptyContinue(string varExpr)
        {
            BeginIfBlock($"{varExpr} == null || {varExpr}.{CodeGenConstants.CountProperty} == 0");
            AppendLine("continue;");
            EndBlock();
            return this;
        }
        
        /// <summary>
        /// 生成 null 检查并 continue: if (xxx == null) { continue; }
        /// </summary>
        public CodeBuilder AppendNullContinue(string varExpr)
        {
            BeginIfBlock($"{varExpr} == null");
            AppendLine("continue;");
            EndBlock();
            return this;
        }
        
        /// <summary>
        /// 生成非 null 且非空检查条件字符串
        /// </summary>
        public static string BuildNotNullAndNotEmptyCondition(string varExpr)
        {
            return $"{varExpr} != null && {varExpr}.{CodeGenConstants.CountProperty} > 0";
        }
        
        /// <summary>
        /// 生成非 null 检查条件字符串
        /// </summary>
        public static string BuildNotNullCondition(string varExpr)
        {
            return $"{varExpr} != null";
        }
        
        #endregion
        
        #region 循环辅助
        
        /// <summary>
        /// 生成标准计数循环: for (int i = 0; i &lt; count; i++)
        /// </summary>
        public CodeBuilder BeginCountLoop(string indexVar, string countExpr)
        {
            return BeginForLoop($"int {indexVar} = 0", $"{indexVar} < {countExpr}", $"{indexVar}++");
        }
        
        /// <summary>
        /// 生成数组/List 索引循环: for (int i = 0; i &lt; collection.Count; i++)
        /// </summary>
        public CodeBuilder BeginIndexLoop(string indexVar, string collectionVar)
        {
            return BeginCountLoop(indexVar, $"{collectionVar}.{CodeGenConstants.CountProperty}");
        }
        
        #endregion
        
        #region 表达式构建辅助（静态方法）
        
        /// <summary>
        /// 构建字段访问表达式: obj.FieldName
        /// </summary>
        public static string BuildFieldAccess(string objVar, string fieldName)
        {
            return $"{objVar}.{fieldName}";
        }
        
        /// <summary>
        /// 构建数组索引访问表达式: array[index]
        /// </summary>
        public static string BuildIndexAccess(string arrayVar, string indexExpr)
        {
            return $"{arrayVar}[{indexExpr}]";
        }
        
        /// <summary>
        /// 构建 GetValueOrDefault 表达式
        /// </summary>
        public static string BuildGetValueOrDefault(string varExpr)
        {
            return $"{varExpr}.{CodeGenConstants.GetValueOrDefaultMethod}()";
        }
        
        /// <summary>
        /// 构建 EnumWrapper 表达式
        /// </summary>
        public static string BuildEnumWrapper(string enumTypeName, string valueExpr)
        {
            return $"new {CodeGenConstants.EnumWrapperPrefix}{enumTypeName}{CodeGenConstants.EnumWrapperSuffix}({valueExpr})";
        }
        
        /// <summary>
        /// 构建 CfgI 泛型类型名
        /// </summary>
        public static string BuildCfgITypeName(string unmanagedTypeName)
        {
            return $"{CodeGenConstants.CfgIGenericPrefix}{unmanagedTypeName}{CodeGenConstants.CfgIGenericSuffix}";
        }
        
        /// <summary>
        /// 构建配置字段访问: config.FieldName
        /// </summary>
        public static string BuildConfigFieldAccess(string fieldName)
        {
            return BuildFieldAccess(CodeGenConstants.ConfigVar, fieldName);
        }
        
        /// <summary>
        /// 构建数据字段访问: data.FieldName
        /// </summary>
        public static string BuildDataFieldAccess(string fieldName)
        {
            return BuildFieldAccess(CodeGenConstants.DataVar, fieldName);
        }
        
        /// <summary>
        /// 构建配置字段索引访问: config.FieldName[index]
        /// </summary>
        public static string BuildConfigFieldIndexAccess(string fieldName, string indexExpr)
        {
            return BuildIndexAccess(BuildConfigFieldAccess(fieldName), indexExpr);
        }
        
        /// <summary>
        /// 构建 BlobContainer 访问表达式（从 data 变量）
        /// </summary>
        public static string BuildBlobContainerAccess(string dataVar = null)
        {
            return $"{(dataVar ?? CodeGenConstants.DataVar)}.BlobContainer";
        }
        
        /// <summary>
        /// 构建 BlobContainer 访问表达式（从 ConfigHolderData）
        /// </summary>
        public static string BuildConfigHolderBlobContainerAccess(string configHolderVar = null)
        {
            var holder = configHolderVar ?? CodeGenConstants.ConfigHolderDataVar;
            return $"{holder}.Data.BlobContainer";
        }
        
        /// <summary>
        /// 构建 GetIndex 调用
        /// </summary>
        public static string BuildGetIndexCall(string indexTypeName, string unmanagedTypeName, string dataVar = null)
        {
            var data = dataVar ?? CodeGenConstants.DataVar;
            return $"var {CodeGenConstants.IndexMapVar} = {data}.{CodeGenConstants.GetIndexMethod}<{indexTypeName}, {unmanagedTypeName}>({indexTypeName}.{CodeGenConstants.IndexTypeProperty});";
        }
        
        /// <summary>
        /// 构建 GetMultiIndex 调用
        /// </summary>
        public static string BuildGetMultiIndexCall(string indexTypeName, string unmanagedTypeName, string dataVar = null)
        {
            var data = dataVar ?? CodeGenConstants.DataVar;
            return $"var {CodeGenConstants.IndexMultiMapVar} = {data}.{CodeGenConstants.GetMultiIndexMethod}<{indexTypeName}, {unmanagedTypeName}>({indexTypeName}.{CodeGenConstants.IndexTypeProperty});";
        }
        
        /// <summary>
        /// 构建 CfgI.As 转换表达式
        /// </summary>
        public static string BuildCfgIAsExpression(string cfgIVar, string unmanagedTypeName)
        {
            return $"{cfgIVar}.{CodeGenConstants.AsMethod}<{unmanagedTypeName}>()";
        }
        
        #endregion
        
        #region 输出
        
        /// <summary>
        /// 获取生成的代码
        /// </summary>
        public string Build()
        {
            return _sb.ToString();
        }
        
        /// <summary>
        /// 清空构建器
        /// </summary>
        public void Clear()
        {
            _sb.Clear();
            _indentStack.Clear();
            _currentIndent = 0;
        }
        
        #endregion
    }
}
