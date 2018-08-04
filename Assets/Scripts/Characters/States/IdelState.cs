using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdelState : ZombieAIState
{
    private ZombieAI zombieAI;      //

    #region public methods
    public IdelState(ZombieAI zAI)
    {
        zombieAI = zAI;

    }
    //虚函数
    //慢慢停下，然后站立
    public override void  AIbehavior()
    {
        zombieAI.walkingToDest = false;
        zombieAI.forward = zombieAI.LerpSpeed(zombieAI.forward, 0, 5);//停下
    }

    #endregion
}
