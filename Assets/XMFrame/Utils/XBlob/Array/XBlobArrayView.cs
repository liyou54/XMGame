using System;

internal ref struct XBlobArrayView<T> where T : unmanaged
{
    internal int Length;
    internal Span<T> Data;
}