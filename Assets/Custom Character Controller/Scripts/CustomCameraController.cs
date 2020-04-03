using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomCharacter
{
    public class CustomCameraController : MonoBehaviour
    {
        public Transform pivot;
        public Camera cam;

        [Range(0, 1)]
        public float rotationSmoothness;

        [Range(0, 1)]
        public float positionSmoothness;

        private Quaternion originRot;
        private Quaternion pivotRot;

        public Quaternion rotation
        {
            get
            {
                return originRot;
            }
            set
            {
                originRot = value;
            }
        }

        private void Start()
        {
            originRot = transform.localRotation;
            pivotRot = pivot.localRotation;

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }


        public void Rotate(float x, float y)
        {
            originRot = Quaternion.Lerp(originRot, originRot * Quaternion.Euler(0, x, 0), rotationSmoothness);
            pivotRot = Quaternion.Lerp(pivotRot, pivotRot * Quaternion.Euler(y, 0, 0), rotationSmoothness);

            transform.localRotation = originRot;
            pivot.localRotation = pivotRot;
        }

        public void UpdatePos(Vector3 pos, bool smooth)
        {
            if (smooth)
                transform.position = Vector3.Lerp(transform.position, pos, positionSmoothness);
            else
                transform.position = pos;
        }

        public void UpdateDistanceFromOrigin(float d)
        {
            cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition, new Vector3(0, 0, d), positionSmoothness);
        }
    }
}
