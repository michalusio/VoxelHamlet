using Assets.General;
using Assets.Scripts.WorldGen;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class MapSelectorScript : MonoBehaviour
{
    public InputField SeedField;
    public Button RandomButton;
    public Button ForwardButton;
    public Slider MountainSlider;
    public Slider ForestSlider;
    public RawImage MapImage;

    public bool Grayscale; 

    private MenuController menuController;

    void Start()
    {
        menuController = GetComponent<MenuController>();

        RandomButton.onClick.RemoveAllListeners();
        RandomButton.onClick.AddListener(GenerateSeed);

        ForwardButton.onClick.RemoveAllListeners();
        ForwardButton.onClick.AddListener(StartMap);

        MountainSlider.onValueChanged.RemoveAllListeners();
        MountainSlider.onValueChanged.AddListener(RenderMap);

        ForestSlider.onValueChanged.RemoveAllListeners();
        ForestSlider.onValueChanged.AddListener(RenderMap);

        GenerateSeed();
    }

    public void GenerateSeed()
    {
        var str = new StringBuilder(SeedField.characterLimit);
        for (int i = 0; i < SeedField.characterLimit; i++)
        {
            str.Append(Random.Range(0, 10).ToString());
        }
        SeedField.text = str.ToString();
        RenderMap();
    }

    private void RenderMap(float value = 0)
    {
        var seed = int.Parse(SeedField.text);
        var generator = new BaseTerrainGenerator(seed, 32, 64, Vector2Int.zero, (int)MountainSlider.value, (int)ForestSlider.value);
        var map = GetValueMap(generator, 96, 64);
        Texture2D texture;
        if (Grayscale) texture = GetGrayscaleTexture(generator, map, 32, 64);
        else texture = GetMapTexture(generator, map);
        MapImage.texture = texture;
    }

    private int[,] GetValueMap(BaseTerrainGenerator generator, int width, int height)
    {
        var map = new int[width + 2, height + 2];
        for (int x = 0; x < width + 2; x++)
        {
            for (int z = 0; z < height + 2; z++)
            {
                map[x, z] = generator.GetHeightAt(x, z);
            }
        }
        return map;
    }

    private Texture2D GetMapTexture(BaseTerrainGenerator generator, int[,] map)
    {
        var texture = new Texture2D(96, 64, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point
        };
        texture.Apply(false, false);
        for (int x = 1; x <= texture.width; x++)
        {
            for (int z = 1; z <= texture.height; z++)
            {
                int height = map[x, z];
                int height1 = map[x - 1, z + 1];
                int height2 = map[x - 1, z];
                int height3 = map[x, z + 1];
                var block = generator.GetBlockAt(x, height - 1, z);
                var color = GetBlockColor(block);
                if (height1 > height || height2 > height || height3 > height)
                {
                    Color.RGBToHSV(color, out float h, out float s, out float v);
                    color = Color.HSVToRGB(h, s, v * 0.9f);
                }
                texture.SetPixel(x - 1, z - 1, color);
            }
        }
        texture.Apply(false, true);
        return texture;
    }

    private Texture2D GetGrayscaleTexture(BaseTerrainGenerator generator, int[,] map, int minValue, int maxValue)
    {
        var texture = new Texture2D(96, 64, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point
        };
        texture.Apply(false, false);
        for (int x = 1; x <= texture.width; x++)
        {
            for (int z = 1; z <= texture.height; z++)
            {
                var height = map[x, z];
                var gray = (height - minValue) / (float)(maxValue - minValue);
                var block = generator.GetBlockAt(x, height - 1, z);
                //var color = GetBlockColor(block);
                var color = new Color(gray, gray, gray);
                texture.SetPixel(x - 1, z - 1, color);
            }
        }
        texture.Apply(false, true);
        return texture;
    }

    private void StartMap()
    {
        menuController.StartMap(int.Parse(SeedField.text), (int)MountainSlider.value, (int)ForestSlider.value);
    }

    private Color GetBlockColor(BlockType block)
    {
        switch(block)
        {
            case BlockType.Stone:
                return Color.gray;
            case BlockType.Dirt:
                return Color.yellow;
            case BlockType.Grass:
                return Color.green;
            case BlockType.ForestGrass:
                return Color.Lerp(Color.green, Color.black, 0.3f);
            default:
                return Color.magenta;
        }
    }
}
