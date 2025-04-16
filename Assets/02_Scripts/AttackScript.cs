using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackScript : MonoBehaviour
{
    [SerializeField]
    private PlayerController playerController;

    [SerializeField]
    private const string enemy = "Enemy";
    [SerializeField]
    private const string dummy = "Dummy";
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag(enemy))
        {
            Destroy(collision.gameObject);
            playerController.m_Attack.SetActive(false);
        }
        else if (collision.gameObject.CompareTag(dummy))
        {
            
        }
    }
}
