using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//附在武器使用者上
public class WeaponHandler : Photon.MonoBehaviour {

    Animator animator;
    SoundController sc;

    [System.Serializable]
    public class UserSettings
    {
        public Transform rightHand;
        public Transform pistolUnequipSpot;
        public Transform rifleUnequipSpot;
    }
    [SerializeField]
    public UserSettings userSettings;
    //动画参数
    [System.Serializable]
    public class Animations
    {
        public string weaponTypeInt = "WeaponType";
        public string reloadingBool = "isReloading";
        public string aimingBool = "aiming";
    }
    [SerializeField]
    public Animations animations;

    //当前武器参数
    public Weapon currentWeapon;
    public List<Weapon> weaponList;
    public Container container;
    public int rifleCarried = 0;
    bool aim;
    public bool reload { get; private set; }
    int weaponType;
    bool settingWeapon;//是否正在切换武器
    //当前人物的背包


	// Use this for initialization
	void Start () {
        animator = GetComponent<Animator>();
        weaponList = new List<Weapon>();
        sc = GameObject.FindGameObjectWithTag("Sound Controller").GetComponent<SoundController>();
        container = GetComponentInChildren<Container>();
        if (!container)
            Debug.LogError("<Color=Red><a>Missing</a></Color> container Reference.");
	}
	
	// Update is called once per frame
	void Update () {
		if(currentWeapon)
        {
            currentWeapon.SetEquipped(true);
            currentWeapon.SetOwner(this);
            
            currentWeapon.ownerAiming = aim;

            if (currentWeapon.ammo.clipAmmo <= 0)
                Reload();
            //如果reload同时切换武器，就立刻终止reload并切换武器
            if(reload)
                if(settingWeapon)
                {
                    //额外操作，不能从弹夹中拿子弹，在别的脚本中
                    reload = false;
                }
        }
        //其他武器为非装备状态
        if(weaponList.Count >0)
        {
            for(int i =0; i < weaponList.Count ; i++)
            {
                if(weaponList[i] != currentWeapon)
                {
                    weaponList[i].SetEquipped(false);
                    weaponList[i].SetOwner(this);
                }
            }
        }

        Animate();
	}
    //角色的武器动画
    void Animate()
    {
        if (!animator)
            return;

        animator.SetBool(animations.aimingBool, aim);
        animator.SetBool(animations.reloadingBool, reload);
        animator.SetInteger(animations.weaponTypeInt, weaponType);

        if(!currentWeapon)
        {
            weaponType = 0;
            return;
        }

        switch(currentWeapon.weaponType)
        {
            case WeaponType.Pistol:
                weaponType = 1;
                break;
            case WeaponType.Rifle:
                weaponType = 2;
                break;
        }

    }

    //捡起武器
    public void AddWeaponToList(Weapon weapon)
    {
         //如果有这武器，就不添加
         if(weaponList.Contains(weapon))
             return;
         weaponList.Add(weapon);
    }
    //扳机状态是否扣下
    //如果扣下，weapon 脚本的Update函数会调用Fire()函数
    public void FIngerOnTriger(bool pulling)
     {
         if (!currentWeapon)
             return;

         currentWeapon.PullTrigger(!settingWeapon&&pulling &&aim && !reload);
     }
    //reload the current weapon
    public void Reload()
    {
        if (reload || !currentWeapon)
            return;

        if (container.GetAmountRemaining(currentWeapon.ammo.AmmoID) <= 0 || 
            currentWeapon.ammo.clipAmmo == currentWeapon.ammo.maxClipAmmo)
            return;

        if(sc != null)
        {
            if(currentWeapon.soundSettings.reloadSound != null)
            {
                if (currentWeapon.soundSettings.audioS != null)
                {
                    sc.PlaySound(currentWeapon.soundSettings.audioS, currentWeapon.soundSettings.reloadSound, true, currentWeapon.soundSettings.pitchMin, currentWeapon.soundSettings.pitchMax);
                    //sc.InstantiateClip(currentWeapon.transform.position, //发出声音的位置
                    //    currentWeapon.soundSettings.reloadSound, //clips
                   //     2); //Destory Audio的时间

                }
            }
        }
        reload = true ;
        StartCoroutine(StopReload());
    }
    //stop the reloading of the weapon
    IEnumerator StopReload()
    {
        yield return new WaitForSeconds(currentWeapon.weaponSettings.reloadDuration);
        if (reload && currentWeapon)
            currentWeapon.LoadClip();
        reload = false;
    }
    //Sets out aim bool to be what we pass it
    public void Aim(bool aiming)
    {
        aim = aiming;
    }

    //Drops the current weapon
    public void DropCurWeapon()
    {
        if (!currentWeapon)
            return;

        currentWeapon.SetEquipped(false);
        currentWeapon.SetOwner(null);
        weaponList.Remove(currentWeapon);

        //飞出去
        currentWeapon.GetComponent<Rigidbody>().AddForce(transform.forward * 300f, ForceMode.Force);

        currentWeapon = null;
    }
    //切换武器
    public void SwitchWeapons()
    {
        if (settingWeapon || weaponList.Count <=0)
            return;

        if(currentWeapon)
        {
            if (weaponList.Count == 1)
                return;
            int currentWeaponIndex = weaponList.IndexOf(currentWeapon);
            int nextWeaponIndex = (currentWeaponIndex + 1) % weaponList.Count;

            currentWeapon = weaponList[nextWeaponIndex];
        }
        else
        {
            currentWeapon = weaponList[0];

        }
        settingWeapon = true;
        StartCoroutine(StopSettingWeapon());

    }

    IEnumerator StopSettingWeapon()
    {
        yield return new WaitForSeconds(0.7f);
        settingWeapon = false;
    }

    void OnAnimatorIK()
    {
        if (!animator)
            return;

        if(currentWeapon)
        {
            //                                                                                            是rifle
            if (currentWeapon && currentWeapon.userSettings.leftHandIKTarget && weaponType == 2 && !reload && !settingWeapon)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
                Transform target = currentWeapon.userSettings.leftHandIKTarget;
                Vector3 targetPos =  target.position;
                 Quaternion targetRot =   target.rotation;
                animator.SetIKPosition(AvatarIKGoal.LeftHand, targetPos);
                animator.SetIKRotation(AvatarIKGoal.LeftHand, targetRot);
            }
            else
            {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
            }
        }

    }
}
