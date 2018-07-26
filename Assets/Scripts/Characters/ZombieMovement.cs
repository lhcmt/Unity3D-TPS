using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class ZombieMovement : MonoBehaviour {

    Animator animator;
    CharacterController characterContoller;

    [System.Serializable]
    public class AnimationSettings
    {
        public string verticalVelocityFloat = "Forward";
    }
    [SerializeField]
    public AnimationSettings animations;

    [System.Serializable]
    public class MovementSettings
    {
        public float moveSpeed = 1f;
        public float margin = 0.1f;
    }
    [SerializeField]
    public MovementSettings movement;
    [System.Serializable]
    public class PhysicsSettings
    {
        public float gravityModfier = 9.81f;
        public float resetDownSpeed = 1.2f;
    }
    [SerializeField]
    public PhysicsSettings physics;

    bool resetGravity;
    float DownSpeed;//下落加速度
    bool isGrounded;
    void Awake()
    {
        animator = GetComponent<Animator>();
        SetupAnimator();
    }
	void Start () {
        characterContoller = GetComponent<CharacterController>();
	}
    void Update()
    {
        ApplyGravity();
        //这个比自己的好
        isGrounded = characterContoller.isGrounded;

    }

    public void AnimateAndMove(float forward, float strafe)
    {
        animator.SetFloat(animations.verticalVelocityFloat, forward);
        Vector3 direction = new Vector3(0,0,forward);
        direction = transform.TransformDirection(direction);
        direction *= movement.moveSpeed * Time.deltaTime;
        characterContoller.Move(direction);
    }
    public void JustAnimate(float forward, float strafe)
    {
        animator.SetFloat(animations.verticalVelocityFloat, forward);

    }


    void ApplyGravity()
    {
        if (isGrounded)
        {
            DownSpeed = physics.resetDownSpeed;
        }
        else{//掉下去
            DownSpeed += Time.deltaTime * physics.gravityModfier;
        }
        Vector3 gravityVector = new Vector3();
        gravityVector.y = -DownSpeed;
        if (characterContoller !=null)
        {
            characterContoller.Move(gravityVector * Time.deltaTime);
        }

    }

    void SetupAnimator()
    {
        Animator wantedAnim = GetComponentsInChildren<Animator>()[1];
        Avatar wantedAvatar = wantedAnim.avatar;

        animator.avatar = wantedAvatar;
        Destroy(wantedAnim);

    }
}
