using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleController : MonoBehaviour
{
   public void StartGame()
    {
        SceneManager.LoadScene("GodSelection"); // godSelection 씬으로 전환
    }

    public void ExitGame()
    {
        Application.Quit(); // 게임 종료
    }
}
