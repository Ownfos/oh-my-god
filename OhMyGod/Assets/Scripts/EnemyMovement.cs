using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMovementAI2D : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float changeDirectionInterval = 2f;
    
    private Rigidbody2D rb2d;
    private Vector2 moveDirection;
    private float timer;
    
    private Vector2 mapStartPosition = new Vector2(-7f, -125f);
    private Vector2 mapSize = new Vector2(180f, 120f);
    private Vector2 mapMinBounds;
    private Vector2 mapMaxBounds;

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        ChangeDirection();
        
        // 경계 설정
        mapMinBounds = mapStartPosition;
        mapMaxBounds = mapStartPosition + mapSize;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= changeDirectionInterval)
        {
            ChangeDirection();
            timer = 0f;
        }
    }

    void FixedUpdate()
    {
        Move();
    }

    void ChangeDirection()
    {
        // 무작위 방향 설정
        float randomAngle = Random.Range(0f, 360f);
        moveDirection = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle)).normalized;
    }

    void Move()
    {
        // AI 이동
        rb2d.velocity = moveDirection * moveSpeed;

        // 맵 경계 확인 및 처리
        Vector2 pos = rb2d.position;
        if (pos.x > mapMaxBounds.x || pos.x < mapMinBounds.x || pos.y > mapMaxBounds.y || pos.y < mapMinBounds.y)
        {
            // 맵 경계에 도달하면 반대 방향으로 방향 전환
            moveDirection = -moveDirection;
            rb2d.velocity = moveDirection * moveSpeed;
        }
    }
}
