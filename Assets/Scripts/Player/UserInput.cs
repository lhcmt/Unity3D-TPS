using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInput : Photon.PunBehaviour ,IPunObservable{

    public CharacterMovement characterMove{get;protected set;}
    public WeaponHandler weaponHandler { get; protected set; }


    [System.Serializable]
    public class InputSettings
    {
        public string verticalAxis = "Vertical";
        public string horizontalAxis = "Horizontal";
        public string jumpButton = "Jump";
        public string reloadButton = "Reload";
        public string aimButton = "Fire2";
        public string fireButton = "Fire1";
        public string dropWeaponButton = "DropWeapon";
        public string switchWeaponButton = "SwitchWeapon";
        public string SprintButton = "Fire3";
        public string TaskButtun = "TaskMenu";
        public string Cancel = "Cancel";

    }
    [SerializeField]
    public InputSettings inputs;

    [System.Serializable]
    public class OtherSettings
    {
        public float lookSpeed = 5.0f;
        public float lookDistance = 10.0f;
        public bool requireInputForTurn = true;
        public LayerMask aimDetectionLayers;
    }
    [SerializeField]
    public OtherSettings other;

    public Camera TPSCamera;
    
    public bool debugAim;

    public Transform spine;
    public bool aiming { get; set; }

    //网络对象要同步参数
    bool isFireButtonDown;
    bool isReloadBottonDown;
    bool isDropBottonDown;
    bool isSweachBottonDown;
    bool isJumpButtonDown;
    bool isSprintButtonDown;
    float verticalAxis;
    float horizontalAxis;
    Quaternion newRotation;
    Vector3 spineLookat;

    [HideInInspector]
    public static bool isMouseOnUI = false;

    //修复跳跃问题
    bool canJump = true;
    #region MonoCallBack

    void Awake()
    {
        if (!PhotonNetwork.connected)
            GameController.LocalPlayerInstance = this.gameObject;
        // #Important
        // used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
        else
        {
            if (photonView.isMine)
            {
                GameController.LocalPlayerInstance = this.gameObject;
            }
            // #Critical
            // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
            DontDestroyOnLoad(this.gameObject);
        } 
    }
    void Start()
    {
        characterMove = GetComponent<CharacterMovement>();
        TPSCamera = Camera.main;
        weaponHandler = GetComponent<WeaponHandler>();
        LockCursor();
    }

    void Update()
    {
        if (photonView.isMine == false && PhotonNetwork.connected == true)
        {
            WeaponLogic();
            CharacterLogic();
            return;
        }
        HandleInput();
        GetSpineTransform();
        CharacterLogic();
        CameraLookLogic();
        WeaponLogic();
    }
    #endregion


    #region public Methods
    public static void LockCursor()
    {
        isMouseOnUI = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public static void UnLockCursor()
    {
        isMouseOnUI = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    #endregion


    void LateUpdate()
    {
        if(weaponHandler)
        {
            if(weaponHandler.currentWeapon)
            {
                if (aiming)
                    PositionSpine(spineLookat);
            }
        }
    }

    void HandleInput()
    {
        aiming = (Input.GetButton(inputs.aimButton) || debugAim) && !characterMove.isSprint && weaponHandler.currentWeapon;
        isFireButtonDown = Input.GetButton(inputs.fireButton);
        isReloadBottonDown = Input.GetButtonDown(inputs.reloadButton);
        isDropBottonDown = Input.GetButtonDown(inputs.dropWeaponButton);
        isSweachBottonDown = Input.GetButtonDown(inputs.switchWeaponButton);
        isJumpButtonDown = Input.GetButtonDown(inputs.jumpButton);
        isSprintButtonDown = Input.GetButton(inputs.SprintButton);
        verticalAxis = Input.GetAxis(inputs.verticalAxis);
        horizontalAxis = Input.GetAxis(inputs.horizontalAxis);
    }
    //处理角色移动的逻辑
    void CharacterLogic()
    {
        if ((!characterMove))
            return;
        if (isJumpButtonDown && canJump)
        {
            characterMove.Jump();
            canJump = false;
            StartCoroutine(CanJump());
        }
        characterMove.isSprint = isSprintButtonDown;
        characterMove.Animate(verticalAxis, horizontalAxis);
        //获取跳跃按键

        
    }
    IEnumerator CanJump()
    {
        yield return new WaitForSeconds(characterMove.movement.jumpTime+0.1f);
        canJump = true;
    }
    //处理摄像机逻辑
     void CameraLookLogic()
    {
        if (!TPSCamera)
            return;

        if (other.requireInputForTurn)
        {
            if (Input.GetAxis(inputs.horizontalAxis) != 0 || Input.GetAxis(inputs.verticalAxis) != 0)
            {
                CharacterLook();
            }
        }
        else
        {
            CharacterLook();
        }

    }
    //处理所有武器的逻辑

    void WeaponLogic()
     {
         if (!weaponHandler)
             return;
        if(weaponHandler.currentWeapon)
        {
           
            weaponHandler.Aim(aiming);
            other.requireInputForTurn = !aiming;
            //开火按钮
            weaponHandler.FIngerOnTriger(isFireButtonDown);

            if (isReloadBottonDown)
                weaponHandler.Reload();

            if (isDropBottonDown)
                weaponHandler.DropCurWeapon();
        }
        if (isSweachBottonDown)
            weaponHandler.SwitchWeapons();

        if (!weaponHandler.currentWeapon)
            return;
        //武器瞄准射线，从Camera向屏幕中央发射
        Vector2 v = new Vector2(Screen.width / 2, Screen.height / 2);
        weaponHandler.currentWeapon.shootRay = Camera.main.ScreenPointToRay(v);//new Ray(TPSCamera.transform.position, TPSCamera.transform.forward);
        
     }

    //make the character look at a forward from the camera
    void CharacterLook()
    {
        Transform mainCamT = TPSCamera.transform;
        Transform pivotT = mainCamT.parent;
        Vector3 pivotPos = pivotT.position;
        Vector3 lookTarget = pivotPos + (pivotT.forward * other.lookDistance);
        Vector3 thisPos = transform.position;
        Vector3 lookDir = lookTarget - thisPos;
        Quaternion lookRot = Quaternion.LookRotation(lookDir);

        lookRot.x = 0;
        lookRot.z = 0;

        newRotation = Quaternion.Lerp(transform.rotation, lookRot, Time.deltaTime * other.lookSpeed);
        transform.rotation = newRotation;
    }

    //瞄准时候使身体朝向有瞄准的摄像方向

    void GetSpineTransform()
    {
        if (!spine || !weaponHandler.currentWeapon || !TPSCamera)
            return;

        //RaycastHit hit;
        Transform mainCamT = TPSCamera.transform;
        Vector3 mainCamPos = mainCamT.position;
        Vector3 dir = mainCamT.forward;
        Ray ray = new Ray(mainCamPos, dir);
        spineLookat = ray.GetPoint(400f);
    }
    void PositionSpine(Vector3 spinelookat)
    {
        if (!spine || !weaponHandler.currentWeapon)
            return;

         //由于Spine的朝向和rootjni不一样，需要做个旋转，这模型很奇怪
        spine.LookAt(spinelookat);
        spine.localEulerAngles = spine.localEulerAngles + new Vector3(0, 0, -90f);

        Vector3 eulerAngleOffset = weaponHandler.currentWeapon.userSettings.spineRotation;
        spine.Rotate(eulerAngleOffset);
    }



    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(aiming);
            stream.SendNext(isFireButtonDown);
            stream.SendNext(isReloadBottonDown);
            stream.SendNext(isDropBottonDown);
            stream.SendNext(isSweachBottonDown);
            stream.SendNext(isJumpButtonDown);
            stream.SendNext(isSprintButtonDown);
            stream.SendNext(verticalAxis);
            stream.SendNext(horizontalAxis);
            stream.SendNext(newRotation);
            stream.SendNext(spineLookat);
        }
        else
        {
            // Network player, receive data
            this.aiming = (bool)stream.ReceiveNext();
            this.isFireButtonDown = (bool)stream.ReceiveNext();
            this.isReloadBottonDown = (bool)stream.ReceiveNext();
            this.isDropBottonDown = (bool)stream.ReceiveNext();
            this.isSweachBottonDown = (bool)stream.ReceiveNext();
            this.isJumpButtonDown = (bool)stream.ReceiveNext();
            this.isSprintButtonDown = (bool)stream.ReceiveNext();
            this.verticalAxis = (float)stream.ReceiveNext();
            this.horizontalAxis = (float)stream.ReceiveNext();
            this.transform.rotation = (Quaternion)stream.ReceiveNext();
            this.spineLookat = (Vector3)stream.ReceiveNext();
        }
    }
}
