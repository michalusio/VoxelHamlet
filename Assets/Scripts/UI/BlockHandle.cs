using Assets.Scripts.ConfigScripts;
using Assets.Scripts.Utilities;
using System;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public class BlockHandle
    {
        [Flags]
        public enum HandleType
        {
            X = 1,
            Y = 2,
            Z = 4,
            NegX = 8,
            NegY = 16,
            NegZ = 32,
            All = X | Y | Z | NegX | NegY | NegZ
        }

        private Matrix4x4 GetTRS()
        {
            switch (Type)
            {
                default:
                    return Matrix4x4.TRS(Position, Quaternion.identity, Vector3.one);
                case HandleType.Z:
                    return Matrix4x4.TRS(Position, Quaternion.AngleAxis(90, Vector3.up), Vector3.one);
                case HandleType.Y:
                    return Matrix4x4.TRS(Position, Quaternion.AngleAxis(90, Vector3.right), Vector3.one);
                case HandleType.NegX:
                    return Matrix4x4.TRS(Position, Quaternion.AngleAxis(180, Vector3.up), Vector3.one);
                case HandleType.NegZ:
                    return Matrix4x4.TRS(Position, Quaternion.AngleAxis(270, Vector3.up), Vector3.one);
                case HandleType.NegY:
                    return Matrix4x4.TRS(Position, Quaternion.AngleAxis(270, Vector3.right), Vector3.one);
            }
        }

        public bool Grabbed;

        private readonly HandleType Type;
        public int PrevValue { get; private set; }
        private readonly Func<int, Vector3> OnValueChange;
        protected Matrix4x4 Matrix;
        private Vector3 Position;
        private Vector3 StartPosition;
        protected readonly Material handleMaterial;

        public BlockHandle(HandleType type, int prevValue, Func<int, Vector3> onValueChange, Vector3 position)
        {
            Type = type;
            PrevValue = prevValue;
            OnValueChange = onValueChange;
            Position = position;
            StartPosition = Position;
            Matrix = GetTRS();
            handleMaterial = new Material(GlobalSettings.Instance.HandleMaterial);
        }

        public void ReInit(int prevValue, Vector3 position)
        {
            PrevValue = prevValue;
            Position = position;
            StartPosition = Position;
            Matrix = GetTRS();
        }

        public virtual void Draw()
        {
            handleMaterial.color = Grabbed ? Color.cyan : Color.white;
            Graphics.DrawMesh(GlobalSettings.Instance.HandleMesh, Matrix, handleMaterial, 0);
        }

        public bool HandleGrab(Ray ray)
        {
            Grabbed = new Bounds(Position, Vector3.one).IntersectRay(ray);
            return Grabbed;
        }

        public void Move(Ray mouseRay)
        {
            Vector3 direction = default;
            switch (Type)
            {
                case HandleType.NegX:
                case HandleType.X:
                    direction = Vector3.right;
                    break;
                case HandleType.NegY:
                case HandleType.Y:
                    direction = Vector3.up;
                    break;
                case HandleType.NegZ:
                case HandleType.Z:
                    direction = Vector3.forward;
                    break;
            }
            var handleRay = new Ray(StartPosition, direction);

            var r1dr2d = Vector3.Dot(mouseRay.direction, handleRay.direction);


            var handleT = (Vector3.Dot(mouseRay.origin - handleRay.origin, handleRay.direction) + Vector3.Dot(handleRay.origin - mouseRay.origin, mouseRay.direction) * r1dr2d) / (1 - r1dr2d * r1dr2d);
            var newPoint = handleRay.GetPoint(Mathf.Round(handleT));
            Position = newPoint;
            Matrix = GetTRS();
        }

        public void Confirm()
        {
            Vector3 direction = default;
            switch (Type)
            {
                case HandleType.NegX:
                    direction = -Vector3.right;
                    break;
                case HandleType.X:
                    direction = Vector3.right;
                    break;
                case HandleType.NegY:
                    direction = -Vector3.up;
                    break;
                case HandleType.Y:
                    direction = Vector3.up;
                    break;
                case HandleType.NegZ:
                    direction = -Vector3.forward;
                    break;
                case HandleType.Z:
                    direction = Vector3.forward;
                    break;
            }
            var dist = Mathf.RoundToInt(Vector3.Distance(StartPosition, Position));
            var sign = Mathf.Sign(Vector3.Dot(Position - StartPosition, direction));
            var newValue = PrevValue + Mathf.RoundToInt(sign * dist);
            Grabbed = false;
            Position = OnValueChange(newValue);
            Matrix = GetTRS();
        }
    }

    public class AreaBlockHandle: BlockHandle
    {
        public AreaBlockHandle(HandleType type, int prevValue, Func<int, Vector3> onValueChange, Vector3 position) : base(type, prevValue, onValueChange, position) { }
        public override void Draw()
        {
            handleMaterial.color = Grabbed ? Color.cyan : Color.white;
            Graphics.DrawMesh(GlobalSettings.Instance.AreaHandleMesh, Matrix, handleMaterial, 0);
        }
    }
}