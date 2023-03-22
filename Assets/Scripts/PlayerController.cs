using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float shootCooldown = 0.5f;
    [SerializeField] Sprite bulletSprite;
    [SerializeField] float bulletSpeed;
    [SerializeField] float damageCooldown = 2;
    [SerializeField] float playerSpeed = 6;
    MyObjectPool bulletPool;

    float elapsedTimeDamage;
    float elapsedTimeShoot;

    int playerHealth = 1;
    //inputs
    PlayerInputs inputs;
    InputAction moveAction;
    InputAction shootAction;
    //limitations of player
    float minY, maxY, minX, maxX;
    //mouvements direction
    Vector2 direction = Vector2.zero;

    AudioSource audioSource;

    void Awake()
    {
        maxY = Camera.main.orthographicSize - 0.5f;
        minY = -maxY;
        maxX = (Camera.main.orthographicSize * Screen.width / Screen.height) - 0.5f;
        minX = -maxX;

        inputs = new PlayerInputs();
        moveAction = inputs.FindAction("Move");
        shootAction = inputs.FindAction("Shoot");

        bulletPool = GameObject.Find("ObjPoolBullet").GetComponent<MyObjectPool>();

        audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
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
        //damage
        elapsedTimeDamage += Time.deltaTime;
        //shoot
        elapsedTimeShoot += Time.deltaTime;
        if (shootAction.IsPressed() && elapsedTimeShoot >= shootCooldown)
        {
            elapsedTimeShoot = 0;
            Shoot();
        }
    }

    void Movement()
    {
        transform.Translate(playerSpeed * Time.deltaTime * direction.normalized);

        //I want to be able to move everywhere on screen
        if (transform.position.x >= maxX)
        {
            transform.position = new Vector2(maxX, transform.position.y);
        }
        else if (transform.position.x <= minX)
        {
            transform.position = new Vector2(minX, transform.position.y);
        }

        if (transform.position.y >= maxY)
        {
            transform.position = new Vector2(transform.position.x, maxY);
        }
        else if (transform.position.y <= minY)
        {
            transform.position = new Vector2(transform.position.x, minY);
        }
    }

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
        if (playerHealth <= 0) return;

        audioSource.Play();

        if (playerHealth == 1)
        {
            CreateBullet(new Vector2(0, 0.5f), Quaternion.identity);
        }
        else if (playerHealth == 2)
        {
            CreateBullet(new Vector2(-0.50f, 0.4f), Quaternion.identity);
            CreateBullet(new Vector2(0.50f, 0.4f), Quaternion.identity);
        }
        else if (playerHealth >= 3)
        {
            CreateBullet(new Vector2(0, 0.5f), Quaternion.identity);
            CreateBullet(new Vector2(0.10f, 0.4f), Quaternion.Euler(0, 0, -15));
            CreateBullet(new Vector2(-0.10f, 0.4f), Quaternion.Euler(0, 0, 15));
        }
    }

    public void PowerUp()
    {
        playerHealth++;
    }

    public void TakeDamage()
    {
        if (damageCooldown <= elapsedTimeDamage)
        {
            elapsedTimeDamage = 0;
            playerHealth--;
            if (playerHealth == 0)
            {
                Death();
            }
        }
    }
    void Death()
    {
        SceneManager.LoadScene(0);
    }
}
