using UnityEngine;

public class NPCMovement : MonoBehaviour
{
    private bool canMove = false;

    void Update()
    {
        if (!canMove)
            return;

        // 기존의 움직임 로직
        // 예: 적 AI 패턴
    }

    public void SetCanMove(bool value)
    {
        canMove = value;
    }
}
