using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
//巡逻,追逐攻击等所有循环都在这个脚本


[RequireComponent(typeof(ZombieMovement))]
[RequireComponent(typeof(Animator))]
public class ZombieAI : Photon.PunBehaviour
{
    #region public variable
    /*
    public enum AIState
    {
        Patrol,
        Chasing,
        Eatting,
        Idel,
        Screaming
    }
    public AIState aiState;*/
    //静态保存几个状态
    [HideInInspector]
    public PatrolState partolState;
    [HideInInspector]
    public ChasingState chasingState;


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

    [HideInInspector]
    public NavMeshAgent navMeshAgent;
    [HideInInspector]
    public ZombieAudio zombieAudio;
    [HideInInspector]
    public bool walkingToDest; //IK 已经忘了当时为什么要写这个了
    [HideInInspector]
    public float forward; //前进速度，在各个状态之间共享
    [HideInInspector]
    public Transform target;//追逐的player对象


    #endregion

    #region private variable
    //
    private ZombieMovement zombieMove
    {
        get { return GetComponent<ZombieMovement>(); }
        set { zombieMove = value; }
    }
    private Animator animator
    {
        get { return GetComponent<Animator>(); }
        set { animator = value; }
    }
    //自身状态
    private ZombieStats zombieStats
    {
        get { return GetComponent<ZombieStats>(); }
        set { zombieStats = value; }
    }

    private Transform currentlookTransform; //当前监视方向
    private Vector3 targetLastKnownPosition;//丢失player后，player最后位置
    private CharacterStats[] allCharacters;
    private ZombieAIState zombieState;//基类对象

    #endregion

    void Start()
    {
        zombieAudio = GetComponent<ZombieAudio>();
        //初始化navMeshAgent
        navMeshAgent = GetComponentInChildren<NavMeshAgent>();
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
        InitializeAIState();
        //获取所有玩家对象
        GetAllCharacter();
    }

    void Update()
    {
        navMeshAgent.transform.position = transform.position;
        //以后加个判断，zombie分区域管理，玩家进去某一区域后此区域僵尸才开始寻找目标
        GetAllCharacter();
        LookforTarget();
        //联机状态下
        //只有主机执行场景中的zombieAI,其他客户端同步主机的ZombieAI 位置，动画
        if (PhotonNetwork.connected && !PhotonNetwork.isMasterClient)
        {
            return;
        }
        zombieState.AIbehavior();

     }

    //获取所有玩家对象
    void GetAllCharacter()
    {   
        allCharacters = GameObject.FindObjectsOfType<CharacterStats>();
    }
    //搜寻目标，这是一个时时刻刻都会进行的行为，所以写在基本AI类中
    //在这里转换状态
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
                //切换到追逐状态
                //追逐状态会转变为 攻击状态，
                SetZombieState(chasingState);
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
    //静态创建所有状态
    //并初始化当前状态为1
    void InitializeAIState()
    {
        partolState = new PatrolState(this);
        chasingState = new ChasingState(this);
        zombieState = partolState;
    }

    #region public methods
    //使zombie 朝向某个position
    public void LookAtPosition(Vector3 pos)
    {
        Vector3 dir = pos - transform.position;
        Quaternion lookRot = Quaternion.LookRotation(dir);
        lookRot.x = 0;
        lookRot.z = 0;

        transform.rotation = Quaternion.Lerp(transform.rotation, lookRot, Time.deltaTime * 5);
    }

    //使用射线投射判定伤害
    //在Animator motion中调用
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
    //修改状态
    public void SetZombieState(ZombieAIState zombieAIState)
    {
        zombieState = zombieAIState;
    }

    public void SetLookAtTransfrom(Transform t)
    {
        currentlookTransform = t;
    }
    //用于缓慢启动 和慢慢停下
    public float LerpSpeed(float curSpeed, float destSpeed, float time)
    {
        return curSpeed = Mathf.Lerp(curSpeed, destSpeed, Time.deltaTime * time);
    }
    #endregion

}

[System.Serializable]
public class WaypointBase
{
    public Transform destination;//巡逻点
    public float waitTime;       //停留时间
    public Transform lookAtTarget;//观察方向，可以为空
}
