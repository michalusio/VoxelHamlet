using Assets.Scripts.ConfigScripts;
using Assets.Scripts.UI;
using Assets.Scripts.Utilities;
using Assets.Scripts.Utilities.Math;
using Assets.Scripts.WorldGen;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.EditMode
{
    public class EditModeController : MonoBehaviour
    {
        [SerializeField] private Material EditModeMaterial = default;
        [SerializeField] private Mesh EditModeMesh = default;
        public Camera MainCamera = default;
        public Camera EditScreenshotCamera = default;
        [SerializeField] private BuildBlockSelectScript SelectScript = default;
        [SerializeField] private BuildMethodSelectScript MethodScript = default;
        public BuildingSelectScript BuildSelectScript = default;
        [SerializeField] private GameObject BuildPanel = default;
        public Color TurnedOffColor = Color.gray;
        public bool EditMode;
        public bool PositionSet;

        public Vector3 ScreenshotForward;

        public CubeMap EditMap;
        public Vector2Int EditSize;

        private Vector3? newTransformPosition;

        public Vector3 ActualPosition => newTransformPosition ?? transform.position;

        void Start()
        {
            GlobalSettings.Instance.EditMode = this;
            EditMap = new CubeMap(64, 64, 64);
            BuildPanel.SetActive(false);
            ScreenshotForward = EditScreenshotCamera.transform.forward;
        }

        public void LoadBuilding(BuildingContainer building)
        {
            if (!PositionSet) return;
            MethodScript.MakeSnapshot();
            EditSize = building.Size;
            for (int x = 0; x < EditMap.W; x++)
            {
                for (int y = 0; y < EditMap.H; y++)
                {
                    for (int z = 0; z < EditMap.D; z++)
                    {
                        var b = building.Blocks[z + EditMap.D * (y + EditMap.H * x)];
                        EditMap[x, y, z] = b;
                    }
                }
            }
            MethodScript.CreateBasicHandles();
        }

        void Update()
        {
            if (EditMode && PositionSet)
            {
                MethodScript.CreateBasicHandles();

                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    MethodScript.HandleInput(SelectScript.GetBlockOrEntity());
                }

                if (newTransformPosition.HasValue)
                {
                    transform.position = newTransformPosition.Value;
                    newTransformPosition = null;
                }
                EditMap.DrawMeshes(MainCamera, GlobalSettings.Instance.EditMapMaterial, "EditMode", ActualPosition, false);
                EditMap.DrawMeshes(EditScreenshotCamera, GlobalSettings.Instance.EditMapMaterial, "EditMode", ActualPosition, false);
            }
            else if (EditMode && !EventSystem.current.IsPointerOverGameObject())
            {
                var ray = MainCamera.ScreenPointToRay(Input.mousePosition);
                if (RaycastUtils.RaycastOOB(GlobalSettings.Instance.Map, ray, 200, out var hitInfo))
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        transform.position = Vector3Int.RoundToInt(hitInfo.Position + hitInfo.Normal);
                    }
                    if (Input.GetMouseButtonUp(0))
                    {
                        PositionSet = true;
                        var start = Vector3Int.RoundToInt(transform.position);
                        var end = start + Utils.Add1IfNegative(new Vector3Int(EditSize.x, 0, EditSize.y));

                        transform.position = Vector3Int.Min(start, end);
                        EditSize = new Vector2Int(Mathf.Abs(EditSize.x), Mathf.Abs(EditSize.y));

                        EditMap.Clear();
                    }
                    if (Input.GetMouseButton(0))
                    {
                        var start = Vector3Int.RoundToInt(transform.position);
                        var otherEnd = Vector3Int.RoundToInt(hitInfo.Position + hitInfo.Normal);

                        var difference = otherEnd - start;

                        EditSize = new Vector2Int(Mathf.Min(64, Mathf.Max(-64, Utils.AddAbsolute1(difference.x))), Mathf.Min(64, Mathf.Max(-64, Utils.AddAbsolute1(difference.z))));
                    }
                }
            }
            var cameraPos = MainCamera.transform.position;
            GlobalSettings.Instance.MapMaterial.SetVector("_CameraPos", cameraPos.ToVector4());
            GlobalSettings.Instance.EditMapMaterial.SetVector("_CameraPos", cameraPos.ToVector4());

            if (!EditMode) return;
            var editScale = new Vector3(EditSize.x / 10f, 1, EditSize.y / 10f);
            EditModeMaterial.SetVector("_Size", new Vector4(Mathf.Abs(EditSize.x), Mathf.Abs(EditSize.y), 0, 0));
            Graphics.DrawMesh(EditModeMesh, Matrix4x4.TRS(transform.position + new Vector3(Utils.Add1IfNegative(EditSize.x/2f), 0.05f, Utils.Add1IfNegative(EditSize.y/2f)), Quaternion.identity, editScale.Abs()), EditModeMaterial, LayerMask.NameToLayer("EditMode"), MainCamera, 0, null, false, false);
        }

        void LateUpdate()
        {
            if (EditMode)
            {
                EditMap.UpdateMeshes();
            }
        }

        public void ToggleEditMode()
        {
            EditMode ^= true;
            PositionSet = false;
            transform.position = Vector3.zero;
            EditSize = Vector2Int.zero;
            BuildPanel.SetActive(EditMode);
            MethodScript.ClearSnapshots();
            MethodScript.ClearHandles();
            MainCamera.GetComponent<JobPlacer>().enabled = !EditMode;
            GlobalSettings.Instance.MapMaterial.SetColor("_AddColor", EditMode ? TurnedOffColor : Color.white);
            GlobalSettings.Instance.EditMapMaterial.SetColor("_AddColor", !EditMode ? TurnedOffColor : Color.white);

            if (EditMode)
            {
                BuildSelectScript.PopulateWithBuildings(GlobalSettings.Instance.BuildingsPath);
            } else
            {
                FindObjectOfType<CommandPanelScript>().UnselectToggles();
            }
        }

        internal void SetSize(int newX, int newZ, bool moving = false)
        {
            var newSize = Vector2Int.Max(Vector2Int.zero, Vector2Int.Min(new Vector2Int(newX, newZ), new Vector2Int(64, 64)));
            if (newSize != EditSize)
            {
                if (moving)
                {
                    if (newSize.x < EditSize.x || newSize.y < EditSize.y)
                    {
                        var delta = EditSize - newSize;
                        for (int y = 0; y < 64; y++)
                        {
                            for (int x = 0; x < newSize.x; x++)
                            {
                                for (int z = 0; z < newSize.y; z++)
                                {
                                    EditMap[x, y, z] = EditMap[x + delta.x, y, z + delta.y];
                                }
                            }
                            for (int x = newSize.x; x < EditSize.x; x++)
                            {
                                for (int z = 0; z < newSize.y; z++)
                                {
                                    EditMap[x, y, z] = default;
                                }
                            }
                            
                            for (int z = newSize.y; z < EditSize.y; z++)
                            {
                                for (int x = 0; x < newSize.x; x++)
                                {
                                    EditMap[x, y, z] = default;
                                }
                            }
                        }
                    }
                    else
                    {
                        var delta = newSize - EditSize;
                        for (int y = 0; y < 64; y++)
                        {
                            for (int x = newSize.x - 1; x >= delta.x; x--)
                            {
                                for (int z = newSize.y - 1; z >= delta.y; z--)
                                {
                                    EditMap[x, y, z] = EditMap[x - delta.x, y, z - delta.y];
                                }
                            }
                            for (int x = 0; x < delta.x; x++)
                            {
                                for (int z = 0; z < newSize.y; z++)
                                {
                                    EditMap[x, y, z] = default;
                                }
                            }

                            for (int z = 0; z < delta.y; z++)
                            {
                                for (int x = 0; x < newSize.x; x++)
                                {
                                    EditMap[x, y, z] = default;
                                }
                            }
                        }
                    }
                    newTransformPosition = transform.position + new Vector3(EditSize.x - newSize.x, 0, EditSize.y - newSize.y);
                }
                else
                {
                    if (newSize.x < EditSize.x)
                    {
                        for (int y = 0; y < 64; y++)
                        {
                            for (int x = newSize.x; x < EditSize.x; x++)
                            {
                                for (int z = 0; z < newSize.y; z++)
                                {
                                    EditMap[x, y, z] = default;
                                }
                            }
                        }
                    }
                    if (newSize.y < EditSize.y)
                    {
                        for (int y = 0; y < 64; y++)
                        {
                            for (int x = 0; x < newSize.x; x++)
                            {
                                for (int z = newSize.y; z < EditSize.y; z++)
                                {
                                    EditMap[x, y, z] = default;
                                }
                            }
                        }
                    }
                }
                EditMap.UpdateMeshes();
            }
            EditSize = newSize;
        }
    }
}
