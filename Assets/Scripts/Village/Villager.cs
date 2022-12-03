using Assets.Scripts.ConfigScripts;
using Assets.Scripts.PathFinding;
using Assets.Scripts.Utilities;
using Assets.Scripts.Utilities.Math;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Village
{
    [RequireComponent(typeof(Animator))]
    public class Villager : MonoBehaviour
    {
        public Transform RightHandTransform;

        public Vector3Int CurrentPosition { get; private set; }
        public int PathingTries { get; private set; }
        public List<Vector3Int> CurrentGoal;
        public VillagerRole Role;

        public bool AllowLaddering = false;
        public bool FreeToWork = true;
        public MoveState State = MoveState.Stable;
        [SerializeField] private bool ShowClosedSet = false;

        public Vector3Int LastPathEnd;

        public float MaxSpeed = 1f;

        private List<Vector3Int> CurrentPath;
        private readonly List<bool> Ladderization = new List<bool>();

        public Animator Animator;

        private HashSet<Vector3Int> lastClosed;
        private readonly List<GameObject> ladders = new List<GameObject>();

        void Start()
        {
            Animator = GetComponent<Animator>();
            transform.position = new Vector3Int(Mathf.FloorToInt(transform.position.x), GlobalSettings.Instance.Map.GetHighestYAt(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.z)) + 1, Mathf.FloorToInt(transform.position.z));
            CurrentPosition = Vector3Int.FloorToInt(transform.position - new Vector3Int(1, 0, 1));
            if (!BlockPathFinding.IsValidForEntity(CurrentPosition))
            {
                transform.position += Vector3.up;
                CurrentPosition += Vector3Int.up;
            }
            CurrentGoal = new List<Vector3Int> { CurrentPosition };
        }

        private Vector3Int FallPosition;
        private bool Updating;
        void Update()
        {
            if (!Updating)
            {
                StartCoroutine(UpdateCoroutine());
            }
        }

        private IEnumerator UpdateCoroutine()
        {
            Updating = true;
            var currentPosition = transform.position - new Vector3Int(1, 0, 1);
            //State changing
            if (State == MoveState.Falling)
            {
                var gravity = GlobalSettings.Variables["Gravity"].AsInt();
                CurrentPath = null;
                transform.position = Vector3.MoveTowards(currentPosition, FallPosition, gravity * Time.deltaTime) + new Vector3Int(1, 0, 1);
            }
            else
            {
                yield return PathFind(currentPosition);
            }
            Updating = false;
        }

        private IEnumerator PathFind(Vector3 currentPosition)
        {
            bool isNotInGoal;
            {
                var min = 999999f;
                foreach (var g in CurrentGoal)
                {
                    min = Mathf.Min(min, (g - currentPosition).sqrMagnitude);
                }
                isNotInGoal = min > 0;
            }
            if (!isNotInGoal)
            {
                DestroyLadders();
            }

            Animator.SetFloat("WalkBlend", (CurrentPath != null) ? 1 : 0);

            if (CurrentPath != null)
            {
                if (CurrentPath.Count == 0)
                {
                    yield return TryContinuePath(isNotInGoal);
                }
                else
                {
                    var nextWaypoint = CurrentPath[CurrentPath.Count - 1];
                    var shouldLadder = Ladderization[Ladderization.Count - 1];
                    if (shouldLadder)
                    {
                        yield return CreateOneLadder(nextWaypoint);
                        State = MoveState.Laddering;
                    }
                    var moveSpeed = (Animator.GetBool("HasItem") ? 0.5f : 1) * MaxSpeed;

                    Vector3 newForward = GetNewForwardVector(currentPosition, nextWaypoint);
                    if (newForward.sqrMagnitude > 0.5f)
                    {
                        yield return MoveLoop(newForward, nextWaypoint, moveSpeed);
                    }

                    if (State == MoveState.Laddering)
                    {
                        if (BlockPathFinding.HasFloor(nextWaypoint))
                        {
                            State = MoveState.Stable;
                        }
                    }
                    else if (CurrentPosition.x == nextWaypoint.x && CurrentPosition.z == nextWaypoint.z && CurrentPosition.y != nextWaypoint.y)
                    {
                        State = MoveState.Laddering;
                    }

                    CurrentPosition = nextWaypoint;
                    transform.position = nextWaypoint + new Vector3Int(1, 0, 1);
                    CurrentPath.RemoveAt(CurrentPath.Count - 1);
                    Ladderization.RemoveAt(Ladderization.Count - 1);
                }
            }
            else if (isNotInGoal)
            {
                yield return FindNewPath();
            }
        }

        private Vector3 GetNewForwardVector(Vector3 currentPosition, Vector3Int nextWaypoint)
        {
            if (State == MoveState.Laddering)
            {
                for (int i = CurrentPath.Count - 1; i >= 0; i--)
                {
                    var forwardWaypoint = CurrentPath[i];
                    if (!(CurrentPosition.x == forwardWaypoint.x && CurrentPosition.z == forwardWaypoint.z && CurrentPosition.y != forwardWaypoint.y))
                    {
                        return (forwardWaypoint - currentPosition)._x0z().normalized;
                    }
                }
            }

            return (nextWaypoint - currentPosition)._x0z().normalized;
        }

        private IEnumerator MoveLoop(Vector3 newForward, Vector3Int nextWaypoint, float moveSpeed)
        {
            var actualMoveTime = 0f;
            var moveTime = 1f / moveSpeed;
            var currentPosition = transform.position - new Vector3Int(1, 0, 1);
            while (actualMoveTime <= moveTime)
            {
                var newPosition = Vector3.MoveTowards(currentPosition, nextWaypoint, moveSpeed * Time.deltaTime);

                var nextForward = Vector3.RotateTowards(transform.forward, newForward, MaxSpeed * Time.deltaTime, 0.1f * Time.deltaTime);
                transform.forward = nextForward;
                if (Vector3.Dot(nextForward, newForward) >= 0.5f)
                {
                    currentPosition = newPosition;
                    transform.position = newPosition + new Vector3Int(1, 0, 1);
                    actualMoveTime += Time.deltaTime;
                }
                yield return null;
            }
        }

        private IEnumerator TryContinuePath(bool isNotInGoal)
        {
            CurrentPath = null;
            DestroyLadders();
            LastPathEnd = CurrentPosition;
            if (isNotInGoal)
            {
                var request = PathingScheduler.RequestPath(LastPathEnd, CurrentGoal, AllowLaddering);
                while (!request.PathingDone)
                {
                    Animator.SetFloat("WalkBlend", 0);
                    yield return null;
                }
                CurrentPath = request.Path;
                TestLadders();
                lastClosed = request.Closed;
                if (request.FoundFullPath == PathingResult.PathNotFound)
                {
                    PathingTries += 100;
                    CurrentPath = null;
                    CurrentGoal = new List<Vector3Int> { CurrentPosition };
                }
                else
                {
                    PathingTries++;
                }
            }
        }

        private IEnumerator FindNewPath()
        {
            LastPathEnd = CurrentPosition;

            PathingRequest request = PathingScheduler.RequestPath(LastPathEnd, CurrentGoal, AllowLaddering);
            while (!request.PathingDone)
            {
                Animator.SetFloat("WalkBlend", 0);
                yield return null;
            }
            CurrentPath = request.Path;
            TestLadders();
            lastClosed = request.Closed;

            if (request.FoundFullPath == PathingResult.PathNotFound)
            {
                PathingTries = 100;
                CurrentPath = null;
                CurrentGoal = new List<Vector3Int> { CurrentPosition };
            }
            else
            {
                PathingTries = 1;
            }
        }

        private void TestLadders()
        {
            if (CurrentPath == null) return;
            Ladderization.Clear();
            if (CurrentPath.Count == 0) return;
            var previousNode = CurrentPath[0];
            Ladderization.Add(false);
            for (int i = 1; i < CurrentPath.Count; i++)
            {
                var node = CurrentPath[i];
                var ladder = (previousNode.x == node.x && previousNode.z == node.z && previousNode.y != node.y);
                if (ladder) Ladderization[Ladderization.Count - 1] = true;
                Ladderization.Add(ladder);
                previousNode = node;
            }
        }

        private IEnumerator CreateOneLadder(Vector3Int pos)
        {
            Animator.Play("Base Layer.MineStart");
            var timeStart = Time.time;
            const float animationTime = 0.5f;
            bool ladderPlaced = false;
            while ((Time.time - timeStart) < animationTime)
            {
                if (!ladderPlaced && (Time.time - timeStart) > animationTime * 0.5f)
                {
                    ladderPlaced = true;
                    ladders.Add(Instantiate(GlobalSettings.Instance.LadderPrefab, new Vector3(pos.x + 1, pos.y + 0.5f, pos.z + 1), Quaternion.identity));
                }
                yield return null;
            }
        }

        private void DestroyLadders()
        {
            float i = 0.25f;
            for (int index = ladders.Count - 1; index >= 0; index--)
            {
                GameObject ladder = ladders[index];
                Destroy(ladder, i);
                i += 0.25f;
            }
            ladders.Clear();
        }

        public void SetGoal(List<Vector3Int> goalList)
        {
            CurrentGoal = goalList;
            PathingTries = 0;
        }
        
        void OnDrawGizmosSelected()
        {
            var halfVector = new Vector3(0.5f, 0.5f, 0.5f);
            var gizmoColor = Gizmos.color;

            if ((CurrentPath?.Count ?? 0) > 1)
            {
                Gizmos.color = new Color(1, 1, 1, 0.5f);

                Vector3Int previousNode = CurrentPath[0];
                for (int i = 1; i < CurrentPath.Count; i++)
                {
                    var node = CurrentPath[i];

                    Gizmos.DrawLine(previousNode + new Vector3(1, 0.5f, 1), node + new Vector3(1, 0.5f, 1));

                    previousNode = node;
                }
            }

            Gizmos.color = Color.blue;
            foreach (var goal in CurrentGoal)
            {
                Gizmos.DrawCube(goal + new Vector3(1, 0.5f, 1), halfVector);
            }

            if (ShowClosedSet)
            {
                Gizmos.color = Color.black;
                if (lastClosed != null)
                {
                    foreach (var v in lastClosed)
                    {
                        Gizmos.DrawCube(v + new Vector3(1, 0.5f, 1), halfVector);
                    }
                }
            }
            Gizmos.color = gizmoColor;
        }
    }

    public enum MoveState
    {
        Stable,
        Falling,
        Laddering
    }
}
