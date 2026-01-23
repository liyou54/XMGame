using System;

internal readonly struct XBlobArray<T> where T : unmanaged
{
    internal readonly int Offset;
    internal XBlobArray(int offset) => Offset = offset;


    public int GetLength(XBlobContainer container)
    {
        var data = container.GetArrayView<T>(Offset);
        return data.Length;
    }

    public T this[XBlobContainer container, int index]
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

    public Enumerable GetEnumerator(XBlobContainer container)
    {
        return new Enumerable(this, container);
    }

    public EnumerableRef GetEnumeratorRef(XBlobContainer container)
    {
        return new EnumerableRef(this, container);
    }

    public ref struct Enumerator
    {
        private Span<T> _data;
        private int _length;
        private int _index;

        internal Enumerator(XBlobArray<T> array, XBlobContainer container)
        {
            var view = container.GetArrayView<T>(array.Offset);
            _data = view.Data;
            _length = view.Length;
            _index = -1;
        }

        public T Current => _data[_index];

        public bool MoveNext()
        {
            _index++;
            return _index < _length;
        }
    }

    public ref struct Enumerable
    {
        private readonly XBlobArray<T> _array;
        private readonly XBlobContainer _container;

        internal Enumerable(XBlobArray<T> array, XBlobContainer container)
        {
            _array = array;
            _container = container;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(_array, _container);
        }
    }

    public ref struct EnumeratorRef
    {
        private Span<T> _data;
        private int _length;
        private int _index;

        internal EnumeratorRef(XBlobArray<T> array, XBlobContainer container)
        {
            var view = container.GetArrayView<T>(array.Offset);
            _data = view.Data;
            _length = view.Length;
            _index = -1;
        }

        public ref T Current => ref _data[_index];

        public bool MoveNext()
        {
            _index++;
            return _index < _length;
        }
    }

    public ref struct EnumerableRef
    {
        private readonly XBlobArray<T> _array;
        private readonly XBlobContainer _container;

        internal EnumerableRef(XBlobArray<T> array, XBlobContainer container)
        {
            _array = array;
            _container = container;
        }

        public EnumeratorRef GetEnumerator()
        {
            return new EnumeratorRef(_array, _container);
        }
    }
}