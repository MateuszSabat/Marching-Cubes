using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MarchingCubes;

namespace CustomCharacter
{
    [RequireComponent(typeof(CharacterController))]
    public class CustomCharacterController : CustomCharacter
    {
        #region Camera
        public enum Perspective { FirstPerson, ThirdPerson }
        [Header("Camera")]
        public Perspective perspective;

        public CustomCameraController cam;

        public Transform camFocusFP;
        public Transform camFocusTP;

        public float camDistance;

        public float cameraSpeed;

        #endregion
        #region Locomotion
        protected CharacterController controller;
        [Header("Locomotion")]
        public float mass;
        public Vector3 force;
        [Range(0, 1)]
        public float terrainDrag;
        [Range(0, 1)]
        public float airDrag;
        public float walkSpeed;
        public float runSpeed;
        public float acceleration;
        public float turnAcceleration;

        protected float speed;

        protected Vector3 inputVelocity;
        protected Vector3 addVelocity;

        protected Vector3 velocity;

        public float angularSpeed;

        public float jumpForce;
        public float jumpInterval;
        public float jumpStamina;
        public bool canJump;

        #endregion
        #region Action
        public enum ActionType { Default, SculptTerrain}
        [Header("Action")]
        public ActionType actionType;

        public MarchingCubes.Mesh MCMesh;

        public float sculptingDistance;
        public LayerMask sculptMask;

        public PlayerBrushDig brushDig;
        public PlayerBrushAdd brushAdd;
        [Range(0, 1)]
        public float addSubstanceForce;

        public GameObject buildCrosshair;
        #endregion

        #region Start Region
        private void Start()
        {
            actionType = ActionType.Default;
            buildCrosshair.SetActive(false);
            canJump = true;
            controller = GetComponent<CharacterController>();
            speed = walkSpeed;

            InitStats();
            InitBrush();
        }

        void InitStats()
        {
            stats._health = new BarStat("health", 0, 100, 100);
            stats._stamina = new BarStat("stamina", 0, 100, 100);
        }

        void InitBrush()
        {
            brushDig.MCMesh = MCMesh;
            brushAdd.MCMesh = MCMesh;

            brushDig.forces = SubstanceTable.digForces;
            brushAdd.set = addSubstanceForce;
        }
        #endregion

        private void Update()
        {
            Debug.Log(controller.isGrounded);
            UpdateCamera();
            GetInput();
            UpdateVelocity();
            UpdateTransform();
            ChangeActionType();
            Action();
        }

        private void FixedUpdate()
        {
            FixedUpdateAddVelocity();
        }

        #region Camera
        void UpdateCamera()
        {
            if (Input.GetKeyDown(KeyCode.V))
            {
                if (perspective == Perspective.FirstPerson)
                    perspective = Perspective.ThirdPerson;
                else
                    perspective = Perspective.FirstPerson;
            }

            float x = Input.GetAxis("Mouse X") * cameraSpeed * Time.fixedDeltaTime;
            float y = Input.GetAxis("Mouse Y") * cameraSpeed * Time.fixedDeltaTime;

            cam.Rotate(x, -y);

            if (perspective == Perspective.FirstPerson)
            {
                cam.UpdatePos(camFocusFP.position, perspective == Perspective.ThirdPerson);
                cam.UpdateDistanceFromOrigin(0);
            }
            else
            {
                cam.UpdatePos(camFocusTP.position, perspective == Perspective.ThirdPerson);
                cam.UpdateDistanceFromOrigin(camDistance);
            }
        }
        #endregion
        #region Locomotion
        void GetInput()
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                stats.StartRun();
                speed = runSpeed;
            }
            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                stats.StopRun();
                speed = walkSpeed;
            }

        }

        bool CanJump()
        {
            if (canJump)
                return stats.CanUseStamina(jumpStamina);
            else
                return false;
        }

        IEnumerator Jump()
        {
            Debug.Log("JUMP");
            canJump = false;
            force += Vector3.up * jumpForce;

            yield return new WaitForSeconds(jumpInterval);
            canJump = true;
        }

        void FixedUpdateAddVelocity()
        {
            float drag;
            if (controller.isGrounded)
            {
                drag = terrainDrag;
            }
            else
            {
                force += Physics.gravity * mass;
                drag = airDrag;
            }

            addVelocity += force / mass * Time.fixedDeltaTime;
            force = Vector3.zero;
            addVelocity *= 1 - drag;
        }

        void UpdateVelocity()
        {
            if (controller.isGrounded)
            {
                float f = Input.GetAxis("Vertical") * speed;
                float r = Input.GetAxis("Horizontal") * speed;

                if (f != 0 || r != 0)
                    tendToMove = true;
                else
                    tendToMove = false;

                if (f != 0 && r != 0)
                {
                    f *= 0.7071f;
                    r *= 0.7071f;
                }

                Vector3 newV = cam.transform.forward * f + cam.transform.right * r;

                inputVelocity = Vector3.MoveTowards(inputVelocity, newV, acceleration * Time.fixedDeltaTime + turnAcceleration * Vector3.Cross(inputVelocity.normalized, newV.normalized).sqrMagnitude);


                if (Input.GetKeyDown(KeyCode.Space))
                    if (CanJump())
                        StartCoroutine("Jump");
            }

            velocity = inputVelocity + addVelocity;
        }

        void UpdateTransform()
        {
            controller.Move(velocity * Time.deltaTime);

            if (perspective == Perspective.FirstPerson)
            {
                transform.rotation = cam.rotation;
            }
            else
            {
                if (tendToMove)
                {
                    Quaternion lookRot = Quaternion.LookRotation(inputVelocity, Vector3.up);
                    Quaternion newRot = Quaternion.RotateTowards(transform.rotation, lookRot, angularSpeed * Time.deltaTime);
                    transform.rotation = newRot;
                }
            }
        }


        public override void RunOutOffStaminaCall()
        {
            speed = walkSpeed;
        }

        #endregion
        #region Action

        void ChangeActionType()
        {
            if (Input.GetMouseButtonDown(2))
            {
                if (actionType == ActionType.Default)
                {
                    actionType = ActionType.SculptTerrain;
                    buildCrosshair.SetActive(true);
                }
                else
                {
                    actionType = ActionType.Default;
                    buildCrosshair.SetActive(false);
                }
            }
        }
        void Action()
        {
            if (Input.GetMouseButton(0))
            {
                if(actionType == ActionType.SculptTerrain)
                {
                    Ray ray = cam.cam.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f));
                    if(Physics.Raycast(ray, out RaycastHit hit, sculptingDistance, sculptMask))
                    {
                        if(hit.transform.parent == MCMesh.transform)
                        {
                            brushDig.Paint(hit.point);
                        }
                    }
                }
            }
            if (Input.GetMouseButton(1))
            {
                if(actionType == ActionType.SculptTerrain)
                {
                    Ray ray = cam.cam.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f));
                    if (Physics.Raycast(ray, out RaycastHit hit, sculptingDistance, sculptMask))
                    {
                        if (hit.transform.parent == MCMesh.transform)
                        {
                            brushAdd.Paint(hit.point);
                        }
                    }
                }
            }
        }
        #endregion
    }
}
