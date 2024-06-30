using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

// 종교 배틀 시작할 때 나오는 컷신 느낌의 UI 조작을 담당하는 클래스.
// 전투 진행도 마찬가지로 맡아서 진행한다.
public class BattleUIController : MonoBehaviour
{
    [SerializeField] private RectTransform curtain; // 공연 시작 느낌으로 샥 나타났다가 사라질 커튼 느낌의 배경
    [SerializeField] private RectTransform skyBackground; // 일반 맵을 가려줄 배경
    [SerializeField] private Text battleIntroText; // "당신의 우상에게 숭배하세요" 문구
    [SerializeField] private RectTransform upperBattleUI; // 신전 천장
    [SerializeField] private Image leftGodImage;
    [SerializeField] private Slider leftWorshipGauge;
    [SerializeField] private RectTransform lowerBattleUI; // 신전 본체를 비롯한 각종 핵심 UI
    [SerializeField] private Image rightGodImage;
    [SerializeField] private Slider rightWorshipGauge;
    [SerializeField] private GameoverController gameoverController;
    [SerializeField] private RankingSystem rankingSystem;
    [SerializeField] private ArrowButtonMinigame arrowButtonMinigame;
    [SerializeField] private AudioSource battleBeginSound;

    // 현재 숭배 배틀이 진행중인가?
    // 이미 시작되었는데 또 시작하는 상황을 방지함
    public bool isBattleActive {get; private set;} = false;
    private bool isMainBattleStarted = false;
    private WorshipPropagationController leftTeam;
    private WorshipPropagationController rightTeam;

    // 다음 미니게임이 팝업되기까지 기다려야하는 시간
    private float arrowButtonMinigameCooltime = 0;

    // 미니게임에 성공해서 게이지 상승 속도 버프를 받을 잔여 시간
    private float minigameBuffDuration = 0f;

    public void PlayBattleStartUI(WorshipPropagationController leftTeam, WorshipPropagationController rightTeam)
    {
        Debug.Log("배틀 시작");
        isBattleActive = true;
        this.leftTeam = leftTeam;
        this.rightTeam = rightTeam;
        RandomizeMinigameCooltime();

        // 땡땡땡 하는 효과음
        battleBeginSound.Stop();
        battleBeginSound.Play();

        // 플레이어 입력 막기
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().enabled = false;

        // 진영별 스프라이트 설정
        leftGodImage.sprite = leftTeam.GodSprite;
        rightGodImage.sprite = rightTeam.GodSprite;

        // 같은 신을 숭배하는지 아닌지에 따라 인트로 멘트가 달라짐
        // Note: 같은 스프라이트 레퍼런스를 사용하므로 단순 비교도 괜찮을 것
        if (leftTeam.GodSprite == rightTeam.GodSprite)
        {
            battleIntroText.text = "누가 더 신앙심이 깊을까?";
        }
        else
        {
            battleIntroText.text = "당신의 우상을 숭배하세요!";
        }

        // 숭배 게이지 초기화
        leftWorshipGauge.value = 0f;
        rightWorshipGauge.value = 0f;

        // 0. 2초간 대기 (느낌표 등 말풍선 표시??)
        float dummy = 1f;
        DOTween.To(() => dummy, (value) => {dummy = value;}, 1f, 2f).OnComplete(()=>{
            // 기본 브금 잠시 멈추고 전투 브금 시작
            BGMController.Instance.StartBattleBGM();

            // 1. 커튼이 샥 하고 화면을 가림
            curtain.DOMoveX(Screen.width *0.5f, 1f).SetEase(Ease.OutSine).OnComplete(() => {
                // 2. 배경이 검은색으로 바뀜
                skyBackground.gameObject.SetActive(true);

                // 3. "당신의 우상에게 숭배하세요" 문구 표시
                battleIntroText.transform.DOScale(1f, 2f).SetEase(Ease.OutBounce).OnComplete(() => {
                    // 4. 문구 다시 숨기기
                    battleIntroText.transform.DOScale(0f, 1f).SetEase(Ease.InCubic).OnComplete(() => {
                        // 5. 커튼 반대편으로 샥 하고 사라지며
                        curtain.transform.DOMoveX(Screen.width * 1.5f, 1f).SetEase(Ease.OutSine).OnComplete(() => {
                            // 6. 양쪽에서 숭배 배틀 UI가 가운데로 쾅 하며 등장
                            upperBattleUI.DOMoveY(Screen.height / 2f, 1f).SetEase(Ease.OutBounce);
                            lowerBattleUI.DOMoveY(Screen.height / 2f, 1f).SetEase(Ease.OutBounce).OnComplete(() => {
                                // 조작 가능한 숭배 배틀 시작
                                isMainBattleStarted = true;
                            });
                        });
                    });
                });
            });
        });
    }

    public void OnMinigameComplete()
    {
        bool successful = arrowButtonMinigame.IsAllCorrect;
        Debug.Log($"미니게임 결과: {successful}");
        if (successful)
        {
            minigameBuffDuration = 0.5f;
        }
    }

