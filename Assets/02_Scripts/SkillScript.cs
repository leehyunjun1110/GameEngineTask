using UnityEngine;

public class SkillScript : MonoBehaviour
{
    public float fadeDuration = 1.5f;
    private float fadeTimer;
    private SpriteRenderer sr;

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
            Destroy(gameObject);
        }
    }
}