using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]//fangbiantiaoshi
public class CameraRig : Photon.MonoBehaviour {

    public Transform target;//player gameobject
    public bool autoTargetPlayer;
    public LayerMask wallLayers;
    UserInput userInput;
    //左右肩模式
    public enum Shoulder
    {
        Right,Left
    }
    public Shoulder shoulder;

    /*
     * camera 参数
     * 第一个为左右肩模式下相对于pivot的偏移参数
     */
    [System.Serializable]
    public class CameraSettings
    {
        [Header("Position")]
        public Camera UIcamera;
        public Vector3 camPositionOffsetLeft;
        public Vector3 camPositionOffsetRight;

        public Vector3 camAimPositionOffsetLeft;
        public Vector3 camAimPositionOffsetRight;

        [Header("Camera Options")]
        public float mouseXSensivity = 5.0f;
        public float mouseYSensivity = 5.0f;
        public float minAngle = -30.0f;
        public float maxAngle = 60.0f;
        public float rotationSpeed = 5.0f;

        [Header("Zoom")]
        public float fieldOfView = 70.0f;
        public float zoomFieldOfView = 30.0f;
        public float zoomSpeed = 4.0f;

        [Header("Visual Options")]
        public float hideMeshWhenDistance = 0.5f;
    }
    [SerializeField]
    public CameraSettings cameraSettings;
    /*
     * 按键的名字
     */
    [System.Serializable]
    public class InputSettings
    {
        public string verticalAxis = "Mouse X";
        public string horizontalAxis = "Mouse Y";
        public string switchShoulderButton = "Fire4";
    }
    [SerializeField]
    public InputSettings input;
    //cameraRig的移动设定
    [System.Serializable]
    public class MovementSettings
    {
        public float movementLerpSpeed = 5.0f;
    }
    [SerializeField]
    public MovementSettings movement;

    Transform pivot;
    Camera mainCamera;
    
    float newX = 0.0f;//鼠标输入参数
    float newY = 0.0f;//鼠标输入参数


	void Start () {
        GameObject player = GameController.LocalPlayerInstance;
        if (player)
            userInput = player.GetComponent<UserInput>();
        mainCamera = Camera.main;
        pivot = transform.GetChild(0);//要求pivot是CameraRig的第一个子物体
	}
	

	void Update () {
        if (!userInput)
        {
            GameObject player = GameController.LocalPlayerInstance;
            if (player)
                userInput = player.GetComponent<UserInput>();
        }

		if(target)
        {
            if (Application.isPlaying && !UserInput.isMouseOnUI)
            {
                RotateCamera();

                CheckMeshRenderer();
                Zoom(userInput.aiming);
                CheckWall();
                if(Input.GetButtonDown(input.switchShoulderButton))
                {
                    SwitchShoulder();
                }
            }
        }
	}
    //camera follow player target
    void LateUpdate()
    {
        if(!target)
        {
            TargetPlayer();
        }
        else
        {
            Vector3 targetPosition = target.position;
            Quaternion targetRotaition = target.rotation;

            FollowTarget(targetPosition, targetRotaition);
        }
    }

