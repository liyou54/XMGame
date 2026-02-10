using System.Collections.Generic;
using XM.Contracts;
using XM.Contracts.Config;

namespace XM
{
    public class UIStackCollection
    {
        private readonly Dictionary<EUILayer, List<UIStack>> _uiStacks = new Dictionary<EUILayer, List<UIStack>>();

        /// <summary>
        /// 检查是否能够重用UI栈
        /// </summary>
        public bool CanReuseUIInstance(in UIConfigUnManaged config)
        {
            var layer = config.UILayer;
            if (!_uiStacks.TryGetValue(layer, out var stacks))
            {
                return false;
            }

            foreach (var stack in stacks)
            {
                if (stack.UIConfigUnmanaged.Id == config.Id)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 按 config 查找栈（Single/Stack 模式取第一个匹配）
        /// </summary>
        public bool TryFindStack(EUILayer layer, CfgI<UIConfigUnManaged> configId, out UIStack stack)
        {
            stack = null;
            if (!_uiStacks.TryGetValue(layer, out var stacks))
            {
                return false;
            }

            foreach (var s in stacks)
            {
                if (s.UIConfigUnmanaged.Id == configId)
                {
                    stack = s;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 按 handle 查找栈（用于 CloseUI(handle) 等）
        /// </summary>
        public bool TryFindStackByHandle(EUILayer layer, UIHandle handle, out UIStack stack)
        {
            stack = null;
            if (!_uiStacks.TryGetValue(layer, out var stacks))
            {
                return false;
            }

            foreach (var s in stacks)
            {
                if (EqualityComparer<UIHandle>.Default.Equals(s.UIHandle, handle))
                {
                    stack = s;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 按 handle 在所有层中查找栈
        /// </summary>
        public bool TryFindStackByHandle(UIHandle handle, out UIStack stack)
        {
            stack = null;
            foreach (var kvp in _uiStacks)
            {
                foreach (var s in kvp.Value)
                {
                    if (EqualityComparer<UIHandle>.Default.Equals(s.UIHandle, handle))
                    {
                        stack = s;
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 获取该 config 的所有栈（Multi 模式）
        /// </summary>
        public IReadOnlyList<UIStack> GetStacksByConfig(EUILayer layer, CfgI<UIConfigUnManaged> configId)
        {
            if (!_uiStacks.TryGetValue(layer, out var stacks))
            {
                return System.Array.Empty<UIStack>();
            }

            var result = new List<UIStack>();
            foreach (var s in stacks)
            {
                if (s.UIConfigUnmanaged.Id == configId)
                {
                    result.Add(s);
                }
            }

            return result;
        }

        /// <summary>
        /// 获取该层所有栈
        /// </summary>
        public IReadOnlyList<UIStack> GetStacksByLayer(EUILayer layer)
        {
            if (!_uiStacks.TryGetValue(layer, out var stacks))
            {
                return System.Array.Empty<UIStack>();
            }

            return stacks;
        }

        /// <summary>
        /// 获取层顶栈（列表最后一个）
        /// </summary>
        public UIStack GetTopStack(EUILayer layer)
        {
            if (!_uiStacks.TryGetValue(layer, out var stacks) || stacks.Count == 0)
            {
                return null;
            }

            return stacks[^1];
        }

        /// <summary>
        /// 添加栈到指定层
        /// </summary>
        public void AddStack(EUILayer layer, UIStack stack)
        {
            EnsureLayerExists(layer);
            _uiStacks[layer].Add(stack);
        }

        /// <summary>
        /// 移除指定栈
        /// </summary>
        public bool RemoveStack(EUILayer layer, UIStack stack)
        {
            if (!_uiStacks.TryGetValue(layer, out var stacks))
            {
                return false;
            }

            return stacks.Remove(stack);
        }

        /// <summary>
        /// 按 config 移除一个栈（Single/Stack 模式）
        /// </summary>
        public bool RemoveStack(EUILayer layer, CfgI<UIConfigUnManaged> configId)
        {
            if (!TryFindStack(layer, configId, out var stack))
            {
                return false;
            }

            return RemoveStack(layer, stack);
        }

        /// <summary>
        /// 移除该 config 的所有栈（Multi 模式）
        /// </summary>
        public int RemoveAllStacksByConfig(EUILayer layer, CfgI<UIConfigUnManaged> configId)
        {
            if (!_uiStacks.TryGetValue(layer, out var stacks))
            {
                return 0;
            }

            var count = 0;
            for (var i = stacks.Count - 1; i >= 0; i--)
            {
                if (stacks[i].UIConfigUnmanaged.Id == configId)
                {
                    stacks.RemoveAt(i);
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// 将栈移到层顶（MoveToTop）
        /// </summary>
        public void MoveStackToTop(EUILayer layer, UIStack stack)
        {
            if (!_uiStacks.TryGetValue(layer, out var stacks))
            {
                return;
            }

            if (!stacks.Remove(stack))
            {
                return;
            }

            stacks.Add(stack);
        }

        /// <summary>
        /// 确保指定层存在
        /// </summary>
        public void EnsureLayerExists(EUILayer layer)
        {
            if (!_uiStacks.ContainsKey(layer))
            {
                _uiStacks[layer] = new List<UIStack>();
            }
        }
    }
}
