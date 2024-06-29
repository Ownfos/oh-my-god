using UnityEngine;

public class NPCMovement : MonoBehaviour
{
    private bool canMove = false;
    public float speed = 3.0f;

    void Update()
    {
        if (!canMove)
            return;

        // NPC의 움직임 로직
    }

    public void SetCanMove(bool value)
    {
        canMove = value;
    }
}
