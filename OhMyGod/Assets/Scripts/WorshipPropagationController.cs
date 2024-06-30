using System;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using UnityEngine;

public enum GodType
{
    Good,
    Evil,
    Weird,
}

// 트리거 범위 안에 들어온 중립 npc를
// 일정 시간이 지나면 자동으로 포교하는 스크립트.
// 플레이어와 AI 모두 사용한다.
public class WorshipPropagationController : MonoBehaviour
{
    // 상대 집단과의 숭배 배틀 UI에 등장할 이미지들
    public Sprite GodSprite;
    public Sprite WorshiperSprite;
    public GodType SelectedGod;

    public List<WorshiperController> ActiveWorshipers = new List<WorshiperController>();

    private GameoverController gameoverController;

    // 포교 범위 트리거가 달린 자식 게임 오브젝트.
    // 신도 수에 따라 반지름이 커진다.
    // 주인공인 경우 cinemachineCamera 레퍼런스를 추가로 등록해
    // 신도 수에 비례해서 카메라의 시야를 넓힐 수 있다.
    [SerializeField] private GameObject propagationRange;
    [SerializeField] private CinemachineVirtualCamera cinemachineCamera;

    public GameObject GetPropagationRange()
    {
        return propagationRange;
    }

    [SerializeField] private BattleUIController battleStartUIController;
    public EmojiController EmojiController;

    public SpriteRenderer SpriteRenderer { get; private set; }

    // 배틀 중 사용하는 액티브 스킬은 포교로만 모을 수 있는 스킬 게이지를 소모함.
    // 최소 0, 최대 10, 한 명 섭외할 때마다 1씩 회복.
    public int SkillGauge { get; set; } = 0;

    private const int MAX_SKILL_GUAGE = 10;

    // 중립 npc 포교에 필요한 노출 시간
    // TODO: 종교 특성에 따라 20% 감소 가능하도록 수정
    private const float REQUIRED_PROPAGATION_TIME = 2f;
    // 포교 성공 확률.
    // 포교에 실패하면 중립 npc는 그대로 소멸한다.
    private const float PROPAGATION_SUCCESS_RATE = 0.8f;

    // 이상한 신이 포교에 성공할 때마다 추가적으로 신도를 한 명 추가할 확률
    private const float WEIRED_GOD_BONUS_PROPAGATION_RATE = 0.3f;

    // 배틀에서 진 팀은 잠시동안 다시 배틀을 할 수 없도록 보호함
    private float protectionPeriod = 0f;
    public bool IsProtected { get => protectionPeriod > 0f; }

    // 포교 범위에 들어온 모든 중립 npc 무리와
    // 무리에 속한 npc를 몇 명 동시에 마주쳤는지 세는 카운터.
    // 카운터가 0이 되는 순간에만 propagationTargetGroups에서 제거한다.
    private HashSet<NeutralWorshiperGroup> propagationTargetGroups = new();
    private Dictionary<NeutralWorshiperGroup, int> groupEncounterCount = new();

    // 좋은신을 고른 경우 포섭 확률이 증가하지만 포섭에 실패하면
    // 오히려 10초동안 포섭 시간이 증가하는 디버프를 지님
    private float propagationSuccessRateDebuffDuration = 0f;

    // 악한 신을 고른 경우 포섭 시간이 빨라지는 대신 10초마다 신도를 잃을 위험이 있음
    private const float EVIL_GOD_DEBUFF_CYCLE = 10f;
    private float evilGodDebuffTimer = EVIL_GOD_DEBUFF_CYCLE;
    private float targetCameraLensOrthoSize = 8f;

    // 신도 수 랭킹 관리
    private RankingSystem rankingSystem;

    private void Awake()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
        battleStartUIController = GetComponent<BattleUIController>();

