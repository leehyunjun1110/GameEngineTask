using System.Runtime.CompilerServices;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using Unity.Mathematics;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [Header("PlayerFunctionAnnounced")]
    [SerializeField] private Animator m_Animator;
    [SerializeField] private Rigidbody2D m_Rb;
    static private Transform m_Transform;
    [SerializeField] private float characterSpeed = 2f;
    [SerializeField] private float characterJumpSpace = 200.0f;
    public GameObject m_Attack;
    public bool shiftdown = false, isAbleAttack = true, isMovable = true, isShiftJumpable = true, isAbleZKey = true, isAbleXKey = true, isInvincivility = false, isSpeedUp = false;

    [Header("PlayerJumpFunc")]
    private KeyCode[] jumpKeys = { KeyCode.UpArrow, KeyCode.Space };
    private float lastJumpTime = 0f;
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckDistance = 0.09f;
    [SerializeField] private float groundCheckCooldown = 0.2f;

    [Header("PlayerVariables")]
    [SerializeField] private float attackCoolDown = 1.1f;
    [SerializeField] private Transform firePoint;
    [SerializeField] private int playerMaxHealth = 3;
    [SerializeField] private int playerHealth;
    [SerializeField] private GameObject[] playerHealthIcon;
    [SerializeField] private float stunTime = 0.5f;
    [SerializeField] private float invincibleTime = 3f, speedUpTime = 5f;
    private float originCharacterSpeed;
    private AudioSource pAs;
    public TextMeshProUGUI inVincibleText;
    public GameObject givt;
    private RectTransform givtRect;
    [SerializeField] private float fadeDuration = 2f;
    private float currentAlpha = 1f;

    [Header("PlayerSounds")]
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip xKeySound;
    [SerializeField] private AudioClip zKeySound;
    [SerializeField] private AudioClip hitSound;

    [Header("ShiftSkill")]
    [SerializeField] private float shiftCool;
    [SerializeField] private GameObject shiftCoolPanel;
    [SerializeField] private RectTransform shiftCooldownUI;
    [SerializeField] private float skillJumpSpace = 400.0f;
    [SerializeField] private float basicJumpSpace = 250.0f;

    [Header("ZKeySkill")]
    [SerializeField] private float ZKeyCool;
    [SerializeField] private GameObject ZKeyCoolPanel;
    [SerializeField] private RectTransform ZKeyCooldownUI;
    [SerializeField] private GameObject swordStingPrefab;
    [SerializeField] private float ZKeyRbAttackspeed;
    private Vector2 Zdirection;

    [Header("XKeySkill")]
    [SerializeField] private float XKeyCool;
    [SerializeField] private GameObject XKeyCoolPanel;
    [SerializeField] private RectTransform XKeyCooldownUI;
    [SerializeField] private GameObject swordSlashPrefab;
    [SerializeField] private float XKeyRbAttackspeed;
    private Vector2 Xdirection;

    void Start()
    {
        playerHealth = playerMaxHealth;
        m_Animator = GetComponent<Animator>();
        m_Transform = GetComponent<Transform>();
        givtRect = givt.GetComponent<RectTransform>();
        pAs = GetComponent<AudioSource>();
        originCharacterSpeed = characterSpeed;
        m_Attack.SetActive(false);
        givt.SetActive(false);
        shiftCoolPanel.SetActive(false);
        XKeyCoolPanel.SetActive(false);
        ZKeyCoolPanel.SetActive(false);
        playerHealthIcon[0].SetActive(true);
        playerHealthIcon[1].SetActive(true);
        playerHealthIcon[2].SetActive(true);
    }

    void Update()
    {
        ShiftCheck();
        Move();
        JumpCheck();
        if (playerHealth <= 0)
        {
            StartCoroutine(PlayerDeath());
        }
        if (Input.GetKeyDown(KeyCode.A) && isAbleAttack && isMovable)
        {
            StartCoroutine(AttackCheck());
        }

        if (Input.GetKeyDown(KeyCode.X) && isAbleXKey && isMovable)
        {
            StartCoroutine(XKeyAttack());
        }

        if (Input.GetKeyDown(KeyCode.Z) && isAbleZKey && isMovable)
        {
            StartCoroutine(ZKeyAttack());
        }
    }

    public IEnumerator PlayerHurt(int Damage)
    {
        if (!isInvincivility)
        {

            CameraController.Instance.Shake(0.2f, 0.2f);

            if (playerHealth <= 0)
            {
                StartCoroutine(PlayerDeath());
            }
            else
            {
                m_Animator.SetTrigger("Hurt");
                isMovable = false;
                playerHealth -= Damage;
                pAs.clip = hitSound;
                pAs.Play();
                yield return new WaitForSeconds(stunTime);
                isMovable = true;
            }
        }
        else
        {
            givt.SetActive(true);
            currentAlpha = 1f;

            while (currentAlpha > 0f)
            {
                currentAlpha -= Time.deltaTime / fadeDuration;
                currentAlpha = Mathf.Clamp01(currentAlpha);
                Color c = inVincibleText.color;
                c.a = currentAlpha;
                inVincibleText.color = c;
                yield return null;
            }

            givt.SetActive(false);
        }
        switch(playerHealth)
        {
            case 3:
                playerHealthIcon[0].SetActive(true);
                playerHealthIcon[1].SetActive(true);
                playerHealthIcon[2].SetActive(true);
                break;
            case 2:
                playerHealthIcon[0].SetActive(true);
                playerHealthIcon[1].SetActive(true);
                playerHealthIcon[2].SetActive(false);
                break;
            case 1:
                playerHealthIcon[0].SetActive(true);
                playerHealthIcon[1].SetActive(false);
                playerHealthIcon[2].SetActive(false);
                break;
            case 0:
                playerHealthIcon[0].SetActive(false);
                playerHealthIcon[1].SetActive(false);
                playerHealthIcon[2].SetActive(false);
                break;
        }
    }
    private IEnumerator PlayerDeath()
    {
        if (playerHealth <= 0)
        {
            m_Animator.SetTrigger("Death");
            isMovable = false;
            pAs.clip = hitSound;
            pAs.Play();
            yield return new WaitForSeconds(2f);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
    IEnumerator AttackCheck()
    {
        m_Attack.SetActive(true);
        m_Animator.SetTrigger("Attack");
        isAbleAttack = false;
        isMovable = false;
        pAs.clip = attackSound;
        pAs.Play();
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
                    bool shiftHeldNow = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

                    m_Animator.SetTrigger("Jump");
                    pAs.clip = jumpSound;
                    pAs.Play();

                    characterJumpSpace = shiftHeldNow && isShiftJumpable ? skillJumpSpace : basicJumpSpace;

                    if (shiftHeldNow && isShiftJumpable)
                    {
                        isShiftJumpable = false;
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

        // ��Ÿ�� ����
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

        isShiftJumpable = true;

        if (shiftCooldownUI != null)
        {
            shiftCooldownUI.localScale = new Vector3(1f, 1f, 1f); // �ʱ�ȭ
            shiftCoolPanel.SetActive(false);
        }

        yield break;
    }
    IEnumerator XKeyAttack()
    {
        m_Animator.SetTrigger("XKeyAttack");
        isAbleXKey = false;
        yield return new WaitForSeconds(0.3f);
        pAs.clip = xKeySound;
        pAs.Play();
        Xdirection = m_Transform.transform.localScale.x > 0 ? Vector2.right : Vector2.left;

        Quaternion isRight = (Xdirection == Vector2.right ? Quaternion.Euler(0f, 0f, 0f) : Quaternion.Euler(0f, 180f, 0f));

        GameObject slash = Instantiate(swordSlashPrefab, firePoint.position, isRight);

        Rigidbody2D rb = slash.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.velocity = Xdirection.normalized * XKeyRbAttackspeed;


        if (XKeyCoolPanel != null)
            XKeyCoolPanel.SetActive(true);

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
            XKeyCooldownUI.localScale = new Vector3(1f, 1f, 1f);
            XKeyCoolPanel.SetActive(false);
        }

        yield break;
    }
    IEnumerator ZKeyAttack()
    {
        m_Animator.SetTrigger("ZKeyAttack");
        isAbleZKey = false;
        yield return new WaitForSeconds(0.2f);
        pAs.clip = zKeySound;
        pAs.Play();
        Zdirection = m_Transform.transform.localScale.x > 0 ? Vector2.right : Vector2.left;

        Quaternion isRight = (Zdirection == Vector2.right ? Quaternion.Euler(0f, 0f, 0f) : Quaternion.Euler(0f, 180f, 0f));

        GameObject slash = Instantiate(swordStingPrefab, firePoint.position, isRight);

        Rigidbody2D rb = slash.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.velocity = Zdirection.normalized * ZKeyRbAttackspeed;


        if (ZKeyCoolPanel != null)
            ZKeyCoolPanel.SetActive(true);

        float elapsed = 0f;
        float startYScale = 1f;

        while (elapsed < ZKeyCool)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / ZKeyCool);
            float newYScale = Mathf.Lerp(startYScale, 0f, t);

            if (ZKeyCooldownUI != null)
            {
                Vector3 scale = ZKeyCooldownUI.localScale;
                ZKeyCooldownUI.localScale = new Vector3(scale.x, newYScale, scale.z);
            }

            yield return null;
        }

        isAbleZKey = true;

        if (ZKeyCooldownUI != null)
        {
            ZKeyCooldownUI.localScale = new Vector3(1f, 1f, 1f);
            ZKeyCoolPanel.SetActive(false);
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
            givtRect.localScale = new Vector3(-1f, 1f);
            m_Animator.SetBool("IsWalk", true);
            return;
        }
        if (Input.GetKey(KeyCode.RightArrow) && isMovable)
        {
            m_Transform.Translate(new Vector2(characterSpeed, 0) * Time.deltaTime);
            m_Transform.localScale = new Vector3(1f, 1f);
            givtRect.localScale = new Vector3(1f, 1f);
            m_Animator.SetBool("IsWalk", true);
            return;
        }
        m_Animator.SetBool("IsWalk", false);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch(collision.gameObject.tag)
        {
            case "Invincivility":
                StartCoroutine(Invincivility());
                Destroy(collision.gameObject);
                break;
            case "SpeedUp":
                StartCoroutine(SpeedUp());
                Destroy(collision.gameObject);
                break;
            case "JumpUp":
                StartCoroutine(JumpUp());
                Destroy(collision.gameObject);
                break;
            case "Door":
                SceneManager.LoadScene(collision.gameObject.name);
                break;
            case "Trap":
                StartCoroutine(PlayerHurt(1));
                break;
            case "DeadZone":
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                break;
        }
    }
    private IEnumerator SpeedUp()
    {
        isSpeedUp = true;
        originCharacterSpeed *= 2;
        yield return new WaitForSeconds(speedUpTime);
        originCharacterSpeed /= 2;
        yield break;
    }
    private IEnumerator JumpUp()
    {
        isSpeedUp = true;
        basicJumpSpace *= 1.5f;
        skillJumpSpace *= 2f;
        yield return new WaitForSeconds(speedUpTime);
        basicJumpSpace /= 1.5f;
        skillJumpSpace /= 2f;
        yield break;
    }
    private IEnumerator Invincivility()
    {
        isInvincivility = true;
        yield return new WaitForSeconds(invincibleTime);
        isInvincivility = false;
        yield break;
    }
}
