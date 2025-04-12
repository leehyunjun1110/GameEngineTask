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

        // ���� ��ǥ���� ����Ʈ ��ǥ�� ��ȯ
        Vector3 viewportPos = mainCamera.WorldToViewportPoint(transform.position);

        // ȭ���� ������ ��� ��� ����
        bool isVisible = viewportPos.x >= -margin && viewportPos.x <= 1 + margin &&
                         viewportPos.y >= -margin && viewportPos.y <= 1 + margin &&
                         viewportPos.z >= 0;

        if (!isVisible)
        {
            Destroy(gameObject); // �Ǵ� gameObject.SetActive(false);
        }
    }
}