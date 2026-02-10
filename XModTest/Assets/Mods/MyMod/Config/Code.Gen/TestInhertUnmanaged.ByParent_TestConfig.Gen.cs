using System;
using XM;
using XM.Contracts.Config;
using Unity.Collections;

namespace XM.Editor.Gen
{
    // TestInhertUnmanaged 的部分类 - ByParent_TestConfig索引
    public partial struct TestInhertUnmanaged
    {
        /// <summary>
        /// ByParent_TestConfig 索引结构体
        /// </summary>
        public struct ByParent_TestConfigIndex : IConfigIndexGroup<global::XM.Editor.Gen.TestInhertUnmanaged>, IEquatable<ByParent_TestConfigIndex>
        {
            private static IndexType? indexType;

            /// <summary>
            /// 索引类型标识（实现 IConfigIndexGroup 接口要求）
            /// </summary>
            public static IndexType IndexType
            {
                get
                {
                    if (indexType == null)
                    {
                        indexType = new IndexType
                        {
                            Tbl = TestInhertClassHelper.TblI,
                            Index = 0
                        };
                    }

                    return indexType.Value;
                }
            }

            // 索引字段
            /// <summary>索引字段: Link</summary>
            public CfgI<TestConfigUnmanaged> Link;

            /// <summary>
            /// 构造方法
            /// </summary>
            /// <param name="link">Link值</param>
            public ByParent_TestConfigIndex(CfgI<TestConfigUnmanaged> link)
            {
                this.Link = link;
            }

            /// <summary>
            /// 判断索引是否相等
            /// </summary>
            public bool Equals(ByParent_TestConfigIndex other)
            {
                return this.Link == other.Link;
            }

            /// <summary>
            /// 获取哈希码
            /// </summary>
            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 31 + Link.GetHashCode();
                    return hash;
                }
            }

        }
    }

    /// <summary>
    /// ByParent_TestConfig 索引扩展方法
    /// </summary>
    public static class TestInhertUnmanaged_ByParent_TestConfigIndexExtensions
    {
        /// <summary>
        /// 获取索引对应的配置引用列表(多值索引)
        /// </summary>
        /// <param name="index">索引值</param>
        /// <param name="data">配置数据容器</param>
        /// <param name="allocator">内存分配器</param>
        /// <param name="returns">配置引用 CfgI 数组</param>
        public static NativeArray<CfgI<global::XM.Editor.Gen.TestInhertUnmanaged>> GetVals(this TestInhertUnmanaged.ByParent_TestConfigIndex index, in XM.ConfigData data, Allocator allocator)
        {
            // 从 ConfigData 获取多值索引容器
            var indexMultiMap = data.GetMultiIndex<TestInhertUnmanaged.ByParent_TestConfigIndex, global::XM.Editor.Gen.TestInhertUnmanaged>(TestInhertUnmanaged.ByParent_TestConfigIndex.IndexType);
            if (!indexMultiMap.Valid)
            {
                return new NativeArray<CfgI<global::XM.Editor.Gen.TestInhertUnmanaged>>(0, allocator);
            }

            // 查询索引获取数量
            var count = indexMultiMap.GetValueCount(data.BlobContainer, index);
            if (count == 0)
            {
                return new NativeArray<CfgI<global::XM.Editor.Gen.TestInhertUnmanaged>>(0, allocator);
            }

            // 遍历并转换为泛型 CfgI 数组
            var results = new NativeArray<CfgI<global::XM.Editor.Gen.TestInhertUnmanaged>>(count, allocator);
            var i = 0;
            foreach (var cfgI in indexMultiMap.GetValuesPerKeyEnumerator(data.BlobContainer, index))
            {
                results[i++] = cfgI.As<global::XM.Editor.Gen.TestInhertUnmanaged>();
            }

            return results;
        }
    }
}
