using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.PathFinding
{
    public class PathingRequest
    {
        public Vector3Int Start { get; }
        public List<Vector3Int> End { get; }

        public List<Vector3Int> Path;
        public PathingResult FoundFullPath;

        public bool Laddering { get; }

        public bool PathingDone;

        public HashSet<Vector3Int> Closed;

        public PathingRequest(Vector3Int start, List<Vector3Int> end, bool laddering)
        {
            Start = start;
            End = new List<Vector3Int>(end);
            Laddering = laddering;
        }
    }
}
