using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    //ObjectPoolComponent bulletPool;
    MyObjectPool bulletPool;
    ScoreManager scoreManager;
    Transform playerTransform;
    AudioSource audioSource;
    public int health = 1;
    [SerializeField] int points;

    [SerializeField] bool shooting;
    public float shootCooldown;
    float shootTime;
    [SerializeField] Sprite bulletSprite;
    public float bulletSpeed;

    [SerializeField] GameObject explosionPrefab;

    private void OnEnable()
    {
        audioSource = GetComponent<AudioSource>();
        scoreManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<ScoreManager>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        if (shooting)
        {
            bulletPool = GameObject.Find("ObjPoolBullet").GetComponent<MyObjectPool>();
            shootTime = shootCooldown;
        }
    }

    public void TakeDamage(int dmg)
    {
        health -= dmg;

        if (health <= 0)
            Death();
    }
    void Death()
    {
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        scoreManager.Score += points;
        gameObject.SetActive(false);
    }

    void Shoot()
    {
        if (shooting)
        {
            GameObject projectile = bulletPool.GetElement();
            projectile.transform.position = transform.position;
            projectile.transform.rotation = Quaternion.LookRotation(new Vector3(0, 0, 1), playerTransform.position - transform.position);
            projectile.GetComponent<BulletController>().targetTags = new List<string> { "Player" };
            projectile.GetComponent<SpriteRenderer>().sprite = bulletSprite;
            projectile.GetComponent<ConstantMovement>().speed = bulletSpeed;
            projectile.SetActive(true);
            audioSource.Play();
        }
    }

    private void Update()
    {
        if (shooting)
        {
            shootTime -= Time.deltaTime;
            if (shootTime <= 0)
            {
                Shoot();
                shootTime += shootCooldown;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        //Le joueur prends du dommage en contact avec un ennemi
        if (col.gameObject.tag == "Player")
        {
            col.gameObject.GetComponent<PlayerController>().TakeDamage();
            TakeDamage(1);
        }
    }
}
