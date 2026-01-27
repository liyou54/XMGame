using System;

/// <summary>哈希表 Entry 头（8 字节），仅含 HashCode 与 Next，与键/值类型无关。</summary>
internal struct XBlobHashEntry
{
    public int HashCode;
    public int Next;
}

/// <summary>
/// 哈希表公共逻辑：桶下标计算、索引边界校验、满容异常文案。
/// Map/Set/MultiMap 共用，减少重复代码。
/// </summary>
internal static class XBlobHashCommon
{
    /// <summary>将 hashCode 映射到 [0, bucketCount) 的桶下标，兼容 C# 负数取模。</summary>
    public static int BucketIndex(int hashCode, int bucketCount)
    {
        int bi = hashCode % bucketCount;
        if (bi < 0) bi += bucketCount;
        return bi;
    }

    /// <summary>校验 index 是否在 [0, count)，否则抛出 ArgumentOutOfRangeException。</summary>
    public static void ThrowIfIndexOutOfRange(int index, int count, string paramName = "index")
    {
        if (index < 0 || index >= count)
            throw new ArgumentOutOfRangeException(paramName, index, $"Index must be in [0, {count}).");
    }

    /// <summary>满容时抛出 InvalidOperationException 的文案。</summary>
    public const string FullMessage = "Container is full, cannot add more elements.";
}
