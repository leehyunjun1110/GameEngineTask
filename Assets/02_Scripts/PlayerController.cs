using System.Runtime.CompilerServices;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using Unity.VisualScripting;

public class PlayerController : MonoBehaviour
{
    [Header("PlayerFunctionAnnounced")]
    [SerializeField] private Animator m_Animator;
    [SerializeField] private Rigidbody2D m_Rb;
    static private Transform m_Transform;
    [SerializeField] private float characterSpeed = 2f;
    [SerializeField] private float characterJumpSpace = 200.0f;
    public GameObject m_Attack;
    public bool shiftdown = false, isAbleAttack = true, isMovable = true, isJumpable = true, isAbleZKey = true, isAbleXKey = true;

    [Header("PlayerJumpFunc")]
    private KeyCode[] jumpKeys = { KeyCode.UpArrow, KeyCode.Space };
    private float lastJumpTime = 0f;
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckDistance = 0.09f;
    [SerializeField] private float groundCheckCooldown = 0.2f;

    [Header("PlayerVariables")]
    [SerializeField] private float attackCoolDown = 1.1f;
    //[SerializeField] private int playerHealth = 3;
    private float originCharacterSpeed;

    [Header("ShiftSkill")]
    [SerializeField] private float shiftCool = 3f;
    [SerializeField] private GameObject shiftCoolPanel;
    [SerializeField] private RectTransform shiftCooldownUI;

    [Header("ZKeySkill")]
    [SerializeField] private float ZKeyCool = 3f;
    [SerializeField] private GameObject ZKeyCoolPanel;
    [SerializeField] private RectTransform ZKeyCooldownUI;

    [Header("XKeySkill")]
    [SerializeField] private float XKeyCool = 4f;
    [SerializeField] private GameObject XKeyCoolPanel;
    [SerializeField] private RectTransform XKeyCooldownUI;
    [SerializeField] private GameObject swordSlashPrefab;
    [SerializeField] private float XKeyRbAttackspeed;
    [SerializeField] private Transform firePoint;
    private Vector2 direction;

    [Header("AudioSource")]
    public AudioSource audioSource;
    public AudioClip swordAttackSound;
    public AudioClip walkSound;

    void Start()
    {
        m_Animator = GetComponent<Animator>();
        m_Transform = GetComponent<Transform>();
        originCharacterSpeed = characterSpeed;
        m_Attack.SetActive(false);
        shiftCoolPanel.SetActive(false);
        XKeyCoolPanel.SetActive(false);
    }

    void Update()
    {
        ShiftCheck();
        Move();
        JumpCheck();

        if (Input.GetKeyDown(KeyCode.A) && isAbleAttack)
        {
            StartCoroutine(AttackCheck());
        }

        if (Input.GetKeyDown(KeyCode.X) && isAbleXKey)
        {
            StartCoroutine(XKeyAttack());
        }
    }

    IEnumerator AttackCheck()
    {
        m_Attack.SetActive(true);
        m_Animator.SetTrigger("Attack");
        isAbleAttack = false;
        isMovable = false;

        yield return new WaitForSeconds(attackCoolDown);

        m_Attack.SetActive(false);
        isAbleAttack = true;
        isMovable = true;
    }


    private void ShiftCheck()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            shiftdown = true;
            characterSpeed = originCharacterSpeed * 2;
            m_Animator.SetBool("IsRun", true);
        }
        else
        {
            shiftdown = false;
            characterSpeed = originCharacterSpeed;
            m_Animator.SetBool("IsRun", false);
        }
    }

    private void JumpCheck()
    {
        if ((Time.time - lastJumpTime > groundCheckCooldown) && GroundCheck() && isMovable)
        {
            foreach (var key in jumpKeys)
            {
                if (Input.GetKeyDown(key))
                {
                    m_Animator.SetTrigger("Jump");
                    characterJumpSpace = shiftdown && isJumpable ? 400.0f : 200.0f;
                    if (shiftdown && isJumpable)
                    {
                        isJumpable = false;
                        StartCoroutine(ShiftJump());
                    }
                    m_Rb.AddForce(Vector2.up * characterJumpSpace);
                    lastJumpTime = Time.time;
                    break;
                }
            }
        }
    }

    IEnumerator ShiftJump()
    {
        if (shiftCoolPanel != null)
        {
            shiftCoolPanel.SetActive(true);
        }

        float elapsed = 0f;
        float startYScale = 1f;

        // 쿨타임 진행 (Y 스케일 점점 줄어듦)
        while (elapsed < shiftCool)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / shiftCool);
            float newYScale = Mathf.Lerp(startYScale, 0f, t);

            if (shiftCooldownUI != null)
            {
                Vector3 scale = shiftCooldownUI.localScale;
                shiftCooldownUI.localScale = new Vector3(scale.x, newYScale, scale.z);
            }

            yield return null;
        }

        isJumpable = true;

        if (shiftCooldownUI != null)
        {
            shiftCooldownUI.localScale = new Vector3(1f, 1f, 1f); // 초기화
            shiftCoolPanel.SetActive(false);
        }

        yield break;
    }
    IEnumerator XKeyAttack()
    {
        direction = m_Transform.transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        m_Animator.SetTrigger("XKeyAttack");
        isAbleXKey = false;
        yield return new WaitForSeconds(0.3f);
        GameObject slash = Instantiate(swordSlashPrefab, firePoint.position, Quaternion.identity);

        Rigidbody2D rb = slash.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = direction.normalized * XKeyRbAttackspeed;
        }


        if (XKeyCoolPanel != null)
        {
            XKeyCoolPanel.SetActive(true);
        }

        float elapsed = 0f;
        float startYScale = 1f;

        while (elapsed < XKeyCool)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / XKeyCool);
            float newYScale = Mathf.Lerp(startYScale, 0f, t);

            if (XKeyCooldownUI != null)
            {
                Vector3 scale = XKeyCooldownUI.localScale;
                XKeyCooldownUI.localScale = new Vector3(scale.x, newYScale, scale.z);
            }

            yield return null;
        }

        isAbleXKey = true;

        if (XKeyCooldownUI != null)
        {
            XKeyCooldownUI.localScale = new Vector3(1f, 1f, 1f); // 초기화
            XKeyCoolPanel.SetActive(false);
        }

        yield break;
    }
    private bool GroundCheck()
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(groundCheckPoint.position, Vector2.down, groundCheckDistance);
        bool gc = false;
        Debug.DrawRay(groundCheckPoint.position, Vector2.down * groundCheckDistance, Color.red);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.gameObject.CompareTag("Ground"))
            {
                gc = true;
                return gc;
            }
        }
        return gc;
    }

    private void Move()
    {
        if (Input.GetKey(KeyCode.LeftArrow) && isMovable)
        {
            m_Transform.Translate(new Vector2(-characterSpeed, 0) * Time.deltaTime);
            m_Transform.localScale = new Vector3(-1f, 1f);
            m_Animator.SetBool("IsWalk", true);
            return;
        }
        if (Input.GetKey(KeyCode.RightArrow) && isMovable)
        {
            m_Transform.Translate(new Vector2(characterSpeed, 0) * Time.deltaTime);
            m_Transform.localScale = new Vector3(1f, 1f);
            m_Animator.SetBool("IsWalk", true);
            return;
        }
        m_Animator.SetBool("IsWalk", false);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch(collision.gameObject.tag)
        {
            case "Door1":
                SceneManager.LoadScene("World2");
                break;

            case "Door2":
                SceneManager.LoadScene("World3");
                break;

            case "Door3":
                SceneManager.LoadScene("End");
                break;
        }
    }
}
