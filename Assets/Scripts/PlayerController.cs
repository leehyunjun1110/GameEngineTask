using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private Animator m_Animator;

    [SerializeField]
    private Rigidbody2D m_Rb;

    private Transform m_Transform;

    [SerializeField]
    private float characterSpeed = 1.0f;

    [SerializeField]
    private float characterJumpSpace = 0.5f;

    public GameObject m_Attack;

    public bool groundCheck = false, shiftdown = false, isAbleAttack = true, isMovable = true;
    void Start()
    {
        m_Animator = GetComponent<Animator>();
        m_Transform = GetComponent<Transform>();
        
        m_Attack.SetActive(false);
    }

    void Update()
    {
        ShiftCheck();
        Move();
        JumpCheck();
        AttackCheck();
    }

    private void AttackCheck()
    {
        if (Input.GetKeyDown(KeyCode.A) && isAbleAttack)
        {
            m_Attack.SetActive(true);
            m_Animator.SetTrigger("Attack");
            isAbleAttack = false;
            isMovable = false;
            Invoke("DisableAttack", 0.7f);
        }
    }

    private void DisableAttack()
    {
        m_Attack.SetActive(false);
        isAbleAttack = true;
        isMovable = true;
    }

    private void ShiftCheck()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            shiftdown = true;
            characterSpeed = 1f;
            m_Animator.SetBool("IsRun", true);
        }
        else
        {
            shiftdown = false;
            characterSpeed = 2f;
            m_Animator.SetBool("IsRun", false);
        }
    }
    private void JumpCheck()
    {
        if ((Input.GetKeyDown(KeyCode.UpArrow) && groundCheck && isMovable) || (Input.GetKeyDown(KeyCode.Space) && groundCheck && isMovable))
        {
            m_Animator.SetTrigger("Jump");
            if (shiftdown == true)
            {
                characterJumpSpace = 100.0f;
            }
            m_Rb.AddForce(Vector2.up * characterJumpSpace);
            groundCheck = false;
        }
    }

    private void Move()
    {
        if (Input.GetKey(KeyCode.LeftArrow) && isMovable)
        {
            m_Transform.Translate(new Vector2(-characterSpeed, 0) * Time.deltaTime);
            m_Transform.localScale = new Vector3(-0.5f, 0.5f);
            m_Animator.SetBool("IsWalk", true);
            return;
        }
        if (Input.GetKey(KeyCode.RightArrow) && isMovable)
        {
            m_Transform.Translate(new Vector2(characterSpeed, 0) * Time.deltaTime);
            m_Transform.localScale = new Vector3(0.5f, 0.5f);
            m_Animator.SetBool("IsWalk", true);
            return;
        }
        m_Animator.SetBool("IsWalk", false);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            groundCheck = true;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Door1"))
        {
            SceneManager.LoadScene("World2");
        }
        if (collision.gameObject.CompareTag("Door2"))
        {
            SceneManager.LoadScene("World3");
        }
        if (collision.gameObject.CompareTag("Door3"))
        {
            SceneManager.LoadScene("End");
        }
    }
}