        // 신도 수 랭킹 관리를 위한 레퍼런스.
        // 자신의 신도 수에 변화가 생길 때마다 랭킹 시스템에 알려준다.
        rankingSystem = GameObject.FindGameObjectWithTag("RankingSystem").GetComponent<RankingSystem>();
    }

    private void Start()
    {
        gameoverController = FindObjectOfType<GameoverController>();
    }

    private void Update()
    {
        // 신도 수가 0이면 패배 처리
        if (ActiveWorshipers.Count == 0)
        {
            if (gameObject.CompareTag("Player"))
            {
                gameoverController.ShowBadEnding();
            }
            else
            {
                // 적 AI가 패배한 경우 적 오브젝트를 제거
                Destroy(gameObject);
            }
        }

        // 중립 npc 포교 활동
        UpdatePropagationTime();

        // 주인공인 경우 신도 수에 비례해서 카메라 시야 확장
        if (cinemachineCamera != null)
        {
            cinemachineCamera.m_Lens.OrthographicSize = Mathf.MoveTowards(cinemachineCamera.m_Lens.OrthographicSize, targetCameraLensOrthoSize, Time.deltaTime);
        }

        // 각종 타이머 시간 기록
        protectionPeriod -= Time.deltaTime;
        propagationSuccessRateDebuffDuration -= Time.deltaTime;

        if (SelectedGod == GodType.Evil)
        {
            evilGodDebuffTimer -= Time.deltaTime;
            if (evilGodDebuffTimer < 0f)
            {
                // 타이머 리셋
                evilGodDebuffTimer = EVIL_GOD_DEBUFF_CYCLE;

                // 10% 확률로 신도를 1~3명 잃음
                if (UnityEngine.Random.Range(0f, 1f) < 0.1f)
                {
                    // 배열의 마지막에서부터 한 명씩 처리
                    int NumWorshipersLost = UnityEngine.Random.Range(1, 3);
                    for (int i = 0; i < NumWorshipersLost; ++i)
                    {
                        int lastWorshiperIndex = ActiveWorshipers.Count - 1;

                        // 이미 모든 신도를 잃었다는 뜻이므로 더 죽이지 않음
                        if (lastWorshiperIndex == -1)
                        {
                            break;
                        }

                        ActiveWorshipers[lastWorshiperIndex].Die();
                        ActiveWorshipers.RemoveAt(lastWorshiperIndex);
                    }

                    // 신도 수가 바뀌었으니 랭킹 재계산
                    rankingSystem.RecalculateRank();
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 중립 npc 집단(1인 이상)이 포교 범위에 들어왔는지 체크.
        // 두 번째 if문 조건은 다른 집단의 포교가 진행중이지 않을 때에만 참이 되어서
        // 남의 포교 활동을 중간에 가로챌 수 없도록 막아줌.
        var group = other.gameObject.GetComponentInParent<NeutralWorshiperGroup>();
        if (group != null && (group.currentPropagation == this || group.currentPropagation == null))
        {
            // 이제 내가 포교 중이라고 마킹
            group.currentPropagation = this;

            // 포교 범위에 들어온 동안 말랑말랑 모션
            other.transform.DOShakeScale(2f, 0.2f, 3, 30);

            propagationTargetGroups.Add(group);

            // 만약 이 집단을 처음 만나는 경우라면 집단 내의 유닛 중 만난 수를 0으로 초기화
            groupEncounterCount.TryAdd(group, 0);

            // 이 집단 소속의 중립 npc를 한 명 더 만났다고 기록
            groupEncounterCount[group]++;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // 나도 상대도 배틀 배패로 인해 보호 기간이 아닌 상태에서
        // 상대 종교 집단과의 조우 => 숭배 배틀 시작
        var enemyGroupController = other.gameObject.GetComponentInParent<WorshipPropagationController>();
        if (enemyGroupController != null && !IsProtected && !enemyGroupController.IsProtected)
        {
            TryStartBattle(enemyGroupController);
        }
    }

    private void TryStartBattle(WorshipPropagationController enemyGroupController)
    {
        // Note: 이 컴포넌트는 플레이어만 갖고있어서 상대방도 배틀을 시작하려 하는 것을 막아줌
        // TriggerEnter 이벤트가 배틀 시작할 때 여러 번 발동할 수 있으므로 active가 아닌 경우에만 배틀을 새로 시작함...
        if (battleStartUIController != null && !battleStartUIController.isBattleActive)
        {
            EmojiController.PopupEmoji(EmojiType.Surprise);
            enemyGroupController.EmojiController.PopupEmoji(EmojiType.Surprise);
            battleStartUIController.PlayBattleStartUI(this, enemyGroupController);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // 중립 npc 집단(1인 이상)의 모든 인원이 포교 범위에서 나갔는지 체크.
        // 두 번째 조건은 내가 포교중이 아니라면 propagationTargetGroups에도
        // group이 존재하지 않으므로 없는 엔트리를 제거하는 버그를 막아준다.
        var group = other.game
