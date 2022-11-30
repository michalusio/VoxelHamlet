using System;

namespace Assets.Scripts.Utilities
{
    /// <summary>
    ///     Struct used in heap, working like Key-Value Pair.
    /// </summary>
    /// <typeparam name="T">Type of contained objects</typeparam>
    public readonly struct HeapNode<T>
    {
        /// <summary>
        ///     Object in this node.
        /// </summary>
        public readonly T Object;

        /// <summary>
        ///     Priority of retrieved object.
        /// </summary>
        public readonly float Priority;

        /// <summary>
        ///     Initializes node with object and its priority.
        /// </summary>
        /// <param name="obj">Said object</param>
        /// <param name="prio">Object's priority</param>
        public HeapNode(T obj, float prio)
        {
            Object = obj;
            Priority = prio;
        }

        /// <summary>
        ///     Checks if first node's priority is higher than the second's.
        /// </summary>
        /// <param name="a">First node</param>
        /// <param name="b">Second node</param>
        public static bool operator >(HeapNode<T> a, HeapNode<T> b)
        {
            return a.Priority > b.Priority;
        }

        /// <summary>
        ///     Checks if first node's priority is lower than the second's.
        /// </summary>
        /// <param name="a">First node</param>
        /// <param name="b">Second node</param>
        public static bool operator <(HeapNode<T> a, HeapNode<T> b)
        {
            return a.Priority < b.Priority;
        }

        /// <summary>
        ///     Checks if first node's priority is equal to the second's.
        /// </summary>
        /// <param name="a">First node</param>
        /// <param name="b">Second node</param>
        public static bool operator ==(HeapNode<T> a, HeapNode<T> b)
        {
            if (b != null && a != null)
                return Math.Abs(a.Priority - b.Priority) < 2.80259692864963E-45;
            return false;
        }

        /// <summary>
        ///     Checks if first node's priority is not equal to the second's.
        /// </summary>
        /// <param name="a">First node</param>
        /// <param name="b">Second node</param>
        public static bool operator !=(HeapNode<T> a, HeapNode<T> b)
        {
            return !(a == b);
        }

        /// <summary>
        ///     Checks if two nodes are equal.
        /// </summary>
        /// <param name="obj">Object to check with</param>
        public override bool Equals(object obj)
        {
            if (obj != null)
                return GetHashCode() == obj.GetHashCode();
            return false;
        }

        /// <summary>
        ///     Calculates the hashcode of object.
        /// </summary>
        public override int GetHashCode()
        {
            return Object.GetHashCode() << (8 + Priority.GetHashCode());
        }

        /// <summary>
        ///     Formats node and returns it's string representation.
        /// </summary>
        public override string ToString()
        {
            return "HeapNode(" + Object + "|" + Priority.ToString("0.00") + ")";
        }
    }
}