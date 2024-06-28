using UnityEngine;

// 종교 지도자(플레이어 또는 상대 종교의 중심 인물)를 따라다니는 인파
public class WorshiperController : MonoBehaviour
{
    [SerializeField] private float maxMoveSpeed = 10f;
    [SerializeField] private float moveAcceleration = 50f;

    public GameObject FollowTarget;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (FollowTarget != null)
        {
            AccelerateTowardsTarget();
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
}
