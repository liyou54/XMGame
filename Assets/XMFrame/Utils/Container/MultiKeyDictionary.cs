using System;
using System.Collections;
using System.Collections.Generic;

namespace XMFrame.Utils
{
    /// <summary>
    /// 多键字典基础类，提供公共功能
    /// </summary>
    internal static class MultiKeyDictionaryHelper
    {
        /// <summary>
        /// 移除值及其所有键关联的通用逻辑 
        /// </summary>
        public static void RemoveValue<TValue>(
            TValue value,
            Dictionary<TValue, object> valueToKeys,
            Action<object> removeKeyAction)
        {
            if (valueToKeys.TryGetValue(value, out var keysObj))
            {
                if (keysObj is HashSet<object> keys)
                {
                    foreach (var key in keys)
                    {
                        removeKeyAction(key);
                    }
                }
                valueToKeys.Remove(value);
            }
        }
    }

    /// <summary>
    /// 双键字典，支持通过两个不同类型的键索引到同一个值（无装箱）
    /// </summary>
    /// <typeparam name="TKey1">第一个键的类型</typeparam>
    /// <typeparam name="TKey2">第二个键的类型</typeparam>
    /// <typeparam name="TValue">值的类型</typeparam>
    public class MultiKeyDictionary<TKey1, TKey2, TValue> : IEnumerable<TValue>
    where TKey1 : notnull
    where TKey2 : notnull
{
    private Dictionary<TKey1, TValue> _key1Map = new Dictionary<TKey1, TValue>();
    private Dictionary<TKey2, TValue> _key2Map = new Dictionary<TKey2, TValue>();
    private Dictionary<TValue, (TKey1 key1, TKey2 key2)> _valueToKeys = new Dictionary<TValue, (TKey1, TKey2)>();

    /// <summary>
    /// 添加或更新值，并关联两个键
    /// </summary>
    public void AddOrUpdate(TValue value, TKey1 key1, TKey2 key2)
    {
        // 如果值已存在，先移除旧的键关联
        if (_valueToKeys.TryGetValue(value, out var oldKeys))
        {
            _key1Map.Remove(oldKeys.key1);
            _key2Map.Remove(oldKeys.key2);
        }

        // 添加新的键关联
        _key1Map[key1] = value;
        _key2Map[key2] = value;
        _valueToKeys[value] = (key1, key2);
    }

    /// <summary>
    /// 通过第一个键获取值
    /// </summary>
    public TValue GetByKey1(TKey1 key1)
    {
        return _key1Map.TryGetValue(key1, out var value) ? value : default(TValue);
    }

    /// <summary>
    /// 通过第二个键获取值
    /// </summary>
    public TValue GetByKey2(TKey2 key2)
    {
        return _key2Map.TryGetValue(key2, out var value) ? value : default(TValue);
    }

    /// <summary>
    /// 尝试通过第一个键获取值
    /// </summary>
    public bool TryGetValueByKey1(TKey1 key1, out TValue value)
    {
        return _key1Map.TryGetValue(key1, out value);
    }

    /// <summary>
    /// 尝试通过第二个键获取值
    /// </summary>
    public bool TryGetValueByKey2(TKey2 key2, out TValue value)
    {
        return _key2Map.TryGetValue(key2, out value);
    }

    /// <summary>
    /// 检查是否包含第一个键
    /// </summary>
    public bool ContainsKey1(TKey1 key1)
    {
        return _key1Map.ContainsKey(key1);
    }

    /// <summary>
    /// 检查是否包含第二个键
    /// </summary>
    public bool ContainsKey2(TKey2 key2)
    {
        return _key2Map.ContainsKey(key2);
    }

    /// <summary>
    /// 检查是否包含指定的值
    /// </summary>
    public bool ContainsValue(TValue value)
    {
        return _valueToKeys.ContainsKey(value);
    }

    /// <summary>
    /// 移除指定值及其所有关联的键
    /// </summary>
    public bool Remove(TValue value)
    {
        if (!_valueToKeys.TryGetValue(value, out var keys))
        {
            return false;
        }

        _key1Map.Remove(keys.key1);
        _key2Map.Remove(keys.key2);
        _valueToKeys.Remove(value);
        return true;
    }

    /// <summary>
    /// 移除第一个键及其关联的值（如果该值没有其他键关联，则同时移除值）
    /// </summary>
    public bool RemoveKey1(TKey1 key1)
    {
        if (!_key1Map.TryGetValue(key1, out var value))
        {
            return false;
        }

        _key1Map.Remove(key1);
        if (_valueToKeys.TryGetValue(value, out var keys))
        {
            _key2Map.Remove(keys.key2);
            _valueToKeys.Remove(value);
        }
        return true;
    }

    /// <summary>
    /// 移除第二个键及其关联的值（如果该值没有其他键关联，则同时移除值）
    /// </summary>
    public bool RemoveKey2(TKey2 key2)
    {
        if (!_key2Map.TryGetValue(key2, out var value))
        {
            return false;
        }

        _key2Map.Remove(key2);
        if (_valueToKeys.TryGetValue(value, out var keys))
        {
            _key1Map.Remove(keys.key1);
            _valueToKeys.Remove(value);
        }
        return true;
    }

    /// <summary>
    /// 获取指定值关联的所有键
    /// </summary>
    public (TKey1 key1, TKey2 key2) GetKeys(TValue value)
    {
        return _valueToKeys.TryGetValue(value, out var keys) ? keys : default;
    }

    /// <summary>
    /// 清空所有数据
    /// </summary>
    public void Clear()
    {
        _key1Map.Clear();
        _key2Map.Clear();
        _valueToKeys.Clear();
    }

    /// <summary>
    /// 获取值的数量
    /// </summary>
    public int Count => _valueToKeys.Count;

    /// <summary>
    /// 通过第一个键访问值（索引器）
    /// </summary>
    public TValue this[TKey1 key1]
    {
        get => GetByKey1(key1);
        set
        {
            if (_key1Map.TryGetValue(key1, out var oldValue))
            {
                Remove(oldValue);
            }
            // 需要提供key2才能添加，这里抛出异常提示
            throw new InvalidOperationException("使用索引器设置值时，请使用 AddOrUpdate 方法并提供所有键");
        }
    }

    /// <summary>
    /// 通过第二个键访问值（索引器）
    /// </summary>
    public TValue this[TKey2 key2]
    {
        get => GetByKey2(key2);
        set
        {
            if (_key2Map.TryGetValue(key2, out var oldValue))
            {
                Remove(oldValue);
            }
            throw new InvalidOperationException("使用索引器设置值时，请使用 AddOrUpdate 方法并提供所有键");
        }
    }

    /// <summary>
    /// 获取所有值的集合
    /// </summary>
    public IEnumerable<TValue> Values => _valueToKeys.Keys;

    /// <summary>
    /// 获取所有键值对的集合（包含值和对应的两个键）
    /// </summary>
    public IEnumerable<(TValue value, TKey1 key1, TKey2 key2)> Pairs
    {
        get
        {
            foreach (var kvp in _valueToKeys)
            {
                yield return (kvp.Key, kvp.Value.key1, kvp.Value.key2);
            }
        }
    }

    /// <summary>
    /// 获取所有第一个键的集合
    /// </summary>
    public IEnumerable<TKey1> Keys1 => _key1Map.Keys;

    /// <summary>
    /// 获取所有第二个键的集合
    /// </summary>
    public IEnumerable<TKey2> Keys2 => _key2Map.Keys;

    /// <summary>
    /// 返回值的迭代器（默认迭代器）
    /// </summary>
    public IEnumerator<TValue> GetEnumerator()
    {
        return _valueToKeys.Keys.GetEnumerator();
    }

    /// <summary>
    /// 返回值的迭代器（非泛型）
    /// </summary>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// 返回第一个键的迭代器
    /// </summary>
    public IEnumerator<TKey1> GetKeys1Enumerator()
    {
        return _key1Map.Keys.GetEnumerator();
    }

    /// <summary>
    /// 返回第二个键的迭代器
    /// </summary>
    public IEnumerator<TKey2> GetKeys2Enumerator()
    {
        return _key2Map.Keys.GetEnumerator();
    }

    /// <summary>
    /// 返回值的迭代器
    /// </summary>
    public IEnumerator<TValue> GetValuesEnumerator()
    {
        return _valueToKeys.Keys.GetEnumerator();
    }

    /// <summary>
    /// 返回键值对的迭代器（包含值和对应的两个键）
    /// </summary>
    public IEnumerator<(TValue value, TKey1 key1, TKey2 key2)> GetPairsEnumerator()
    {
        foreach (var kvp in _valueToKeys)
        {
            yield return (kvp.Key, kvp.Value.key1, kvp.Value.key2);
        }
    }
}

/// <summary>
/// 三键字典，支持通过三个不同类型的键索引到同一个值（无装箱）
/// </summary>
/// <typeparam name="TKey1">第一个键的类型</typeparam>
/// <typeparam name="TKey2">第二个键的类型</typeparam>
/// <typeparam name="TKey3">第三个键的类型</typeparam>
/// <typeparam name="TValue">值的类型</typeparam>
public class MultiKeyDictionary<TKey1, TKey2, TKey3, TValue> : IEnumerable<TValue>
    where TKey1 : notnull, IEquatable<TKey1>
    where TKey2 : notnull, IEquatable<TKey2>
    where TKey3 : notnull, IEquatable<TKey3>
{
    private Dictionary<TKey1, TValue> _key1Map = new Dictionary<TKey1, TValue>();
    private Dictionary<TKey2, TValue> _key2Map = new Dictionary<TKey2, TValue>();
    private Dictionary<TKey3, TValue> _key3Map = new Dictionary<TKey3, TValue>();
    private Dictionary<TValue, (TKey1 key1, TKey2 key2, TKey3 key3)> _valueToKeys = new Dictionary<TValue, (TKey1, TKey2, TKey3)>();

    /// <summary>
    /// 添加或更新值，并关联三个键
    /// </summary>
    public void AddOrUpdate(TValue value, TKey1 key1, TKey2 key2, TKey3 key3)
    {
        // 如果值已存在，先移除旧的键关联
        if (_valueToKeys.TryGetValue(value, out var oldKeys))
        {
            _key1Map.Remove(oldKeys.key1);
            _key2Map.Remove(oldKeys.key2);
            _key3Map.Remove(oldKeys.key3);
        }

        // 添加新的键关联
        _key1Map[key1] = value;
        _key2Map[key2] = value;
        _key3Map[key3] = value;
        _valueToKeys[value] = (key1, key2, key3);
    }

    /// <summary>
    /// 通过第一个键获取值
    /// </summary>
    public TValue GetByKey1(TKey1 key1)
    {
        return _key1Map.TryGetValue(key1, out var value) ? value : default(TValue);
    }

    /// <summary>
    /// 通过第二个键获取值
    /// </summary>
    public TValue GetByKey2(TKey2 key2)
    {
        return _key2Map.TryGetValue(key2, out var value) ? value : default(TValue);
    }

    /// <summary>
    /// 通过第三个键获取值
    /// </summary>
    public TValue GetByKey3(TKey3 key3)
    {
        return _key3Map.TryGetValue(key3, out var value) ? value : default(TValue);
    }

    /// <summary>
    /// 尝试通过第一个键获取值
    /// </summary>
    public bool TryGetValueByKey1(TKey1 key1, out TValue value)
    {
        return _key1Map.TryGetValue(key1, out value);
    }

    /// <summary>
    /// 尝试通过第二个键获取值
    /// </summary>
    public bool TryGetValueByKey2(TKey2 key2, out TValue value)
    {
        return _key2Map.TryGetValue(key2, out value);
    }

    /// <summary>
    /// 尝试通过第三个键获取值
    /// </summary>
    public bool TryGetValueByKey3(TKey3 key3, out TValue value)
    {
        return _key3Map.TryGetValue(key3, out value);
    }

    /// <summary>
    /// 检查是否包含第一个键
    /// </summary>
    public bool ContainsKey1(TKey1 key1)
    {
        return _key1Map.ContainsKey(key1);
    }

    /// <summary>
    /// 检查是否包含第二个键
    /// </summary>
    public bool ContainsKey2(TKey2 key2)
    {
        return _key2Map.ContainsKey(key2);
    }

    /// <summary>
    /// 检查是否包含第三个键
    /// </summary>
    public bool ContainsKey3(TKey3 key3)
    {
        return _key3Map.ContainsKey(key3);
    }

    /// <summary>
    /// 检查是否包含指定的值
    /// </summary>
    public bool ContainsValue(TValue value)
    {
        return _valueToKeys.ContainsKey(value);
    }

    /// <summary>
    /// 移除指定值及其所有关联的键
    /// </summary>
    public bool Remove(TValue value)
    {
        if (!_valueToKeys.TryGetValue(value, out var keys))
        {
            return false;
        }

        _key1Map.Remove(keys.key1);
        _key2Map.Remove(keys.key2);
        _key3Map.Remove(keys.key3);
        _valueToKeys.Remove(value);
        return true;
    }

    /// <summary>
    /// 移除第一个键及其关联的值（如果该值没有其他键关联，则同时移除值）
    /// </summary>
    public bool RemoveKey1(TKey1 key1)
    {
        if (!_key1Map.TryGetValue(key1, out var value))
        {
            return false;
        }

        _key1Map.Remove(key1);
        if (_valueToKeys.TryGetValue(value, out var keys))
        {
            _key2Map.Remove(keys.key2);
            _key3Map.Remove(keys.key3);
            _valueToKeys.Remove(value);
        }
        return true;
    }

    /// <summary>
    /// 移除第二个键及其关联的值（如果该值没有其他键关联，则同时移除值）
    /// </summary>
    public bool RemoveKey2(TKey2 key2)
    {
        if (!_key2Map.TryGetValue(key2, out var value))
        {
            return false;
        }

        _key2Map.Remove(key2);
        if (_valueToKeys.TryGetValue(value, out var keys))
        {
            _key1Map.Remove(keys.key1);
            _key3Map.Remove(keys.key3);
            _valueToKeys.Remove(value);
        }
        return true;
    }

    /// <summary>
    /// 移除第三个键及其关联的值（如果该值没有其他键关联，则同时移除值）
    /// </summary>
    public bool RemoveKey3(TKey3 key3)
    {
        if (!_key3Map.TryGetValue(key3, out var value))
        {
            return false;
        }

        _key3Map.Remove(key3);
        if (_valueToKeys.TryGetValue(value, out var keys))
        {
            _key1Map.Remove(keys.key1);
            _key2Map.Remove(keys.key2);
            _valueToKeys.Remove(value);
        }
        return true;
    }

    /// <summary>
    /// 获取指定值关联的所有键
    /// </summary>
    public (TKey1 key1, TKey2 key2, TKey3 key3) GetKeys(TValue value)
    {
        return _valueToKeys.TryGetValue(value, out var keys) ? keys : default;
    }

    /// <summary>
    /// 清空所有数据
    /// </summary>
    public void Clear()
    {
        _key1Map.Clear();
        _key2Map.Clear();
        _key3Map.Clear();
        _valueToKeys.Clear();
    }

    /// <summary>
    /// 获取值的数量
    /// </summary>
    public int Count => _valueToKeys.Count;

    /// <summary>
    /// 通过第一个键访问值（索引器）
    /// </summary>
    public TValue this[TKey1 key1]
    {
        get => GetByKey1(key1);
        set => throw new InvalidOperationException("使用索引器设置值时，请使用 AddOrUpdate 方法并提供所有键");
    }

    /// <summary>
    /// 通过第二个键访问值（索引器）
    /// </summary>
    public TValue this[TKey2 key2]
    {
        get => GetByKey2(key2);
        set => throw new InvalidOperationException("使用索引器设置值时，请使用 AddOrUpdate 方法并提供所有键");
    }

    /// <summary>
    /// 通过第三个键访问值（索引器）
    /// </summary>
    public TValue this[TKey3 key3]
    {
        get => GetByKey3(key3);
        set => throw new InvalidOperationException("使用索引器设置值时，请使用 AddOrUpdate 方法并提供所有键");
    }

    /// <summary>
    /// 获取所有值的集合
    /// </summary>
    public IEnumerable<TValue> Values => _valueToKeys.Keys;

    /// <summary>
    /// 获取所有键值对的集合（包含值和对应的三个键）
    /// </summary>
    public IEnumerable<(TValue value, TKey1 key1, TKey2 key2, TKey3 key3)> Pairs
    {
        get
        {
            foreach (var kvp in _valueToKeys)
            {
                yield return (kvp.Key, kvp.Value.key1, kvp.Value.key2, kvp.Value.key3);
            }
        }
    }

    /// <summary>
    /// 获取所有第一个键的集合
    /// </summary>
    public IEnumerable<TKey1> Keys1 => _key1Map.Keys;

    /// <summary>
    /// 获取所有第二个键的集合
    /// </summary>
    public IEnumerable<TKey2> Keys2 => _key2Map.Keys;

    /// <summary>
    /// 获取所有第三个键的集合
    /// </summary>
    public IEnumerable<TKey3> Keys3 => _key3Map.Keys;

    /// <summary>
    /// 返回值的迭代器（默认迭代器）
    /// </summary>
    public IEnumerator<TValue> GetEnumerator()
    {
        return _valueToKeys.Keys.GetEnumerator();
    }

    /// <summary>
    /// 返回值的迭代器（非泛型）
    /// </summary>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// 返回第一个键的迭代器
    /// </summary>
    public IEnumerator<TKey1> GetKeys1Enumerator()
    {
        return _key1Map.Keys.GetEnumerator();
    }

    /// <summary>
    /// 返回第二个键的迭代器
    /// </summary>
    public IEnumerator<TKey2> GetKeys2Enumerator()
    {
        return _key2Map.Keys.GetEnumerator();
    }

    /// <summary>
    /// 返回第三个键的迭代器
    /// </summary>
    public IEnumerator<TKey3> GetKeys3Enumerator()
    {
        return _key3Map.Keys.GetEnumerator();
    }

    /// <summary>
    /// 返回值的迭代器
    /// </summary>
    public IEnumerator<TValue> GetValuesEnumerator()
    {
        return _valueToKeys.Keys.GetEnumerator();
    }

    /// <summary>
    /// 返回键值对的迭代器（包含值和对应的三个键）
    /// </summary>
    public IEnumerator<(TValue value, TKey1 key1, TKey2 key2, TKey3 key3)> GetPairsEnumerator()
    {
        foreach (var kvp in _valueToKeys)
        {
            yield return (kvp.Key, kvp.Value.key1, kvp.Value.key2, kvp.Value.key3);
        }
    }
}

/// <summary>
/// 四键字典，支持通过四个不同类型的键索引到同一个值（无装箱）
/// </summary>
/// <typeparam name="TKey1">第一个键的类型</typeparam>
/// <typeparam name="TKey2">第二个键的类型</typeparam>
/// <typeparam name="TKey3">第三个键的类型</typeparam>
/// <typeparam name="TKey4">第四个键的类型</typeparam>
/// <typeparam name="TValue">值的类型</typeparam>
public class MultiKeyDictionary<TKey1, TKey2, TKey3, TKey4, TValue> : IEnumerable<TValue>
    where TKey1 : notnull, IEquatable<TKey1>
    where TKey2 : notnull, IEquatable<TKey2>
    where TKey3 : notnull, IEquatable<TKey3>
    where TKey4 : notnull, IEquatable<TKey4>
{
    private Dictionary<TKey1, TValue> _key1Map = new Dictionary<TKey1, TValue>();
    private Dictionary<TKey2, TValue> _key2Map = new Dictionary<TKey2, TValue>();
    private Dictionary<TKey3, TValue> _key3Map = new Dictionary<TKey3, TValue>();
    private Dictionary<TKey4, TValue> _key4Map = new Dictionary<TKey4, TValue>();
    private Dictionary<TValue, (TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4)> _valueToKeys = new Dictionary<TValue, (TKey1, TKey2, TKey3, TKey4)>();

    /// <summary>
    /// 添加或更新值，并关联四个键
    /// </summary>
    public void AddOrUpdate(TValue value, TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4)
    {
        // 如果值已存在，先移除旧的键关联
        if (_valueToKeys.TryGetValue(value, out var oldKeys))
        {
            _key1Map.Remove(oldKeys.key1);
            _key2Map.Remove(oldKeys.key2);
            _key3Map.Remove(oldKeys.key3);
            _key4Map.Remove(oldKeys.key4);
        }

        // 添加新的键关联
        _key1Map[key1] = value;
        _key2Map[key2] = value;
        _key3Map[key3] = value;
        _key4Map[key4] = value;
        _valueToKeys[value] = (key1, key2, key3, key4);
    }

    /// <summary>
    /// 通过第一个键获取值
    /// </summary>
    public TValue GetByKey1(TKey1 key1)
    {
        return _key1Map.TryGetValue(key1, out var value) ? value : default(TValue);
    }

    /// <summary>
    /// 通过第二个键获取值
    /// </summary>
    public TValue GetByKey2(TKey2 key2)
    {
        return _key2Map.TryGetValue(key2, out var value) ? value : default(TValue);
    }

    /// <summary>
    /// 通过第三个键获取值
    /// </summary>
    public TValue GetByKey3(TKey3 key3)
    {
        return _key3Map.TryGetValue(key3, out var value) ? value : default(TValue);
    }

    /// <summary>
    /// 通过第四个键获取值
    /// </summary>
    public TValue GetByKey4(TKey4 key4)
    {
        return _key4Map.TryGetValue(key4, out var value) ? value : default(TValue);
    }

    /// <summary>
    /// 尝试通过第一个键获取值
    /// </summary>
    public bool TryGetValueByKey1(TKey1 key1, out TValue value)
    {
        return _key1Map.TryGetValue(key1, out value);
    }

    /// <summary>
    /// 尝试通过第二个键获取值
    /// </summary>
    public bool TryGetValueByKey2(TKey2 key2, out TValue value)
    {
        return _key2Map.TryGetValue(key2, out value);
    }

    /// <summary>
    /// 尝试通过第三个键获取值
    /// </summary>
    public bool TryGetValueByKey3(TKey3 key3, out TValue value)
    {
        return _key3Map.TryGetValue(key3, out value);
    }

    /// <summary>
    /// 尝试通过第四个键获取值
    /// </summary>
    public bool TryGetValueByKey4(TKey4 key4, out TValue value)
    {
        return _key4Map.TryGetValue(key4, out value);
    }

    /// <summary>
    /// 检查是否包含第一个键
    /// </summary>
    public bool ContainsKey1(TKey1 key1)
    {
        return _key1Map.ContainsKey(key1);
    }

    /// <summary>
    /// 检查是否包含第二个键
    /// </summary>
    public bool ContainsKey2(TKey2 key2)
    {
        return _key2Map.ContainsKey(key2);
    }

    /// <summary>
    /// 检查是否包含第三个键
    /// </summary>
    public bool ContainsKey3(TKey3 key3)
    {
        return _key3Map.ContainsKey(key3);
    }

    /// <summary>
    /// 检查是否包含第四个键
    /// </summary>
    public bool ContainsKey4(TKey4 key4)
    {
        return _key4Map.ContainsKey(key4);
    }

    /// <summary>
    /// 检查是否包含指定的值
    /// </summary>
    public bool ContainsValue(TValue value)
    {
        return _valueToKeys.ContainsKey(value);
    }

    /// <summary>
    /// 移除指定值及其所有关联的键
    /// </summary>
    public bool Remove(TValue value)
    {
        if (!_valueToKeys.TryGetValue(value, out var keys))
        {
            return false;
        }

        _key1Map.Remove(keys.key1);
        _key2Map.Remove(keys.key2);
        _key3Map.Remove(keys.key3);
        _key4Map.Remove(keys.key4);
        _valueToKeys.Remove(value);
        return true;
    }

    /// <summary>
    /// 移除第一个键及其关联的值（如果该值没有其他键关联，则同时移除值）
    /// </summary>
    public bool RemoveKey1(TKey1 key1)
    {
        if (!_key1Map.TryGetValue(key1, out var value))
        {
            return false;
        }

        _key1Map.Remove(key1);
        if (_valueToKeys.TryGetValue(value, out var keys))
        {
            _key2Map.Remove(keys.key2);
            _key3Map.Remove(keys.key3);
            _key4Map.Remove(keys.key4);
            _valueToKeys.Remove(value);
        }
        return true;
    }

    /// <summary>
    /// 移除第二个键及其关联的值（如果该值没有其他键关联，则同时移除值）
    /// </summary>
    public bool RemoveKey2(TKey2 key2)
    {
        if (!_key2Map.TryGetValue(key2, out var value))
        {
            return false;
        }

        _key2Map.Remove(key2);
        if (_valueToKeys.TryGetValue(value, out var keys))
        {
            _key1Map.Remove(keys.key1);
            _key3Map.Remove(keys.key3);
            _key4Map.Remove(keys.key4);
            _valueToKeys.Remove(value);
        }
        return true;
    }

    /// <summary>
    /// 移除第三个键及其关联的值（如果该值没有其他键关联，则同时移除值）
    /// </summary>
    public bool RemoveKey3(TKey3 key3)
    {
        if (!_key3Map.TryGetValue(key3, out var value))
        {
            return false;
        }

        _key3Map.Remove(key3);
        if (_valueToKeys.TryGetValue(value, out var keys))
        {
            _key1Map.Remove(keys.key1);
            _key2Map.Remove(keys.key2);
            _key4Map.Remove(keys.key4);
            _valueToKeys.Remove(value);
        }
        return true;
    }

    /// <summary>
    /// 移除第四个键及其关联的值（如果该值没有其他键关联，则同时移除值）
    /// </summary>
    public bool RemoveKey4(TKey4 key4)
    {
        if (!_key4Map.TryGetValue(key4, out var value))
        {
            return false;
        }

        _key4Map.Remove(key4);
        if (_valueToKeys.TryGetValue(value, out var keys))
        {
            _key1Map.Remove(keys.key1);
            _key2Map.Remove(keys.key2);
            _key3Map.Remove(keys.key3);
            _valueToKeys.Remove(value);
        }
        return true;
    }

    /// <summary>
    /// 获取指定值关联的所有键
    /// </summary>
    public (TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4) GetKeys(TValue value)
    {
        return _valueToKeys.TryGetValue(value, out var keys) ? keys : default;
    }

    /// <summary>
    /// 清空所有数据
    /// </summary>
    public void Clear()
    {
        _key1Map.Clear();
        _key2Map.Clear();
        _key3Map.Clear();
        _key4Map.Clear();
        _valueToKeys.Clear();
    }

    /// <summary>
    /// 获取值的数量
    /// </summary>
    public int Count => _valueToKeys.Count;

    /// <summary>
    /// 通过第一个键访问值（索引器）
    /// </summary>
    public TValue this[TKey1 key1]
    {
        get => GetByKey1(key1);
        set => throw new InvalidOperationException("使用索引器设置值时，请使用 AddOrUpdate 方法并提供所有键");
    }

    /// <summary>
    /// 通过第二个键访问值（索引器）
    /// </summary>
    public TValue this[TKey2 key2]
    {
        get => GetByKey2(key2);
        set => throw new InvalidOperationException("使用索引器设置值时，请使用 AddOrUpdate 方法并提供所有键");
    }

    /// <summary>
    /// 通过第三个键访问值（索引器）
    /// </summary>
    public TValue this[TKey3 key3]
    {
        get => GetByKey3(key3);
        set => throw new InvalidOperationException("使用索引器设置值时，请使用 AddOrUpdate 方法并提供所有键");
    }

    /// <summary>
    /// 通过第四个键访问值（索引器）
    /// </summary>
    public TValue this[TKey4 key4]
    {
        get => GetByKey4(key4);
        set => throw new InvalidOperationException("使用索引器设置值时，请使用 AddOrUpdate 方法并提供所有键");
    }

    /// <summary>
    /// 获取所有值的集合
    /// </summary>
    public IEnumerable<TValue> Values => _valueToKeys.Keys;

    /// <summary>
    /// 获取所有键值对的集合（包含值和对应的四个键）
    /// </summary>
    public IEnumerable<(TValue value, TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4)> Pairs
    {
        get
        {
            foreach (var kvp in _valueToKeys)
            {
                yield return (kvp.Key, kvp.Value.key1, kvp.Value.key2, kvp.Value.key3, kvp.Value.key4);
            }
        }
    }

    /// <summary>
    /// 获取所有第一个键的集合
    /// </summary>
    public IEnumerable<TKey1> Keys1 => _key1Map.Keys;

    /// <summary>
    /// 获取所有第二个键的集合
    /// </summary>
    public IEnumerable<TKey2> Keys2 => _key2Map.Keys;

    /// <summary>
    /// 获取所有第三个键的集合
    /// </summary>
    public IEnumerable<TKey3> Keys3 => _key3Map.Keys;

    /// <summary>
    /// 获取所有第四个键的集合
    /// </summary>
    public IEnumerable<TKey4> Keys4 => _key4Map.Keys;

    /// <summary>
    /// 返回值的迭代器（默认迭代器）
    /// </summary>
    public IEnumerator<TValue> GetEnumerator()
    {
        return _valueToKeys.Keys.GetEnumerator();
    }

    /// <summary>
    /// 返回值的迭代器（非泛型）
    /// </summary>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// 返回第一个键的迭代器
    /// </summary>
    public IEnumerator<TKey1> GetKeys1Enumerator()
    {
        return _key1Map.Keys.GetEnumerator();
    }

    /// <summary>
    /// 返回第二个键的迭代器
    /// </summary>
    public IEnumerator<TKey2> GetKeys2Enumerator()
    {
        return _key2Map.Keys.GetEnumerator();
    }

    /// <summary>
    /// 返回第三个键的迭代器
    /// </summary>
    public IEnumerator<TKey3> GetKeys3Enumerator()
    {
        return _key3Map.Keys.GetEnumerator();
    }

    /// <summary>
    /// 返回第四个键的迭代器
    /// </summary>
    public IEnumerator<TKey4> GetKeys4Enumerator()
    {
        return _key4Map.Keys.GetEnumerator();
    }

    /// <summary>
    /// 返回值的迭代器
    /// </summary>
    public IEnumerator<TValue> GetValuesEnumerator()
    {
        return _valueToKeys.Keys.GetEnumerator();
    }

    /// <summary>
    /// 返回键值对的迭代器（包含值和对应的四个键）
    /// </summary>
    public IEnumerator<(TValue value, TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4)> GetPairsEnumerator()
    {
        foreach (var kvp in _valueToKeys)
        {
            yield return (kvp.Key, kvp.Value.key1, kvp.Value.key2, kvp.Value.key3, kvp.Value.key4);
        }
    }
}
}
