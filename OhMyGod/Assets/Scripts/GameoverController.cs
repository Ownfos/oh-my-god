using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameoverController : MonoBehaviour
{
    [SerializeField] private AudioSource gameoverSound;
    [SerializeField] private RectTransform badEndingUI;
    [SerializeField] private RectTransform goodEndingUI;
    [SerializeField] private RectTransform soSoEndingUI;
    [SerializeField] private RankingSystem rankingSystem;
    [SerializeField] private Text goodRank1;
    [SerializeField] private Text goodRank2;
    [SerializeField] private Text goodRank3; 
    [SerializeField] private Text sosoRank1;
    [SerializeField] private Text sosoRank2;
    [SerializeField] private Text sosoRank3;

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

        // BadEndingUI 활성화
        badEndingUI.gameObject.SetActive(true);

        // BadEndingUI 크기와 위치 설정
        badEndingUI.localScale = Vector3.zero; // 처음에는 크기를 0으로 설정

        // BadEndingUI를 화면 중앙으로 이동시키기 위해 앵커 설정
        badEndingUI.anchoredPosition = Vector2.zero; // 화면 중앙으로 이동

        // 애니메이션으로 크기를 키우기
        badEndingUI.DOScale(1f, 1f).SetEase(Ease.OutBounce);
    }

    public void ShowGoodEnding()
    {
        BlockPlayerInput();
        goodEndingUI.gameObject.SetActive(true);
        goodEndingUI.localScale = Vector3.zero; // 처음에는 크기를 0으로 설정
        goodEndingUI.anchoredPosition = Vector2.zero; // 화면 중앙으로 이동
        goodEndingUI.DOScale(1f, 1f).SetEase(Ease.OutBounce);
        goodEndingUI.GetComponent<EndingUIController>().SyncSpritesToPlayerGod();

        goodRank1.text = "PLAYER";
        goodRank2.text = rankingSystem.GetSecondRankName();
        goodRank3.text = rankingSystem.GetThirdRankName();
    }

    public void ShowSoSoEnding()
    {
        BlockPlayerInput();
        soSoEndingUI.gameObject.SetActive(true);
        soSoEndingUI.localScale = Vector3.zero; // 처음에는 크기를 0으로 설정
        soSoEndingUI.anchoredPosition = Vector2.zero; // 화면 중앙으로 이동
        soSoEndingUI.DOScale(1f, 1f).SetEase(Ease.OutBounce);
        soSoEndingUI.GetComponent<EndingUIController>().SyncSpritesToPlayerGod();

        

        sosoRank1.text = rankingSystem.GetFirstRankName();
        sosoRank2.text = rankingSystem.GetSecondRankName();
        sosoRank3.text = rankingSystem.GetThirdRankName();
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

        // 모든 적의 AI 비활성화
        var enemies = FindObjectsOfType<RandomMovementAI2D>();
        foreach (var enemy in enemies)
        {
            enemy.gameObject.SetActive(false);
        }

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


    // 남은 적 두 명의 신도 수가 0이 된 경우 호출
    public void CheckForEarlyVictory()
    {
        if (rankingSystem.IsEarlyVictory())
        {
            ShowGoodEnding();
        }
    }
}
