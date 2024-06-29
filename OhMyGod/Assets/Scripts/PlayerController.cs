using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Sprite goodGodWorshiper;
    [SerializeField] private Sprite evilGodWorshiper;
    [SerializeField] private Sprite weirdGodWorshiper;
    [SerializeField] private Sprite goodGod;
    [SerializeField] private Sprite evilGod;
    [SerializeField] private Sprite weirdGod;

    [SerializeField] private Camera mainCamera;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private Ease moveEase = Ease.OutSine; // 처음엔 빠르고 도착할 때 감속
    [SerializeField] private Slider dashGaugeSlider;

    private InputActions inputActions;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector3 moveDestination;

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
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 초기 위치에서 가만히 서있도록 설정
        moveDestination = rb.position;

        // 플레이어가 숭배하기로 한 신에 맞는 신도의 스프라이트를 사용
        WorshipPropagationController propagationController = GetComponent<WorshipPropagationController>();
        string godName = SessionData.Instance.SelectedGod;
        if (godName == "Strange")
        {
            spriteRenderer.sprite = weirdGodWorshiper;
            propagationController.WorshiperSprite = weirdGodWorshiper;
            propagationController.GodSprite = weirdGod;
        }
        else if (godName == "Good")
        {
            spriteRenderer.sprite = goodGodWorshiper;
            propagationController.WorshiperSprite = goodGodWorshiper;
            propagationController.GodSprite = goodGod;
        }
        else if (godName == "Evil")
        {
            spriteRenderer.sprite = evilGodWorshiper;
            propagationController.WorshiperSprite = evilGodWorshiper;
            propagationController.GodSprite = evilGod;
        }
    }

    private void Update()
    {
        // 대쉬 게이지
        UpdateDashGauge();

        // Note: DOTween으로 움직이니까 velocity 변화가 없어서 속도 기반의 방향 판단이 불가능했음...
        spriteRenderer.flipX = moveDestination.x < rb.position.x;
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
}
