using UnityEngine;

namespace Assets.Scripts.CameraSettings
{
    [RequireComponent(typeof(Camera))]
    public class CameraMovement : MonoBehaviour
    {
        [SerializeField] private MovementType Type = MovementType.Free;
        [SerializeField] private Vector3 RotationCenter = default;

        public float RotationDistance = 10;

        private enum MovementType
        {
            Free,
            Radial
        }

        void Start()
        {
            Cursor.lockState = CursorLockMode.Confined;
        }

        void Update()
        {
            switch (Type)
            {
                case MovementType.Free:
                    FreeMovement();
                    break;
                case MovementType.Radial:
                    RadialMovement();
                    break;
            }
        }

        private void FreeMovement()
        {
            RotationCenter = transform.position + transform.forward * RotationDistance;
            var euler = transform.eulerAngles;

            if (Input.GetMouseButtonDown(1))
            {
                Cursor.lockState = CursorLockMode.Locked;
            }

            if (Input.GetMouseButton(1))
            {
                var rotationMovement = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
                rotationMovement *= Time.unscaledDeltaTime * 100;

                euler.y += rotationMovement.x;

                var rotX = euler.x - rotationMovement.y;
                if (rotX > 180) rotX = Mathf.Max(271, rotX);
                else rotX = Mathf.Min(89, rotX);
                euler.x = rotX;

                transform.eulerAngles = euler;
            }
            if (Input.GetMouseButtonUp(1))
            {
                Cursor.lockState = CursorLockMode.Confined;
            }

            var upDown = (Input.GetKey(KeyCode.Q) ? -1 : 0) + (Input.GetKey(KeyCode.E) ? 1 : 0);

            var linearMovement = Input.GetAxisRaw("Horizontal") * Vector3.right + Input.GetAxisRaw("Vertical") * Vector3.forward + upDown * Vector3.up;
            linearMovement = Quaternion.Euler(0, euler.y, 0) * linearMovement;

            transform.position = RotationCenter + transform.forward * (-RotationDistance) + linearMovement;
        }

        private void RadialMovement()
        {
            Vector2 aroundMove = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (Input.GetMouseButtonDown(1))
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
            if (Input.GetMouseButton(1))
            {
                aroundMove += new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
            }
            if (Input.GetMouseButtonUp(1))
            {
                Cursor.lockState = CursorLockMode.Confined;
            }
            aroundMove *= Time.unscaledDeltaTime * 100;

            RotationDistance = Vector3.Distance(RotationCenter, transform.position);

            var euler = transform.eulerAngles;
            euler.y += aroundMove.x;

            var rotX = euler.x - aroundMove.y;
            if (rotX > 180) rotX = Mathf.Max(271, rotX);
            else rotX = Mathf.Min(89, rotX);
            euler.x = rotX;

            transform.eulerAngles = euler;
            transform.position = RotationCenter + transform.forward * (-RotationDistance);
        }
    }
}
