using UnityEngine;

public class SlashScript : MonoBehaviour
{
    public float fadeDuration = 1.5f; // 몇 초 동안 페이드될지
    private float fadeTimer;
    private SpriteRenderer sr; // 또는 MeshRenderer 등 사용 가능

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        fadeTimer = fadeDuration;
    }

    void Update()
    {
        if (sr == null) return;

        fadeTimer -= Time.deltaTime;
        float alpha = Mathf.Clamp01(fadeTimer / fadeDuration);

        Color newColor = sr.color;
        newColor.a = alpha;
        sr.color = newColor;

        if (fadeTimer <= 0f)
        {
            Destroy(gameObject); // 사라지면 삭제
        }
    }
}