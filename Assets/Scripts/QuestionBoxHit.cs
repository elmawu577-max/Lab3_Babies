using System.Collections;
using UnityEngine;

public class QuestionBoxHit : MonoBehaviour
{
    public GameObject coin;
    public Sprite disabledSprite;

    private bool used = false;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        coin.SetActive(false);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (used)
            return;

        if (collision.gameObject.name == "mario_0")
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 1.0f)
                {
                    used = true;
                    StartCoroutine(HitBox());
                    break;
                }
            }
        }
    }

    IEnumerator HitBox()
    {
        Vector3 boxStart = transform.localPosition;

        transform.localPosition = boxStart + Vector3.up * 0.15f;
        yield return new WaitForSeconds(0.08f);

        transform.localPosition = boxStart;

        coin.SetActive(true);
        audioSource.Play();

        Vector3 coinStart = coin.transform.localPosition;

        coin.transform.localPosition = coinStart + Vector3.up * 0.8f;
        yield return new WaitForSeconds(0.25f);

        coin.transform.localPosition = coinStart;
        coin.SetActive(false);

        spriteRenderer.sprite = disabledSprite;
    }
}
