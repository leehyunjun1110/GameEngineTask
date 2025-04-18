using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillAttackScript : MonoBehaviour
{
    [SerializeField] private int attackDamage = 5;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch (collision.gameObject.tag)
        {
            case "Enemy":
                EnemyFSM enemy = collision.gameObject.GetComponent<EnemyFSM>();
                enemy.health -= attackDamage;
                break;
            case "Dummy":
                DummyScript dummy = collision.gameObject.GetComponent<DummyScript>();
                dummy.isDamaged = true;
                break;
        }
    }
}
