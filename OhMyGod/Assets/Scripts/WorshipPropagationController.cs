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

    public SpriteRenderer SpriteRenderer {get; private set;}
    public List<WorshiperController> ActiveWorshipers {get; private set;}

    // 배틀 중 사용하는 액티브 스킬은 포교로만 모을 수 있는 스킬 게이지를 소모함.
    // 최소 0, 최대 10, 한 명 섭외할 때마다 1씩 회복.
    public int SkillGauge {get; set;} = 0;

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
    public bool IsProtected {get => protectionPeriod > 0f; }

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
        ActiveWorshipers = new();

        SpriteRenderer = GetComponent<SpriteRenderer>();
        battleStartUIController = GetComponent<BattleUIController>();

        // 신도 수 랭킹 관리를 위한 레퍼런스.
        // 자신의 신도 수에 변화가 생길 때마다 랭킹 시스템에 알려준다.
        rankingSystem = GameObject.FindGameObjectWithTag("RankingSystem").GetComponent<RankingSystem>();
    }

    private void Update()
    {
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
        // Note: 한 명은 나갔지만 다른 인원들이 범위 안에 있을 수 있으므로 npc 카운팅을 해줘야 함
        var group = other.gameObject.GetComponentInParent<NeutralWorshiperGroup>();
        if (group != null && group.currentPropagation == this)
        {
            // 포교 범위에서 벗어나면 말랑말랑 모션 끝, 정상 scale 복구
            other.transform.DOKill();
            other.transform.localScale = Vector3.one;
            
            groupEncounterCount[group]--;
            if (groupEncounterCount[group] == 0)
            {
                propagationTargetGroups.Remove(group);
                group.PropagationDuration = 0f;

                // 이제 나는 포교 멈췄다고 마킹.
                // 다른 세력은 이 값이 null이어야만 포교를 시작할 수 있다!
                group.currentPropagation = null;
            }
        }
    }

    // 트리거 범위에 들어온 중립 npc들의 포교 타이머 관리 및 포교 성공 판정 확률 굴리기
    private void UpdatePropagationTime()
    {
        List<NeutralWorshiperGroup> groupsToDelete = new();
        foreach(NeutralWorshiperGroup group in propagationTargetGroups)
        {
            // 얼마나 오랜 시간 포교에 노출되었는가?
            group.PropagationDuration += Time.deltaTime;

            float finalRequiredTime = REQUIRED_PROPAGATION_TIME;
            // 악한 신을 믿는 경우 포교에 필요한 시간이 20% 감소함
            if (SelectedGod == GodType.Evil)
            {
                finalRequiredTime *= 0.8f;
            }
            // 선한 신을 믿는 경우 10초 이내에 포교에 실패한 기록이 있다면 소요 시간이 50% 증가함
            else if (SelectedGod == GodType.Good && propagationSuccessRateDebuffDuration > 0f)
            {
                finalRequiredTime *= 1.5f;
            }

            if (group.PropagationDuration > finalRequiredTime)
            {
                // 포교 성공 판정을 굴린 npc들은 더이상 관리하지 않음 (신도가 되거나 소멸하거나)
                groupsToDelete.Add(group);

                // 이번 그룹에 속한 중립 NPC의 수
                int maxNumSuccess = group.NumWorshipers;

                // 착한 신을 믿는 경우 포교 성공 확률이 10%p 증가함
                float finalSuccessRate = SelectedGod == GodType.Good ? PROPAGATION_SUCCESS_RATE + 0.1f : PROPAGATION_SUCCESS_RATE;
                int numSuccess = group.PerformPropagation(finalSuccessRate, this);

                // 선한 신을 믿는 경우 한 명이라도 포교에 실패하면 10초간 포교 시간 디버프 부여
                if (SelectedGod == GodType.Good && numSuccess != maxNumSuccess)
                {
                    propagationSuccessRateDebuffDuration = 10f;
                }

                // 이상한 신을 믿는 경우 30% 확률로 도플갱어 획득
                if (SelectedGod == GodType.Weird)
                {
                    int numExtraSuccess = 0;
                    for (int i = 0; i < numSuccess; ++i)
                    {
                        if (UnityEngine.Random.Range(0f, 1f) < WEIRED_GOD_BONUS_PROPAGATION_RATE)
                        {
                            numExtraSuccess++;

                            // 마지막 신도 복제
                            WorshiperController worshiper = ActiveWorshipers[ActiveWorshipers.Count - 1];
                            GameObject bonusWorshiper = Instantiate(worshiper.gameObject);
                            AddWorshiper(bonusWorshiper.GetComponent<WorshiperController>());
                        }
                    }
                    numSuccess += numExtraSuccess;
                }


                // 포교 성공한 인원 수만큼 스킬 게이지 회복
                SkillGauge = Math.Clamp(SkillGauge + numSuccess, 0, MAX_SKILL_GUAGE);

                // 집단 포교에 단 한명도 성공하지 못한 경우 패널티로 신도수-1 부여
                if (group.NumWorshipers > 1 && numSuccess == 0)
                {
                    WorshiperController worshiper = ActiveWorshipers[ActiveWorshipers.Count - 1];
                    ActiveWorshipers.Remove(worshiper);

                    worshiper.Die();
                }

                // 신도 수가 바뀌었을 가능성이 있으니 랭킹 재계산
                rankingSystem.RecalculateRank();
            }
        }
        
        foreach (var group in groupsToDelete)
        {
            // 일단 null reference를 피하기 위해 포교 대상 리스트에서 삭제하고
            propagationTargetGroups.Remove(group);

            // 나의 신도가 되지 않은 오브젝트를 포함한 중립 npc 무리의 부모 오브젝트를 없애버림.
            // 난 죽음을 택하겠다!!!
            foreach (var worshiper in group.gameObject.GetComponentsInChildren<WorshiperController>())
            {
                // 부모로부터 detach해서 사망 모션 출력하고 삭제될 수 있도록 만듦
                worshiper.transform.parent = null;
                worshiper.Die();
            }

            // 이제 자식이 없는 빈 부모 오브젝트는 바로 삭제
            Destroy(group.gameObject);
        }
    }

    // 중립 npc의 포교에 성공했을 경우 호출되는 함수.
    // 내 신도 목록을 갱신해서 포교 범위나 카메라 뷰 줌아웃 등에 사용한다.
    public void AddWorshiper(WorshiperController worshiper)
    {
        ActiveWorshipers.Add(worshiper);
        worshiper.FollowTarget = gameObject;

        // 그룹으로 묶고 있던 오브젝트 탈출
        worshiper.gameObject.transform.parent = null;

        // 포교 대상의 종교에 맞게 스프라이트 교체하기
        // TODO: 스프라이트가 아니라 애니메이터 교체가 필요할 수도 있음
        worshiper.GetComponent<SpriteRenderer>().sprite = SpriteRenderer.sprite;

        worshiper.GetComponentInChildren<EmojiController>().PopupEmoji(EmojiType.Happy);

        // 본인을 포함한 신도 수에 비례해 포교범위 조정 (하나 들어갈 때마다 3.5정도 크기 필요)
        // ex) 9명 정도는 3.5짜리 원 안에 들어감
        // ex) 25명 정도는 7짜리 원 안에 들어감
        float desiredRadius = Mathf.Ceil(Mathf.Sqrt(ActiveWorshipers.Count + 1)) * 2.8f;
        propagationRange.transform.localScale = new Vector3(desiredRadius, desiredRadius, desiredRadius);

        // 플레이어인 경우 시야 범위도 조금씩 넓어지게 만듦
        if (cinemachineCamera != null)
        {
            targetCameraLensOrthoSize = 6f + desiredRadius * 0.35f;
        }
    }

    // 한 번 배틀에서 지면 10초간은 연속으로 공격할 수 없도록 보호함
    public void GiveProtectionPeriod()
    {
        protectionPeriod = 10f;
    }
}
