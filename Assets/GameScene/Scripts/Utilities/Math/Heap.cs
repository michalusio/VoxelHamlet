using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Assets.Scripts.Utilities.Math
{
    /// <summary>
    ///     Class utilizing the min-max heap data structure.
    /// </summary>
    public class Heap<T>
    {
        private readonly HashSet<T> _containing = new HashSet<T>();
        private int _c = 1;
        private HeapNode<T>[] _nodes;

        /// <summary>
        ///     Set if heap is min-heap or max-heap.
        /// </summary>
        public bool MinHeap;

        /// <summary>
        ///     Initializes heap with initial capacity of 10 elements.
        /// </summary>
        public Heap()
        {
            _nodes = new HeapNode<T>[11];
        }

        /// <summary>
        ///     Initializes heap with given initial capacity clamped to 10+ elements.
        /// </summary>
        /// <param name="capacity">Initial capacity</param>
        public Heap(int capacity)
        {
            _nodes = new HeapNode<T>[capacity > 10 ? capacity : 11];
        }

        /// <summary>
        ///     Retrieve how many items are in the heap.
        /// </summary>
        public int Count => _c - 1;

        /// <summary>
        ///     Get/Set maximum capacity of base array.
        /// </summary>
        public int Capacity
        {
            get { return _nodes.Length - 1; }
            set
            {
                if (value < Count)
                    throw new IndexOutOfRangeException("Data loss while resizing heap!");
                var heapNodeArray = new HeapNode<T>[value + 1];
                _nodes.CopyTo(heapNodeArray, 0);
                _nodes = heapNodeArray;
            }
        }

        /// <summary>
        ///     Retrieves heap node like in an array.
        /// </summary>
        /// <param name="i">Index of node</param>
        public HeapNode<T> this[int i]
        {
            get { return _nodes[i + 1]; }
            set { _nodes[i + 1] = value; }
        }

        /// <summary>
        ///     Add object to heap and situate it in its place.
        /// </summary>
        /// <param name="Object">Added object</param>
        /// <param name="priority">Priority of said object</param>
        public void Add(T Object, float priority)
        {
            if (_c - 1 == Capacity)
            {
                var heapNodeArray = new HeapNode<T>[_nodes.Length << 2];
                _nodes.CopyTo(heapNodeArray, 0);
                _nodes = heapNodeArray;
            }
            var i = _c;
            _nodes[i] = new HeapNode<T>(Object, priority);
            TravelUp(i);
            _containing.Add(Object);
            _c += 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TravelUp(int i)
        {
            while (i > 1)
            {
                var me = _nodes[i];
                var parent = _nodes[i / 2];
                if (!Comp(me, parent))
                    break;
                var heapNode = me;
                _nodes[i] = parent;
                _nodes[i / 2] = heapNode;
                i /= 2;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool Comp(HeapNode<T> me, HeapNode<T> parent) => MinHeap ? me < parent : me > parent;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TravelDown(int i)
        {
            int index;
            for (; i << 1 < _c; i = index)
            {
                index = i << 1;
                if (index < _c - 1 && Comp(_nodes[index + 1], _nodes[index]))
                    ++index;
                if (!Comp(_nodes[index], _nodes[i]))
                    break;
                var heapNode = _nodes[i];
                _nodes[i] = _nodes[index];
                _nodes[index] = heapNode;
            }
        }

        /// <summary>
        ///     Retrieves first object in a heap.
        ///     If heap is min-heap, then the object has minimum priority.
        ///     If heap is max-heap, then the object has maximum priority.
        /// </summary>
        public T PopFirst()
        {
            var heapNodeObject = _nodes[1].Object;
            if (heapNodeObject.Equals(default(T))) return heapNodeObject;
            _containing.Remove(heapNodeObject);
            _nodes[1] = _nodes[_c - 1];
            _nodes[_c - 1] = default;
            if (_c > 1) _c -= 1;
            TravelDown(1);
            return heapNodeObject;
        }

        /// <summary>
        ///     Checks if object is contained in heap.
        ///     Uses HashSet for quick checking.
        /// </summary>
        /// <param name="neighbor">Object to check</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T neighbor) => _containing.Contains(neighbor);

        /// <summary>
        ///     Clears the heap.
        /// </summary>
        public void Clear()
        {
            _containing.Clear();
            for (var index = 0; index < _nodes.Length; ++index)
                _nodes[index] = default;
            _c = 1;
        }
    }
}