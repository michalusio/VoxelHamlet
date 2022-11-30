using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.CameraSettings
{
    public class SetUpCameras : MonoBehaviour
    {
        public List<Camera> Cameras;

        void Start()
        {
            foreach (var cam in Cameras)
            {
                cam.clearStencilAfterLightingPass = true;
            }
        }
    }
}