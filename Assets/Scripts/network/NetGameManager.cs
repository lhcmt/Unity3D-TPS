using System.Collections;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class NetGameManager : Photon.PunBehaviour
{
    [Tooltip("The prefab to use for representing the player")]
    public GameObject playerPrefab;
    public Transform birthPlace;
    [Serializable]
    public class AmmoPrefabs
    {
        public GameObject HandGunAmmoPrefab;
        public GameObject RifleGunAmmoPrefab;
    }
    public AmmoPrefabs ammoPrefabs;
    public Transform[] HandGunAmmoPrefabBirthPlace;
    public Transform[] RifleGunAmmoPrefabBirthPlace;

    #region Photon Messages
    /// <summary>
    /// 当本地玩家离开房间时候被调用，加载launcher场景
    /// </summary>
    public void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }
    #endregion


    #region Public Methods
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }
    #endregion 


    #region Private Methods
    /// <summary>
    /// only called by MasterClient
    /// </summary>
    void LoadArena()
    {
        if(!PhotonNetwork.isMasterClient)
        {
            Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
        }
        Debug.Log("PhotonNetwork : Loading Level :NetScene ");
        PhotonNetwork.LoadLevel("NetScene");
    }
    #endregion

    #region Photon Messages
    //每次有新玩家都会重新加载游戏
    public override void OnPhotonPlayerConnected(PhotonPlayer other)
    {
        Debug.Log("OnPhotonPlayerConnected()" + other.NickName);

        if(PhotonNetwork.isMasterClient)
        {
          // LoadArena(); //加载竞技场
        }
    }

    public override void OnPhotonPlayerDisconnected( PhotonPlayer other )
    {
            //if(PhotonNetwork.isMasterClient)
          //  {
        //        LoadArena(); //加载竞技场
        //    }
    }
    #endregion

    void Start()
    {
        if(PhotonNetwork.connected == false)
        {
            GameObject.Instantiate(this.playerPrefab, birthPlace.position, birthPlace.rotation);
            //RespawnAllAmmoDisconnect();
            return;
        }
        if (playerPrefab == null)
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'", this);
        }
        else
        {
            if (GameController.LocalPlayerInstance == null)
            {
                Debug.Log("We are Instantiating LocalPlayer from " + Application.loadedLevelName);
                // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
                PhotonNetwork.Instantiate(this.playerPrefab.name, birthPlace.position, birthPlace.rotation, 0);
            }
            else
            {
                Debug.Log("Ignoring scene load for " + Application.loadedLevelName);
            }
            PhotonNetwork.automaticallySyncScene = true;
        }
        // RespawnAllAmmo();
        /*
        if(PhotonNetwork.isMasterClient)
        {
            Debug.Log("MasterCLient:RespawnAllAmmo");
            RespawnAllAmmo();
        }*/

    }

    //暂时不用
    //直接在场景中创建初始物资
    void RespawnAllAmmo()
    {
        if (ammoPrefabs.HandGunAmmoPrefab == null || ammoPrefabs.RifleGunAmmoPrefab==null)
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> AmmoPrefab Reference. Please set it up in GameObject 'Game Manager'", this);
            return;
        }
        else
        {
            for (int i = 0; i < HandGunAmmoPrefabBirthPlace.Length;i++ )
            {
                GameObject go = PhotonNetwork.InstantiateSceneObject(this.ammoPrefabs.HandGunAmmoPrefab.name,
                    HandGunAmmoPrefabBirthPlace[i].position, HandGunAmmoPrefabBirthPlace[i].rotation, 0,null);
                //if (go)
                  //  go.transform.SetParent(GameController.gc.ammossInScene.transform);
            }
            for (int i = 0; i < RifleGunAmmoPrefabBirthPlace.Length; i++)
            {
                GameObject go = PhotonNetwork.InstantiateSceneObject(this.ammoPrefabs.RifleGunAmmoPrefab.name,
                    RifleGunAmmoPrefabBirthPlace[i].position, RifleGunAmmoPrefabBirthPlace[i].rotation, 0, null);
               // if (go)
               //     go.transform.SetParent(GameController.gc.ammossInScene.transform);
            }
        }
    }
    /// <summary>
    /// 当不在房间时候，用于测试
    /// </summary>
    void RespawnAllAmmoDisconnect()
    {
        if (ammoPrefabs.HandGunAmmoPrefab == null || ammoPrefabs.RifleGunAmmoPrefab == null)
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> AmmoPrefab Reference. Please set it up in GameObject 'Game Manager'", this);
            return;
        }
        else
        {
            for (int i = 0; i < HandGunAmmoPrefabBirthPlace.Length; i++)
            {
                GameObject go = GameObject.Instantiate(this.ammoPrefabs.HandGunAmmoPrefab, HandGunAmmoPrefabBirthPlace[i].position,
                    HandGunAmmoPrefabBirthPlace[i].rotation);
                if (go)
                    go.transform.SetParent(GameController.gc.ammossInScene.transform);
            }
            for (int i = 0; i < RifleGunAmmoPrefabBirthPlace.Length; i++)
            {
                GameObject go = GameObject.Instantiate(this.ammoPrefabs.RifleGunAmmoPrefab,
                    RifleGunAmmoPrefabBirthPlace[i].position, RifleGunAmmoPrefabBirthPlace[i].rotation);
                if (go)
                    go.transform.SetParent(GameController.gc.ammossInScene.transform);
            }
        }
    }

}
