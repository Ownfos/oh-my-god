using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

// 종교 배틀 시작할 때 나오는 컷신 느낌의 UI 조작을 담당하는 클래스
public class BattleStartUIController : MonoBehaviour
{
    [SerializeField] private RectTransform curtain; // 공연 시작 느낌으로 샥 나타났다가 사라질 커튼 느낌의 배경
    [SerializeField] private RectTransform darkBackground; // 일반 맵을 가려줄 검은 배경
    [SerializeField] private RectTransform battleIntroText; // "당신의 우상에게 숭배하세요" 문구
    [SerializeField] private RectTransform leftBattleUI; // 좌측 진영 UI
    [SerializeField] private Image leftGodImage;
    [SerializeField] private RectTransform rightBattleUI; // 우측 진영 UI
    [SerializeField] private Image rightGodImage;

    // 현재 숭배 배틀이 진행중인가?
    // 이미 시작되었는데 또 시작하는 상황을 방지함
    public bool isBattleActive {get; private set;} = false;

    private void Start()
    {
        // PlayBattleStartUI(); // 테스트 목적으로 바로 재생
    }

    public void PlayBattleStartUI(WorshipPropagationController leftTeam, WorshipPropagationController rightTeam, Action onComplete)
    {
        isBattleActive = true;

        // 진영별 스프라이트 설정
        leftGodImage.sprite = leftTeam.GodSprite;
        rightGodImage.sprite = rightTeam.GodSprite;

        // 1. 커튼이 샥 하고 화면을 가림
        curtain.DOMoveX(Screen.width *0.5f, 1f).SetEase(Ease.OutSine).OnComplete(() => {
            // 2. 배경이 검은색으로 바뀜
            darkBackground.gameObject.SetActive(true);

            // 3. "당신의 우상에게 숭배하세요" 문구 표시
            battleIntroText.transform.DOScale(1f, 2f).SetEase(Ease.OutBounce).OnComplete(() => {
                // 4. 문구 다시 숨기기
                battleIntroText.transform.DOScale(0f, 1f).SetEase(Ease.InCubic).OnComplete(() => {
                    // 5. 커튼 반대편으로 샥 하고 사라지며
                    curtain.transform.DOMoveX(Screen.width * 1.5f, 1f).SetEase(Ease.OutSine).OnComplete(() => {
                        // 6. 양쪽에서 숭배 배틀 UI가 가운데로 쾅 하며 등장
                        leftBattleUI.DOMoveX(0f, 1f).SetEase(Ease.OutBounce);
                        rightBattleUI.DOMoveX(Screen.width, 1f).SetEase(Ease.OutBounce).OnComplete(() => onComplete());
                    });
                });

            });
        });

    }

    // 숭배 배틀을 시작하기 전의 상태로 복원한다.
    public void ResetBattleUI()
    {
        isBattleActive = false;
        darkBackground.gameObject.SetActive(false);
        curtain.position = new Vector2(-Screen.width * 0.5f, Screen.height * 0.5f);
        leftBattleUI.position = new Vector2(-Screen.width * 0.5f, Screen.height * 0.5f);
        rightBattleUI.position = new Vector2(Screen.width * 1.5f, Screen.height * 0.5f);
    }
}
