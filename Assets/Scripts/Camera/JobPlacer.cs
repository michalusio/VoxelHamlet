using Assets.Scripts.Jobs;
using Assets.Scripts.PathFinding;
using Assets.Scripts.Utilities;
using Assets.Scripts.Village;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts
{
    [RequireComponent(typeof(Camera))]
    public class JobPlacer : MonoBehaviour
    {
        public CommandPanelScript CommandScript;

        public Material SelectionRenderMaterial;

        private bool Selecting;
        private Vector3Int MouseStart, MouseEnd;
        private Vector3Int AreaStart, AreaEnd;

        private CommandAction lastCommandAction;
        private Camera _camera;

        void Start()
        {
            _camera = GetComponent<Camera>();
        }

        void Update()
        {
            var raycastHit = RaycastUtils.Raycast(GlobalSettings.Instance.Map, _camera.ScreenPointToRay(Input.mousePosition), out var hitInfo);
            lastCommandAction = CommandScript.GetChosenJob();
            if (lastCommandAction.Type != CommandActionType.NULL && !EventSystem.current.IsPointerOverGameObject())
            {
                if (Input.GetMouseButtonUp(0) && Selecting)
                {
                    switch (lastCommandAction.Type)
                    {
                        case CommandActionType.JobTemplate:
                            GlobalSettings.Instance.JobScheduler.AddAreaJob(AreaStart, AreaEnd, lastCommandAction.JobTemplate);
                            break;
                        case CommandActionType.StorageDesignation:
                            GlobalSettings.Instance.JobScheduler.AddStorage(new Storage(AreaStart, AreaEnd));
                            break;
                        case CommandActionType.WorkshopDesignation:
                            break;
                        case CommandActionType.EditMode:
                            break;
                    }
                    ResetSelection();
                }
                if (Input.GetMouseButtonDown(0) && raycastHit)
                {
                    MouseStart = hitInfo.Position + (lastCommandAction.OnNormalSide ? Vector3Int.RoundToInt(hitInfo.Normal) : Vector3Int.zero);
                    Selecting = true;
                }
                if (Input.GetMouseButton(0) && raycastHit)
                {
                    MouseEnd = hitInfo.Position + (lastCommandAction.OnNormalSide ? Vector3Int.RoundToInt(hitInfo.Normal) : Vector3Int.zero);
                    if (lastCommandAction.Type != CommandActionType.JobTemplate)
                    {
                        (AreaStart, AreaEnd) = GetValidDesignation();
                    }
                    else
                    {
                        (AreaStart, AreaEnd) = (MouseStart, MouseEnd);
                    }
                }
            }
            else
            {
                ResetSelection();
            }

            if (Input.GetMouseButton(1))
            {
                ResetSelection();
            }
        }

        private void ResetSelection()
        {
            Selecting = false;
            MouseStart = Vector3Int.one * -5;
            MouseEnd = MouseStart;
            (AreaStart, AreaEnd) = (MouseStart, MouseEnd);
        }

        private (Vector3Int start, Vector3Int end) GetValidDesignation()
        {
            var posX = MouseEnd.x > MouseStart.x;
            var posZ = MouseEnd.z > MouseStart.z;
            var xBigger = Mathf.Abs(MouseEnd.x - MouseStart.x) > Mathf.Abs(MouseEnd.z - MouseStart.z);

            var map = GlobalSettings.Instance.Map;

            var xScope = 0;
            var zScope = 0;

            if (xBigger)
            {
                for (int z = MouseStart.z; posZ ? z <= MouseEnd.z : z >= MouseEnd.z; z = posZ ? (z + 1) : (z - 1))
                {
                    for (int x = MouseStart.x; posX ? x <= MouseEnd.x : x >= MouseEnd.x; x = posX ? (x + 1) : (x - 1))
                    {
                        if (map[x, MouseStart.y, z].IsWalkable())
                        {
                            xScope = x;
                            zScope = z;
                        }
                        else
                        {
                            return (MouseStart, new Vector3Int(xScope, MouseStart.y, zScope));
                        }
                    }
                }
            }
            else
            {
                for (int x = MouseStart.x; posX ? x <= MouseEnd.x : x >= MouseEnd.x; x = posX ? (x + 1) : (x - 1))
                {
                    for (int z = MouseStart.z; posZ ? z <= MouseEnd.z : z >= MouseEnd.z; z = posZ ? (z + 1) : (z - 1))
                    {
                        if (map[x, MouseStart.y, z].IsWalkable())
                        {
                            xScope = x;
                            zScope = z;
                        }
                        else
                        {
                            return (MouseStart, new Vector3Int(xScope, MouseStart.y, zScope));
                        }
                    }
                }
            }
            return (MouseStart, new Vector3Int(MouseEnd.x, MouseStart.y, MouseEnd.z));
        }

        void OnPostRender()
        {
            var inst = GlobalSettings.Instance;

            if (inst != null && inst.Map != null && inst.Map.IsInBounds(AreaStart) && inst.Map.IsInBounds(AreaEnd))
            {
                Vector3 min = Vector3Int.Min(AreaStart, AreaEnd);
                var max = Vector3Int.Max(AreaStart, AreaEnd) + Vector3.one;

                if (!SelectionRenderMaterial.SetPass(0)) return;
                if (lastCommandAction.Type == CommandActionType.NULL) return;
                switch (lastCommandAction.Type)
                {
                    case CommandActionType.JobTemplate:
                        SelectionRenderMaterial.color = lastCommandAction.JobColor;
                        break;
                    case CommandActionType.StorageDesignation:
                    case CommandActionType.WorkshopDesignation:
                        SelectionRenderMaterial.color = new Color(1, 1, 0, 0.5f);
                        break;
                }
                
                SelectionRenderMaterial.SetVector("_Size", new Vector4(max.x - min.x, max.y - min.y, max.z - min.z, 0));
                GL.Begin(GL.QUADS);
                
                if (lastCommandAction.Type == CommandActionType.JobTemplate)
                {
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
                }
                else
                {
                    GL.TexCoord3(0, 0, 0); GL.Vertex3(min.x, min.y + 0.1f, min.z);
                    GL.TexCoord3(1, 0, 0); GL.Vertex3(max.x, min.y + 0.1f, min.z);
                    GL.TexCoord3(1, 0, 1); GL.Vertex3(max.x, min.y + 0.1f, max.z);
                    GL.TexCoord3(0, 0, 1); GL.Vertex3(min.x, min.y + 0.1f, max.z);//Y-
                }
                GL.End();
            }
        }
    }
}