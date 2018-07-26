using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 对每一把枪的设置
 * 适用于子弹类的枪械
 */
public enum WeaponType
{
    //可扩展
    Pistol, Rifle
}


[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class Weapon : Photon.PunBehaviour {

    Collider colBox;
    Collider colSphere;
    Rigidbody rgbody;
    Animator animator;
    SoundController sc;
    //一些属性类
    //

    public WeaponType weaponType;


    [System.Serializable]
    public class UserSettings
    {
        public Transform leftHandIKTarget;
        public Vector3 spineRotation;
    }
    [SerializeField]
    public UserSettings userSettings;

    //武器设置
    [System.Serializable]
    public class WeaponSettings
    {
        [Header("-Bullet Options-")]
        public Transform bulletSpawn;//枪口位置
        public float damage = 20.0f;
        public float bulletSpread = 5.0f; //枪口晃动
        public float fireRate = 0.2f;
        public LayerMask bulletLayers;
        public float range = 200.0f;//射程
       
        [Header("-Effects-")]
        public GameObject muzzleFlash;//枪口火花GO
        public GameObject decal; //弹孔go
        public GameObject shell;//弹壳出口go
        public GameObject clips;//弹夹go
        public GameObject bloodSplat;

        [Header("-Other-")]
        public GameObject crossHairPrefeb;
        public float reloadDuration = 2.0f;
        public Transform shellEjectSpot;//弹壳飞出位置
        public float shellEjectSpeed = 7.5f;
        public Transform clipEjectPos;//弹夹位置，这里模型不适合
        public GameObject clipGO;//本项目默认为空

        [Header("-Positioning-")]
        public Vector3 equipPosition;
        public Vector3 equipRotation;
        public Vector3 unequipPosition;
        public Vector3 unequipRotation;

        [Header("-Animation-")]
        public bool useAnimation;
        public int fireAnimationLayer = 0;
        public string fireAnimationName = "Fire";

    }
    [SerializeField]
    public WeaponSettings weaponSettings;

    //弹夹属性
    [System.Serializable]
    public class Ammunition
    {
        //public int carryingAmmo;
        public int AmmoID;
        public int clipAmmo;
        public int maxClipAmmo;
    }
    [SerializeField]
    public Ammunition ammo;

    [System.Serializable]
    public class SoundSettings
    {
        public AudioClip[] gunshotSounds;
        public AudioClip reloadSound;
        [Range(0, 3)] public float pitchMin = 1f;
        [Range(0, 3)] public float pitchMax = 1.2f;
        public AudioSource audioS;
    }
    [SerializeField]
    public SoundSettings soundSettings;

    public Ray shootRay{protected get;set; }
    public bool ownerAiming{get;set;}

    //此脚本所在武器的状态
    WeaponHandler owner;
    bool equipped;//此武器是否在被装备
    bool pullingTrigger;//开火按钮
    bool resettingCartridge;//切换武器

    

    void Start()
    {
        colBox = GetComponent<BoxCollider>();
        colSphere = GetComponent<SphereCollider>();
        rgbody = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        sc = GameObject.FindGameObjectWithTag("Sound Controller").GetComponent<SoundController>();

        //游戏刚开始时隐藏准星
        if (weaponSettings.crossHairPrefeb != null)
        {
            weaponSettings.crossHairPrefeb = Instantiate(weaponSettings.crossHairPrefeb);
            ToglleCrosshair(false);
        }
    }

    void Update()
    {
        if (owner)
        {

            //DisableEnableComponents(false);移动到SetOwner中
            if (equipped)
            {
                if (owner.userSettings.rightHand)   
                {
                    Equip();
                    if (pullingTrigger) 
                    {
                        fire(shootRay);
                    }

                    if (ownerAiming && (!PhotonNetwork.connected || owner.photonView.isMine))
                    {
                        ToglleCrosshair(true);
                    }
                    else
                    {
                        ToglleCrosshair(false);
                    }
                }
            }
            else {
               /* if(weaponSettings.bulletSpawn.childCount > 0)
                {
                    foreach (Transform t in weaponSettings.bulletSpawn.GetComponentsInChildren<Transform>())
                    {
                        if (t != weaponSettings.bulletSpawn)
                            Destroy(t);
                    }
                }*/
                UnEquip(weaponType);
            }
        }
        else {
            if (GameController.gc.weaponsInScene)
                transform.SetParent(GameController.gc.weaponsInScene.transform);
            else
                transform.SetParent(null);
            ownerAiming = false;
            ToglleCrosshair(false);
        }
    }

    //开枪，
    //着弹点的特效
    void fire(Ray ray)
    {
        if (ammo.clipAmmo <= 0 || resettingCartridge || !weaponSettings.bulletSpawn || owner.reload)
            return;
        //瞄准点
        RaycastHit aimHit;
        Vector3 startPos = ray.origin;
        Vector3 aimdir = ray.direction;
        Physics.Raycast(startPos, aimdir, out aimHit);

        //像瞄准点射击
        RaycastHit hit;
        Transform bSpawn = weaponSettings.bulletSpawn;
        Vector3 bSpawnPoint = bSpawn.position;
        Vector3 dir = aimHit.point - bSpawnPoint;  

        dir += (Vector3)Random.insideUnitCircle * weaponSettings.bulletSpread;
        if (Physics.Raycast(bSpawnPoint, dir, out hit, weaponSettings.range, weaponSettings.bulletLayers))
        {

            HitEffect(hit);
        }
        

        GunEffects();

        //枪支没有动画
        if(weaponSettings.useAnimation)
            animator.Play(weaponSettings.fireAnimationName,weaponSettings.fireAnimationLayer);

        ammo.clipAmmo --;
        resettingCartridge =true;
        StartCoroutine(LoadNextBullet());
    }
    //load the next bullet to the gun
     IEnumerator LoadNextBullet()
    {
        yield return new WaitForSeconds(weaponSettings.fireRate);
        resettingCartridge = false;
    }
     //弹孔实现
    void HitEffect(RaycastHit hit)
     {
         #region Ragdoll
         //伤害判断
        if (hit.collider.gameObject.tag == "Ragdoll")
        {
            ZombieStats ZombieHP = hit.collider.gameObject.GetComponentInParent<ZombieStats>();
            ZombieHP.ApplyDamage(weaponSettings.damage);
            //hitsound
            
            //hit forece 有点问题

            //bloodSplat
            if(weaponSettings.bloodSplat)
            {
                Vector3 hitPoint = hit.point;
                Quaternion lookRotation = Quaternion.LookRotation(hit.normal);
                GameObject bloodSplat = Instantiate(weaponSettings.bloodSplat, hitPoint, lookRotation) as GameObject;
                Transform bloodSplatT = bloodSplat.transform;
                Transform hitT = hit.transform;
                bloodSplatT.SetParent(hitT);
                Destroy(bloodSplat, 1.0f);
            }
        }
        //简单的玩家判断
        if (hit.collider.gameObject.tag == "Player")
        {
            CharacterStats otherPlayerState = hit.collider.gameObject.GetComponent<CharacterStats>();
            {
                otherPlayerState.ApplyDamage(weaponSettings.damage);
            }
        }

        #endregion

        #region decal
        //墙体标记tag,
         if (hit.collider.gameObject.tag == "StaticObj")
         {
             if (weaponSettings.decal)
             {
                 Vector3 hitPoint = hit.point;
                 Quaternion lookRotaion = Quaternion.LookRotation(hit.normal);
                 GameObject decal = Instantiate(weaponSettings.decal, hitPoint, lookRotaion) as GameObject;
                 Transform decalT = decal.transform;
                 Transform hitT = hit.transform;
                 decalT.SetParent(hitT);
                 Destroy(decal, Random.Range(30.0f, 45.0f));
             }
         }
         #endregion
    }

    void GunEffects()
    {
        //枪口火花效果实现
        #region muzzle flash
        if (weaponSettings.muzzleFlash)
        {
            Vector3 buttleSpawnPos = weaponSettings.bulletSpawn.position;
            GameObject muzzleFlash = Instantiate(weaponSettings.muzzleFlash, buttleSpawnPos, Quaternion.identity) as GameObject;
            Transform muzzleT = muzzleFlash.transform;
            muzzleT.SetParent(weaponSettings.bulletSpawn);
            Destroy(muzzleFlash, 1.0f);
        }
        #endregion
        //弹壳效果实现
        #region shell
        if (weaponSettings.shell)
        {
            if (weaponSettings.shellEjectSpot)
            {
                Vector3 shellEjectPos = weaponSettings.shellEjectSpot.position;
                Quaternion shellEjectRot = weaponSettings.shellEjectSpot.rotation;
                GameObject shell = Instantiate(weaponSettings.shell, shellEjectPos, shellEjectRot) as GameObject;

                if (shell.GetComponent<Rigidbody>())
                {
                    Rigidbody rigidB = shell.GetComponent<Rigidbody>();
                    rigidB.AddForce(weaponSettings.shellEjectSpot.forward * weaponSettings.shellEjectSpeed + (Vector3)Random.insideUnitCircle, ForceMode.Impulse);
                }
                Destroy(shell, Random.Range(30.0f, 45.0f));
            }
        }
        #endregion
        //音效
        if(sc == null)
        {
            Debug.Log("sc == null");
            return;
        }
        if(soundSettings.audioS != null)
        {
            if (soundSettings.gunshotSounds.Length > 0)
            {
                sc.InstantiateClip(weaponSettings.bulletSpawn.position, //发出声音的位置
                    soundSettings.gunshotSounds[Random.Range(0, soundSettings.gunshotSounds.Length)], //clips
                    2, //Destory Audio的时间
                    true,//随机音高大小
                    soundSettings.pitchMin, //最低音高
                    soundSettings.pitchMax);//最高音高
            }
        }
    }

    //Position the crosshair to the point that we are aiming
    //修改，摄像机直射方向，固定距离
    //UIcamera要设置成OrthoGraph
    //动态的不用了
    void PositionCrosshair(Ray ray)
    {
        
        /*RaycastHit hit;
        Transform startT = weaponSettings.bulletSpawn;
        Vector3 startPos = startT.position;
        Vector3 dir = ray.GetPoint(weaponSettings.range);

        if (Physics.Raycast(startPos, dir, out hit, weaponSettings.range, weaponSettings.bulletLayers))
        {
            
            if(weaponSettings.crossHairPrefeb != null)
            {
                ToglleCrosshair(true);
                weaponSettings.crossHairPrefeb.transform.position = hit.point;
                weaponSettings.crossHairPrefeb.transform.LookAt(Camera.main.transform);
            }
        }
        else
        {
            ToglleCrosshair(false);
            
        }*/

        RaycastHit aimHit;
        Vector3 startPos = ray.origin;
        Vector3 aimdir = ray.GetPoint(weaponSettings.range);
        Physics.Raycast(startPos, aimdir, out aimHit);

        if (weaponSettings.crossHairPrefeb != null)
        {
            ToglleCrosshair(true);
            weaponSettings.crossHairPrefeb.transform.position = ray.GetPoint(50) ;
            weaponSettings.crossHairPrefeb.transform.LookAt(Camera.main.transform);
        }
}

    //toggle on and off the crosshair prefebs
    void ToglleCrosshair(bool enabled)
    {
        if (weaponSettings.crossHairPrefeb != null)
            weaponSettings.crossHairPrefeb.SetActive(enabled);
    }

    //使用或者使用Collider 和rigidbody
    //武器在地上时候，使用Collider 和rigidbody
    void DisableEnableComponents(bool enabled)
    {
        if(!enabled)
        {
            rgbody.isKinematic = true;
            colBox.enabled = false;
        }
        if(enabled)
        {
            rgbody.isKinematic = false;
            colBox.enabled = true;
        }
    }
    //Equips this weapon to the hand
    void Equip()
    {
        if (!owner)
            return;
        else if (!owner.userSettings.rightHand)
            return;

        transform.SetParent(owner.userSettings.rightHand);
        transform.localPosition = weaponSettings.equipPosition;
        Quaternion equipRot = Quaternion.Euler(weaponSettings.equipRotation);
        transform.localRotation = equipRot;
    }

    //Unequips this weapon and places it to the desired location
    void UnEquip(WeaponType weaponType)
    {
        if (!owner)
            return;
        
        switch(weaponType)
        {
            case WeaponType.Pistol:
                transform.SetParent(owner.userSettings.pistolUnequipSpot);
                break;
            case WeaponType.Rifle:
                transform.SetParent(owner.userSettings.rifleUnequipSpot);
                break;
        }
        transform.localPosition = weaponSettings.unequipPosition;
        Quaternion unEquipRot = Quaternion.Euler(weaponSettings.unequipRotation);
        transform.localRotation = unEquipRot;
        ToglleCrosshair(false);
    }

    /*************************public method*******************************/

    //换子弹，计算弹夹数量
    public void LoadClip()
    {
        var container = owner.container;
        int ammoNeeded = ammo.maxClipAmmo - ammo.clipAmmo;
        //如果子弹不够了，把剩余的子弹换到弹夹中
        ammo.clipAmmo += container.TakeFromContainer(ammo.AmmoID, ammoNeeded);

    }

    //设置武器的装备状态
    public void SetEquipped(bool equip)
    {
        equipped = equip;
    }
    //扳机状态是否扣下
    public void PullTrigger(bool isPulling)
    {
        pullingTrigger = isPulling;
    }
    //Set the owner of this weapon,对象需要WeaponHandler脚本
    public void SetOwner(WeaponHandler wp)
    {
        owner = wp;
        if (owner == null)
        {
            DisableEnableComponents(true);
            StartCoroutine(CanBePickUp());
        }
        else
        {
            DisableEnableComponents(false);
        }
    }
    //内置，丢弃2秒后可以拾取否则
    IEnumerator CanBePickUp()
    {
        yield return new WaitForSeconds(2.0f);
        colSphere.enabled = true;
    }


}

