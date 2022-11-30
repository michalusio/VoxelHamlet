using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Utilities
{
    public class Texture2DArrayCreator : ScriptableObject
    {
        public int MIP_COUNT = 4;
        public List<Texture2D> Textures;
        private Texture2DArray Array;

        public Texture2DArray GetTexture2DArray()
        {
            if (Array) return Array;

            var firstTexture = Textures.First(t => t);

            int width = firstTexture.width;
            int height = firstTexture.height;
            var format = firstTexture.format;

            if (Textures.Any(t => t && (t.width != width || t.height != height || t.format != format))) return null;

            Array = new Texture2DArray(width, height, Textures.Count, format, MIP_COUNT, false)
            {
                filterMode = FilterMode.Point,
                anisoLevel = 3
            };
            for (int i = 0; i < Textures.Count; i++)
            {
                if (!Textures[i]) continue;

                var pixels = Textures[i].GetPixels();
                var pixelWidth = width;
                var pixelHeight = height;

                Array.SetPixels(pixels, i, 0);

                for (int mip = 1; mip < MIP_COUNT; mip++)
                {
                    var pixelsMip = new Color[pixels.Length >> 2];

                    for (int y = 0; y < pixelHeight; y++)
                    {
                        for (int x = 0; x < pixelWidth; x++)
                        {
                            var pixelIndex = y * pixelWidth + x;
                            var mipIndex = (y >> 1) * (pixelWidth >> 1) + (x >> 1);
                            pixelsMip[mipIndex] += pixels[pixelIndex] / 4;
                        }
                    }

                    Array.SetPixels(pixelsMip, i, mip);

                    pixels = pixelsMip;
                    pixelWidth >>= 1;
                    pixelHeight >>= 1;
                }
            }
            Array.Apply(false, true);

            return Array;
        }
    }
}
