using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackScript : MonoBehaviour
{
    [SerializeField] private int basicAttack = 3;

    private EnemyFSM enemyFSM;
    private BoundaryEnemy enemyBoundary;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch(collision.gameObject.tag)
        {
            case "Enemy":
                if(collision.gameObject.TryGetComponent<EnemyFSM>(out enemyFSM))
                {
                    enemyFSM.health -= basicAttack;
                    enemyFSM.Hit();
                }
                else if (collision.gameObject.TryGetComponent<BoundaryEnemy>(out enemyBoundary))
                {
                    enemyBoundary.health -= basicAttack;
                    enemyBoundary.Hit();
                }
                else
                {
                    Debug.Log("클래스를 찾을 수 없음.");
                }
                break;
            case "Boss":
                BossController bossController = collision.gameObject.GetComponent<BossController>();
                bossController.TakeDamage(basicAttack);
                break;
            case "Dummy":
                DummyScript dummy = collision.gameObject.GetComponent<DummyScript>();
                dummy.isDamaged = true;
                break;
        }
    }
}
