using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyScript : MonoBehaviour
{
    public bool isDamaged = false;
    [SerializeField] private Animator enemyAni;

    private void Start()
    {
        StartCoroutine(Damaged());
    }

    IEnumerator Damaged()
    {
        while(true)
        {
            if (isDamaged)
            {
                enemyAni.SetTrigger("Damaged");
                yield return null;
                isDamaged = false;
            }
            yield return null;
        }
    }
}
