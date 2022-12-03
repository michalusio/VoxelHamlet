using Assets.Scripts.ConfigScripts;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.PathFinding
{
    public static class PathingScheduler
    {
        private static readonly Thread PathingThread = new Thread(PathingLoop)
        {
            IsBackground = true
        };

        private static readonly ConcurrentQueue<PathingRequest> Requests = new ConcurrentQueue<PathingRequest>();
        
        static PathingScheduler()
        {
            PathingThread.Start();
        }

        public static PathingRequest RequestPath(Vector3Int start, List<Vector3Int> end, bool laddering)
        {
            var request = new PathingRequest(start, end, laddering);
            Requests.Enqueue(request);
            return request;
        }

        private static void PathingLoop()
        {
            while (true)
            {
                if (Requests.TryDequeue(out var request))
                {
                    var pathfinderMaxIterations = GlobalSettings.Variables["PathFinderMaxIterations"].AsInt();
                    (request.FoundFullPath, request.Path) = BlockPathing.MultiAStar(request.Start, request.End, request.Laddering, pathfinderMaxIterations);
                    request.Closed = BlockPathing.LastClosedSet;
                    request.PathingDone = true;
                }
                else
                {
                    Thread.Sleep(1);
                }
            }
        }
    }
}
