using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private Ease moveEase = Ease.OutSine; // 처음엔 빠르고 도착할 때 감속
    [SerializeField] private Slider dashGaugeSlider;

    private InputActions inputActions;
    private Rigidbody2D rb;
    private Vector3 moveDestination;

    // 포교 범위에 들어온 모든 중립 npc 무리
    private HashSet<NeutralWorshiperGroup> propagationTargetGroups = new();

    // 중립 npc 포교에 필요한 노출 시간
    // TODO: 종교 특성에 따라 20% 감소 가능하도록 수정
    private const float REQUIRED_PROPAGATION_TIME = 2f;
    // 포교 성공 확률.
    // 포교에 실패하면 중립 npc는 그대로 소멸한다.
    private const float PROPAGATION_SUCCESS_RATE = 0.5f;

    private const float MAX_DASH_GAUGE = 10f;
    private const float DASH_REPLENISH_TIME = 5f;

    // 잔여 대쉬 사용 시간
    private float dashGauge = 10f;
    private enum DashState
    {
        None, // 일반 걷기
        Dashing, // 대쉬 게이지 소모중
        Replenishing, // 대쉬 게이지 충전중
    }
    private DashState dashState = DashState.None;

    private void Awake()
    {
        inputActions = new InputActions();
        inputActions.Player.Enable();
    }

    private void OnEnable()
    {
        inputActions.Player.MouseClick.performed += ChangeDestination;
        inputActions.Player.Dash.performed += ToggleDash;
    }

    private void OnDisable()
    {
        inputActions.Player.MouseClick.performed -= ChangeDestination;
        inputActions.Player.Dash.performed -= ToggleDash;
    }

    private void ToggleDash(InputAction.CallbackContext context)
    {
        // 재충전 중에는 대쉬 처리 x
        if (dashState == DashState.Replenishing)
        {
            return;
        }

        // Case 1) 버튼 누름 & 게이지가 충분함 & 충전중 아님 => 이속 증가
        var isPressed = inputActions.Player.Dash.IsPressed();
        if (isPressed && dashGauge > 0f)
        {
            dashState = DashState.Dashing;

            // 빨라진 이동 속도 반영
            RefreshMovementTween();
        }
        // Case 2) 버튼 뗌 => 일반적인 걷기 상태로 복귀 (이미 걷기 상태여도 ok)
        else if (!isPressed)
        {
            dashState = DashState.None;
        }
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // 초기 위치에서 가만히 서있도록 설정
        moveDestination = rb.position;
    }

    private void Update()
    {
        // 대쉬 게이지
        UpdateDashGauge();

        // 중립 npc 포교 활동
        UpdatePropagationTime();
    }

    // 대쉬 사용 또는 재충전에 의한 게이지 변동 처리
    private void UpdateDashGauge()
    {
        if (dashState == DashState.Dashing)
        {
            // 매 초마다 1씩 게이지 감소
            dashGauge -= Time.deltaTime;

            // 게이지 전부 소모하면 재충전 시작.
            // 재충전 도중에는 대쉬 사용 불가!
            if (dashGauge <= 0f)
            {
                dashState = DashState.Replenishing;

                // 대쉬가 끝났으니 기본 이동속도로 돌아가야 함
                RefreshMovementTween();
            }
        }
        else if (dashState == DashState.Replenishing)
        {
            // 재충전 시간동안 0부터 최대치까지 게이지 회복
            dashGauge += MAX_DASH_GAUGE / DASH_REPLENISH_TIME * Time.deltaTime;

            // 충전 완료!
            if (dashGauge >= MAX_DASH_GAUGE)
            {
                dashGauge = MAX_DASH_GAUGE;
                dashState = DashState.None;
            }
        }

        dashGaugeSlider.value = dashGauge / MAX_DASH_GAUGE;
    }

    private void ChangeDestination(InputAction.CallbackContext context)
    {
        var mouseScreenPos = inputActions.Player.MousePos.ReadValue<Vector2>();
        moveDestination = mainCamera.ScreenToWorldPoint(mouseScreenPos);

        RefreshMovementTween();
    }

    // 대쉬의 활성화/비활성화, 그리고 새로운 목표 지점 선택에 따른 rigidbody 이동 처리
    private void RefreshMovementTween()
    {
        Vector2 displacement = rb.transform.position - moveDestination;
        float distance = displacement.magnitude;
        float duration = distance / CurrentMoveSpeed();

        // 기존 이동 취소
        rb.DOKill();

        // 목표 지점으로 다시 이동 시작
        rb.DOMove(moveDestination, duration).SetEase(moveEase);
    }

    // 대쉬를 고려한 이동 속도
    private float CurrentMoveSpeed()
    {
        if (dashState == DashState.Dashing)
        {
            return dashSpeed;
        }
        else
        {
            return moveSpeed;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 중립 npc 집단(1인 이상)이 포교 범위에 들어왔는지 체크
        var group = other.gameObject.GetComponentInParent<NeutralWorshiperGroup>();
        if (group != null)
        {
            // TODO: 내가 먼저 포교를 시작한게 맞는지 확인할 것!
            // 적이 먼저 포교중이었다면 나는 우선 순위가 낮아서 처리하면 안됨
            propagationTargetGroups.Add(group);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // 중립 npc 집단(1인 이상)이 포교 범위에서 나갔는지 체크
        var group = other.gameObject.GetComponentInParent<NeutralWorshiperGroup>();
        if (group != null)
        {
            propagationTargetGroups.Remove(group);
            group.PropagationDuration = 0f;
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
                int numSuccess = group.PerformPropagation(PROPAGATION_SUCCESS_RATE, gameObject);

                // 집단 포교에 단 한명도 성공하지 못한 경우 패널티로 신도수-1 부여
                if (group.NumWorshipers > 1 && numSuccess == 0)
                {
                    // TODO: 내 신도 한 명 없애기
                    Debug.Log("집단 전도에 완전히 실패해 신도 수가 감소합니다...");
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
}
