using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieAudio : MonoBehaviour {

    [System.Serializable]
    public class ZombieSound
    {
        public AudioClip[] attackSounds;
        public float delayBetweenClips = 2f;//低吟的间隔
        public AudioClip[] walkSounds;
        public AudioClip Screaming;
        public AudioClip deadSound;
        [Range(0, 3)] public float pitchMin = 1f;
        [Range(0, 3)] public float pitchMax = 1.2f;
        public AudioSource audioS;
    }
    public ZombieSound zombieSound;

    private bool canPlay;

    private SoundController sc;
	// Use this for initialization
	void Start () {
        sc = GameObject.FindGameObjectWithTag("Sound Controller").GetComponent<SoundController>();
        zombieSound.audioS = GetComponent<AudioSource>();
        canPlay = true;
	}
	

    public void PlayZombieAttackSound()
    {
        if (sc != null)
        {
            if (zombieSound.audioS != null)
            {
                if (zombieSound.attackSounds.Length > 0)
                {
                    sc.PlaySound(zombieSound.audioS,
                        zombieSound.attackSounds[Random.Range(0, zombieSound.attackSounds.Length)], //clips
                        true, zombieSound.pitchMin, zombieSound.pitchMax);
                }
            }
        }
    }
    public void PlayZombieWalkSound()
    {
        if(!canPlay)
        {
            return;
        }
        GameController.gc.timer.Add(() =>{
            canPlay = true;
        },zombieSound.delayBetweenClips);
        canPlay = false;
        if (sc != null)
        {
            if (zombieSound.audioS != null)
            {
                if (zombieSound.walkSounds.Length > 0)
                {
                    sc.PlaySound(zombieSound.audioS,
                        zombieSound.walkSounds[Random.Range(0, zombieSound.walkSounds.Length)], //clips
                        true, zombieSound.pitchMin, zombieSound.pitchMax);
                }
            }
        }
    }

    public void PlayZombieDyingSound()
    {
        if (sc != null)
        {
            if (zombieSound.audioS != null)
            {
                if (zombieSound.deadSound)
                {
                    sc.InstantiateClip(transform.position, //发出声音的位置
                        zombieSound.deadSound, //clips
                        2, //Destory Audio的时间
                        true,//随机音高大小
                        zombieSound.pitchMin, //最低音高
                        zombieSound.pitchMax);//最高音高
                }
            }
        }
    }

    public void PlayZombieScreamingSound()
    {
        if (sc != null)
        {
            if (zombieSound.audioS != null)
            {
                if (zombieSound.Screaming)
                {
                    sc.InstantiateClip(transform.position, //发出声音的位置
                        zombieSound.Screaming, //clips
                        2, //Destory Audio的时间
                        true,//随机音高大小
                        zombieSound.pitchMin, //最低音高
                        zombieSound.pitchMax);//最高音高
                }
            }
        }
    }

}
