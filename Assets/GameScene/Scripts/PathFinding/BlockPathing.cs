using Assets.Scripts.Utilities;
using Assets.Scripts.Utilities.Math;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.PathFinding
{
    public static class BlockPathing
    {
        private static double _at;
        private static double _dt;

        /// <summary>
        ///     Time in milliseconds using A* algorithm.
        /// </summary>
        public static double AStarFindingTime => _at;

        /// <summary>
        ///     Time in milliseconds using Dijkstra's algorithm.
        /// </summary>
        public static double DijkstraFindingTime => _dt;

        /// <summary>
        ///     Resets algorithm times.
        /// </summary>
        public static void ResetTimes()
        {
            _at = 0;
            _dt = 0;
        }

        public static HashSet<Vector3Int> LastClosedSet;

        private static List<Vector3Int> ToList(Vector3Int current, IDictionary<Vector3Int, Vector3Int> cameFrom)
        {
            Vector3Int defVec = default;
            defVec.x = -1;
            defVec.y = -1;
            defVec.z = -1;

            var objList = new List<Vector3Int>();
            while (current.x >= 0)
            {
                objList.Add(current);
                current = cameFrom.GetValueOrDefault(current, defVec);
            }
            return objList;
        }

        /// <summary>
        ///     Thread-safely adds a value to a double variable.
        /// </summary>
        /// <param name="variable">Variable to add to</param>
        /// <param name="value">Value to add</param>
        private static void Add(ref double variable, double value)
        {
            double newCurrentValue = 0;
            while (true)
            {
                var currentValue = newCurrentValue;
                var newValue = currentValue + value;
                newCurrentValue = Interlocked.CompareExchange(ref variable, newValue, currentValue);
                if (Math.Abs(newCurrentValue - currentValue) < 0.000001)
                    return;
            }
        }

        /// <summary>
        ///     Performs path search using Dijkstra's algorithm for a given predicate.
        ///     <para>Returns list of nodes from ending to starting node.</para>
        ///     <para>Returns null if there is no path to goal.</para>
        /// </summary>
        /// <param name="start">Starting node</param>
        /// <param name="targeting">Predicate distinguishing end nodes from the rest</param>
        /// <param name="limiter">Maximum length of path to search for</param>
        public static List<Vector3Int> Dijkstra(Vector3Int start, Predicate<Vector3Int> targeting, bool laddering, int limiter = int.MaxValue)
        {
            Stopwatch s = Stopwatch.StartNew();
            var open = new Heap<Vector3Int> { MinHeap = true };
            var closed = new HashSet<Vector3Int>();
            var gs = new Dictionary<Vector3Int, float>();
            var cameFrom = new Dictionary<Vector3Int, Vector3Int>();
            var neighbors = new List<Vector3Int>(25);
            open.Add(start, 0.0f);
            while (open.Count > 0)
            {
                var current = open.PopFirst();
                if (targeting.Invoke(current))
                {
                    var t = ToList(current, cameFrom);
                    s.Stop();
                    LastClosedSet = closed;
                    Add(ref _dt, s.Elapsed.TotalMilliseconds);
                    return t;
                }
                closed.Add(current);
                var gsC = gs.GetValueOrDefault(current, 0f);
                if (gsC > limiter) break;
                neighbors.Clear();
                BlockPathFinding.GetNeighbours(current, neighbors, laddering);
                for (int i = 0; i < neighbors.Count; i++)
                {
                    Vector3Int neighbor = neighbors[i];
                    var num = gsC + BlockPathFinding.Distance(current, neighbor);
                    if ((closed.Contains(neighbor) ||
                            open.Contains(neighbor)) && num >= gs.GetValueOrDefault(neighbor, 0f)) continue;
                    cameFrom[neighbor] = current;
                    gs[neighbor] = num;
                    if (!open.Contains(neighbor)) open.Add(neighbor, gs[neighbor]);
                }
            }
            s.Stop();
            LastClosedSet = closed;
            Add(ref _dt, s.Elapsed.TotalMilliseconds);
            return null;
        }

        /// <summary>
        ///     Performs path search using A* algorithm for a given start and goal.
        ///     <para>Returns list of nodes from ending to starting node.</para>
        ///     <para>Returns null if there is no path to goal.</para>
        /// </summary>
        /// <param name="start">Starting node</param>
        /// <param name="goal">Ending node</param>
        /// <param name="limiter">Maximum length of path to search for</param>
        public static (PathingResult found, List<Vector3Int> Path) AStar(Vector3Int start, Vector3Int goal, bool laddering, int limiter = int.MaxValue)
        {
            Stopwatch s = Stopwatch.StartNew();
            var open = new Heap<Vector3Int>(100) { MinHeap = true };
            var closed = new HashSet<Vector3Int>();
            var gs = new Dictionary<Vector3Int, float>(100);
            var fs = new Dictionary<Vector3Int, float>(100);
            var cameFrom = new Dictionary<Vector3Int, Vector3Int>(100);
            open.Add(start, 0.0f);
            gs[start] = 0;
            fs[start] = BlockPathFinding.Heuristic(start, goal);
            var neighbors = new List<Vector3Int>(25);
            while (open.Count > 0)
            {
                var current = open.PopFirst();
                if (BlockPathFinding.IsValidForEntity(current) && BlockPathFinding.NodeEqual(current, goal))
                {
                    var t = ToList(current, cameFrom);
                    s.Stop();
                    LastClosedSet = closed;
                    Add(ref _at, s.Elapsed.TotalMilliseconds);
                    return (PathingResult.PathFound_Full, t);
                }
                closed.Add(current);
                var gsC = gs.GetValueOrDefault(current, 0f);
                if (closed.Count > limiter) break;
                neighbors.Clear();
                BlockPathFinding.GetNeighbours(current, neighbors, laddering);
                for (int i = 0; i < neighbors.Count; i++)
                {
                    Vector3Int neighbor = neighbors[i];
                    var num = gsC + BlockPathFinding.Distance(current, neighbor);
                    if ((closed.Contains(neighbor) || open.Contains(neighbor)) && num >= gs.GetValueOrDefault(neighbor, 0f)) continue;
                    cameFrom[neighbor] = current;
                    gs[neighbor] = num;
                    var fsn = num + BlockPathFinding.Heuristic(neighbor, goal);
                    fs[neighbor] = fsn;
                    if (!open.Contains(neighbor))
                    {
                        open.Add(neighbor, fsn);
                    }
                }
            }
            s.Stop();
            Add(ref _at, s.Elapsed.TotalMilliseconds);
            LastClosedSet = closed;
            var fsSorted = fs.Where(x => BlockPathFinding.IsValidForEntity(x.Key)).OrderBy(x => (int)(BlockPathFinding.Heuristic(x.Key, goal) * 10));
            return (closed.Count > limiter ? PathingResult.PathFound_NotFull : PathingResult.PathNotFound, fsSorted.Count() == 0 ? new List<Vector3Int>() : ToList(fsSorted.First().Key, cameFrom));
        }

        /// <summary>
        ///     Performs path search using A* algorithm for a given start and list of goals.
        ///     <para>Returns list of nodes from best ending node to starting node.</para>
        ///     <para>Returns null if there is no path to any of the goals.</para>
        /// </summary>
        /// <param name="start">Starting node</param>
        /// <param name="goals">List of ending nodes</param>
        /// <param name="limiter">Maximum length of path to search for</param>
        public static (PathingResult found, List<Vector3Int> Path) MultiAStar(Vector3Int start, List<Vector3Int> goals, bool laddering, int limiter = int.MaxValue)
        {
            Stopwatch s = Stopwatch.StartNew();
            var open = new Heap<Vector3Int> { MinHeap = true };
            var closed = new HashSet<Vector3Int>();
            var gs = new Dictionary<Vector3Int, float>();
            var fs = new Dictionary<Vector3Int, float>();
            var cameFrom = new Dictionary<Vector3Int, Vector3Int>();
            open.Add(start, 0.0f);
            gs[start] = 0;
            fs[start] = goals.Min(t => BlockPathFinding.Heuristic(start, t));
            var neighbors = new List<Vector3Int>(25);
            while (open.Count > 0)
            {
                var current = open.PopFirst();
                if (BlockPathFinding.IsValidForEntity(current) && goals.Exists(t => BlockPathFinding.NodeEqual(current, t)))
                {
                    var t = ToList(current, cameFrom);
                    s.Stop();
                    LastClosedSet = closed;
                    Add(ref _at, s.Elapsed.TotalMilliseconds);
                    return (PathingResult.PathFound_Full, t);
                }
                closed.Add(current);
                var gsC = gs.GetValueOrDefault(current, 0f);
                if (closed.Count > limiter) break;
                neighbors.Clear();
                BlockPathFinding.GetNeighbours(current, neighbors, laddering);
                for (int i = 0; i < neighbors.Count; i++)
                {
                    Vector3Int neighbor = neighbors[i];
                    var num = gsC + BlockPathFinding.Distance(current, neighbor);
                    if ((closed.Contains(neighbor) ||
                         open.Contains(neighbor)) && num >= gs.GetValueOrDefault(neighbor, 0f)) continue;
                    cameFrom[neighbor] = current;
                    gs[neighbor] = num;
                    fs[neighbor] = gs[neighbor] + goals.Min(t => BlockPathFinding.Heuristic(neighbor, t));
                    if (!open.Contains(neighbor))
                        open.Add(neighbor, fs[neighbor]);
                }
            }
            s.Stop();
            LastClosedSet = closed;
            Add(ref _at, s.Elapsed.TotalMilliseconds);
            var fsSorted = fs.Where(x => BlockPathFinding.IsValidForEntity(x.Key)).OrderBy(x => (int)(goals.Min(t => BlockPathFinding.Heuristic(x.Key, t)) * 10));
            return (closed.Count > limiter ? PathingResult.PathFound_NotFull : PathingResult.PathNotFound, fsSorted.Count() == 0 ? new List<Vector3Int>() : ToList(fsSorted.First().Key, cameFrom));
        }

    }

    public enum PathingResult
    {
        PathFound_NotFull,
        PathFound_Full,
        PathNotFound
    }
}
