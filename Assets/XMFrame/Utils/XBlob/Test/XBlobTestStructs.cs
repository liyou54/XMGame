using System;

namespace XM.Utils.Tests
{
    /// <summary>测试用自定义结构体，用于 Ptr / Array 的 unmanaged 元素。</summary>
    public struct TestBlobStruct
    {
        public int A;
        public int B;
    }

    /// <summary>测试用自定义键结构体，用于 Map/Set/MultiMap 的 key，需 IEquatable。</summary>
    public struct TestKey : IEquatable<TestKey>
    {
        public int Id;
        public int Tag;

        public bool Equals(TestKey other) => Id == other.Id;
        public override int GetHashCode() => Id;
    }

    /// <summary>测试用自定义值结构体，用于 Map/MultiMap 的 value。</summary>
    public struct TestValue
    {
        public int X;
        public int Y;
    }
}
