using UnityEngine;
using UnityEngine.UI;
public class Launcher : Photon.PunBehaviour
{

    #region Public Variables
    /// <summary>
    /// PUN日志等级
    /// </summary>
    public PhotonLogLevel LogLevel = PhotonLogLevel.Informational;


    public InputField inpuField;
    /// <summary>
    /// 每个房间的最大玩家数
    /// </summary>
    [Tooltip("每个房间的最大玩家数。当一个房间满了，它不能加入新的玩家，所以新的房间将被创建")]
    public byte maxPlayersPerRoom = 4;
    [Tooltip("让用户输入名称、连接和开始游戏的UI 面板")]
    public GameObject controlPanel;
    [Tooltip("通知用户连接正在进行中的UI 标签")]
    public GameObject progressLabel;
    #endregion


    #region private Variables
    /// <summary>
    ///  版本号
    /// </summary>
    string _gameVersion = "1";
    static string playerNamePrefKey = "PlayerName";
    /// <summary>
    /// 跟踪当前进程。因为连接是异步的，且是基于来自Photon 的几个回调，
    /// 我们需要跟踪这一点，以在我们收到Photon 回调时适当地调整该行为。
    /// 这通常是用于OnConnectedToMaster()回调。
    /// </summary>
    bool isConnecting;
    #endregion

    #region MonoBehaviour CallBacks

    void Awake()
    {
        PhotonNetwork.autoJoinLobby = false;

        PhotonNetwork.automaticallySyncScene = true;

        PhotonNetwork.logLevel = LogLevel;
    }

    void Start()
    {

    }


    #endregion

    #region Public Methods
    /// <summary>
    /// 启动连接进程
    /// - 如果已经连接，我们试图加入一个随机的房间
    /// - 如果尚未连接，请将此应用程序实例连接到Photon 
    /// </summary>
    public  void Connect()
    {
        SetPlayerName(inpuField.text);

        isConnecting = true;
        //我们检查是否连接，如果我们已连接则加入，否则我们启动连接到服务器
        if(PhotonNetwork.connected)
        {
            //极重要-我们需要在这个点上企图加入一个随机房间
            //如果失败，我们将在OnPhotonRandomJoinFailed()里面得到通知，这样我们将创建一个房
            PhotonNetwork.JoinRandomRoom();

        }
        else
        {
            PhotonNetwork.ConnectUsingSettings(_gameVersion);
        }
    }
    #endregion


    #region Photon.PunBehaviour CallBacks
    // below, we implement some callbacks of PUN
    // you can find PUN's callbacks in the class PunBehaviour or in enum PhotonNetworkingMessage


    /// <summary>
    /// Called after the connection to the master is established and authenticated but only when PhotonNetwork.autoJoinLobby is false.
    /// </summary>
    public override void OnConnectedToMaster()
    {
        Debug.Log("Region:" + PhotonNetwork.networkingPeer.CloudRegion);

        // we don't want to do anything if we are not attempting to join a room. 
        // this case where isConnecting is false is typically when you lost or quit the game, when this level is loaded, OnConnectedToMaster will be called, in that case
        // we don't want to do anything.
        if (isConnecting)
        {
            Debug.Log("DemoAnimator/Launcher: OnConnectedToMaster() was called by PUN. Now this client is connected and could join a room.\n Calling: PhotonNetwork.JoinRandomRoom(); Operation will fail if no room found");

            // #Critical: The first we try to do is to join a potential existing room. If there is, good, else, we'll be called back with OnPhotonRandomJoinFailed()
            PhotonNetwork.JoinRandomRoom();
        }
    }
    /// <summary>
    /// Called after disconnecting from the Photon server.
    /// </summary>
    /// <remarks>
    /// In some cases, other callbacks are called before OnDisconnectedFromPhoton is called.
    /// Examples: OnConnectionFail() and OnFailedToConnectToPhoton().
    /// </remarks>
    public override void OnDisconnectedFromPhoton()
    {
        Debug.LogWarning("Launcher:Disconnected");
        isConnecting = false;
    }
    /// <summary>
    /// Called when a JoinRandom() call failed. The parameter provides ErrorCode and message.
    /// </summary>
    /// <remarks>
    /// Most likely all rooms are full or no rooms are available. <br/>
    /// </remarks>
    /// <param name="codeAndMsg">codeAndMsg[0] is short ErrorCode. codeAndMsg[1] is string debug msg.</param>
    //JoinRandomRoom失败时会回调这个函数
    public override void OnPhotonRandomJoinFailed(object[] codeAndMsg)
    {

        Debug.Log("Launcher:No random room available,call PhotonNetwork.CreateRoom()");
        PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = this.maxPlayersPerRoom }, null);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Launcher: OnJoinedRoom() called. Now this client is in a room.");

        if (PhotonNetwork.room.PlayerCount == 1)
        {
            Debug.Log("We load the 'NetScene' ");
            PhotonNetwork.LoadLevel("NetScene");
        }
    }
    #endregion

    /// <summary>
    /// 设置该玩家的名字，并把它保存在PlayerPrefs 
    /// </summary>
    /// <param name="value"></param>
    public void SetPlayerName(string value)
    {
        Debug.Log("SetPlayerName:" + value);
        // #Important
        PhotonNetwork.playerName = value + " "; //强制加一个拖尾空格字符串，以免该值是一个空字符串，否则playerName 不会被更新。
        PlayerPrefs.SetString(playerNamePrefKey, value);
    }
}
