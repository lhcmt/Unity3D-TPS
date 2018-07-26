using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour {

	public void  PlaySound(AudioSource audioS,AudioClip clip,bool randomizePith = false,float randomPitchMin =1,float randomPitchMax =1)
    {
        audioS.clip = clip;
        if(randomizePith ==true)
        {
            //音高
            audioS.pitch = Random.Range(randomPitchMin, randomPitchMax);
        }

        audioS.Play();
    }
    //实现方案2：
    //每次发出声音都会创建一个有AudioSource组件的GameObject在发出声音的位置
    //过一段时间删除
    public void  InstantiateClip(Vector3 pos,AudioClip clip,float time = 2f,bool randomizePitch =false,float randomPitchMin =1,float randomPitchMax = 1)
    {
        GameObject clone = new GameObject("one shot audio");
        clone.transform.position = pos;
        AudioSource audio = clone.AddComponent<AudioSource>();
        audio.maxDistance = 50f;
        audio.spatialBlend = 1;//3D
        audio.clip = clip;
        audio.Play();

        Destroy(clone, time);
    }
}
