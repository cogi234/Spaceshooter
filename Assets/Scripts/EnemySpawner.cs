using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    //Les parametres du wavemotion
    public Vector2 waveMotionDirection;
    public float waveMotionRadPerSecond;
    //L'ennemi est spawner apres ce delai
    public float delay;

    //Les parametres dont les ennemis ont besoin
    [HideInInspector] public MyObjectPool enemyPool;
    [HideInInspector] public float enemySpeed;
    [HideInInspector] public float enemyHealth;
    [HideInInspector] public float enemyShootCooldown;
    [HideInInspector] public float enemyBulletSpeed;

    void Update()
    {
        delay -= Time.deltaTime;

        if (delay <= 0)
            SpawnEnemy();
    }

    void SpawnEnemy()
    {
        //On cree un ennemi et lui assigne tous les parametres necessaires
        GameObject enemy = enemyPool.GetElement();
        enemy.transform.position = transform.position;
        enemy.GetComponent<ConstantMovement>().speed = enemySpeed;
        WaveMotion wave = enemy.GetComponent<WaveMotion>();
        wave.direction = waveMotionDirection;
        wave.radPerSecond = waveMotionRadPerSecond;
        EnemyController controller = enemy.GetComponent<EnemyController>();
        controller.shootCooldown = enemyShootCooldown;
        controller.bulletSpeed = enemyBulletSpeed;
        HealthComponent health = enemy.GetComponent<HealthComponent>();
        health.maxHealth = Mathf.FloorToInt(enemyHealth);
        health.startingHealth = Mathf.FloorToInt(enemyHealth);
        enemy.SetActive(true);
        //Le spawner disparait
        Destroy(gameObject);
    }
}
