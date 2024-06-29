using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleController : MonoBehaviour
{
   public void StartGame()
    {
        Debug.Log("StartGame button clicked"); 
        SceneManager.LoadScene("GodSelection"); // godSelection 씬으로 전환
    }

    public void ExitGame()
    {
        Debug.Log("ExitGame button clicked");
        Application.Quit(); // 게임 종료
    }
}
