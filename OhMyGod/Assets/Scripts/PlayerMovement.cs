using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private bool canMove = false;
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
