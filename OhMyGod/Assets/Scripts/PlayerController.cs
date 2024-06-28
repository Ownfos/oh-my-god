using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float moveSpeed;
    [SerializeField] private Ease moveEase = Ease.OutSine; // 처음엔 빠르고 도착할 때 감속

    private InputActions inputActions;
    private Rigidbody2D rb;

    private void Awake()
    {
        inputActions = new InputActions();
        inputActions.Player.Enable();
    }

    private void OnEnable()
    {
        inputActions.Player.MouseClick.performed += ChangeDestination;
    }

    private void OnDisable()
    {
        inputActions.Player.MouseClick.performed -= ChangeDestination;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void ChangeDestination(InputAction.CallbackContext context)
    {
        var mouseScreenPos = inputActions.Player.MousePos.ReadValue<Vector2>();
        var clickWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);

        Vector2 displacement = rb.transform.position - clickWorldPos;
        float distance = displacement.magnitude;
        float duration = distance / moveSpeed;

        // 기존 이동 취소
        rb.DOKill();

        // 새 목표 지점으로 이동 시작
        rb.DOMove(clickWorldPos, duration).SetEase(moveEase);
    }
}
