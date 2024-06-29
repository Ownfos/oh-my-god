using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleController : MonoBehaviour
{
   public void StartGame()
    {
        SceneManager.LoadScene("GodSelection"); // godSelection 씬으로 전환
        void StartGame()
{
    // 게임 시작 로직을 여기에 구현합니다.
    Debug.Log("Game Started"); // 디버그 로그 추가

    // 오브젝트 활성화 상태와 위치를 확인하는 디버그 로그 추가
    foreach (var obj in gameObjects)
    {
        if (obj != null)
        {
            Debug.Log(obj.name + " is at position " + obj.transform.position + " and is active: " + obj.activeSelf);
        }
        else
        {
            Debug.LogError("gameObjects 배열에 null 오브젝트가 있습니다.");
        }
    }
}

    }

    public void ExitGame()
    {
        Application.Quit(); // 게임 종료
    }
}
