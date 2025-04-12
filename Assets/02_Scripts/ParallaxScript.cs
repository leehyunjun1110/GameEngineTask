using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ParallaxBackground
{
    public Transform backgroundTransform;
    [Range(0f, 1f)] public float parallaxSpeed = 0.5f;
    public float repeatWidth = -1f; // 기본값 -1이면 자동으로 계산
}

public class ParallaxScript : MonoBehaviour
{
    public Transform cameraTransform;
    public List<ParallaxBackground> parallaxLayers = new List<ParallaxBackground>();

    private Vector3 previousCameraPosition;

    private void Start()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        previousCameraPosition = cameraTransform.position;

        // 반복 너비를 자동으로 계산
        foreach (var layer in parallaxLayers)
        {
            if (layer.backgroundTransform != null && layer.repeatWidth < 0f)
            {
                SpriteRenderer sr = layer.backgroundTransform.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    layer.repeatWidth = sr.bounds.size.x;
                }
                else
                {
                    Debug.LogWarning($"[{layer.backgroundTransform.name}]Fucked");
                }
            }
        }
    }

    private void LateUpdate()
    {
        Vector3 deltaMovement = cameraTransform.position - previousCameraPosition;

        foreach (ParallaxBackground layer in parallaxLayers)
        {
            if (layer.backgroundTransform != null)
            {
                Vector3 movement = new Vector3(deltaMovement.x * layer.parallaxSpeed, deltaMovement.y * layer.parallaxSpeed, 0);
                layer.backgroundTransform.position += movement;

                float distanceFromCamera = Mathf.Abs(cameraTransform.position.x - layer.backgroundTransform.position.x);

                if (distanceFromCamera >= layer.repeatWidth)
                {
                    float offset = layer.repeatWidth * 2f;
                    Vector3 newPos = layer.backgroundTransform.position;
                    newPos.x += (cameraTransform.position.x > layer.backgroundTransform.position.x) ? offset : -offset;
                    layer.backgroundTransform.position = newPos;
                }
            }
        }

        previousCameraPosition = cameraTransform.position;
    }
}