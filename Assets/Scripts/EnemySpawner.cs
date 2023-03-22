using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public Vector2 waveMotionDirection;
    public float waveMotionRadPerSecond;
    public float delay;

    [HideInInspector] public float difficulty;
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
        GameObject enemy = enemyPool.GetElement();
        enemy.transform.position = transform.position;
        enemy.GetComponent<ConstantMovement>().speed = enemySpeed * difficulty;
        WaveMotion wave = enemy.GetComponent<WaveMotion>();
        wave.direction = waveMotionDirection;
        wave.radPerSecond = waveMotionRadPerSecond * difficulty;
        EnemyController controller = enemy.GetComponent<EnemyController>();
        controller.health = Mathf.FloorToInt(enemyHealth);
        controller.shootCooldown = enemyShootCooldown / difficulty;
        controller.bulletSpeed = enemyBulletSpeed * difficulty;
        enemy.SetActive(true);

        Destroy(gameObject);
    }
}
