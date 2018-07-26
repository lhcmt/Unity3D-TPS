using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
//巡逻,追逐攻击等所有循环都在这个脚本


[RequireComponent(typeof(ZombieMovement))]
[RequireComponent(typeof(Animator))]
public class ZombieAI : Photon.PunBehaviour {

    private NavMeshAgent navMeshAgent;
    //
    private ZombieMovement zombieMove {
        get { return GetComponent<ZombieMovement>(); }
        set { zombieMove = value; }
    }
    private Animator animator { 
        get { return GetComponent<Animator>();}
        set { animator = value;} }
    //自身状态
    private ZombieStats zombieStats{
        get { return GetComponent<ZombieStats>(); }
        set { zombieStats = value; }
    }

	public enum AIState
    {
        Patrol,Chasing,Eatting,Idel,Screaming
    }
    public AIState aiState;

    [System.Serializable]
    public class PatrolSettings
    {
        public WaypointBase[] waypoints;
    }
    public PatrolSettings patrolSettings;

    [System.Serializable]
    public class SightSettings
    {
        public LayerMask sightLayers;
        public float sightRange = 30f;
        public float fieldOfView = 80f;//视角的一半
        public float senseRange = 10f;
        public float eyeheight = 2.0f;
    }
    public SightSettings sight;

    [System.Serializable]
    public class AttackSettings
    {
        public float damage = 5f;
        public float attackRange = 3f;
        public float attackDelay = 1.5f;
        public float attackCooldown = 2f;
        public int attackType = 2;
    }
    public AttackSettings attackSettings;


    private float currentWaitTime = 5f;
    private int waypointIndex = 0;
    private Transform currentlookTransform;
    private bool walkingToDest; //IK
    private bool attacked =false;

    
    private float forward;

    private Transform target;//player
    private Vector3 targetLastKnownPosition;//丢失player后，最后位置
    private CharacterStats[] allCharacters;
    private Timer timer;
    private ZombieAudio zombieAudio;
    private bool screamed;

    void Start()
    {
        navMeshAgent = GetComponentInChildren<NavMeshAgent>();
        timer = GameObject.FindGameObjectWithTag("GameController").GetComponent<Timer>();
        zombieAudio = GetComponent<ZombieAudio>();
        if (navMeshAgent == null)
        {
            Debug.LogError("We need a navmesh to traverse the world with");
            enabled = false;
        }
        if (navMeshAgent.transform == this.transform)
        {
            Debug.LogError("The navmesh agent should be a child of the character");
            enabled = false;
        }

        navMeshAgent.speed = 0;
        navMeshAgent.acceleration = 0;
        sight.sightLayers = LayerMask.GetMask("Rigdoll");

        if(navMeshAgent.stoppingDistance == 0)
        {
            //Debug.Log("Auto settings stoppoing distance to 0.3f");
            navMeshAgent.stoppingDistance = 1.5f;
        }
        GetAllCharacter();
    }

    void Update()
    {
        navMeshAgent.transform.position = transform.position;
        //以后加个判断，zombie分区域管理，玩家进去某一区域后此区域僵尸才开始寻找目标
        GetAllCharacter();
        LookforTarget();

        //简单状态机使用Switch实现，
        switch(aiState)
        {
            case AIState.Patrol:
                Patrol();
                break;
            case AIState.Chasing:
                ChasingPlayer();
                break;
            case AIState.Idel:
                Idel();
                break;
            case AIState.Screaming:
                Screaming();
                break;
        }

     }
    //巡逻
    void Patrol()
    {
        if (!navMeshAgent.isOnNavMesh)
            return;
        if (patrolSettings.waypoints.Length == 0)
            return;

        navMeshAgent.SetDestination(patrolSettings.waypoints[waypointIndex].destination.position);

        LookAtPosition(navMeshAgent.steeringTarget);
        zombieAudio.PlayZombieWalkSound();
        //走到一个点，走向下一个点
        if(navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            walkingToDest = false; 
            forward = LerpSpeed(forward, 0, 15);//停下
            currentWaitTime -= Time.deltaTime;//等待
            if(patrolSettings.waypoints[waypointIndex].lookAtTarget != null)
            {
                currentlookTransform = patrolSettings.waypoints[waypointIndex].lookAtTarget;
            }
            if (currentWaitTime <= 0)
            {
                waypointIndex = (waypointIndex + 1) % patrolSettings.waypoints.Length;
                currentWaitTime = patrolSettings.waypoints[waypointIndex].waitTime;
            }

        }
        else//走向目标点
        {
            walkingToDest = true;
            forward = LerpSpeed(forward, 0.5f, 15);
            currentWaitTime = patrolSettings.waypoints[waypointIndex].waitTime;
        }
        zombieMove.AnimateAndMove(forward, 0);
    }

