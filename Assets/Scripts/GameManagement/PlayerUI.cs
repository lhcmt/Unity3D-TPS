using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class PlayerUI : MonoBehaviour {

    public Text ammoText;
    public Slider healthBar;
    public Text healthText;
    public GameObject damage_react;
    public Text TaskUI;
    public Canvas taskCanvas;
    public Canvas QuitCanvas;

    void Start()
    {
        taskCanvas.enabled = false;
        QuitCanvas.enabled = false;
    }


    public void TaskCanvasPress()
    {

        taskCanvas.enabled = !taskCanvas.enabled;
    }

    public void  QuitButtonPress()
    {
        QuitCanvas.enabled = !QuitCanvas.enabled;
        if (QuitCanvas.enabled)
            UserInput.UnLockCursor();
        else
            UserInput.LockCursor();
    }
    //离线版
    public void LeftGame()
    {
        SceneManager.LoadScene(0);
    }
}
