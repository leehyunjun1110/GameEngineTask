using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplainScript : MonoBehaviour
{
    private SpriteRenderer[] spriteRenderers;

    [SerializeField] private float fadeSpeed = 1.0f; // 알파값 변화 속도
    private bool fadingOut = true;

    void Start()
    {
        // 이 오브젝트의 모든 하위 SpriteRenderer 가져오기
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        StartCoroutine(FadeLoop());
    }

    private IEnumerator FadeLoop()
    {
        while (true)
        {
            foreach (SpriteRenderer sr in spriteRenderers)
            {
                Color color = sr.color;

                float currentAlpha = color.a * 255f;

                if (fadingOut)
                    currentAlpha -= fadeSpeed;
                else
                    currentAlpha += fadeSpeed;

                currentAlpha = Mathf.Clamp(currentAlpha, 100f, 255f);

                color.a = currentAlpha / 255f;
                sr.color = color;
            }

            if (spriteRenderers.Length > 0)
            {
                float checkAlpha = spriteRenderers[0].color.a * 255f;
                if (checkAlpha <= 100f) fadingOut = false;
                else if (checkAlpha >= 255f) fadingOut = true;
            }

            yield return new WaitForSeconds(0.02f);
        }
    }
}
