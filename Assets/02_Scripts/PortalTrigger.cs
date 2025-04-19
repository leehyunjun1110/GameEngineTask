using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalTrigger : MonoBehaviour
{
    private EnemyFSM enemyFSM;

    private BoundaryEnemy enemyBD;

    public GameObject portal;
    void Start()
    {
        portal.SetActive(false);
    }

    void Update()
    {
        if (TryGetComponent<EnemyFSM>(out enemyFSM))
        {
            if (enemyFSM.isPortalVisible == true)
            {
                portal.SetActive(true);
            }
        }
        else if (TryGetComponent<BoundaryEnemy>(out enemyBD))
        {
            if (enemyBD.isPortalVisible == true)
            {
                portal.SetActive(true);
            }
        }
    }
}
