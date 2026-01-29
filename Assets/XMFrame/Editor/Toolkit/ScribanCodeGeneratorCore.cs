using System;
using Scriban;
using Scriban.Runtime;

namespace XModToolkit
{
    /// <summary>
    /// 纯 Scriban 渲染核心，不依赖 Unity。模板从字符串加载，渲染结果返回字符串。
    /// </summary>
    public static class ScribanCodeGeneratorCore
    {
        /// <summary>
        /// 从字符串解析 Scriban 模板。
        /// </summary>
        public static bool TryParseTemplate(string templateContent, out Template template)
        {
            template = null;
            if (string.IsNullOrEmpty(templateContent))
                return false;
            try
            {
                var parsed = Template.Parse(templateContent);
                if (parsed.HasErrors)
                    return false;
                template = parsed;
                return true;
            }
            catch
            {
                return false;
            }
        } 

        /// <summary>
        /// 使用 ScriptObject 渲染模板，返回生成文本。不依赖 Unity。
        /// </summary>
        public static bool TryRender(Template template, ScriptObject model, out string result)
        { 
            result = null;
            if (template == null || model == null)
                return false; 
            try
            {
                var context = new TemplateContext();
                context.PushGlobal(model);
                result = template.Render(context); 
                return !template.HasErrors;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
