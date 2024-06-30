using UnityEngine;
using UnityEngine.UI;

public class EndingUIController : MonoBehaviour
{
    [SerializeField] private GameObject rankingUI; // RankingUI 오브젝트
    [SerializeField] private GameObject goodEndingUI; // GoodEndingUI 오브젝트
    [SerializeField] private GameObject soSoEndingUI; // SoSoEndingUI 오브젝트
    [SerializeField] private GameObject border; // GoodEndingUI의 Border 오브젝트
    [SerializeField] private Image godImage; // GoodEndingUI의 God 이미지
    [SerializeField] private Image[] playerImages; // GoodEndingUI의 Player 이미지들
    [SerializeField] private PlayerController playerController; // PlayerController 참조

    private void Start()
    {
        // RankingUI를 복제
        GameObject rankingUICopy = Instantiate(rankingUI);

        // RankingUI 복제를 GoodEndingUI의 자식으로 설정
        rankingUICopy.transform.SetParent(goodEndingUI.transform, false);

        // Border 위치를 가져와서 RankingUI 복제의 위치 설정
        RectTransform borderRect = border.GetComponent<RectTransform>();
        RectTransform rankingUICopyRect = rankingUICopy.GetComponent<RectTransform>();
        rankingUICopyRect.anchoredPosition = borderRect.anchoredPosition;
        rankingUICopyRect.sizeDelta = borderRect.sizeDelta;
        rankingUICopyRect.localScale = borderRect.localScale;

        // 플레이어가 선택한 신에 따라 God과 Player 이미지 설정
        if (playerController != null)
        {
            godImage.sprite = playerController.GodSprite;
            foreach (var playerImage in playerImages)
            {
                playerImage.sprite = playerController.WorshiperSprite;
            }
        }
    }
}
