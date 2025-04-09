using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject characterObj;
    private Transform character;
    private void Start()
    {
        character = characterObj.transform;
    }
    void Update()
    {
        transform.position = Vector3.Lerp(new Vector3(transform.position.x, transform.position.y, -10), new Vector3(character.position.x, character.position.y, 10), 1f * Time.deltaTime);
    }
}
