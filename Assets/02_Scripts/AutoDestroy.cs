using UnityEngine;

public class AutoDestroyOffscreen : MonoBehaviour
{
    public Camera mainCamera;
    public float margin = 1f;

    private Renderer objectRenderer;

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        objectRenderer = GetComponent<Renderer>();
    }

    private void Update()
    {
        if (objectRenderer == null) return;

        // 월드 좌표에서 뷰포트 좌표로 변환
        Vector3 viewportPos = mainCamera.WorldToViewportPoint(transform.position);

        // 화면을 완전히 벗어난 경우 제거
        bool isVisible = viewportPos.x >= -margin && viewportPos.x <= 1 + margin &&
                         viewportPos.y >= -margin && viewportPos.y <= 1 + margin &&
                         viewportPos.z >= 0;

        if (!isVisible)
        {
            Destroy(gameObject); // 또는 gameObject.SetActive(false);
        }
    }
}