    //follow the target with Time.DeltaTime smooothly
    void FollowTarget(Vector3 targetPosition,Quaternion targetRotaition){
        if(!Application.isPlaying)
        {
            transform.position = targetPosition;
            transform.rotation = targetRotaition;
        }
        else
        {
            Vector3 newPos = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * movement.movementLerpSpeed);
            transform.position = newPos;
        }
    }

    //找到player GameObject ,并赋值给target
    void TargetPlayer()
    {
        if (autoTargetPlayer)
        {
            GameObject player = null;
            if (PhotonNetwork.connected == false)
            {
                player = GameObject.FindGameObjectWithTag("Player"); ;
            }
            else
            {
                GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
                foreach (var pl in players)
                {
                    if (pl.GetPhotonView().isMine)
                    {
                        player = pl;
                        break;
                    }
                }
            }
            if (player)
            {
                Transform playerT = player.transform;
                target = playerT;
            }
        }
    }

    //rotate camera with input
    void RotateCamera()
    {
        if (!pivot)
            return;
        newX += cameraSettings.mouseXSensivity * Input.GetAxis(input.verticalAxis);
        newY += cameraSettings.mouseYSensivity * Input.GetAxis(input.horizontalAxis);

        Vector3 eulerAngleAxis = new Vector3();
        eulerAngleAxis.x = -newY;//并没有弄反
        eulerAngleAxis.y = newX;
        //limit range
        newX = Mathf.Repeat(newX, 360);
        newY = Mathf.Clamp(newY, cameraSettings.minAngle, cameraSettings.maxAngle);
        //从a角度转向b，经过t时间
        Quaternion newRotation = Quaternion.Slerp(pivot.localRotation, Quaternion.Euler(eulerAngleAxis), Time.deltaTime * cameraSettings.rotationSpeed);

        pivot.localRotation = newRotation;
    }

   //当摄像机靠近墙体时,前移摄像机，防止穿墙
    void CheckWall()
    {
        if (!pivot || !mainCamera)
            return;
        RaycastHit hit;
        Transform mainCamT = mainCamera.transform;
        Vector3 mainCamPos = mainCamT.position;
        Vector3 pivotPos = pivot.position;

        Vector3 start = pivotPos;
        Vector3 dir = mainCamPos - pivotPos;//指向mainCam背面
        //z距离
        float dist = Mathf.Abs(shoulder == Shoulder.Left ? cameraSettings.camPositionOffsetLeft.z : cameraSettings.camPositionOffsetRight.z);

        if (Physics.SphereCast(start, mainCamera.nearClipPlane, dir, out hit, dist, wallLayers))
        {
            MoveCamUp(hit,pivotPos,dir,mainCamT);
        }
        else
        {
            switch(shoulder)
            {
                case Shoulder.Left:
                    PositionCamera(cameraSettings.camPositionOffsetLeft);
                    break;
                case Shoulder.Right:
                    PositionCamera(cameraSettings.camPositionOffsetRight);
                    break;
            }
        }
    }

    //当摄像机撞到墙时，前移Camera
    void MoveCamUp(RaycastHit hit,Vector3 pivotPos,Vector3 dir,Transform cameraT)
    {
        
        float hitDist = hit.distance;
        Vector3 sphereCastCenter = pivotPos + (dir.normalized * hitDist);//摄像机要移动到的位置
        cameraT.position = sphereCastCenter;
    }

    //当摄像机不在被墙体遮挡时，从当前位置，移动回cameraPos位置,使用插值方法，每帧调用
    void PositionCamera(Vector3 cameraPos)
    {
        if (!mainCamera)
            return;
        Transform mainCamT = mainCamera.transform;
        Vector3 mainCamPos = mainCamT.localPosition;
        Vector3 newPos = Vector3.Lerp(mainCamPos, cameraPos, Time.deltaTime * movement.movementLerpSpeed);
        mainCamT.localPosition = newPos;
    }

    //隐藏太近的character Mesh
    void CheckMeshRenderer()
    {
        if (!mainCamera || !target)
            return;
        SkinnedMeshRenderer[] meshs = target.GetComponentsInChildren<SkinnedMeshRenderer>();
        Transform mainCamT = mainCamera.transform;
        Vector3 mainCamPos = mainCamT.position;
        Vector3 targetPos = target.position;
        float dist = Vector3.Distance(mainCamPos,targetPos + target.up);

        if(meshs.Length > 0){
            for(int i =0; i<meshs.Length;i++)
            {
                if(dist <= cameraSettings.hideMeshWhenDistance)
                {
                    meshs[i].enabled = false;
                }
                else
                {
                    meshs[i].enabled = true;
                }
            }
        }
    }

    //通过缩放摄像机视角参数fieldOfView，起到ZOOM效果
    void Zoom(bool isZooming)
    {
        if (!mainCamera)
            return;
        if (isZooming)
        {

            float newFieldOfView = Mathf.Lerp(mainCamera.fieldOfView, cameraSettings.zoomFieldOfView, Time.deltaTime * cameraSettings.zoomSpeed);
            mainCamera.fieldOfView = newFieldOfView;
            //由于缩放子弹偏左，需要调整
            if(cameraSettings.UIcamera != null)
            {
                cameraSettings.UIcamera.fieldOfView = newFieldOfView;
            }
            switch (shoulder)
            {
                case Shoulder.Left:
                    PositionCamera(cameraSettings.camAimPositionOffsetLeft);
                    break;
                case Shoulder.Right:
                    PositionCamera(cameraSettings.camAimPositionOffsetRight);
                    break;
            }
        }
        else 
        {
            float originalFieldOfView = Mathf.Lerp(mainCamera.fieldOfView, cameraSettings.fieldOfView, Time.deltaTime * cameraSettings.zoomSpeed);
            mainCamera.fieldOfView = originalFieldOfView;
            //由于缩放子弹偏左，需要调整
            if (cameraSettings.UIcamera != null)
            {
                cameraSettings.UIcamera.fieldOfView = originalFieldOfView;
            }
            switch (shoulder)
            {
                case Shoulder.Left:
                    PositionCamera(cameraSettings.camPositionOffsetLeft);
                    break;
                case Shoulder.Right:
                    PositionCamera(cameraSettings.camPositionOffsetRight);
                    break;
            }
        }
    }

    void SwitchShoulder()
    {
        switch(shoulder)
        {
            case Shoulder.Left:
                shoulder = Shoulder.Right;
                break;
            case Shoulder.Right:
                shoulder = Shoulder.Left;
                break;
        }
    }
}
