using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    //Pour le tir
    MyObjectPool bulletPool;
    [SerializeField] float shootCooldown = 0.5f;
    [SerializeField] Sprite bulletSprite;
    [SerializeField] float bulletSpeed;
    float elapsedTimeShoot;
    bool shooting = false;

    //Pour le mouvement
    // Vitesse du joueur
    [SerializeField] float playerSpeed = 6;
    [SerializeField] float touchMovementDeadzone = 10;
    [SerializeField] float touchMovementRange = 100;
    // Les limites de l'ecran
    float minY, maxY, minX, maxX;
    Vector2 direction = Vector2.zero;

    //Pour le son
    AudioSource audioSource;
    [SerializeField] AudioClip laserClip, powerupClip, hitClip;

    //La vie du joueur
    HealthComponent healthComponent;

    //Pour la mort
    [SerializeField] GameObject explosionPrefab;


    void Awake()
    {
        //On trouve les limites de l'ecran
        maxY = Camera.main.orthographicSize - 0.5f;
        minY = -maxY;
        maxX = (Camera.main.orthographicSize * Screen.width / Screen.height) - 0.5f;
        minX = -maxX;

        //On initialise les references
        bulletPool = GameObject.Find("ObjPoolBullet").GetComponent<MyObjectPool>();
        audioSource = GetComponent<AudioSource>();
        healthComponent = GetComponent<HealthComponent>();
    }

    void Update()
    {
        //Les inputs
        if (Application.isMobilePlatform)//Controles mobile
        {
            //Si on touche a l'ecran, la direction de mouvement est la direction du toucher
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                Vector2 rawDirection = touch.position - touch.rawPosition;
                if (rawDirection.magnitude > touchMovementDeadzone)
                    direction = rawDirection.normalized * Mathf.Min(1, rawDirection.magnitude / touchMovementRange); // la direction est scaled au range, pour avoir un mouvement plus graduel.
                else
                    direction = Vector2.zero;
            } else
            {
                direction = Vector2.zero;
            }
            //On tire si un toucher est detecter
            shooting = Input.touchCount > 0;
        }
        else//Controles ordinateur
        {
            direction = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;
            shooting = Input.GetButton("Shoot");
        }

        //On regarde si on veut et peut tirer
        elapsedTimeShoot += Time.deltaTime;
        if (shooting && elapsedTimeShoot >= shootCooldown)
        {
            elapsedTimeShoot = 0;
            Shoot();
        }

        //On s'occupe du mouvement
        Movement();
    }

    void Movement()
    {


        //On bouge le joueur dans la direction des controles
        transform.Translate(playerSpeed * Time.deltaTime * direction);

        //Si le joueur bouge hors de l'ecran, on le met sur le bord
        //Horizontalement
        if (transform.position.x >= maxX)
        {
            transform.position = new Vector2(maxX, transform.position.y);
        }
        else if (transform.position.x <= minX)
        {
            transform.position = new Vector2(minX, transform.position.y);
        }
        //Verticalement
        if (transform.position.y >= maxY)
        {
            transform.position = new Vector2(transform.position.x, maxY);
        }
        else if (transform.position.y <= minY)
        {
            transform.position = new Vector2(transform.position.x, minY);
        }
    }

    /// <summary>
    /// On prends une balle de l'object pool et lui donne la position et rotation selectionnee,
    /// en plus de lui dire d'attaquer les ennemis et de lui donner le bon sprite
    /// </summary>
    /// <returns>La balle qu'on vient de creer</returns>
    private GameObject CreateBullet(Vector3 position, Quaternion rotation)
    {
        GameObject projectile = bulletPool.GetElement();
        projectile.transform.position = transform.position;
        projectile.transform.position += position;
        projectile.transform.localRotation = rotation;
        projectile.GetComponent<BulletController>().targetTags = new List<string> { "Enemy" };
        projectile.GetComponent<SpriteRenderer>().sprite = bulletSprite;
        projectile.GetComponent<ConstantMovement>().speed = bulletSpeed;
        projectile.SetActive(true);

        return projectile;
    }
    private void Shoot()
    {
        //Si on n'a pas de vie, on ne tire pas
        if (healthComponent.Health <= 0) return;

        //On joue l'effet sonore du tir
        audioSource.clip = laserClip;
        audioSource.Play();

        //On tire un nombre de balles correspondant a la vie
        if (healthComponent.Health == 1)
        {
            CreateBullet(new Vector2(0, 0.5f), Quaternion.identity);
        }
        else if (healthComponent.Health == 2)
        {
            CreateBullet(new Vector2(-0.50f, 0.4f), Quaternion.identity);
            CreateBullet(new Vector2(0.50f, 0.4f), Quaternion.identity);
        }
        else if (healthComponent.Health >= 3)
        {
            CreateBullet(new Vector2(0, 0.5f), Quaternion.identity);
            CreateBullet(new Vector2(0.10f, 0.4f), Quaternion.Euler(0, 0, -15));
            CreateBullet(new Vector2(-0.10f, 0.4f), Quaternion.Euler(0, 0, 15));
        }
    }

    public void TakeDamage()
    {
        audioSource.clip = hitClip;
        audioSource.Play();
    }
    public void GetHealed()
    {
        audioSource.clip = powerupClip;
        audioSource.Play();
    }
    public void Death()
    {
        StartCoroutine(DeathCoroutine());
        GetComponent<Collider2D>().enabled = false;
        enabled = false;
    }

    IEnumerator DeathCoroutine()
    {
        for (int i = 0; i < 10; i++)
        {
            Instantiate(explosionPrefab, transform.position + new Vector3(Random.value - 0.5f, Random.value - 0.5f), Quaternion.identity);

            yield return new WaitForSeconds(Random.Range(0.05f, 0.3f));
        }

        Instantiate(explosionPrefab, transform.position + new Vector3(Random.value - 0.5f, Random.value - 0.5f), Quaternion.identity);
        yield return new WaitForSeconds(0.05f);
        Instantiate(explosionPrefab, transform.position + new Vector3(Random.value - 0.5f, Random.value - 0.5f), Quaternion.identity);
        yield return new WaitForSeconds(0.05f);
        Instantiate(explosionPrefab, transform.position + new Vector3(Random.value - 0.5f, Random.value - 0.5f), Quaternion.identity);


        gameObject.SetActive(false);
    }
}