    //追逐玩家
    void ChasingPlayer()
    {
        if (target == null)
        {
            aiState = AIState.Patrol;
            return;
        }
        if (!navMeshAgent.isOnNavMesh)
            return;
        if (patrolSettings.waypoints.Length == 0)
            return;
        navMeshAgent.SetDestination(target.position);

        LookAtPosition(navMeshAgent.steeringTarget);

        zombieAudio.PlayZombieWalkSound();
        //走到玩家面前了
        if (navMeshAgent.remainingDistance <= attackSettings.attackRange-1)
        {
            walkingToDest = false;
            forward = 0;
            Attacking();
        }
        else
        {
            walkingToDest = true;
            forward = LerpSpeed(forward, 1f, 15);
            animator.SetBool("Attack", false);
        }
        zombieMove.JustAnimate(forward, 0);
    }
    //攻击
    void Attacking()
    {
        if(attacked ==false && target!= null)
        {
            //print("Attack!");
            int attackType = Random.Range(0, attackSettings.attackType);
            animator.SetInteger("attackType", attackType);
            animator.SetBool("Attack", true);         
            attacked = true;
            //sound在动画里
            timer.Add(()=>{attacked =false;},attackSettings.attackCooldown);
            //延迟后进行判定，最好放在动画里
            //timer.Add(AttackJudge, attackSettings.attackDelay);
            //已经放到动画的motion里面去了
        }
        else
        {
            
        }
    }
    public void AttackJudge()
    {
        RaycastHit hit;
        Vector3 start = transform.position + transform.up;
        Vector3 dir = (target.transform.position + target.transform.up) - start;
        if (Physics.Raycast(start, dir, out hit, attackSettings.attackRange, sight.sightLayers))
        {
            if (hit.collider.GetComponent<CharacterStats>())
            {
                CharacterStats c = hit.collider.GetComponent<CharacterStats>();
                c.ApplyDamage(attackSettings.damage);
            }
        }
    }

    //发呆
    void Idel()
    {
        walkingToDest = false;
        forward = LerpSpeed(forward, 0, 5);//停下
    }
    //吼叫！没用了
    void Screaming()
    {
        walkingToDest = false;
        forward = 0;//停下
    }

    void GetAllCharacter()
    {
        //获取所有玩家对象
        allCharacters = GameObject.FindObjectsOfType<CharacterStats>();
    }

    //寻找目标
    void LookforTarget()
    {
        if (allCharacters.Length > 0)
        {

            CharacterStats c = ClosestEnemy();
            Vector3 start = transform.position + (transform.up * sight.eyeheight);
            Vector3 dir = (c.transform.position + c.transform.up) - start;
            //zombie和玩家的距离
            float distance = Vector3.Distance(c.transform.position, start);
            //zombie 正前方和玩家的夹角
            float sightAngle = Vector3.Angle(dir, transform.forward);
            //在视野范围内
            if (sightAngle < sight.fieldOfView && distance < sight.sightRange || distance <sight.senseRange)
            {
                target = c.transform;
                targetLastKnownPosition = Vector3.zero;
                
                if(screamed)//只吼一次
                {
                    aiState = AIState.Chasing;
                    animator.SetBool("Scream", false);
                }
                else
                {
                    aiState = AIState.Screaming;
                    LookAtPosition(target.position);
                    screamed = true;
                    animator.SetBool("Scream", true);
                } 
            }
            else
            {
                if (target != null)
                {
                    targetLastKnownPosition = target.position;
                }
                target = null;                
            }       
        }
    }
    //寻找最近的player
    CharacterStats ClosestEnemy()
    {
        CharacterStats closestCharacter = null;
        float minDistance = Mathf.Infinity;
        Vector3 currentPos = transform.position;
        foreach (CharacterStats c in allCharacters)
        {
            float distToCharacter = Vector3.Distance(c.transform.position, transform.position);
            if (distToCharacter < minDistance)
            {
                closestCharacter = c;
                minDistance = distToCharacter;
            }
        }

        return closestCharacter;
    }

    float LerpSpeed(float curSpeed,float destSpeed,float time)
    {
        return curSpeed = Mathf.Lerp(curSpeed, destSpeed, Time.deltaTime * time);
    }

    void LookAtPosition(Vector3 pos)
    {
        Vector3 dir = pos - transform.position;
        Quaternion lookRot = Quaternion.LookRotation(dir);
        lookRot.x = 0;
        lookRot.z = 0;

        transform.rotation = Quaternion.Lerp(transform.rotation, lookRot, Time.deltaTime * 5);
    }
    //
    void OnAnimatorIK()
    {
        if(currentlookTransform !=null)
        {
            if(!walkingToDest)
            {
                animator.SetLookAtPosition(currentlookTransform.position);
                animator.SetLookAtWeight(1, 0, 0.5f, 0.7f);
            }
            else
            {
                animator.SetLookAtWeight(0);
            }
        }
    }



    
}

[System.Serializable]
public class WaypointBase
{
    public Transform destination;//巡逻点
    public float waitTime;
    public Transform lookAtTarget;
}