    private void RandomizeMinigameCooltime()
    {
        arrowButtonMinigameCooltime = UnityEngine.Random.Range(4f, 7f);
    }

    private void Update()
    {
        if (isMainBattleStarted)
        {
            IncreaseWorshipGauge(leftTeam, leftWorshipGauge);
            IncreaseWorshipGauge(rightTeam, rightWorshipGauge);

            // 미니게임 쿨타임 차면 방향키 미니게임 시작하기
            minigameBuffDuration -= Time.deltaTime;
            if (!arrowButtonMinigame.IsMinigameActive)
            {
                arrowButtonMinigameCooltime -= Time.deltaTime;
                if (arrowButtonMinigameCooltime < 0f)
                {
                    RandomizeMinigameCooltime();
                    arrowButtonMinigame.StartNewMinigame();
                }
            }

            if (leftWorshipGauge.value >= 1f)
            {
                EndBattle(leftTeam, rightTeam);
            }
            else if (rightWorshipGauge.value >= 1f)
            {
                EndBattle(rightTeam, leftTeam);
            }
        }
    }

    // magic number 남발이라 이해하기 난해한데 그냥 수식이 원래부터 이렇습니다...
    // 대충 신도가 많으면 숭배 게이지를 금방 채운다는 뜻.
    private void IncreaseWorshipGauge(WorshipPropagationController propagationController, Slider slider)
    {
        float increaseSpeed = 1f;

        int NumWorshipers = propagationController.ActiveWorshipers.Count;
        if (NumWorshipers > 10)
        {
            increaseSpeed = Mathf.Clamp(Mathf.Log(NumWorshipers, 10f), 1f, 3f);
        }

        increaseSpeed *= 2f / 100f;

        // 주인공(왼쪽 팀)에게 미니게임 성공에 따른 게이지 상승 속도 버프를 부여
        if (minigameBuffDuration > 0f && propagationController == leftTeam)
        {
            increaseSpeed *= 10f;
        }

        slider.value += increaseSpeed * Time.deltaTime;
    }

    // 승패 결과를 정산하고 모든 UI 및 상태 변수를
    // 배틀을 시작하기 전의 상태로 복원한다.
    public void EndBattle(WorshipPropagationController winTeam, WorshipPropagationController loseTeam)
    {
        BGMController.Instance.ResumeMainGameBGM();

        isMainBattleStarted = false;
        skyBackground.gameObject.SetActive(false);
        curtain.position = new Vector2(-Screen.width * 0.5f, Screen.height * 0.5f);
        upperBattleUI.DOMoveY(Screen.height * 1.5f, 1f).SetEase(Ease.OutSine);
        lowerBattleUI.DOMoveY(Screen.width * -0.5f, 1f).SetEase(Ease.OutSine).OnComplete(()=>{
            isBattleActive = false;

            // 이모지 띄우기
            loseTeam.EmojiController.PopupEmoji(EmojiType.Sad);
            winTeam.EmojiController.PopupEmoji(EmojiType.Celebrate);

            // 플레이어 입력 허용하기
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().enabled = true;

            // 이긴 팀에게 신도의 절반 이동.
            // 만약 신도가 3명 이하라면 즉시 맵에서 퇴출
            if (loseTeam.ActiveWorshipers.Count <= 3)
            {
                for (int i = 0;  i < loseTeam.ActiveWorshipers.Count; ++i)
                {
                    loseTeam.ActiveWorshipers[i].transform.parent = null;
                    loseTeam.ActiveWorshipers[i].Die();
                }

                // 플레이어가 죽는 경우 게임오버 처리하기
                if (loseTeam.gameObject.CompareTag("Player"))
                {
                    gameoverController.ShowBadEnding();
                }
                // 적이었다면 그냥 삭제
                else
                {
                    rankingSystem.RemoveCompetitor(loseTeam);
                    Destroy(loseTeam.gameObject);
                }
            }
            else
            {
                // 기본적으로는 배틀에서 패배하면 절반의 신도를 빼앗기지만
                // 이상한 신을 믿는 경우 디버프로 인해 2/3을 빼앗김
                int previousWorshiperCount = loseTeam.ActiveWorshipers.Count;
                int removeIndexLowerbound = previousWorshiperCount / 2;
                if (loseTeam.SelectedGod == GodType.Weird)
                {
                    removeIndexLowerbound = previousWorshiperCount * 2 / 3;
                }

                for (int i = previousWorshiperCount - 1;  i > removeIndexLowerbound; --i)
                {
                    winTeam.AddWorshiper(loseTeam.ActiveWorshipers[i]);
                    loseTeam.ActiveWorshipers.RemoveAt(i);
                }
            }

            // 진 팀에 10초간 보호기간 부여
            loseTeam.GiveProtectionPeriod();

            // 신도 수 및 경쟁자 현황에 변화가 생기었으니 랭킹 재계산
            rankingSystem.RecalculateRank();
        });

    }
}
