using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMovementAI2D : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float recognitionInterval = 5f; // 중립 NPC를 인식하는 간격
    public float detectionRadius = 10f; // 포교 범위의 2배
    private float timer;
    
    private Rigidbody2D rb2d;
    private Vector2 moveDirection;
    
    private Vector2 mapStartPosition = new Vector2(-7f, -125f);
    private Vector2 mapSize = new Vector2(180f, 120f);
    private Vector2 mapMinBounds;
    private Vector2 mapMaxBounds;
    
    private NeutralWorshiperGroup[] neutralGroups;

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        neutralGroups = FindObjectsOfType<NeutralWorshiperGroup>();

        mapMinBounds = mapStartPosition;
        mapMaxBounds = mapStartPosition + mapSize;

        ChangeDirectionRandomly(); // 게임 시작 시 랜덤한 방향으로 이동
        StartCoroutine(BehaviorRoutine());
    }

    void FixedUpdate()
    {
        Move();
    }

    void OnDisable()
    {
        rb2d.velocity = Vector2.zero;
    }

    void OnDestroy()
    {
        StopAllCoroutines();    
    }

    IEnumerator BehaviorRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            NeutralWorshiperGroup targetGroup = CheckForNeutralGroups();
            if (targetGroup == null)
            {
                // 중립 NPC가 없다면 같은 방향으로 계속 이동
                yield return new WaitForSeconds(recognitionInterval);
            }
            else
            {
                float randomValue = Random.Range(0f, 1f);
                if (randomValue < 0.3f)
                {
                    MoveInOppositeDirection(targetGroup);
                }
                else
                {
                    MoveTowardsGroup(targetGroup);
                }
                yield return new WaitForSeconds(recognitionInterval);
            }
        }
    }

    void ChangeDirectionRandomly()
    {
        float randomAngle = Random.Range(0f, 360f);
        moveDirection = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle)).normalized;
    }

    NeutralWorshiperGroup CheckForNeutralGroups()
    {
        List<NeutralWorshiperGroup> groupsInRange = new List<NeutralWorshiperGroup>();

        foreach (NeutralWorshiperGroup group in neutralGroups)
        {
            float distance = Vector2.Distance(transform.position, group.transform.position);
            if (distance <= detectionRadius)
            {
                groupsInRange.Add(group);
            }
        }

        if (groupsInRange.Count == 0)
        {
            return null;
        }

        int randomIndex = Random.Range(0, groupsInRange.Count);
        return groupsInRange[randomIndex];
    }

    void MoveInOppositeDirection(NeutralWorshiperGroup group)
    {
        Vector2 directionToGroup = (group.transform.position - transform.position).normalized;
        moveDirection = -directionToGroup;
    }

    void MoveTowardsGroup(NeutralWorshiperGroup group)
    {
        Vector2 directionToGroup = (group.transform.position - transform.position).normalized;
        moveDirection = directionToGroup;
    }

    void Move()
    {
        rb2d.velocity = moveDirection * moveSpeed;

        Vector2 pos = rb2d.position;
        if (pos.x > mapMaxBounds.x || pos.x < mapMinBounds.x || pos.y > mapMaxBounds.y || pos.y < mapMinBounds.y)
        {
            moveDirection = -moveDirection;
            rb2d.velocity = moveDirection * moveSpeed;

            // 경계에 도달하면 알고리즘을 2번부터 다시 시작
            StartCoroutine(RestartBehaviorRoutine());
        }
    }

    IEnumerator RestartBehaviorRoutine()
    {
        StopCoroutine(BehaviorRoutine()); // 현재 코루틴 중지
        yield return null; // 한 프레임 대기
        StartCoroutine(BehaviorRoutine()); // 알고리즘을 2번부터 다시 시작
    }
}
