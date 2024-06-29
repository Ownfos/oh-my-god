using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private bool canMove = true; // 기본값을 true로 설정하여 움직일 수 있게 함
    public float speed = 5.0f;

    void Update()
    {
        if (!canMove)
            return;

        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector2 movement = new Vector2(moveHorizontal, moveVertical);
        GetComponent<Rigidbody2D>().velocity = movement * speed;
    }

    public void SetCanMove(bool value)
    {
        canMove = value;
    }
}
