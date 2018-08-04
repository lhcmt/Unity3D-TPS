using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolState : ZombieAIState
{

    private ZombieAI zombieAI;      //
    private ZombieMovement zombieMove;
    private int waypointIndex;  //当前巡逻点的编号
    private float currentWaitTime; //巡逻点的等待时间，默认为5
    public PatrolState(ZombieAI zAI)
    {
        zombieAI = zAI;
        zombieMove = zombieAI.GetComponent<ZombieMovement>();
        waypointIndex = 0;
        currentWaitTime = 5f;
    }

    //AI逻辑行为:巡逻
    public override void AIbehavior()
    {
        //如果不存在寻路代理，或者巡逻点为0，则返回
        if (!zombieAI.navMeshAgent.isOnNavMesh || zombieAI.patrolSettings.waypoints.Length == 0)
            return;

        zombieAI.navMeshAgent.SetDestination(zombieAI.patrolSettings.waypoints[waypointIndex].destination.position);
        zombieAI.LookAtPosition(zombieAI.navMeshAgent.steeringTarget);
        zombieAI.zombieAudio.PlayZombieWalkSound();

        //走到一个点，走向下一个点
        if (zombieAI.navMeshAgent.remainingDistance <= zombieAI.navMeshAgent.stoppingDistance)
        {
            zombieAI.walkingToDest = false;
            zombieAI.forward = zombieAI.LerpSpeed(zombieAI.forward, 0, 15);//停下
            currentWaitTime -= Time.deltaTime;//等待
            if (zombieAI.patrolSettings.waypoints[waypointIndex].lookAtTarget != null)
            {
                zombieAI.SetLookAtTransfrom(zombieAI.patrolSettings.waypoints[waypointIndex].lookAtTarget);
            }
            if (currentWaitTime <= 0)
            {
                waypointIndex = (waypointIndex + 1) % zombieAI.patrolSettings.waypoints.Length;
                currentWaitTime = zombieAI.patrolSettings.waypoints[waypointIndex].waitTime;
            }

        }
        else//走向目标点
        {
            zombieAI.walkingToDest = true;
            zombieAI.forward = zombieAI.LerpSpeed(zombieAI.forward, 0.5f, 15);
            currentWaitTime = zombieAI.patrolSettings.waypoints[waypointIndex].waitTime;
        }
        zombieMove.AnimateAndMove(zombieAI.forward, 0);
    }

    
}
