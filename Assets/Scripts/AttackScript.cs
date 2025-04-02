using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackScript : MonoBehaviour
{
    [SerializeField]
    private PlayerController playerController;

    [SerializeField]
    private List<GameObject> targets = new List<GameObject>();
    private void OnTriggerEnter2D(Collider2D collision)
    {
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i].gameObject == collision.gameObject)
                Destroy(collision.gameObject);
        }
        playerController.m_Attack.SetActive(false);
    }
}
