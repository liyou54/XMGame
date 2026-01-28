using System;
using System.Collections;
using System.Collections.Generic;

namespace XM.Utils
{
    /// <summary>
    /// 双向索引容器，支持通过键查找值，也支持通过值查找键（一对一映射，无装箱）
    /// </summary>
    /// <typeparam name="TKey">键的类型</typeparam>
    /// <typeparam name="TValue">值的类型</typeparam>
    public class BidirectionalDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
        where TKey : notnull
        where TValue : notnull
    {
        private Dictionary<TKey, TValue> _keyToValue = new Dictionary<TKey, TValue>();
        private Dictionary<TValue, TKey> _valueToKey = new Dictionary<TValue, TKey>();

        /// <summary>
        /// 添加或更新键值对
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <exception cref="ArgumentException">如果键或值已存在且对应不同的值/键时抛出</exception>
        public void AddOrUpdate(TKey key, TValue value)
        {
            // 如果键已存在，先移除旧的映射
            if (_keyToValue.TryGetValue(key, out var oldValue))
            {
                if (oldValue.Equals(value))
                {
                    return; // 键值对已存在，无需更新
                }
                _valueToKey.Remove(oldValue);
            }

            // 如果值已存在，先移除旧的映射
            if (_valueToKey.TryGetValue(value, out var oldKey))
            {
                if (oldKey.Equals(key))
                {
                    return; // 键值对已存在，无需更新
                }
                _keyToValue.Remove(oldKey);
            }

            // 添加新的映射
            _keyToValue[key] = value;
            _valueToKey[value] = key;
        }

        /// <summary>
        /// 添加键值对（如果键或值已存在则抛出异常）
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <exception cref="ArgumentException">如果键或值已存在时抛出</exception>
        public void Add(TKey key, TValue value)
        {
            if (_keyToValue.ContainsKey(key))
            {
                throw new ArgumentException($"键 '{key}' 已存在", nameof(key));
            }
            if (_valueToKey.ContainsKey(value))
            {
                throw new ArgumentException($"值 '{value}' 已存在", nameof(value));
            }

            _keyToValue[key] = value;
            _valueToKey[value] = key;
        }

        /// <summary>
        /// 通过键获取值
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>对应的值，如果不存在则返回default(TValue)</returns>
        public TValue GetByKey(TKey key)
        {
            return _keyToValue.TryGetValue(key, out var value) ? value : default(TValue);
        }

        /// <summary>
        /// 通过值获取键
        /// </summary>
        /// <param name="value">值</param>
        /// <returns>对应的键，如果不存在则返回default(TKey)</returns>
        public TKey GetByValue(TValue value)
        {
            return _valueToKey.TryGetValue(value, out var key) ? key : default(TKey);
        }

        /// <summary>
        /// 尝试通过键获取值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">输出的值</param>
        /// <returns>是否成功获取到值</returns>
        public bool TryGetValueByKey(TKey key, out TValue value)
        {
            return _keyToValue.TryGetValue(key, out value);
        }

        /// <summary>
        /// 尝试通过值获取键
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="key">输出的键</param>
        /// <returns>是否成功获取到键</returns>
        public bool TryGetKeyByValue(TValue value, out TKey key)
        {
            return _valueToKey.TryGetValue(value, out key);
        }

        /// <summary>
        /// 检查是否包含指定的键
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>是否包含该键</returns>
        public bool ContainsKey(TKey key)
        {
            return _keyToValue.ContainsKey(key);
        }

        /// <summary>
        /// 检查是否包含指定的值
        /// </summary>
        /// <param name="value">值</param>
        /// <returns>是否包含该值</returns>
        public bool ContainsValue(TValue value)
        {
            return _valueToKey.ContainsKey(value);
        }

        /// <summary>
        /// 移除指定键及其关联的值
        /// </summary>
        /// <param name="key">要移除的键</param>
        /// <returns>是否成功移除</returns>
        public bool RemoveByKey(TKey key)
        {
            if (!_keyToValue.TryGetValue(key, out var value))
            {
                return false;
            }

            _keyToValue.Remove(key);
            _valueToKey.Remove(value);
            return true;
        }

        /// <summary>
        /// 移除指定值及其关联的键
        /// </summary>
        /// <param name="value">要移除的值</param>
        /// <returns>是否成功移除</returns>
        public bool RemoveByValue(TValue value)
        {
            if (!_valueToKey.TryGetValue(value, out var key))
            {
                return false;
            }

            _valueToKey.Remove(value);
            _keyToValue.Remove(key);
            return true;
        }

        /// <summary>
        /// 清空所有数据
        /// </summary>
        public void Clear()
        {
            _keyToValue.Clear();
            _valueToKey.Clear();
        }

        /// <summary>
        /// 获取键值对的数量
        /// </summary>
        public int Count => _keyToValue.Count;

        /// <summary>
        /// 通过键访问值（索引器）
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>对应的值</returns>
        public TValue this[TKey key]
        {
            get => GetByKey(key);
            set => AddOrUpdate(key, value);
        }

        /// <summary>
        /// 获取所有键的集合
        /// </summary>
        public IEnumerable<TKey> Keys => _keyToValue.Keys;

        /// <summary>
        /// 获取所有值的集合
        /// </summary>
        public IEnumerable<TValue> Values => _valueToKey.Keys;

        /// <summary>
        /// 获取所有键值对的集合
        /// </summary>
        public IEnumerable<KeyValuePair<TKey, TValue>> Pairs => _keyToValue;

        /// <summary>
        /// 返回键值对的迭代器（默认迭代器）
        /// </summary>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _keyToValue.GetEnumerator();
        }

        /// <summary>
        /// 返回键值对的迭代器（非泛型）
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// 返回键的迭代器
        /// </summary>
        public IEnumerator<TKey> GetKeysEnumerator()
        {
            return _keyToValue.Keys.GetEnumerator();
        }

        /// <summary>
        /// 返回值的迭代器
        /// </summary>
        public IEnumerator<TValue> GetValuesEnumerator()
        {
            return _valueToKey.Keys.GetEnumerator();
        }

        /// <summary>
        /// 返回键值对的迭代器
        /// </summary>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetPairsEnumerator()
        {
            return _keyToValue.GetEnumerator();
        }
    }
}
