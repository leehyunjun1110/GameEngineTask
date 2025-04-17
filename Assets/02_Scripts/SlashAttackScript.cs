using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlashAttackScript : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch (collision.gameObject.tag)
        {
            case "Enemy":
                Destroy(collision.gameObject);
                break;
            case "Dummy":
                DummyScript dummy = collision.gameObject.GetComponent<DummyScript>();
                dummy.isDamaged = true;
                break;
        }
    }
}
