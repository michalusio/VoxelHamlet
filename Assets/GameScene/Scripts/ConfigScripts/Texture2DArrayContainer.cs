using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.ConfigScripts
{
    [CreateAssetMenu(fileName = "Texture2DContainerDB", menuName = "Texture2DContainer", order = 4)]
    public class Texture2DArrayContainer : ScriptableObject
    {
        public int MIP_COUNT = 4;
        public List<Texture2D> AlbedoTextures;

        private Texture2DArrayCreator AlbedoCreator;

        public Texture2DArray GetAlbedoArray()
        {
            if (!AlbedoCreator)
            {
                AlbedoCreator = CreateInstance<Texture2DArrayCreator>();
                AlbedoCreator.MIP_COUNT = MIP_COUNT;
                AlbedoCreator.Textures = AlbedoTextures;
            }
            return AlbedoCreator.GetTexture2DArray();
        }
    }
}
