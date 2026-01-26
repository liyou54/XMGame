using System;
using Unity.Burst;

[BurstCompile]
public readonly struct XBlobArray<T> where T : unmanaged
{
    internal readonly int Offset;
    internal XBlobArray(int offset) => Offset = offset;


    [BurstCompile]
    public int GetLength(in XBlobContainer container)
    {
        var data = container.GetArrayView<T>(Offset);
        return data.Length;
    }

    public T this[in XBlobContainer container, int index]
    {
        get
        {
            var data = container.GetArrayView<T>(Offset);
            if (index < 0 || index >= data.Length)
                throw new IndexOutOfRangeException($"Index {index} is out of range [0, {data.Length})");
            return data.Data[index];
        }
        set
        {
            var data = container.GetArrayView<T>(Offset);
            if (index < 0 || index >= data.Length)
                throw new IndexOutOfRangeException($"Index {index} is out of range [0, {data.Length})");
            data.Data[index] = value;
        }
    }

    [BurstCompile]
    public Enumerable GetEnumerator(in XBlobContainer container)
    {
        return new Enumerable(this, container);
    }

    [BurstCompile]
    public EnumerableRef GetEnumeratorRef(in XBlobContainer container)
    {
        return new EnumerableRef(this, container);
    }

    [BurstCompile]
    public ref struct Enumerator
    {
        private Span<T> _data;
        private int _length;
        private int _index;

        internal Enumerator(in XBlobArray<T> array, in XBlobContainer container)
        {
            var view = container.GetArrayView<T>(array.Offset);
            _data = view.Data;
            _length = view.Length;
            _index = -1;
        }

        public T Current => _data[_index];

        [BurstCompile]
        public bool MoveNext()
        {
            _index++;
            return _index < _length;
        }
    }

    [BurstCompile]
    public ref struct Enumerable
    {
        private readonly XBlobArray<T> _array;
        private readonly XBlobContainer _container;

        internal Enumerable(in XBlobArray<T> array, in XBlobContainer container)
        {
            _array = array;
            _container = container;
        }

        [BurstCompile]
        public Enumerator GetEnumerator()
        {
            return new Enumerator(_array, _container);
        }
    }

    [BurstCompile]
    public ref struct EnumeratorRef
    {
        private Span<T> _data;
        private int _length;
        private int _index;

        internal EnumeratorRef(in XBlobArray<T> array, in XBlobContainer container)
        {
            var view = container.GetArrayView<T>(array.Offset);
            _data = view.Data;
            _length = view.Length;
            _index = -1;
        }

        public ref T Current => ref _data[_index];

        [BurstCompile]
        public bool MoveNext()
        {
            _index++;
            return _index < _length;
        }
    }

    [BurstCompile]
    public ref struct EnumerableRef
    {
        private readonly XBlobArray<T> _array;
        private readonly XBlobContainer _container;

        internal EnumerableRef(in XBlobArray<T> array, in XBlobContainer container)
        {
            _array = array;
            _container = container;
        }

        [BurstCompile]
        public EnumeratorRef GetEnumerator()
        {
            return new EnumeratorRef(_array, _container);
        }
    }
}