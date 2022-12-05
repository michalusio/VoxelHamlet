using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Utilities
{
    public class InstanceBatcher
    {
        public Mesh Mesh;
        public Material[] Materials;

        private readonly List<List<Matrix4x4>> Positions;
        private readonly List<MaterialPropertyBlock> PropertyBlocks;

        public InstanceBatcher()
        {
            Positions = new List<List<Matrix4x4>>();
            PropertyBlocks = new List<MaterialPropertyBlock>();
        }

        public void Rebatch(List<Matrix4x4> positions)
        {
            PropertyBlocks.Clear();

            var positionListIndex = 0;
            var actualPositionList = positionListIndex < Positions.Count ? Positions[positionListIndex] : new List<Matrix4x4>(1024);
            if (positionListIndex >= Positions.Count) Positions.Add(actualPositionList);
            actualPositionList.Clear();

            for (int i = 0; i < positions.Count; i++)
            {
                actualPositionList.Add(positions[i]);
                if ((i % 1022) == 0 && i > 0)
                {
                    positionListIndex++;
                    actualPositionList = positionListIndex < Positions.Count ? Positions[positionListIndex] : new List<Matrix4x4>(1024);
                    if (positionListIndex >= Positions.Count) Positions.Add(actualPositionList);
                    actualPositionList.Clear();
                }
            }

            Positions.RemoveRange(positionListIndex + 1,Positions.Count - positionListIndex - 1);
        }

        public void Rebatch(List<Matrix4x4> positions, string propertyName, List<Vector4> propertyValues)
        {
            PropertyBlocks.Clear();
            if (positions.Count != propertyValues.Count) throw new System.Exception("Batching lists must have the same size!");

            var positionListIndex = 0;
            var actualPositionList = positionListIndex < Positions.Count ? Positions[positionListIndex] : new List<Matrix4x4>(1024);
            if (positionListIndex >= Positions.Count) Positions.Add(actualPositionList);
            actualPositionList.Clear();

            var actualPropertyList = new List<Vector4>(1024);
            for (int i = 0; i < positions.Count; i++)
            {
                actualPositionList.Add(positions[i]);
                actualPropertyList.Add(propertyValues[i]);
                if ((i % 1022) == 0 && i > 0)
                {
                    var block = new MaterialPropertyBlock();
                    block.SetVectorArray(propertyName, actualPropertyList);
                    PropertyBlocks.Add(block);

                    positionListIndex++;
                    actualPositionList = positionListIndex < Positions.Count ? Positions[positionListIndex] : new List<Matrix4x4>(1024);
                    if (positionListIndex >= Positions.Count) Positions.Add(actualPositionList);
                    actualPositionList.Clear();
                    actualPropertyList.Clear();
                }
            }
            if (actualPositionList.Count > 0)
            {
                var block = new MaterialPropertyBlock();
                block.SetVectorArray(propertyName, actualPropertyList);
                PropertyBlocks.Add(block);
            }

            Positions.RemoveRange(positionListIndex + 1, Positions.Count - positionListIndex - 1);
        }

        public void Render(int layer = 0)
        {
            if (PropertyBlocks.Count == 0)
            {
                for (int matIndex = 0; matIndex < Materials.Length; matIndex++)
                {
                    for (int i = 0; i < Positions.Count; i++)
                    {
                        Graphics.DrawMeshInstanced(Mesh, matIndex, Materials[matIndex], Positions[i], null, UnityEngine.Rendering.ShadowCastingMode.Off, false, layer);
                    }
                }
            }
            else
            {
                for (int matIndex = 0; matIndex < Materials.Length; matIndex++)
                {
                    for (int i = 0; i < Positions.Count; i++)
                    {
                        Graphics.DrawMeshInstanced(Mesh, matIndex, Materials[matIndex], Positions[i], PropertyBlocks[i], UnityEngine.Rendering.ShadowCastingMode.Off, false, layer);
                    }
                }
            }
        }
    }
}
