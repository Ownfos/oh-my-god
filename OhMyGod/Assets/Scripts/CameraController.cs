using UnityEngine;

public class CameraBoundary : MonoBehaviour
{
    public GameObject boundaryObject; // 경계를 정의하는 오브젝트
    private Collider2D[] boundaryColliders;
    private Camera mainCamera;
    private Bounds combinedBounds;

    void Start()
    {
        boundaryColliders = boundaryObject.GetComponentsInChildren<Collider2D>();
        mainCamera = Camera.main;
        combinedBounds = CalculateCombinedBounds();
    }

    void LateUpdate()
    {
        Vector3 camPos = mainCamera.transform.position;
        Vector3 minBounds = combinedBounds.min;
        Vector3 maxBounds = combinedBounds.max;

        float camHeight = mainCamera.orthographicSize;
        float camWidth = mainCamera.orthographicSize * mainCamera.aspect;

        camPos.x = Mathf.Clamp(camPos.x, minBounds.x + camWidth, maxBounds.x - camWidth);
        camPos.y = Mathf.Clamp(camPos.y, minBounds.y + camHeight, maxBounds.y - camHeight);

        mainCamera.transform.position = camPos;
    }

    private Bounds CalculateCombinedBounds()
    {
        Bounds bounds = boundaryColliders[0].bounds;
        foreach (var collider in boundaryColliders)
        {
            bounds.Encapsulate(collider.bounds);
        }
        return bounds;
    }
}
