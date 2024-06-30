using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameoverController : MonoBehaviour
{
    [SerializeField] private AudioSource gameoverSound;
    [SerializeField] private RectTransform badEndingUI;
    [SerializeField] private RectTransform goodEndingUI;
    [SerializeField] private RankingSystem rankingSystem;

    private void BlockPlayerInput()
    {
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().enabled = false;
    }

    // 배틀에서 패배해 제한시간이 다 지나지 않았는데 게임오버된 경우
    public void ShowBadEnding()
    {
        gameoverSound.Stop();
        gameoverSound.Play();
        
        BlockPlayerInput();
        badEndingUI.DOMoveY(Screen.height / 2f, 1f).SetEase(Ease.OutBounce);
    }

    public void OnRestartButtonClick()
    {
        // TODO: 페이딩하고 끝나면 DOKillAll
        SceneManager.LoadScene("GodSelection");
    }

    public void OnMainMenuButtonClick()
    {
        // TODO: 페이딩하고 끝나면 DOKillAll
        SceneManager.LoadScene("TitleScene");
    }

    // 미션 제한 시간이 다 되어서 게임이 끝난 경우 TimeoutController의 이벤트에 의해 호출됨
    public void OnTimeout()
    {
        BGMController.Instance.SwitchToGameClearBGM();

        // 1위로 마무리
        if (rankingSystem.FindPlayerRank() == 0)
        {
            ShowGoodEnding();
        }
        // 2위 이하로 마무리
        else
        {
            ShowSoSoEnding();
        }
    }

    // 제한시간은 가득 채웠지만 1등은 아닌 경우
    public void ShowSoSoEnding()
    {
        // TODO: 
    }

    // 1등으로 게임을 마무리한 경우
    public void ShowGoodEnding()
    {
        BlockPlayerInput();
        goodEndingUI.DOScale(1f, 1f).SetEase(Ease.OutBounce);
    }
}
