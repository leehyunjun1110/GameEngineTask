using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public GameObject characterObj;
    private Transform character;

    public static CameraController Instance;

    private Vector3 offset = new Vector3(0, 0, -10f); // 기본 카메라 오프셋
    private Vector3 shakeOffset = Vector3.zero;       // 흔들림에 의한 오프셋
    private float shakeDuration = 0f;
    private float shakeMagnitude = 0f;

    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        character = characterObj.transform;
    }

    void Update()
    {
        if (character == null) return;

        Vector3 targetPosition = character.position + offset + shakeOffset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, 10f * Time.deltaTime);

        if (shakeDuration > 0)
        {
            shakeOffset = new Vector3(
                Random.Range(-1f, 1f) * shakeMagnitude,
                Random.Range(-1f, 1f) * shakeMagnitude,
                0f
            );
            shakeDuration -= Time.deltaTime;
        }
        else
        {
            shakeOffset = Vector3.zero;
        }
    }

    public void Shake(float duration, float magnitude)
    {
        shakeDuration = duration;
        shakeMagnitude = magnitude;
    }
}