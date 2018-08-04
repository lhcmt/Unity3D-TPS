using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChasingState : ZombieAIState
{

    private ZombieAI zombieAI;      //
    private ZombieMovement zombieMove;
    private Animator animator;
    private Timer timer;
    private bool isScreamed;
    private bool attacked;
    public ChasingState(ZombieAI zAI)
    {
        zombieAI = zAI;
        zombieMove = zombieAI.GetComponent<ZombieMovement>();
        animator = zombieAI.GetComponent<Animator>();
        timer = GameObject.FindGameObjectWithTag("GameController").GetComponent<Timer>();
        isScreamed = false;
        attacked = false;
    }

    public override void AIbehavior()
    {
        if(!isScreamed)
        {
            Screaming();
        }
        else
        {
            animator.SetBool("Scream", false);

            if (zombieAI.target == null)
            {
                zombieAI.SetZombieState(zombieAI.partolState);
                return;
            }
            if (!zombieAI.navMeshAgent.isOnNavMesh ||zombieAI.patrolSettings.waypoints.Length == 0)
                return;
            zombieAI.navMeshAgent.SetDestination(zombieAI.target.position);

            zombieAI.LookAtPosition(zombieAI.navMeshAgent.steeringTarget);

            zombieAI.zombieAudio.PlayZombieWalkSound();
            //走到玩家面前了
            if (zombieAI.navMeshAgent.remainingDistance <= zombieAI.attackSettings.attackRange - 1)
            {
                zombieAI.walkingToDest = false;
                zombieAI.forward = 0;
                Attacking();
            }
            else
            {
                zombieAI.walkingToDest = true;
                zombieAI.forward = zombieAI.LerpSpeed(zombieAI.forward, 1f, 15);
                animator.SetBool("Attack", false);
            }
            zombieMove.JustAnimate(zombieAI.forward, 0);
        }

    }
    //进入动作
    void Screaming()
    {
        zombieAI.walkingToDest = false;
        zombieAI.forward = 0;//停下
        zombieAI.LookAtPosition(zombieAI.target.position);
        isScreamed = true;
        animator.SetBool("Scream", true);
    }

    //攻击
    void Attacking()
    {
        if (attacked == false && zombieAI.target != null)
        {
            //print("Attack!");
            int attackType = Random.Range(0, zombieAI.attackSettings.attackType);
            animator.SetInteger("attackType", attackType);
            animator.SetBool("Attack", true);
            attacked = true;
            //sound在动画里
            timer.Add(() => { attacked = false; }, zombieAI.attackSettings.attackCooldown);
            //延迟后进行伤害判定，已经放到动画的motion里面去了
            //timer.Add(AttackJudge, attackSettings.attackDelay);

        }
    }
}
