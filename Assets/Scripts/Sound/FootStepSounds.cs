using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootStepSounds : MonoBehaviour {

    //这里用TAG代替
    public TextureType[] terrainTypes;

    public AudioSource audioS;

    SoundController sc;

	// Use this for initialization
	void Start () {
        sc = GameObject.FindGameObjectWithTag("Sound Controller").GetComponent<SoundController>();
        audioS = this.gameObject.GetComponent<AudioSource>();
	}
	//在动画系统中调用
    //在unity editor中点击动画片段FBX，Inspector面板中的Animation标签， Events
    void PlayFootstepSound()
    {
        RaycastHit hit;
        Vector3 start = transform.position + transform.up;
        Vector3 dir = Vector3.down;

        if (Physics.Raycast(start,dir, out hit, 1.3f))
        {
            if(hit.collider)
            {
                PlaySurfaceSound(hit.collider.gameObject);
            }
        }
    }



    void PlaySurfaceSound(GameObject GroundSurface)
    {
        if (audioS == null)
        {
            Debug.LogError("PlayMeshSound we have no audio source to play the sound");
            return;
        }

        if (sc == null)
        {
            Debug.LogError("PlayMeshSound No Sound manager!");
        }

        if (terrainTypes.Length > 0)
        {
            foreach (TextureType type in terrainTypes)
            {
                if(type.footstepSounds.Length ==0)
                {
                    return;
                }

                if (GroundSurface.tag == type.GroundTag)
                {
                    //sc.PlaySound(audioS, type.footstepSounds[Random.Range(0, type.footstepSounds.Length)], true, 1, 1.2f);
                    sc.InstantiateClip(transform.position + Vector3.down,
                        type.footstepSounds[Random.Range(0, type.footstepSounds.Length)]);
                }
                
            }
        }

    }





    //不适合本资源
    //之后改成使用Tag标记
    /*
    void PlayMeshSound(MeshRenderer renderer)
    {
        if (audioS == null)
        {
            Debug.LogError("PlayMeshSound we have no audio source to play the sound");
            return;
        }

        if (sc == null)
        {
            Debug.LogError("PlayMeshSound No Sound manager!");
        }

        if (textureTypes.Length > 0)
        {
            foreach (TextureType type in textureTypes)
            {
                if(type.footstepSounds.Length ==0)
                {
                    return;
                }
                foreach (Texture tex in type.textures)
                {
                    if (renderer.material.mainTexture == tex)
                    {
                        sc.PlaySound(audioS, type.footstepSounds[Random.Range(0, type.footstepSounds.Length)], true, 1, 1.2f);
                    }
                }
            }
        }

    }*/
}

[System.Serializable]
public class TextureType
{
    public string GroundTag;
    public AudioClip[] footstepSounds;

}