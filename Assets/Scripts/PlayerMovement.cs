using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 50f;
    public float stompBounceForce = 25f;

    public GameObject gameOverPanel;
    public TMP_Text scoreText;
    public AudioSource marioAudio;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private bool isGrounded;
    private bool isGameOver = false;

    private int score = 0;
    private float previousMove = 0f;
    private float moveInput = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        gameOverPanel.SetActive(false);
        Time.timeScale = 1f;

        UpdateScore();

        GoombaMovement[] goombas =
            FindObjectsByType<GoombaMovement>(FindObjectsSortMode.None);

        foreach (GoombaMovement goomba in goombas)
        {
            goomba.Stomped += AddScore;
        }
    }

    void OnDestroy()
    {
        GoombaMovement[] goombas =
            FindObjectsByType<GoombaMovement>(FindObjectsSortMode.None);

        foreach (GoombaMovement goomba in goombas)
        {
            goomba.Stomped -= AddScore;
        }
    }

    void Update()
    {
        if (isGameOver)
            return;

        float move = moveInput;

        bool isSkidding =
            isGrounded &&
            move != 0 &&
            previousMove != 0 &&
            Mathf.Sign(move) != Mathf.Sign(previousMove);

        animator.SetBool("IsSkidding", isSkidding);
        animator.SetFloat("Speed", Mathf.Abs(move));
        animator.SetBool("IsJumping", !isGrounded);

        rb.linearVelocity = new Vector2(
            move * moveSpeed,
            rb.linearVelocity.y
        );

        if (move < 0)
        {
            spriteRenderer.flipX = true;
        }
        else if (move > 0)
        {
            spriteRenderer.flipX = false;
        }

        if (move != 0)
        {
            previousMove = move;
        }
    }

    public void OnMove(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();
        moveInput = input.x;
    }

    public void OnJump(InputValue value)
    {
        if (!value.isPressed)
            return;

        if (isGameOver || !isGrounded)
            return;

        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            jumpForce
        );

        isGrounded = false;
        animator.SetBool("IsJumping", true);

        if (marioAudio != null)
        {
            marioAudio.PlayOneShot(marioAudio.clip);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsPlatform(collision.gameObject))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    isGrounded = true;
                    animator.SetBool("IsJumping", false);
                    break;
                }
            }
        }

        if (collision.gameObject.TryGetComponent<GoombaMovement>(
            out GoombaMovement goomba))
        {
            bool stomped = false;

            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    stomped = true;
                    break;
                }
            }

            if (stomped)
            {
                goomba.Stomp();

                rb.linearVelocity = new Vector2(
                    rb.linearVelocity.x,
                    stompBounceForce
                );

                isGrounded = false;
                animator.SetBool("IsJumping", true);
            }
            else
            {
                GameOver();
            }
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (IsPlatform(collision.gameObject))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    isGrounded = true;
                    return;
                }
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (IsPlatform(collision.gameObject))
        {
            isGrounded = false;
            animator.SetBool("IsJumping", true);
        }
    }

    void GameOver()
    {
        isGameOver = true;

        rb.linearVelocity = Vector2.zero;

        gameOverPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    bool IsPlatform(GameObject obj)
    {
        return obj.name == "Ground" ||
               obj.name.StartsWith("Brick") ||
               obj.name.StartsWith("QuestionBox");
    }

    public void AddScore()
    {
        score++;
        UpdateScore();
    }

    void UpdateScore()
    {
        scoreText.text = "SCORE: " + score;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }
}