using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossController : MonoBehaviour
{
    //References necessaires
    GameManager gameManager;
    SpriteRenderer coreSpriteRenderer;
    MyObjectPool bulletPool;
    MyObjectPool rocketPool;
    [SerializeField] List<Sprite> coreSprites;
    [SerializeField] HealthComponent myHealth, leftShieldGen, rightShieldGen;
    [SerializeField] Transform leftGunTransform, centerGunTransform, rightGunTransform;
    Transform coreTransform, leftShieldGenTransform, rightShieldGenTransform;
    [SerializeField] GameObject explosionPrefab;
    [SerializeField] Sprite bulletSprite;
    Slider healthBar;
    //audio stuff
    AudioSource audioSource;
    [SerializeField] AudioClip bossMusic, laserClip, rocketClip, circleLaserClip;

    [SerializeField] Vector3 mainPosition = new Vector3(0, 3, 0);
    [SerializeField] float movementSpeed = 4;
    /// <summary>
    /// Le nombre de points donnes par le boss
    /// </summary>
    [SerializeField] int points = 1000;
    /// <summary>
    /// Chaque KeyValuePair a la fonction d'attaque comme clee et comme valeur a partir de quelle phase elle peut etre utilisee
    /// </summary>
    List<(Func<IEnumerator>, int)> attacks = new List<(Func<IEnumerator>, int)>();

    // phases 1,2,3,4
    int phase = 1;
    //Est-ce qu'on peut commencer une nouvelle attaque?
    bool canAttack = false;
    Coroutine currentAttack;
    bool dying = false;

    private void Awake()
    {
        //Tout initialiser
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        gameManager.pauseEnemySpawning = true;
        gameManager.ChangeMusic(bossMusic);

        coreSpriteRenderer = myHealth.gameObject.GetComponent<SpriteRenderer>();
        healthBar = GetComponentInChildren<Slider>();
        bulletPool = GameObject.Find("ObjPoolBullet").GetComponent<MyObjectPool>();
        rocketPool = GameObject.Find("ObjPoolRocket").GetComponent<MyObjectPool>();
        audioSource = GetComponent<AudioSource>();

        coreTransform = myHealth.transform;
        leftShieldGenTransform = leftShieldGen.transform;
        rightShieldGenTransform = rightShieldGen.transform;


        InitializeAttacks();
    }

    private IEnumerator Start()
    {
        //Initialiser la barre de vie
        healthBar.maxValue = myHealth.maxHealth;
        healthBar.value = myHealth.Health;


        //Le boss ne peut pas prendre de dommage quand il apparait
        foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
        {
            col.enabled = false;
        }

        //Bouger vers la position principale
        Vector3 direction = (mainPosition - transform.position).normalized;
        float distance = float.PositiveInfinity;
        //Tant qu'on est pas rendu la, on continue a bouger
        while (transform.position != mainPosition)
        {
            //On attends une frame
            yield return null;
            //On bouge a la vitesse voulue
            transform.Translate(direction * Time.deltaTime * movementSpeed * 0.5f);
            //Si on est rendu plus loin, c'est qu'on a depasser. Dans ce cas on se met a la position principale
            if (Vector3.Distance(transform.position, mainPosition) > distance)
                transform.position = mainPosition;
            //On calcule la nouvelle distance
            distance = Vector3.Distance(transform.position, mainPosition);
        }

        //Le boss peut maintenant prendre du dommage
        foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
        {
            col.enabled = true;
        }
        canAttack = true;
    }

    private void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.pauseEnemySpawning = false;
            gameManager.ChangeMusic(gameManager.music);
        }
    }

    private void Update()
    {
        if (canAttack && !dying && attacks.Count > 0)
        {
            //On trouve les attaques possibles dans la phase dans laquelle on est
            List<Func<IEnumerator>> possibleAttacks = new List<Func<IEnumerator>>();
            foreach ((Func<IEnumerator>, int) tuple in attacks)
            {
                if (tuple.Item2 == phase)
                    possibleAttacks.Add(tuple.Item1);
            }

            if (possibleAttacks.Count > 0)
            {
                //Puis on attends 2 secondes et commence une attaque aleatoire parmis celles-ci
                currentAttack = StartCoroutine(WaitAndExecute(possibleAttacks[UnityEngine.Random.Range(0, possibleAttacks.Count)], 1.5f));

                //On ne peut plus commencer une attaque
                canAttack = false;
            }
        }
    }

    //On attends un certaint temps avant de commencer une coroutine
    IEnumerator WaitAndExecute(Func<IEnumerator> func, float time)
    {
        yield return new WaitForSeconds(time);
        currentAttack = StartCoroutine(func());
    }


    public void OnCoreDamage(int damage, int health)
    {
        healthBar.value = myHealth.Health;
        float healthpercent = (float)health / (float)myHealth.maxHealth;

        int newPhase = 4 - Mathf.FloorToInt(healthpercent * 4);
        if (newPhase > phase)
        {
            phase = newPhase;
            ChangePhase();
        }

        coreSpriteRenderer.sprite = coreSprites[Mathf.FloorToInt(healthpercent * 4)];
    }

    void ChangePhase()
    {
        //On change pas de phase si on est mort
        if (myHealth.Health > 0)
        {
            //On reactive les generateur de bouclier
            leftShieldGen.Heal(int.MaxValue);
            rightShieldGen.Heal(int.MaxValue);
        }
    }

    public void OnDeath()
    {
        StartCoroutine(DeathCoroutine());
        if (currentAttack != null)
        {
            StopCoroutine(currentAttack);
        }
    }

    IEnumerator DeathCoroutine()
    {
        //On donne les points
        gameManager.Score += points;
        //On desactive les collisions
        foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
        {
            col.enabled = false;
        }
        //Combien de temps la sequence de mort va-t-elle prendre
        float timer = 5;
        float explosionChancePerSecond = 10;
        while (timer >= 0)
        {
            if (UnityEngine.Random.value <= explosionChancePerSecond * Time.deltaTime)
            {
                Vector3 explosionPosition = transform.GetChild(UnityEngine.Random.Range(0, transform.childCount)).position;
                Instantiate(explosionPrefab, explosionPosition, Quaternion.identity);
            }

            timer -= Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }



    void ShootBullet(Vector3 position, Quaternion rotation, float speed, bool sfx)
    {
        GameObject bullet = bulletPool.GetElement();
        bullet.transform.position = position;
        bullet.transform.rotation = rotation;
        bullet.GetComponent<BulletController>().targetTags = new List<string> { "Player" };
        bullet.GetComponent<SpriteRenderer>().sprite = bulletSprite;
        bullet.GetComponent<ConstantMovement>().speed = speed;
        bullet.SetActive(true);

        if (sfx)
        {
            audioSource.clip = laserClip;
            audioSource.Play();
        }
    }

    void ShootRocket(Vector3 position, Quaternion rotation, float speed, float rotationSpeed, bool sfx)
    {
        GameObject rocket = rocketPool.GetElement();
        rocket.transform.position = position;
        rocket.transform.rotation = rotation;
        rocket.GetComponent<BulletController>().targetTags = new List<string> { "Player" };
        rocket.GetComponent<ConstantMovement>().speed = speed;
        rocket.GetComponent<RocketController>().rotationSpeed = rotationSpeed;
        rocket.SetActive(true);

        if (sfx)
        {
            audioSource.clip = rocketClip;
            audioSource.Play();
        }
    }

    void InitializeAttacks()
    {
        //Phase 1
        attacks.Add((BulletCircleAttack, 1));
        attacks.Add((BulletSpiralAttack, 1));
        attacks.Add((MovingBlasterAttack, 1));
        attacks.Add((SingleBlasterAttack, 1));


        //Phase 2
        attacks.Add((BulletCircleAttack, 2));
        attacks.Add((BulletSpiralAttack, 2));
        attacks.Add((MovingBlasterAttack, 2));
        attacks.Add((DoubleBlasterAttack, 2));

        //Phase 3
        attacks.Add((DoubleBulletCircleAttack, 3));
        attacks.Add((BulletSpiralAttack, 3));
        attacks.Add((RocketAttack, 3));

        //Phase 4
        attacks.Add((DoubleBulletCircleAttack, 4));
        attacks.Add((RocketAttack, 4));
        attacks.Add((TripleBlasterAttack, 4));
    }
    
    //Les attaques:
    IEnumerator BulletCircleAttack()
    {
        //Stuff to tweak for balance
        float timeToShoot = 0.5f;
        int shotNum = 5;
        int bulletNum = 20;

        //Calculated stuff
        float anglePerBullet = 360 / bulletNum;
        float offsetAngle = anglePerBullet / 4;

        for (int j = 0; j < shotNum; j++)
        {
            for (int i = 0; i < bulletNum; i++)
            {
                ShootBullet(coreTransform.position, Quaternion.Euler(0, 0, (anglePerBullet * i) + offsetAngle), 8, false);
            }
            audioSource.clip = circleLaserClip;
            audioSource.Play();
            offsetAngle *= -1;
            yield return new WaitForSeconds(timeToShoot);
        }

        //On a fini d'attaquer, donc on peut dire au boss d'attaquer encore
        canAttack = true;
    }

    IEnumerator BulletSpiralAttack()
    {
        //Stuff to tweak for balance
        float timeToShoot = 0.1f;
        int shotNum = 20;
        int bulletNum = 8;
        float offsetIncrement = 3.5f * Mathf.Sign(UnityEngine.Random.value - 0.5f); //Dans une direction aleatoire

        //Calculated stuff
        float anglePerBullet = 360 / bulletNum;
        float offsetAngle = 0;

        audioSource.clip = circleLaserClip;
        audioSource.Play();
        for (int j = 0; j < shotNum; j++)
        {
            for (int i = 0; i < bulletNum; i++)
            {
                ShootBullet(coreTransform.position, Quaternion.Euler(0, 0, (anglePerBullet * i) + offsetAngle), 8, false);
            }
            offsetAngle += offsetIncrement;
            yield return new WaitForSeconds(timeToShoot);
        }

        //On a fini d'attaquer, donc on peut dire au boss d'attaquer encore
        canAttack = true;
    }

    IEnumerator DoubleBulletCircleAttack()
    {
        //Stuff to tweak for balance
        float timeToShoot = 0.5f;
        int shotNum = 4;
        int bulletNum = 15;

        //Calculated stuff
        float anglePerBullet = 360 / bulletNum;
        float offsetAngle = anglePerBullet / 4;

        for (int j = 0; j < shotNum; j++)
        {
            for (int i = 0; i < bulletNum; i++)
            {
                ShootBullet(leftShieldGenTransform.position, Quaternion.Euler(0, 0, (anglePerBullet * i) + offsetAngle), 8, false);
            }
            for (int i = 0; i < bulletNum; i++)
            {
                ShootBullet(rightShieldGenTransform.position, Quaternion.Euler(0, 0, (anglePerBullet * i) + offsetAngle), 8, false);
            }
            audioSource.clip = circleLaserClip;
            audioSource.Play();
            offsetAngle *= -1;
            yield return new WaitForSeconds(timeToShoot);
        }

        //On a fini d'attaquer, donc on peut dire au boss d'attaquer encore
        canAttack = true;
    }
    IEnumerator RocketAttack()
    {
        //Stuff to tweak for balance
        float timeToShoot = 0.4f;
        float currentTime = 0;

        //Calculated stuff
        float maxX;
        float minX;
        int direction;
        // direction gauche droite aléatoire
        if (UnityEngine.Random.value < 0.5f)
        {
            minX = gameManager.MinX + 3f;
            maxX = gameManager.MaxX - 3f;
            direction = -1;
        }
        else
        {
            minX = gameManager.MaxX - 3f;
            maxX = gameManager.MinX + 3f;
            direction = 1;
        }
        
        // premier déplacement
        while (Mathf.Abs(transform.position.x - minX) > 0.2f)
        {
            transform.Translate(direction * Vector3.right * Time.deltaTime * movementSpeed * 3f);
            yield return null;
        }

        // inverser la direction du déplacement
        direction = -direction;
        // tirer et déplacement
        while (Mathf.Abs(transform.position.x - maxX) > 0.2f)
        {
            transform.Translate(direction * Vector3.right * Time.deltaTime * movementSpeed * 1.5f);
            if (currentTime >= timeToShoot)
            {
                ShootRocket(coreTransform.position, Quaternion.identity, 6, 180, true);
                currentTime = 0;
            }
            currentTime += Time.deltaTime;
            yield return null;
        }

        //Retourner a la position principale
        Vector3 returnDirection = (mainPosition - transform.position).normalized;
        float distance = float.PositiveInfinity;
        //Tant qu'on est pas rendu la, on continue a bouger
        while (transform.position != mainPosition)
        {
            //On attends une frame
            yield return null;
            //On bouge a la vitesse voulue
            transform.Translate(returnDirection * Time.deltaTime * movementSpeed);
            //Si on est rendu plus loin, c'est qu'on a depasser. Dans ce cas on se met a la position principale
            if (Vector3.Distance(transform.position, mainPosition) > distance)
                transform.position = mainPosition;
            //On calcule la nouvelle distance
            distance = Vector3.Distance(transform.position, mainPosition);
        }

        //On a fini d'attaquer, donc on peut dire au boss d'attaquer encore
        canAttack = true;
    }
    IEnumerator MovingBlasterAttack()
    {
        //Stuff to tweak for balance
        float shootCooldown = 0.7f;
        float shotCount = 10;

        Transform playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        float shootTimer = shootCooldown;

        //On tire tout droit en bougeant pour se mettre devant le joueur
        while (shotCount > 0)
        {
            int xDirection = (int)Mathf.Sign(playerTransform.position.x - transform.position.x); //La direction pour bouger vers le joueur
            transform.Translate(new Vector3(xDirection, 0, 0) * movementSpeed * Time.deltaTime); //On bouge pour se mettre devant le joueur

            if (shootTimer <= 0)
            {
                ShootBullet(leftGunTransform.GetChild(0).position, Quaternion.Euler(0, 0, 180), 8, true);
                ShootBullet(rightGunTransform.GetChild(0).position, Quaternion.Euler(0, 0, 180), 8, false);
                ShootBullet(centerGunTransform.GetChild(0).position, Quaternion.Euler(0, 0, 180), 8, false);
                shootTimer = shootCooldown;
                shotCount--;
            }

            yield return null;
            shootTimer -= Time.deltaTime;
        }

        //Retourner a la position principale
        Vector3 returnDirection = (mainPosition - transform.position).normalized;
        float distance = float.PositiveInfinity;
        //Tant qu'on est pas rendu la, on continue a bouger
        while (transform.position != mainPosition)
        {
            //On attends une frame
            yield return null;
            //On bouge a la vitesse voulue
            transform.Translate(returnDirection * Time.deltaTime * movementSpeed);
            //Si on est rendu plus loin, c'est qu'on a depasser. Dans ce cas on se met a la position principale
            if (Vector3.Distance(transform.position, mainPosition) > distance)
                transform.position = mainPosition;
            //On calcule la nouvelle distance
            distance = Vector3.Distance(transform.position, mainPosition);
        }

        //On a fini d'attaquer, donc on peut dire au boss d'attaquer encore
        canAttack = true;
    }

    IEnumerator SingleBlasterAttack()
    {
        //Stuff to tweak for balance
        float shootCooldown = 0.3f;
        float shotCount = 10;
        float rotationSpeed = 45;

        Transform playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        float shootTimer = shootCooldown;

        while (shotCount > 0)
        {
            //Le joueur est dans quelle direction
            float targetZRotation = Quaternion.LookRotation(new Vector3(0, 0, 1), centerGunTransform.position - playerTransform.position).eulerAngles.z;
            //Je dois tourner le gun dans quelle direction pour aller vers le joueur?
            float diff1 = targetZRotation - centerGunTransform.rotation.eulerAngles.z;
            float diff2 = diff1 + (360 * Mathf.Sign(diff1) * -1);
            float direction = Mathf.Abs(diff1) > Mathf.Abs(diff2) ? Mathf.Sign(diff2) : Mathf.Sign(diff1);
            //Je tourne a la vitesse desiree dans cette direction
            centerGunTransform.Rotate(new Vector3(0, 0, direction * rotationSpeed * Time.deltaTime));

            //Je tire le gun si le cooldown a fini
            if (shootTimer <= 0)
            {
                ShootBullet(centerGunTransform.GetChild(0).position, Quaternion.Euler(0, 0, centerGunTransform.rotation.eulerAngles.z + 180), 8, true);
                shootTimer = shootCooldown;
                shotCount--;
            }

            yield return null;
            shootTimer -= Time.deltaTime;
        }

        //On a fini d'attaquer, donc on peut dire au boss d'attaquer encore et on reset les rotations des guns
        centerGunTransform.rotation = Quaternion.identity;
        canAttack = true;
    }
    IEnumerator DoubleBlasterAttack()
    {
        //Stuff to tweak for balance
        float shootCooldown = 0.3f;
        float shotCount = 10;
        float rotationSpeed = 45;

        Transform playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        List<Transform> guns = new List<Transform> { leftGunTransform, rightGunTransform };
        float shootTimer = shootCooldown;

        while (shotCount > 0)
        {
            foreach (Transform gun in guns)
            {
                //Le joueur est dans quelle direction
                float targetZRotation = Quaternion.LookRotation(new Vector3(0, 0, 1), gun.position - playerTransform.position).eulerAngles.z;
                //Je dois tourner le gun dans quelle direction pour aller vers le joueur?
                float diff1 = targetZRotation - gun.rotation.eulerAngles.z;
                float diff2 = diff1 + (360 * Mathf.Sign(diff1) * -1);
                float direction = Mathf.Abs(diff1) > Mathf.Abs(diff2) ? Mathf.Sign(diff2) : Mathf.Sign(diff1);
                //Je tourne a la vitesse desiree dans cette direction
                gun.Rotate(new Vector3(0, 0, direction * rotationSpeed * Time.deltaTime));
            }

            //Je tire le gun si le cooldown a fini
            if (shootTimer <= 0)
            {
                ShootBullet(leftGunTransform.GetChild(0).position, Quaternion.Euler(0, 0, leftGunTransform.rotation.eulerAngles.z + 180), 8, false);
                ShootBullet(rightGunTransform.GetChild(0).position, Quaternion.Euler(0, 0, rightGunTransform.rotation.eulerAngles.z + 180), 8, true);
                shootTimer = shootCooldown;
                shotCount--;
            }

            yield return null;
            shootTimer -= Time.deltaTime;
        }

        //On a fini d'attaquer, donc on peut dire au boss d'attaquer encore et on reset les rotations des guns
        leftGunTransform.rotation = Quaternion.identity;
        rightGunTransform.rotation = Quaternion.identity;
        canAttack = true;
    }
    IEnumerator TripleBlasterAttack()
    {
        //Stuff to tweak for balance
        float shootCooldown = 0.3f;
        float shotCount = 10;
        float rotationSpeed = 45;

        Transform playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        List<Transform> guns = new List<Transform> { leftGunTransform, centerGunTransform, rightGunTransform };
        float shootTimer = shootCooldown;

        while (shotCount > 0)
        {
            foreach (Transform gun in guns)
            {
                //Le joueur est dans quelle direction
                float targetZRotation = Quaternion.LookRotation(new Vector3(0, 0, 1), gun.position - playerTransform.position).eulerAngles.z;
                //Je dois tourner le gun dans quelle direction pour aller vers le joueur?
                float diff1 = targetZRotation - gun.rotation.eulerAngles.z;
                float diff2 = diff1 + (360 * Mathf.Sign(diff1) * -1);
                float direction = Mathf.Abs(diff1) > Mathf.Abs(diff2) ? Mathf.Sign(diff2) : Mathf.Sign(diff1);
                //Je tourne a la vitesse desiree dans cette direction
                gun.Rotate(new Vector3(0, 0, direction * rotationSpeed * Time.deltaTime));
            }

            //Je tire le gun si le cooldown a fini
            if (shootTimer <= 0)
            {
                ShootBullet(leftGunTransform.GetChild(0).position, Quaternion.Euler(0, 0, leftGunTransform.rotation.eulerAngles.z + 180), 8, true);
                ShootBullet(rightGunTransform.GetChild(0).position, Quaternion.Euler(0, 0, rightGunTransform.rotation.eulerAngles.z + 180), 8, false);
                ShootBullet(centerGunTransform.GetChild(0).position, Quaternion.Euler(0, 0, centerGunTransform.rotation.eulerAngles.z + 180), 8, false);
                shootTimer = shootCooldown;
                shotCount--;
            }

            yield return null;
            shootTimer -= Time.deltaTime;
        }

        //On a fini d'attaquer, donc on peut dire au boss d'attaquer encore et on reset les rotations des guns
        leftGunTransform.rotation = Quaternion.identity;
        rightGunTransform.rotation = Quaternion.identity;
        centerGunTransform.rotation = Quaternion.identity;
        canAttack = true;
    }
}
