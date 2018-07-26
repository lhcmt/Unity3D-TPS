using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuUI : MonoBehaviour
{

    #region Public Variables
    public Canvas quitMenu;
    public Canvas helpMenu;
    public Canvas ConnectMenu;
    public Button starText;
    public Button MultiPlayerButton;
    public Button helpText;
    public Button exitText;
    public InputField nameInputField;
    #endregion

    #region Priavte Variables

    Launcher launcher;
    #endregion



    #region MonoBehaviour CallBacks
    void Start()
    {
        launcher = this.GetComponent<Launcher>();
		quitMenu = quitMenu.GetComponent<Canvas>();
        helpMenu = helpMenu.GetComponent<Canvas>();
        ConnectMenu = ConnectMenu.GetComponent<Canvas>();
        if (nameInputField == null)
            nameInputField = ConnectMenu.GetComponent<InputField>();

        quitMenu.enabled = false;
        helpMenu.enabled = false;
        ConnectMenu.enabled = false;
        
	}
    #endregion


    #region Public Methods
    //点击退出，
    public void ExitPress()
    {
        quitMenu.enabled = true;
        starText.enabled = false;
        MultiPlayerButton.enabled = false;
        helpText.enabled = false;
        exitText.enabled = false;
    }
    //返回主菜单
    public void NoPress()
    {
        quitMenu.enabled = false;
        starText.enabled = true;
        MultiPlayerButton.enabled = false;
        helpText.enabled = true;
        exitText.enabled = true;
    }

    public void YesPress()
    {
        Application.Quit();
    }

    public void StartLevel()
    {
        SceneManager.LoadScene(1);
    }
    //help按钮，显示菜单
    public void HelpPress()
    {
        helpMenu.enabled = true;
        starText.enabled = false;
        MultiPlayerButton.enabled = false;
        helpText.enabled = false;
        exitText.enabled = false;
    }

    //关闭help菜单
    public void HelpMenuOK()
    {
        helpMenu.enabled = false;
        starText.enabled = true;
        MultiPlayerButton.enabled = true;
        helpText.enabled = true;
        exitText.enabled = true;
    }



    //MultiplayerButton
    public void OnClickMultiPlayerButton()
    {
        ConnectMenu.enabled = true;
        starText.enabled = false;
        MultiPlayerButton.enabled = false;
        helpText.enabled = false;
        exitText.enabled = false;
    }
    //连接操作
    public void OnClickConnect()
    {
        launcher.Connect();
    }
    /// <summary>
    /// 取消连接返回主菜单
    /// </summary>
    public void OnClickConnectCancel()
    {
        ConnectMenu.enabled = false;
        starText.enabled = true;
        MultiPlayerButton.enabled = true;
        helpText.enabled = true;
        exitText.enabled = true;
    }

    #endregion

}
