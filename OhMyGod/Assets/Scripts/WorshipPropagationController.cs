using System.Collections.Generic;
using UnityEngine;

// 트리거 범위 안에 들어온 중립 npc를
// 일정 시간이 지나면 자동으로 포교하는 스크립트.
// 플레이어와 AI 모두 사용한다.
public class WorshipPropagationController : MonoBehaviour
{
    // 포교 범위 트리거가 달린 자식 게임 오브젝트.
    // 신도 수에 따라 반지름이 커진다.
    [SerializeField] private GameObject propagationRange;

    public SpriteRenderer SpriteRenderer {get; private set;}
    public List<WorshiperController> ActiveWorshipers {get; private set;}

    // 중립 npc 포교에 필요한 노출 시간
    // TODO: 종교 특성에 따라 20% 감소 가능하도록 수정
    private const float REQUIRED_PROPAGATION_TIME = 2f;
    // 포교 성공 확률.
    // 포교에 실패하면 중립 npc는 그대로 소멸한다.
    private const float PROPAGATION_SUCCESS_RATE = 0.8f;

    // 포교 범위에 들어온 모든 중립 npc 무리와
    // 무리에 속한 npc를 몇 명 동시에 마주쳤는지 세는 카운터.
    // 카운터가 0이 되는 순간에만 propagationTargetGroups에서 제거한다.
    private HashSet<NeutralWorshiperGroup> propagationTargetGroups = new();
    private Dictionary<NeutralWorshiperGroup, int> groupEncounterCount = new();

    private void Awake()
    {
        ActiveWorshipers = new();

        SpriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // 중립 npc 포교 활동
        UpdatePropagationTime();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 중립 npc 집단(1인 이상)이 포교 범위에 들어왔는지 체크.
        // 두 번째 if문 조건은 내가 처음 포교를 시도하는 것인지 확인해줌.
        //
        // Note:
        // 만약 먼저 포교를 시도하던 상대방이 있다면
        // 무조건 PropagationDuration이 0 이상이기 때문에
        // 이 값이 0이라면 내가 첫 순서라고 확신할 수 있음
        var group = other.gameObject.GetComponentInParent<NeutralWorshiperGroup>();
        if (group != null && Mathf.Approximately(group.PropagationDuration, 0f))
        {
            propagationTargetGroups.Add(group);

            // 만약 이 집단을 처음 만나는 경우라면 집단 내의 유닛 중 만난 수를 0으로 초기화
            groupEncounterCount.TryAdd(group, 0);

            // 이 집단 소속의 중립 npc를 한 명 더 만났다고 기록
            groupEncounterCount[group]++;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // 중립 npc 집단(1인 이상)의 모든 인원이 포교 범위에서 나갔는지 체크
        // Note: 한 명은 나갔지만 다른 인원들이 범위 안에 있을 수 있으므로 npc 카운팅을 해줘야 함
        var group = other.gameObject.GetComponentInParent<NeutralWorshiperGroup>();
        if (group != null)
        {
            groupEncounterCount[group]--;
            if (groupEncounterCount[group] == 0)
            {
                propagationTargetGroups.Remove(group);
                group.PropagationDuration = 0f;
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

            // TODO: 종교 특성에 따라 필요 시간 단축 가능하도록 수정
            if (group.PropagationDuration > REQUIRED_PROPAGATION_TIME)
            {
                // 포교 성공 판정을 굴린 npc들은 더이상 관리하지 않음 (신도가 되거나 소멸하거나)
                groupsToDelete.Add(group);

                // TODO: 종교 특성에 따라 성공 확률 달라지도록 수정
                int numSuccess = group.PerformPropagation(PROPAGATION_SUCCESS_RATE, this);

                // 집단 포교에 단 한명도 성공하지 못한 경우 패널티로 신도수-1 부여
                if (group.NumWorshipers > 1 && numSuccess == 0)
                {
                    WorshiperController worshiper = ActiveWorshipers[ActiveWorshipers.Count - 1];
                    ActiveWorshipers.Remove(worshiper);

                    Destroy(worshiper);
                }
            }
        }
        
        foreach (var group in groupsToDelete)
        {
            // 일단 null reference를 피하기 위해 포교 대상 리스트에서 삭제하고
            propagationTargetGroups.Remove(group);

            // 나의 신도가 되지 않은 오브젝트를 포함한 중립 npc 무리의 부모 오브젝트를 없애버림.
            // 난 죽음을 택하겠다!!!
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

        // 신도 수에 비례해 포교범위 조정 (하나 들어갈 때마다 3.5정도 크기 필요)
        // ex) 9명 정도는 3.5짜리 원 안에 들어감
        // ex) 25명 정도는 7짜리 원 안에 들어감
        float desiredRadius = Mathf.Ceil(Mathf.Sqrt(ActiveWorshipers.Count)) * 3.5f;
        propagationRange.transform.localScale = new Vector3(desiredRadius, desiredRadius, desiredRadius);
    }
}
