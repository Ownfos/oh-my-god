using DG.Tweening;
using UnityEngine;

// 종교 지도자(플레이어 또는 상대 종교의 중심 인물)를 따라다니는 인파
public class WorshiperController : MonoBehaviour
{
    [SerializeField] private float maxMoveSpeed = 10f;
    [SerializeField] private float moveAcceleration = 50f;

    // 속도가 0 근처일 때 스프라이트의 방향이 왼쪽 오른쪽을
    // 번갈아가며 바라보는 현상을 막기 위한 속도 하한선.
    // 이 값보다 x축 속도의 절댓값이 커야 스프라이트 방향을 전환한다.
    private const float SPRITE_FLIP_VELOCITY_THRESHOLD = 0.2f;

    public GameObject FollowTarget;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void FixedUpdate()
    {
        if (FollowTarget != null)
        {
            AccelerateTowardsTarget();
        }

        // 속도가 0 근처일 때 스프라이트 방향이 양옆으로
        // 진동하지 않도록 막으면서도 이동 방향을 바라보도록 설정
        if (rb.velocity.x < -SPRITE_FLIP_VELOCITY_THRESHOLD)
        {
            spriteRenderer.flipX = true;
        }
        else if (rb.velocity.x > SPRITE_FLIP_VELOCITY_THRESHOLD)
        {
            spriteRenderer.flipX = false;
        }
    }

    private void AccelerateTowardsTarget()
    {
        // 목표를 향해 가속
        Vector2 offsetToTarget = FollowTarget.transform.position - rb.transform.position;
        rb.AddForce(offsetToTarget.normalized * moveAcceleration, ForceMode2D.Force);

        // 최대 이동 속도를 넘어서지 않도록 clamping
        if (rb.velocity.magnitude > maxMoveSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxMoveSpeed;
        }
    }

    public void Die()
    {
        transform.DOShakeScale(1f).OnComplete(()=> {
            transform.DOScale(0f, 1f).SetEase(Ease.InExpo).OnComplete(() => {
                Destroy(gameObject);
            });
        });
    }
}
