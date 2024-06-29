using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameoverController : MonoBehaviour
{
    [SerializeField] private RectTransform badEndingUI;

    private void BlockPlayerInput()
    {
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().enabled = false;
    }

    // 배틀에서 패배해 제한시간이 다 지나지 않았는데 게임오버된 경우
    public void ShowBadEnding()
    {
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

    // 제한시간은 가득 채웠지만 1등은 아닌 경우
    public void ShowSoSoEnding()
    {
        // TODO: 순위 시스템 만들고 나면 완성합시다
    }

    // 1등으로 게임을 마무리한 경우
    public void ShowGoodEnding()
    {
        // TODO: 순위 시스템 만들고 나면 완성합시다
    }
}
