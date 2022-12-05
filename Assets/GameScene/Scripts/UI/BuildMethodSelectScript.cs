using Assets.Scripts.ConfigScripts;
using Assets.Scripts.Entities;
using Assets.Scripts.Utilities;
using Assets.Scripts.Utilities.Math;
using Assets.Scripts.WorldGen;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    [RequireComponent(typeof(ToggleGroup))]
    public class BuildMethodSelectScript : MonoBehaviour
    {
        private ToggleGroup group;

        public Material SelectionMaterial;
        public Mesh SelectionMesh;
        public GameObject RoofHandlingObject;

        private bool RenderSelection;

        private List<BlockSnapshot> snapshots;

        private readonly List<AreaBlockHandle> basicHandles = new List<AreaBlockHandle>();
        private readonly List<BlockHandle> toolHandles = new List<BlockHandle>();

        private const int WallHeight = 5;

        void Start()
        {
            group = GetComponent<ToggleGroup>();
            snapshots = new List<BlockSnapshot>();
        }

        private Vector3Int StartPos, EndPos;
        private (Block, Entity) SelectedBlock;
        private bool HandlingInput;

        private BlockMethod GetPlaceMethod()
        {
            var methodName = group.ActiveToggles().First().name.Split(' ')[1];
            return (BlockMethod)Enum.Parse(typeof(BlockMethod), methodName);
        }

        private enum BlockMethod
        {
            Block,
            Line,
            Plane,
            Cube,
            Wall,
            Frame,
            Room,
            Roof,
            Stair
        }

        private BlockHandle.HandleType RoofHandling = BlockHandle.HandleType.All;
        private float RoofHeightMultiplier = 1;

        private bool StairFull;
        private int StairWidth = 1;

        void LateUpdate()
        {
            foreach (var handle in basicHandles)
            {
                handle.Draw();
            }
            foreach (var handle in toolHandles)
            {
                handle.Draw();
            }
        }

        public void ClearSnapshots()
        {
            snapshots?.Clear();
        }

        public void MakeSnapshot()
        {
            snapshots.Add(new BlockSnapshot(GlobalSettings.Instance.EditMode.EditMap, GlobalSettings.Instance.EditMode.EditSize));
        }

        public void Undo(bool delete = true)
        {
            if (snapshots.Count > 0)
            {
                var lastSnapshot = snapshots[snapshots.Count - 1];
                if (delete)
                {
                    snapshots.RemoveAt(snapshots.Count - 1);
                    toolHandles.Clear();
                }
                var editMap = GlobalSettings.Instance.EditMode.EditMap;
                lastSnapshot.Load(editMap);
            }
        }

        public void ChangeStairFull(bool hollow)
        {
            StairFull = !hollow;
            Undo(false);
            BlockPlaceFunctions.PlaceStair(GlobalSettings.Instance.EditMode.EditMap, SelectedBlock.Item1, StartPos, EndPos, StairFull, StairWidth);
        }

        public void ChangeStairWidth(float width)
        {
            StairWidth = (int) width;
            Undo(false);
            BlockPlaceFunctions.PlaceStair(GlobalSettings.Instance.EditMode.EditMap, SelectedBlock.Item1, StartPos, EndPos, StairFull, StairWidth);
        }

        public void ChangeRoofHeight(float height)
        {
            RoofHeightMultiplier = height;
            Undo(false);
            BlockPlaceFunctions.PlaceRoof(GlobalSettings.Instance.EditMode.EditMap, SelectedBlock.Item1, StartPos, EndPos, RoofHandling, RoofHeightMultiplier);
        }

        public void ChangeXRoofHandling(bool value)
        {
            RoofHandling &= BlockHandle.HandleType.All ^ BlockHandle.HandleType.X;
            if (value) RoofHandling |= BlockHandle.HandleType.X;
            Undo(false);
            BlockPlaceFunctions.PlaceRoof(GlobalSettings.Instance.EditMode.EditMap, SelectedBlock.Item1, StartPos, EndPos, RoofHandling, RoofHeightMultiplier);
        }

        public void ChangeNegXRoofHandling(bool value)
        {
            RoofHandling &= BlockHandle.HandleType.All ^ BlockHandle.HandleType.NegX;
            if (value) RoofHandling |= BlockHandle.HandleType.NegX;
            Undo(false);
            BlockPlaceFunctions.PlaceRoof(GlobalSettings.Instance.EditMode.EditMap, SelectedBlock.Item1, StartPos, EndPos, RoofHandling, RoofHeightMultiplier);
        }

        public void ChangeZRoofHandling(bool value)
        {
            RoofHandling &= BlockHandle.HandleType.All ^ BlockHandle.HandleType.Z;
            if (value) RoofHandling |= BlockHandle.HandleType.Z;
            Undo(false);
            BlockPlaceFunctions.PlaceRoof(GlobalSettings.Instance.EditMode.EditMap, SelectedBlock.Item1, StartPos, EndPos, RoofHandling, RoofHeightMultiplier);
        }

        public void ChangeNegZRoofHandling(bool value)
        {
            RoofHandling &= BlockHandle.HandleType.All ^ BlockHandle.HandleType.NegZ;
            if (value) RoofHandling |= BlockHandle.HandleType.NegZ;
            Undo(false);
            BlockPlaceFunctions.PlaceRoof(GlobalSettings.Instance.EditMode.EditMap, SelectedBlock.Item1, StartPos, EndPos, RoofHandling, RoofHeightMultiplier);
        }

        public void HandleInput((Block, Entity)? selectedBlockOption)
        {
            if (Input.GetKeyDown(KeyCode.Z)) Undo();
            var ray = GlobalSettings.Instance.EditMode.MainCamera.ScreenPointToRay(Input.mousePosition);

            bool mouseDown = Input.GetMouseButtonDown(0);
            bool mouseUp = Input.GetMouseButtonUp(0);
            if (HandleHandles(ray)) return;

            RenderSelection = false;
            var editMap = GlobalSettings.Instance.EditMode.EditMap;

            if (!selectedBlockOption.HasValue) return;
            SelectedBlock = selectedBlockOption.Value;

            ray.origin -= GlobalSettings.Instance.EditMode.transform.position;
            (var b, var e) = SelectedBlock;
            if (!RaycastUtils.RaycastOOB(editMap, ray, 100, out var hitInfo)) return;
            var pos = b.BlockType == BlockType.Air ? hitInfo.Position : Vector3Int.RoundToInt(hitInfo.Position + hitInfo.Normal);
            if (!editMap.IsInBounds(pos)) return;
            if (mouseDown)
            {
                StartPos = pos;
                HandlingInput = true;
            }
            if (Input.GetMouseButton(0) && HandlingInput)
            {
                EndPos = pos;
                RenderSelection = true;
                SetRenderPositions();
            }
            if (mouseUp && HandlingInput)
            {
                RenderSelection = false;
                MakeSnapshot();
                toolHandles.Clear();
                switch (GetPlaceMethod())
                {
                    case BlockMethod.Block:
                        BlockPlaceFunctions.SetBlock(editMap, EndPos, b);
                        break;
                    case BlockMethod.Line:
                        PlaceHandles(PlaceLineHandles, editMap, b);
                        BlockPlaceFunctions.PlaceLine(editMap, b, StartPos, EndPos);
                        break;
                    case BlockMethod.Plane:
                        BlockPlaceFunctions.PlacePlane(editMap, b, StartPos, EndPos);
                        break;
                    case BlockMethod.Cube:
                        BlockPlaceFunctions.PlaceCube(editMap, b, StartPos, EndPos);
                        break;
                    case BlockMethod.Wall:
                        PlaceHandles(PlaceWallHandles, editMap, b);
                        BlockPlaceFunctions.PlaceWall(editMap, b, StartPos, EndPos, WallHeight);
                        break;
                    case BlockMethod.Room:
                        PlaceHandles(PlaceRoomHandles, editMap, b);
                        BlockPlaceFunctions.PlaceRoom(editMap, b, StartPos, EndPos, WallHeight);
                        break;
                    case BlockMethod.Frame:
                        BlockPlaceFunctions.PlaceFrame(editMap, b, StartPos, EndPos);
                        break;
                    case BlockMethod.Roof:
                        PlaceHandles(PlaceRoofHandles, editMap, b);
                        BlockPlaceFunctions.PlaceRoof(editMap, b, StartPos, EndPos, RoofHandling, RoofHeightMultiplier);
                        break;
                    case BlockMethod.Stair:
                        PlaceHandles(PlaceStairHandles, editMap, b);
                        BlockPlaceFunctions.PlaceStair(editMap, b, StartPos, EndPos, StairFull, StairWidth);
                        break;
                }
                HandlingInput = false;
            }
        }

        private bool HandleHandles(Ray ray)
        {
            bool mouseDown = Input.GetMouseButtonDown(0);
            bool mouseUp = Input.GetMouseButtonUp(0);
            foreach (var handle in basicHandles)
            {
                if (handle.Grabbed)
                {
                    handle.Move(ray);
                    if (mouseUp)
                    {
                        handle.Confirm();
                    }
                    return true;
                }
                if (mouseDown && handle.HandleGrab(ray))
                {
                    return true;
                }
            }
            foreach (var handle in toolHandles)
            {
                if (handle.Grabbed)
                {
                    handle.Move(ray);
                    if (mouseUp)
                    {
                        handle.Confirm();
                    }
                    return true;
                }
                if (mouseDown && handle.HandleGrab(ray))
                {
                    return true;
                }
            }
            return false;
        }

        private void PlaceHandles(Action<CubeMap> handlePlacer, CubeMap editMap, Block block)
        {
            if (block.BlockType == BlockType.Air) return;
            handlePlacer(editMap);
        }

        private void PlaceWallHandles(CubeMap editMap)
        {
            var editMode = GlobalSettings.Instance.EditMode;
            var dx = Mathf.Abs(StartPos.x - EndPos.x);
            var dz = Mathf.Abs(StartPos.z - EndPos.z);
            if (dx >= dz)
            {
                EndPos.y = StartPos.y;
                EndPos.z = StartPos.z;
                BlockHandle handleEndX = new BlockHandle(
                        BlockHandle.HandleType.X,
                        EndPos.x,
                        newX => {
                            EndPos.x = newX;
                            Undo(false);
                            BlockPlaceFunctions.PlaceWall(editMap, SelectedBlock.Item1, StartPos, EndPos, WallHeight);
                            return editMode.ActualPosition + EndPos + new Vector3(Mathf.Clamp01(Mathf.Sign(EndPos.x - StartPos.x)), 3.5f, 0.5f);
                        },
                        editMode.ActualPosition + EndPos + new Vector3(Mathf.Clamp01(Mathf.Sign(EndPos.x - StartPos.x)), 3.5f, 0.5f)
                        );
                BlockHandle handleStartX = new BlockHandle(
                        BlockHandle.HandleType.X,
                        StartPos.x,
                        newX => {
                            StartPos.x = newX;
                            Undo(false);
                            BlockPlaceFunctions.PlaceWall(editMap, SelectedBlock.Item1, StartPos, EndPos, WallHeight);
                            return editMode.ActualPosition + StartPos + new Vector3(Mathf.Clamp01(Mathf.Sign(StartPos.x - EndPos.x)), 3.5f, 0.5f);
                        },
                        editMode.ActualPosition + StartPos + new Vector3(Mathf.Clamp01(Mathf.Sign(StartPos.x - EndPos.x)), 3.5f, 0.5f)
                        );
                toolHandles.Add(handleEndX);
                toolHandles.Add(handleStartX);
            }
            else
            {
                EndPos.y = StartPos.y;
                EndPos.x = StartPos.x;
                BlockHandle handleEndZ = new BlockHandle(
                        BlockHandle.HandleType.Z,
                        EndPos.z,
                        newZ => {
                            EndPos.z = newZ;
                            Undo(false);
                            BlockPlaceFunctions.PlaceWall(editMap, SelectedBlock.Item1, StartPos, EndPos, WallHeight);
                            return editMode.ActualPosition + EndPos + new Vector3(0.5f, 3.5f, Mathf.Clamp01(Mathf.Sign(EndPos.z - StartPos.z)));
                        },
                        editMode.ActualPosition + EndPos + new Vector3(0.5f, 3.5f, Mathf.Clamp01(Mathf.Sign(EndPos.z - StartPos.z)))
                        );
                BlockHandle handleStartZ = new BlockHandle(
                        BlockHandle.HandleType.Z,
                        StartPos.z,
                        newZ => {
                            StartPos.z = newZ;
                            Undo(false);
                            BlockPlaceFunctions.PlaceWall(editMap, SelectedBlock.Item1, StartPos, EndPos, WallHeight);
                            return editMode.ActualPosition + StartPos + new Vector3(0.5f, 3.5f, Mathf.Clamp01(Mathf.Sign(StartPos.z - EndPos.z)));
                        },
                        editMode.ActualPosition + StartPos + new Vector3(0.5f, 3.5f, Mathf.Clamp01(Mathf.Sign(StartPos.z - EndPos.z)))
                        );
                toolHandles.Add(handleEndZ);
                toolHandles.Add(handleStartZ);
            }
        }

        private void PlaceLineHandles(CubeMap editMap)
        {
            var editMode = GlobalSettings.Instance.EditMode;
            var dx = Mathf.Abs(StartPos.x - EndPos.x);
            var dy = Mathf.Abs(StartPos.y - EndPos.y);
            var dz = Mathf.Abs(StartPos.z - EndPos.z);
            if (dx >= dy && dx >= dz)
            {
                EndPos.y = StartPos.y;
                EndPos.z = StartPos.z;
                BlockHandle handleEndX = new BlockHandle(
                        BlockHandle.HandleType.X,
                        EndPos.x,
                        newX => {
                            EndPos.x = newX;
                            Undo(false);
                            BlockPlaceFunctions.PlaceLine(editMap, SelectedBlock.Item1, StartPos, EndPos);
                            return editMode.ActualPosition + EndPos + new Vector3(Mathf.Clamp01(Mathf.Sign(EndPos.x - StartPos.x)), 0.5f, 0.5f);
                        },
                        editMode.ActualPosition + EndPos + new Vector3(Mathf.Clamp01(Mathf.Sign(EndPos.x - StartPos.x)), 0.5f, 0.5f)
                        );
                BlockHandle handleStartX = new BlockHandle(
                        BlockHandle.HandleType.X,
                        StartPos.x,
                        newX => {
                            StartPos.x = newX;
                            Undo(false);
                            BlockPlaceFunctions.PlaceLine(editMap, SelectedBlock.Item1, StartPos, EndPos);
                            return editMode.ActualPosition + StartPos + new Vector3(Mathf.Clamp01(Mathf.Sign(StartPos.x - EndPos.x)), 0.5f, 0.5f);
                        },
                        editMode.ActualPosition + StartPos + new Vector3(Mathf.Clamp01(Mathf.Sign(StartPos.x - EndPos.x)), 0.5f, 0.5f)
                        );
                toolHandles.Add(handleEndX);
                toolHandles.Add(handleStartX);
            }
            else if (dy > dz)
            {
                EndPos.x = StartPos.x;
                EndPos.z = StartPos.z;
                BlockHandle handleEndY = new BlockHandle(
                        BlockHandle.HandleType.Y,
                        EndPos.y,
                        newY => {
                            EndPos.y = newY;
                            Undo(false);
                            BlockPlaceFunctions.PlaceLine(editMap, SelectedBlock.Item1, StartPos, EndPos);
                            return editMode.ActualPosition + EndPos + new Vector3(0.5f, Mathf.Clamp01(Mathf.Sign(EndPos.y - StartPos.y)), 0.5f);
                        },
                        editMode.ActualPosition + EndPos + new Vector3(0.5f, Mathf.Clamp01(Mathf.Sign(EndPos.y - StartPos.y)), 0.5f)
                        );
                BlockHandle handleStartY = new BlockHandle(
                        BlockHandle.HandleType.Y,
                        StartPos.y,
                        newY => {
                            StartPos.y = newY;
                            Undo(false);
                            BlockPlaceFunctions.PlaceLine(editMap, SelectedBlock.Item1, StartPos, EndPos);
                            return editMode.ActualPosition + StartPos + new Vector3(0.5f, Mathf.Clamp01(Mathf.Sign(StartPos.y - EndPos.y)), 0.5f);
                        },
                        editMode.ActualPosition + StartPos + new Vector3(0.5f, Mathf.Clamp01(Mathf.Sign(StartPos.y - EndPos.y)), 0.5f)
                        );
                toolHandles.Add(handleEndY);
                toolHandles.Add(handleStartY);
            }
            else
            {
                EndPos.y = StartPos.y;
                EndPos.x = StartPos.x;
                BlockHandle handleEndZ = new BlockHandle(
                        BlockHandle.HandleType.Z,
                        EndPos.z,
                        newZ => {
                            EndPos.z = newZ;
                            Undo(false);
                            BlockPlaceFunctions.PlaceLine(editMap, SelectedBlock.Item1, StartPos, EndPos);
                            return editMode.ActualPosition + EndPos + new Vector3(0.5f, 0.5f, Mathf.Clamp01(Mathf.Sign(EndPos.z - StartPos.z)));
                        },
                        editMode.ActualPosition + EndPos + new Vector3(0.5f, 0.5f, Mathf.Clamp01(Mathf.Sign(EndPos.z - StartPos.z)))
                        );
                BlockHandle handleStartZ = new BlockHandle(
                        BlockHandle.HandleType.Z,
                        StartPos.z,
                        newZ => {
                            StartPos.z = newZ;
                            Undo(false);
                            BlockPlaceFunctions.PlaceLine(editMap, SelectedBlock.Item1, StartPos, EndPos);
                            return editMode.ActualPosition + StartPos + new Vector3(0.5f, 0.5f, Mathf.Clamp01(Mathf.Sign(StartPos.z - EndPos.z)));
                        },
                        editMode.ActualPosition + StartPos + new Vector3(0.5f, 0.5f, Mathf.Clamp01(Mathf.Sign(StartPos.z - EndPos.z)))
                        );
                toolHandles.Add(handleEndZ);
                toolHandles.Add(handleStartZ);
            }
        }

        private void PlaceStairHandles(CubeMap editMap)
        {
            var editMode = GlobalSettings.Instance.EditMode;
            var dx = StartPos.x - EndPos.x;
            var dz = StartPos.z - EndPos.z;

            if (Math.Abs(dx) > Math.Abs(dz))
            {
                BlockHandle handleZ = new BlockHandle(
                    BlockHandle.HandleType.Z,
                    StartPos.z,
                    newZ =>
                    {
                        StartPos.z = newZ;
                        EndPos.z = StartPos.z - dz;
                        Undo(false);
                        BlockPlaceFunctions.PlaceStair(editMap, SelectedBlock.Item1, StartPos, EndPos, StairFull, StairWidth);
                        return editMode.ActualPosition + (StartPos + EndPos).ToVector3() / 2 + Vector3.one / 2;
                    },
                    editMode.ActualPosition + (StartPos + EndPos).ToVector3() / 2 + Vector3.one / 2
                );
                toolHandles.Add(handleZ);
            }
            else
            {
                BlockHandle handleX = new BlockHandle(
                    BlockHandle.HandleType.X,
                    StartPos.x,
                    newX =>
                    {
                        StartPos.x = newX;
                        EndPos.x = StartPos.x - dx;
                        Undo(false);
                        BlockPlaceFunctions.PlaceStair(editMap, SelectedBlock.Item1, StartPos, EndPos, StairFull, StairWidth);
                        return editMode.ActualPosition + (StartPos + EndPos).ToVector3() / 2 + Vector3.one / 2;
                    },
                    editMode.ActualPosition + (StartPos + EndPos).ToVector3() / 2 + Vector3.one / 2
                );
                toolHandles.Add(handleX);
            }
        }

        private void PlaceRoofHandles(CubeMap editMap)
        {
            var editMode = GlobalSettings.Instance.EditMode;
            BlockHandle handleX = new BlockHandle(
                        BlockHandle.HandleType.X,
                        Mathf.Max(StartPos.x, EndPos.x),
                        newX => {
                            if (StartPos.x > EndPos.x) StartPos.x = newX;
                            else EndPos.x = newX;
                            Undo(false);
                            BlockPlaceFunctions.PlaceRoof(editMap, SelectedBlock.Item1, StartPos, EndPos, RoofHandling, RoofHeightMultiplier);
                            toolHandles.Clear();
                            PlaceRoofHandles(editMap);
                            return editMode.ActualPosition + new Vector3(Mathf.Max(StartPos.x, EndPos.x) + 0.5f, 0.5f, (StartPos.z + EndPos.z) / 2f + 0.5f);
                        },
                        editMode.ActualPosition + new Vector3(Mathf.Max(StartPos.x, EndPos.x) + 0.5f, 0.5f, (StartPos.z + EndPos.z) / 2f + 0.5f)
                        );
            BlockHandle handleNegX = new BlockHandle(
                        BlockHandle.HandleType.X,
                        Mathf.Min(StartPos.x, EndPos.x),
                        newX => {
                            if (StartPos.x < EndPos.x) StartPos.x = newX;
                            else EndPos.x = newX;
                            Undo(false);
                            BlockPlaceFunctions.PlaceRoof(editMap, SelectedBlock.Item1, StartPos, EndPos, RoofHandling, RoofHeightMultiplier);
                            toolHandles.Clear();
                            PlaceRoofHandles(editMap);
                            return editMode.ActualPosition + new Vector3(Mathf.Min(StartPos.x, EndPos.x) + 0.5f, 0.5f, (StartPos.z + EndPos.z) / 2f + 0.5f);
                        },
                        editMode.ActualPosition + new Vector3(Mathf.Min(StartPos.x, EndPos.x) + 0.5f, 0.5f, (StartPos.z + EndPos.z) / 2f + 0.5f)
                        );
            BlockHandle handleZ = new BlockHandle(
                        BlockHandle.HandleType.Z,
                        Mathf.Max(StartPos.z, EndPos.z),
                        newZ => {
                            if (StartPos.z > EndPos.z) StartPos.z = newZ;
                            else EndPos.z = newZ;
                            Undo(false);
                            BlockPlaceFunctions.PlaceRoof(editMap, SelectedBlock.Item1, StartPos, EndPos, RoofHandling, RoofHeightMultiplier);
                            toolHandles.Clear();
                            PlaceRoofHandles(editMap);
                            return editMode.ActualPosition + new Vector3((StartPos.x + EndPos.x) / 2f + 0.5f, 0.5f, Mathf.Max(StartPos.z, EndPos.z) + 0.5f);
                        },
                        editMode.ActualPosition + new Vector3((StartPos.x + EndPos.x) / 2f + 0.5f, 0.5f, Mathf.Max(StartPos.z, EndPos.z) + 0.5f)
                        );
            BlockHandle handleNegZ = new BlockHandle(
                        BlockHandle.HandleType.Z,
                        Mathf.Min(StartPos.z, EndPos.z),
                        newZ => {
                            if (StartPos.z < EndPos.z) StartPos.z = newZ;
                            else EndPos.z = newZ;
                            Undo(false);
                            BlockPlaceFunctions.PlaceRoof(editMap, SelectedBlock.Item1, StartPos, EndPos, RoofHandling, RoofHeightMultiplier);
                            toolHandles.Clear();
                            PlaceRoofHandles(editMap);
                            return editMode.ActualPosition + new Vector3((StartPos.x + EndPos.x) / 2f + 0.5f, 0.5f, Mathf.Min(StartPos.z, EndPos.z) + 0.5f);
                        },
                        editMode.ActualPosition + new Vector3((StartPos.x + EndPos.x) / 2f + 0.5f, 0.5f, Mathf.Min(StartPos.z, EndPos.z) + 0.5f)
                        );
            toolHandles.Add(handleX);
            toolHandles.Add(handleNegX);
            toolHandles.Add(handleZ);
            toolHandles.Add(handleNegZ);
        }

        private void PlaceRoomHandles(CubeMap editMap)
        {
            var editMode = GlobalSettings.Instance.EditMode;
            BlockHandle handleX = new BlockHandle(
                        BlockHandle.HandleType.X,
                        Mathf.Max(StartPos.x, EndPos.x),
                        newX => {
                            if (StartPos.x > EndPos.x) StartPos.x = newX;
                            else EndPos.x = newX;
                            Undo(false);
                            BlockPlaceFunctions.PlaceRoom(editMap, SelectedBlock.Item1, StartPos, EndPos, WallHeight);
                            toolHandles.Clear();
                            PlaceRoomHandles(editMap);
                            return editMode.ActualPosition + new Vector3(Mathf.Max(StartPos.x, EndPos.x) + 0.5f, 3.5f, (StartPos.z + EndPos.z) / 2f + 0.5f);
                        },
                        editMode.ActualPosition + new Vector3(Mathf.Max(StartPos.x, EndPos.x) + 0.5f, 3.5f, (StartPos.z + EndPos.z) / 2f + 0.5f)
                        );
            BlockHandle handleNegX = new BlockHandle(
                        BlockHandle.HandleType.X,
                        Mathf.Min(StartPos.x, EndPos.x),
                        newX => {
                            if (StartPos.x < EndPos.x) StartPos.x = newX;
                            else EndPos.x = newX;
                            Undo(false);
                            BlockPlaceFunctions.PlaceRoom(editMap, SelectedBlock.Item1, StartPos, EndPos, WallHeight);
                            toolHandles.Clear();
                            PlaceRoomHandles(editMap);
                            return editMode.ActualPosition + new Vector3(Mathf.Min(StartPos.x, EndPos.x) + 0.5f, 3.5f, (StartPos.z + EndPos.z) / 2f + 0.5f);
                        },
                        editMode.ActualPosition + new Vector3(Mathf.Min(StartPos.x, EndPos.x) + 0.5f, 3.5f, (StartPos.z + EndPos.z) / 2f + 0.5f)
                        );
            BlockHandle handleZ = new BlockHandle(
                        BlockHandle.HandleType.Z,
                        Mathf.Max(StartPos.z, EndPos.z),
                        newZ => {
                            if (StartPos.z > EndPos.z) StartPos.z = newZ;
                            else EndPos.z = newZ;
                            Undo(false);
                            BlockPlaceFunctions.PlaceRoom(editMap, SelectedBlock.Item1, StartPos, EndPos, WallHeight);
                            toolHandles.Clear();
                            PlaceRoomHandles(editMap);
                            return editMode.ActualPosition + new Vector3((StartPos.x + EndPos.x) / 2f + 0.5f, 3.5f, Mathf.Max(StartPos.z, EndPos.z) + 0.5f);
                        },
                        editMode.ActualPosition + new Vector3((StartPos.x + EndPos.x) / 2f + 0.5f, 3.5f, Mathf.Max(StartPos.z, EndPos.z) + 0.5f)
                        );
            BlockHandle handleNegZ = new BlockHandle(
                        BlockHandle.HandleType.Z,
                        Mathf.Min(StartPos.z, EndPos.z),
                        newZ => {
                            if (StartPos.z < EndPos.z) StartPos.z = newZ;
                            else EndPos.z = newZ;
                            Undo(false);
                            BlockPlaceFunctions.PlaceRoom(editMap, SelectedBlock.Item1, StartPos, EndPos, WallHeight);
                            toolHandles.Clear();
                            PlaceRoomHandles(editMap);
                            return editMode.ActualPosition + new Vector3((StartPos.x + EndPos.x) / 2f + 0.5f, 3.5f, Mathf.Min(StartPos.z, EndPos.z) + 0.5f);
                        },
                        editMode.ActualPosition + new Vector3((StartPos.x + EndPos.x) / 2f + 0.5f, 3.5f, Mathf.Min(StartPos.z, EndPos.z) + 0.5f)
                        );
            toolHandles.Add(handleX);
            toolHandles.Add(handleNegX);
            toolHandles.Add(handleZ);
            toolHandles.Add(handleNegZ);
        }

        public void ClearHandles()
        {
            basicHandles.Clear();
            toolHandles.Clear();
        }

        public void CreateBasicHandles()
        {
            if (basicHandles.Count != 0) return;
            var editMode = GlobalSettings.Instance.EditMode;
            var handleX = new AreaBlockHandle(
                        BlockHandle.HandleType.X,
                        editMode.EditSize.x,
                        newX => {
                            editMode.SetSize(newX, editMode.EditSize.y);
                            ReInitBasicHandles();
                            return editMode.ActualPosition + new Vector3(editMode.EditSize.x, 0.05f, editMode.EditSize.y / 2f);
                        },
                        editMode.ActualPosition + new Vector3(editMode.EditSize.x, 0.05f, editMode.EditSize.y / 2f)
                        );
            var handleZ = new AreaBlockHandle(
                        BlockHandle.HandleType.Z,
                        editMode.EditSize.y,
                        newZ => {
                            editMode.SetSize(editMode.EditSize.x, newZ);
                            ReInitBasicHandles();
                            return editMode.ActualPosition + new Vector3(editMode.EditSize.x / 2f, 0.05f, editMode.EditSize.y);
                        },
                        editMode.ActualPosition + new Vector3(editMode.EditSize.x / 2f, 0.05f, editMode.EditSize.y)
                        );
            var handleNegX = new AreaBlockHandle(
                        BlockHandle.HandleType.NegX,
                        editMode.EditSize.x,
                        newX => {
                            editMode.SetSize(newX, editMode.EditSize.y, true);
                            ReInitBasicHandles();
                            return editMode.ActualPosition + new Vector3(0, 0.05f, editMode.EditSize.y / 2f);
                        },
                        editMode.ActualPosition + new Vector3(0, 0.05f, editMode.EditSize.y / 2f)
                        );
            var handleNegZ = new AreaBlockHandle(
                        BlockHandle.HandleType.NegZ,
                        editMode.EditSize.y,
                        newZ => {
                            editMode.SetSize(editMode.EditSize.x, newZ, true);
                            ReInitBasicHandles();
                            return editMode.ActualPosition + new Vector3(editMode.EditSize.x / 2f, 0.05f, 0);
                        },
                        editMode.ActualPosition + new Vector3(editMode.EditSize.x / 2f, 0.05f, 0)
                        );
            basicHandles.Add(handleX);
            basicHandles.Add(handleZ);
            basicHandles.Add(handleNegX);
            basicHandles.Add(handleNegZ);
        }

        private void ReInitBasicHandles()
        {
            var editMode = GlobalSettings.Instance.EditMode;
            BlockHandle handleX = basicHandles[0];
            BlockHandle handleZ = basicHandles[1];
            BlockHandle handleNegX = basicHandles[2];
            BlockHandle handleNegZ = basicHandles[3];
            handleX.ReInit(editMode.EditSize.x, editMode.ActualPosition + new Vector3(editMode.EditSize.x, 0.05f, editMode.EditSize.y / 2f));
            handleZ.ReInit(editMode.EditSize.y, editMode.ActualPosition + new Vector3(editMode.EditSize.x / 2f, 0.05f, editMode.EditSize.y));
            handleNegX.ReInit(editMode.EditSize.x, editMode.ActualPosition + new Vector3(0, 0.05f, editMode.EditSize.y / 2f));
            handleNegZ.ReInit(editMode.EditSize.y, editMode.ActualPosition + new Vector3(editMode.EditSize.x / 2f, 0.05f, 0));
        }

        private void SetRenderPositions()
        {
            RenderStart = StartPos;
            var dx = Mathf.Abs(StartPos.x - EndPos.x);
            var dy = Mathf.Abs(StartPos.y - EndPos.y);
            var dz = Mathf.Abs(StartPos.z - EndPos.z);
            switch (GetPlaceMethod())
            {
                case BlockMethod.Block:
                    RenderEnd = StartPos;
                    break;
                case BlockMethod.Stair:
                    if (dz <= dx)
                    {
                        RenderEnd = new Vector3Int(EndPos.x, EndPos.y, StartPos.z);
                    }
                    else
                    {
                        RenderEnd = new Vector3Int(StartPos.x, EndPos.y, EndPos.z);
                    }
                    break;
                case BlockMethod.Line:
                    if (dx >= dy && dx >= dz)
                    {
                        RenderEnd = new Vector3Int(EndPos.x, StartPos.y, StartPos.z);
                    }
                    else if (dy > dz)
                    {
                        RenderEnd = new Vector3Int(StartPos.x, EndPos.y, StartPos.z);
                    }
                    else
                    {
                        RenderEnd = new Vector3Int(StartPos.x, StartPos.y, EndPos.z);
                    }
                    break;
                case BlockMethod.Frame:
                case BlockMethod.Plane:
                    if (dy <= dx && dy <= dz)
                    {
                        RenderEnd = new Vector3Int(EndPos.x, StartPos.y, EndPos.z);
                    }
                    else if (dz <= dx && dz <= dy)
                    {
                        RenderEnd = new Vector3Int(EndPos.x, EndPos.y, StartPos.z);
                    }
                    else
                    {
                        RenderEnd = new Vector3Int(StartPos.x, EndPos.y, EndPos.z);
                    }
                    break;
                case BlockMethod.Roof:
                    RenderEnd = new Vector3Int(EndPos.x, StartPos.y, EndPos.z);
                    break;
                case BlockMethod.Wall:
                    if (dx >= dz)
                    {
                        RenderEnd = new Vector3Int(EndPos.x, StartPos.y + WallHeight - 1, StartPos.z);
                    }
                    else
                    {
                        RenderEnd = new Vector3Int(StartPos.x, StartPos.y + WallHeight - 1, EndPos.z);
                    }
                    break;
                case BlockMethod.Room:
                    RenderEnd = new Vector3Int(EndPos.x, StartPos.y + WallHeight - 1, EndPos.z);
                    break;
                default:
                    RenderEnd = EndPos;
                    break;
            }
        }

        private Vector3Int RenderStart, RenderEnd;

        void OnRenderObject()
        {
            if (RenderSelection)
            {
                Vector3 min = GlobalSettings.Instance.EditMode.transform.position + Vector3Int.Min(RenderStart, RenderEnd);
                var max = GlobalSettings.Instance.EditMode.transform.position + Vector3Int.Max(RenderStart, RenderEnd) + Vector3.one;

                if (SelectionMaterial.SetPass(0))
                {

                    SelectionMaterial.SetVector("_Size", new Vector4(max.x - min.x, max.y - min.y, max.z - min.z, 0));
                    GL.Begin(GL.QUADS);

                    min -= Vector3.one * 0.1f;
                    max += Vector3.one * 0.1f;
                    GL.TexCoord3(0, 0, 0); GL.Vertex3(min.x, min.y, min.z);
                    GL.TexCoord3(0, 0, 1); GL.Vertex3(min.x, min.y, max.z);
                    GL.TexCoord3(1, 0, 1); GL.Vertex3(max.x, min.y, max.z);
                    GL.TexCoord3(1, 0, 0); GL.Vertex3(max.x, min.y, min.z);//Y-

                    GL.TexCoord3(0, 1, 0); GL.Vertex3(min.x, max.y, min.z);
                    GL.TexCoord3(1, 1, 0); GL.Vertex3(max.x, max.y, min.z);
                    GL.TexCoord3(1, 1, 1); GL.Vertex3(max.x, max.y, max.z);
                    GL.TexCoord3(0, 1, 1); GL.Vertex3(min.x, max.y, max.z);//Y+

                    GL.TexCoord3(0, 0, 0); GL.Vertex3(min.x, min.y, min.z);
                    GL.TexCoord3(1, 0, 0); GL.Vertex3(max.x, min.y, min.z);
                    GL.TexCoord3(1, 1, 0); GL.Vertex3(max.x, max.y, min.z);
                    GL.TexCoord3(0, 1, 0); GL.Vertex3(min.x, max.y, min.z);//Z-

                    GL.TexCoord3(0, 0, 1); GL.Vertex3(min.x, min.y, max.z);
                    GL.TexCoord3(0, 1, 1); GL.Vertex3(min.x, max.y, max.z);
                    GL.TexCoord3(1, 1, 1); GL.Vertex3(max.x, max.y, max.z);
                    GL.TexCoord3(1, 0, 1); GL.Vertex3(max.x, min.y, max.z);//Z+

                    GL.TexCoord3(1, 0, 0); GL.Vertex3(max.x, min.y, min.z);
                    GL.TexCoord3(1, 0, 1); GL.Vertex3(max.x, min.y, max.z);
                    GL.TexCoord3(1, 1, 1); GL.Vertex3(max.x, max.y, max.z);
                    GL.TexCoord3(1, 1, 0); GL.Vertex3(max.x, max.y, min.z);//X+

                    GL.TexCoord3(0, 0, 0); GL.Vertex3(min.x, min.y, min.z);
                    GL.TexCoord3(0, 1, 0); GL.Vertex3(min.x, max.y, min.z);
                    GL.TexCoord3(0, 1, 1); GL.Vertex3(min.x, max.y, max.z);
                    GL.TexCoord3(0, 0, 1); GL.Vertex3(min.x, min.y, max.z);//X-
                    GL.End();
                }
            }
        }
    }
}
