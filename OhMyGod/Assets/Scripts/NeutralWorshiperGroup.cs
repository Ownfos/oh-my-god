using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class NeutralWorshiperGroup : MonoBehaviour
{
    // 몇초마다 중립 NPC가 이동 목적지를 재설정하는지
    [SerializeField] private float delayBetweenMovements = 5f;
    [SerializeField] private float randomMoveRange = 20f;
    [SerializeField] private GameObject followTarget; // 소속 NPC들의 이동 목적지

    private List<WorshiperController> worshipers = new List<WorshiperController>();
    public int NumWorshipers { get => worshipers.Count; }

    public float PropagationDuration { get; set; } = 0;

    private AudioSource lureStartSound;

    private float moveTimer = 0f; // 이 타이머가 0이 되면 새로운 이동 목적지를 선택함


    public void InitializeGroup(List<GameObject> worshiperObjects)
    {
        foreach (GameObject worshiperObject in worshiperObjects)
        {
            WorshiperController worshiper = worshiperObject.GetComponent<WorshiperController>();
            if (worshiper != null)
            {
                worshiper.FollowTarget = followTarget; // 그룹 오브젝트를 FollowTarget으로 설정
                worshipers.Add(worshiper);
            }
        }
    }

    // 현재 포교를 시도중인 단체를 구분하는 식별자.
    // 다른 세력의 포교를 중간에 낚아챌 수 없도록 막아준다.
    public WorshipPropagationController currentPropagation = null;

    private void Awake()
    {
        // 내 gameobject의 자식으로 설정된 모든 worshiper 오브젝트를 확인하고 목록으로 정리해둔다.
        // 나중에 민간인 집단 포교 처리에 유용하게 사용될 것...
        // worshipers.AddRange(GetComponentsInChildren<WorshiperController>());
        lureStartSound = GetComponent<AudioSource>();
    }

    public void PlayLureStartSoundEffect()
    {
        lureStartSound.Stop();
        lureStartSound.Play();
    }

    // PlayerController에서 충분한 포교 시간을 확보한 경우 호출하는 함수.
    // 포교 성공 확률에 따라 이 집단에 속한 중립 npc들의 포교 성공/실패 처리를 개별적으로 판단함.
    // 리턴 값으로 포교에 성공한 중립 npc의 수를 반환함.
    // successRate: 포교 성공 확률 (독립 시행)
    // followTarget: 포교에 성공한 경우 따라다닐 오브젝트
    public int PerformPropagation(float successRate, WorshipPropagationController followTarget)
    {
        int numSuccess = 0;
        foreach (WorshiperController worshiper in worshipers)
        {
            // 포교 성공 => 신도로 편입
            if (Random.Range(0f, 1f) < successRate)
            {
                numSuccess++;

                // 신도 목록에 등록하고 따라다니기 시작
                followTarget.AddWorshiper(worshiper);
            }
        }

        return numSuccess;
    }

    void Update()
    {
        // 그룹의 움직임 로직을 여기에 작성합니다.
        // 예를 들어, 마우스 클릭 시 특정 위치로 이동하는 로직을 추가할 수 있습니다.

        // 일정 시간마다 근처의 랜덤한 위치로 이동.
        // 중립 NPC가 게임 내내 가만히 서있지 않도록 해준다.
        moveTimer -= Time.deltaTime;
        if (moveTimer < 0f)
        {
            moveTimer = delayBetweenMovements * Random.Range(0.7f, 1.3f); // 쿨타임도 랜덤하게 설정
            
            if (Random.Range(0f, 1f) < 0.7f)
            {
                ChooseRandomDestination(moveTimer);
            }
        }
    }

    private void ChooseRandomDestination(float duration)
    {
        float angle = Mathf.Deg2Rad * Random.Range(0, 360);
        float newX = transform.position.x + randomMoveRange * Mathf.Cos(angle);
        float newY = transform.position.y + randomMoveRange * Mathf.Sin(angle);
        followTarget.transform.DOMove(new Vector2(newX, newY), duration).SetEase(Ease.InOutSine);
    }
}
