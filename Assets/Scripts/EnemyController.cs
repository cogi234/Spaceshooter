using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    MyObjectPool bulletPool;
    ScoreManager scoreManager;
    Transform playerTransform;
    AudioSource audioSource;
    [SerializeField] int points;

    //Les parametres de tir
    // Est-ce que cet ennemi va tirer
    [SerializeField] bool shooting;
    // Le temps entre les tirs
    public float shootCooldown;
    float shootTime;
    //La vitesse des balles
    public float bulletSpeed;

    [SerializeField] Sprite bulletSprite;
    [SerializeField] GameObject explosionPrefab;
    HealthComponent healthComponent;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        healthComponent = GetComponent<HealthComponent>();
    }

    private void OnEnable()
    {
        scoreManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<ScoreManager>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        if (shooting)
        {
            bulletPool = GameObject.Find("ObjPoolBullet").GetComponent<MyObjectPool>();
            shootTime = shootCooldown;
        }
    }

    public void Death()
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
        //Lors d'un contact, le joueur et l'ennemi prennent du dommage
        if (col.gameObject.tag == "Player")
        {
            col.gameObject.GetComponent<HealthComponent>().TakeDamage(1);
            healthComponent.TakeDamage(1);
        }
    }
}
