using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    //Pour le tir
    MyObjectPool bulletPool;
    [SerializeField] float shootCooldown = 0.5f;
    [SerializeField] Sprite bulletSprite;
    [SerializeField] float bulletSpeed;
    float elapsedTimeShoot;

    //Pour le mouvement
    // Inputs
    PlayerInputs inputs;
    InputAction moveAction;
    InputAction shootAction;
    // Vitesse du joueur
    [SerializeField] float playerSpeed = 6;
    // Les limites de l'ecran
    float minY, maxY, minX, maxX;
    // La direction du mouvement
    Vector2 direction = Vector2.zero;

    //Pour le son
    AudioSource audioSource;

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

        //On initialise les inputs
        inputs = new PlayerInputs();
        moveAction = inputs.FindAction("Move");
        shootAction = inputs.FindAction("Shoot");

        //On initialise les references
        bulletPool = GameObject.Find("ObjPoolBullet").GetComponent<MyObjectPool>();
        audioSource = GetComponent<AudioSource>();
        healthComponent = GetComponent<HealthComponent>();
    }

    private void OnEnable()
    {
        //On active les inputs
        shootAction.Enable();
        moveAction.Enable();
        moveAction.performed += (InputAction.CallbackContext ctx) =>
        {
            direction = ctx.ReadValue<Vector2>();
        };
        moveAction.canceled += _ =>
        {
            direction = Vector2.zero;
        };

    }

    private void OnDisable()
    {
        //On desactive les inputs
        shootAction.Disable();
        moveAction.Disable();
        moveAction.performed -= (InputAction.CallbackContext ctx) =>
        {
            direction = ctx.ReadValue<Vector2>();
        };
        moveAction.canceled -= _ =>
        {
            direction = Vector2.zero;
        };
    }

    void Update()
    {
        //On regarde si on veut et peut tirer
        elapsedTimeShoot += Time.deltaTime;
        if (shootAction.IsPressed() && elapsedTimeShoot >= shootCooldown)
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
        transform.Translate(playerSpeed * Time.deltaTime * direction.normalized);

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
