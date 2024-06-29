using UnityEngine;

// 한 명 이상의 민간인 무리의 포교 처리를 담당하는 클래스
public class NeutralWorshiperGroup : MonoBehaviour
{
    private WorshiperController[] worshipers;
    public int NumWorshipers { get => worshipers.Length; }

    public float PropagationDuration {get; set;} = 0;

    private void Start()
    {
        // 내 gameobject의 자식으로 설정된 모든 worshiper
        // 오브젝트를 확인하고 목록으로 정리해둔다.
        //
        // 나중에 민간인 집단 포교 처리에 유용하게 사용될 것...
        worshipers = GetComponentsInChildren<WorshiperController>();
    }

    // PlayerController에서 충분한 포교 시간을 확보한 경우 호출하는 함수.
    // 포교 성공 확률에 따라 이 집단에 속한 중립 npc들의 포교 성공/실패 처리를 개별적으로 판단함.
    //
    // 리턴 값으로 포교에 성공한 중립 npc의 수를 반환함.
    //
    // successRate: 포교 성공 확률 (독립 시행)
    // followTarget: 포교에 성공한 경우 따라다닐 오브젝트
    public int PerformPropagation(float successRate, GameObject followTarget)
    {
        int numSuccess = 0;
        foreach (WorshiperController worshiper in worshipers)
        {
            // 포교 성공 => 신도로 편입
            if (Random.Range(0f, 1f) < successRate)
            {
                numSuccess++;
                worshiper.FollowTarget = followTarget;

                // 그룹으로 묶고 있던 오브젝트 탈출
                worshiper.gameObject.transform.parent = null;
                
                // TODO: 포교 대상의 종교에 맞게 스프라이트 교체하기
            }
        }

        return numSuccess;
    }
}
