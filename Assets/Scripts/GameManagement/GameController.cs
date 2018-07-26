using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//单例
[RequireComponent(typeof(Timer))]

public class GameController : MonoBehaviour {
    //单例,静态对象
    public static GameController gc;

    public static GameObject LocalPlayerInstance;
    private UserInput player { get { return LocalPlayerInstance.GetComponent<UserInput>(); } set { player = value; } }
    private PlayerUI playerUI { get { return FindObjectOfType<PlayerUI>(); } set { playerUI = value; } }
    private WeaponHandler wp { get { return LocalPlayerInstance.GetComponent<WeaponHandler>(); } set { wp = value; } }
    private CharacterStats characterStats { get { return LocalPlayerInstance.GetComponent<CharacterStats>(); } set { characterStats = value; } }
    //收纳所有场景中的枪支
    private GameObject m_weaponsInScene;
    public GameObject weaponsInScene
    {
        get
        {
            if (m_weaponsInScene == null)
                m_weaponsInScene = GameObject.Find("WeaponsInScene");
            return m_weaponsInScene;
        }
    }
    private GameObject m_ammosInScene;
    public GameObject ammossInScene
    {
        get
        {
            if (m_ammosInScene == null)
                m_ammosInScene = GameObject.Find("Ammos");
            return m_ammosInScene;
        }
    }

    private Timer m_Timer;
    public Timer timer{
        get{
            if (m_Timer == null)
                m_Timer = gameObject.GetComponent<Timer>();
            return m_Timer;
        }
    }

    private Container m_Container;
    public Container loaclPlayerContainer
    {
        get
        {
            if (m_Container == null)
                m_Container = LocalPlayerInstance.GetComponentInChildren<Container>();
            return m_Container;
        }
    }
    /*
    private EventBus m_EventBus;
    public EventBus EventBus
    {
        get
        {
            if (m_EventBus == null)
                m_EventBus = new EventBus();
            return m_EventBus;
        }
    }*/


    void Awake()
    {
        if (gc == null)
        {
            gc = this;
        }
        else
        {
            if (gc != this)
            {
                Destroy(this);
            }
        }
        if(PhotonNetwork.connected==false)
        {
            LocalPlayerInstance = GameObject.FindGameObjectWithTag("Player");
        }

    }

    void Update()
    {
        if (!LocalPlayerInstance)
            GetMyPlayer();
        UpdateUI();
    }

    void UpdateUI()
    {
        if(player)
        {
            if (playerUI)
            {
                if(wp)
                {
                    if (playerUI.ammoText)
                    {
                        //手里没有武器
                        if (wp.currentWeapon == null)
                        {
                            playerUI.ammoText.text = "Unarmed";
                        }
                        else
                        {
                            playerUI.ammoText.text = wp.currentWeapon.ammo.clipAmmo +
                                "//" +
                                GameController.gc.loaclPlayerContainer.GetAmountRemaining(wp.currentWeapon.ammo.AmmoID);

                        }
                    }

                }
                if (playerUI.healthBar && playerUI.healthText)
                {
                    playerUI.healthBar.value = characterStats.health;
                    playerUI.healthText.text = Mathf.Round(playerUI.healthBar.value).ToString();
                }
                //taskMenu
                if (Input.GetButtonDown(player.inputs.TaskButtun))
                {
                    playerUI.TaskCanvasPress();
                }
                if (Input.GetButtonDown(player.inputs.Cancel))
                {
                    playerUI.QuitButtonPress();
                }
            }


        }


    }
    void GetMyPlayer()
    {
        //单机和联网模块兼容
        if (PhotonNetwork.connected == false)
        {
            LocalPlayerInstance = GameObject.FindGameObjectWithTag("Player");
        }
        else
        {

            GameObject[] myPlayers = GameObject.FindGameObjectsWithTag("Player");
            foreach (var pl in myPlayers)
            {
                if (pl.GetPhotonView().isMine)
                {
                    LocalPlayerInstance = pl;
                    break;
                }
            }

        }
    }

}
