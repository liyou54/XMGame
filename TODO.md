# TODO 与规划

## Mod 与 XModTest

- [ ] 主项目首次进入时若 `Mods` 为空，可给出提示或引导到 XModTest 打包
- [ ] Mod 依赖/版本校验（ModDefine 中声明依赖与最低版本）
- [ ] XModTest 打包前可选「仅导出不打包」或「仅打包不导出」
- [ ] 导出时可选目标：主项目 Mods 或自定义目录

## 配置与资源

- [ ] 配置热更流程与版本校验（IXConfig / ConfigDataCenter）
- [ ] YooAsset 与 Mod 资源加载错误码与重试策略统一
- [ ] Mod 内资源配置与主项目资源冲突检测或命名规范

## 框架与工具

- [ ] CopyDllsToXModTest 拷贝完成后可选项：自动打开 XModTest 或仅提示
- [ ] ConfigEditor 生成代码与运行时接口的兼容性测试
- [ ] 对象池、Mod、配置等模块的单元测试与示例场景

## 文档与规范

- [ ] README 补充主项目启动流程与 Mod 加载顺序
- [ ] 新增 Mod 开发规范（目录结构、DllPath、YooAsset 包名）
- [ ] 提交信息统一使用 UTF-8（如 `git commit -F 文件` 或编辑器保存为 UTF-8）

---

*最后更新：随 README 与项目结构调整。*
