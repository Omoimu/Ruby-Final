using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RubyController : MonoBehaviour
{
    public int scoreVal = 0;
    public Text score;
    public Text results;
    public static int level;

    public int cogs = 4;
    public Text cogText;

    public bool gameOver = false;

    public float speed = 3.0f;

    public int maxHealth = 5;

    public GameObject projectilePrefab;

    public AudioClip throwSound;
    public AudioClip hitSound;
    public AudioClip winSound;
    public AudioClip loseSound;
    public AudioClip potionSound;
    public AudioClip frogSound;
    public AudioClip BGM;
    public AudioSource musicSource;

    public int health { get { return currentHealth; } }
    int currentHealth;

    public float timeInvincible = 2.0f;
    bool isInvincible;
    float invincibleTimer;

    Rigidbody2D rigidbody2d;
    float horizontal;
    float vertical;

    Animator animator;
    Vector2 lookDirection = new Vector2(1, 0);

    AudioSource audioSource;

    public GameObject healthEffect;
    public GameObject damageEffect;


    // Start is called before the first frame update
    void Start()
    {
        musicSource.clip = BGM;
        musicSource.loop = true;
        musicSource.Play();
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        currentHealth = maxHealth;

        audioSource = GetComponent<AudioSource>();
        cogs = 4;
        score.text = "Fixed Robots: " + scoreVal.ToString();
        cogText.text = "Cogs: " + cogs.ToString();
        results.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        Vector2 move = new Vector2(horizontal, vertical);

        if (!Mathf.Approximately(move.x, 0.0f) || !Mathf.Approximately(move.y, 0.0f))
        {
            lookDirection.Set(move.x, move.y);
            lookDirection.Normalize();
        }

        animator.SetFloat("Look X", lookDirection.x);
        animator.SetFloat("Look Y", lookDirection.y);
        animator.SetFloat("Speed", move.magnitude);

        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;
            if (invincibleTimer < 0)
                isInvincible = false;
        }

        if (Input.GetKeyDown(KeyCode.C) && cogs > 0)
        {
            Launch();
            cogs = cogs - 1;
            cogText.text = "Cogs: " + cogs.ToString();
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            RaycastHit2D hit = Physics2D.Raycast(rigidbody2d.position + Vector2.up * 0.2f, lookDirection, 1.5f, LayerMask.GetMask("NPC"));
            if (hit.collider != null)
            {
                if (scoreVal == 4)
                {
                    NonPlayerCharacter character = hit.collider.GetComponent<NonPlayerCharacter>();
                    if (character != null)
                    {
                        PlaySound(frogSound);
                        level = 2;
                        SceneManager.LoadScene("Level 2");
                    }
                }
                else
                {
                    NonPlayerCharacter character = hit.collider.GetComponent<NonPlayerCharacter>();
                    if (character != null)
                    {
                        PlaySound(frogSound);
                        character.DisplayDialog();
                    }
                }
            }
        }

        if (Input.GetKey(KeyCode.R))
        {
            if (gameOver == true)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // this loads the currently active scene
            }
        }
    }

    void FixedUpdate()
    {
        Vector2 position = rigidbody2d.position;
        position.x = position.x + speed * horizontal * Time.deltaTime;
        position.y = position.y + speed * vertical * Time.deltaTime;

        rigidbody2d.MovePosition(position);

        if (Input.GetKey("escape"))
        {
            Application.Quit();
        }
    }

    public void ChangeHealth(int amount)
    {
        if (amount < 0)
        {
            if (isInvincible)
                return;

            isInvincible = true;
            invincibleTimer = timeInvincible;
            animator.SetTrigger("Hit");
            GameObject projectileObject = Instantiate(damageEffect, rigidbody2d.position + Vector2.up * 0.5f, Quaternion.identity);
            PlaySound(hitSound);
        }
        if (amount > 0)
        {
            GameObject projectileObject = Instantiate(healthEffect, rigidbody2d.position + Vector2.up * 0.5f, Quaternion.identity);

        }

        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);

        UIHealthBar.instance.SetValue(currentHealth / (float)maxHealth);

        if (currentHealth <= 0 && gameOver != true)
        {
            results.text = "You Lose! Press R to Restart. Game Created by Dominique Mobley";
            speed = 0;
            gameOver = true;
            musicSource.clip = loseSound;
            musicSource.Play();
        }
    }

    public void ChangeSpeed(int amount)
    {
        if(speed >= 3)
        {
            speed = speed - 1;
        }
    }
        public void IncreaseScore()
    {
        scoreVal = scoreVal + 1;
        score.text = "Fixed Robots: " + scoreVal.ToString();

        if(scoreVal == 4 && level == 2)
        {
            results.text = "You Win! Press R to Restart. Game Created by Dominique Mobley";
            speed = 0;
            gameOver = true;
            musicSource.clip = winSound;
            musicSource.Play();
        }
        else if (scoreVal == 4)
        {
            results.text = "Talk to Jambi to visit stage two!";
            //gameOver = true;
            //musicSource.clip = winSound;
            //musicSource.Play();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.tag == "CogBag")
        {
            cogs += 3;
            cogText.text = "Cogs: " + cogs.ToString();
            Destroy(collision.collider.gameObject);
        }
        else if(collision.collider.tag == "Potion")
        {
            PlaySound(potionSound);
            speed = 5.0f;
            Destroy(collision.collider.gameObject);
        }

    }

    void Launch()
    {
        GameObject projectileObject = Instantiate(projectilePrefab, rigidbody2d.position + Vector2.up * 0.5f, Quaternion.identity);

        Projectile projectile = projectileObject.GetComponent<Projectile>();
        projectile.Launch(lookDirection, 300);

        animator.SetTrigger("Launch");
        PlaySound(throwSound);
    }

    public void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }
}