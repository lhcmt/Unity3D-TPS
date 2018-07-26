using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class CharacterMovement : Photon.MonoBehaviour {

    Animator animator;
    CharacterController characterContoller;
    /*
     * AnimatorController参数
     */
    [System.Serializable]
    public class AnimationSettings
    {
        public string verticalVelocityFloat = "Forward";
        public string horizontalVelocityFloat = "Strafe";
        public string groundedBool = "isGrounded";
        public string jumpBool = "isJumping";
        public string sprintBool = "Sprint";

    }
    [SerializeField]
    public AnimationSettings animations;
    /*
     * 重力加速度，
     * 默认重力
     * 最初下落速度
     */
    [System.Serializable]
    public class PhysicsSettings
    {
        public float gravityModfier = 9.81f;
        public float resetDownSpeed = 1.2f;
    }
    [SerializeField]
    public PhysicsSettings physics;
    /*
     * 起跳速度
     * 跳跃时间
     */
    [System.Serializable]
    public class MovementSettings
    {
        public float jumpSpeed = 6f;
        public float jumpTime = 0.5f;
        public float margin = 0.1f;
    }

    [SerializeField]public MovementSettings movement;
    [HideInInspector] public bool isSprint{get;set;}
    bool resetGravity;
    float DownSpeed;//下落加速度
    bool isGrounded;
    bool jumping;
    void Awake()
    {
        animator = GetComponent<Animator>();
        SetupAnimator();
    }

	void Start () {

        characterContoller = GetComponent<CharacterController>();

	}
	
	void Update () {
        ApplyGravity();
        //这个比自己的好
        isGrounded = characterContoller.isGrounded;

	}
    //
    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, -Vector3.up, movement.margin);
    }  

    public void Jump()
    {
        if (jumping || animator.GetBool("isJumping")||isSprint)
        {
            return;
        }
            
        if (isGrounded)
        {
            jumping = true;
            StartCoroutine(StopJump());
        }
    }
    //过jumpTime秒后让状态jumping变回false
    IEnumerator StopJump()
    {
        yield return new WaitForSeconds(movement.jumpTime);
        jumping = false;
    }
    //在空中，重置下落速度，然后加速下落
    //gravityVector,jia速度矢量
    /*private void ApplyGravity()
    {
        if (isGrounded)
        {
            if(!resetGravity){
                gravity = physics.resetGravityValue;
                resetGravity = true;
            }
            gravity += Time.deltaTime * physics.gravityModfier;
        }
        else{
            gravity = physics.baseGravity;
            resetGravity = false;
        }
        Vector3 gravityVector = new Vector3();

        if (!jumping){
            gravityVector.y -= gravity;
        }
        else{
            gravityVector.y = movement.jumpSpeed;
        }

        characterContoller.Move(gravityVector * Time.deltaTime);
    }*/

    void ApplyGravity()
    {
        if(isGrounded)
        {
            DownSpeed = physics.resetDownSpeed ;
        }
        else
        {
            DownSpeed += Time.deltaTime * physics.gravityModfier;
        }
        Vector3 gravityVector = new Vector3();
        if (!jumping)
        {
            gravityVector.y = -DownSpeed;
        }
        else
        {
            gravityVector.y = movement.jumpSpeed - DownSpeed;
        }

        characterContoller.Move(gravityVector * Time.deltaTime);
    }


    //Animates the character and root motion handles the movement
    //使用root motion
    //如果手动移动 move player
    //不能使用transform.position+ ，characterController的碰撞会失效
    public void Animate(float forward,float strafe){
        animator.SetFloat(animations.verticalVelocityFloat, forward);
        animator.SetFloat(animations.horizontalVelocityFloat, strafe);
        animator.SetBool(animations.groundedBool, isGrounded);
        animator.SetBool(animations.jumpBool, jumping);
        animator.SetBool(animations.sprintBool, isSprint);
    }


    /*
     * 从child中获取avatar
     */
    void SetupAnimator(){
        Animator wantedAnim = GetComponentsInChildren<Animator>()[1];
        Avatar wantedAvatar = wantedAnim.avatar;

        animator.avatar = wantedAvatar;
        Destroy(wantedAnim);

    }


}
