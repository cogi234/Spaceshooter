using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpawningController : MonoBehaviour
{
    MyObjectPool meteorPool;
    MyObjectPool enemyPool;
    [SerializeField] GameObject powerUpPrefab;

    /// <summary>
    /// A multiplier on spawning chances
    ///  -Meteor chance
    ///  -Enemy chance
    /// </summary>
    [SerializeField] float difficulty = 1;
    public float Difficulty { get => difficulty; }
    /// <summary>
    /// How much difficulty multiplies by every minute
    /// </summary>
    [SerializeField] float difficultyScaling = 1.1f;

    //Meteor Stuff
    /// <summary>
    /// The chance for a meteor to appear every second, or the average amount of meteors that will appear every second
    /// </summary>
    [SerializeField] float meteorChance = 1;
    /// <summary>
    /// The speed of meteors
    /// </summary>
    [SerializeField] float meteorSpeed = -1.5f;
    /// <summary>
    /// How many shots a meteor survives
    /// </summary>
    [SerializeField] float meteorHealth = 1;

    //Enemy Stuff
    /// <summary>
    /// The chance for an enemy formation to appear every second, or the average amount of enemy formations that will appear every second
    /// </summary>
    [SerializeField] float enemyChance = 0.2f;
    /// <summary>
    /// The speed of enemies
    /// </summary>
    [SerializeField] float enemySpeed = -1f;
    /// <summary>
    /// How many shots an enemy survives
    /// </summary>
    [SerializeField] float enemyHealth = 1;
    /// <summary>
    /// How much time between shots
    /// </summary>
    [SerializeField] float enemyShootCooldown = 3;
    /// <summary>
    /// How fast do the bullets go
    /// </summary>
    [SerializeField] float enemyBulletSpeed = 6;

    /// <summary>
    /// Every formation is an empty object with multiple child spawning points
    /// </summary>
    [SerializeField] List<GameObject> enemyFormations;
    [SerializeField] List<int> formationWeights;

    //Where do we spawn stuff:
    float spawningY, minX, maxX;

    private void Awake()
    {
        spawningY = Camera.main.orthographicSize + 1;
        maxX = (Camera.main.orthographicSize * Screen.width / Screen.height) - 1.5f;
        minX = -maxX;

        meteorPool = GameObject.Find("ObjPoolMeteor").GetComponent<MyObjectPool>();
        enemyPool = GameObject.Find("ObjPoolEnemy").GetComponent<MyObjectPool>();
    }

    void Update()
    {
        difficulty = Mathf.Pow(difficultyScaling, (Time.timeSinceLevelLoad / 60));

        //Meteor Spawning
        if (Random.value <= (meteorChance * difficulty * Time.deltaTime))
            SpawnMeteor();

        //Enemy Spawning
        if (Random.value <= (enemyChance * difficulty * Time.deltaTime))
            SpawnFormation();
    }

    void SpawnMeteor()
    {
        GameObject meteor = meteorPool.GetElement();
        meteor.transform.position = new Vector2(Random.Range(minX, maxX), spawningY);
        meteor.GetComponent<ConstantMovement>().speed = meteorSpeed;
        meteor.GetComponent<ConstantRotation>().rotationSpeed = Random.Range(-90f, 90f);//Rotation aleatoire
        meteor.GetComponent<EnemyController>().health = Mathf.FloorToInt(meteorHealth);
        meteor.SetActive(true);
    }

    void SpawnFormation()
    {
        //On prend une formation aleatoire selon les poids des formations
        int rnd = Random.Range(0, formationWeights.Sum());
        int formationIndex = 0;
        for (int i = 0; i < formationWeights.Count; i++)
        {
            rnd -= formationWeights[i];
            if (rnd <= 0)
            {
                formationIndex = i;
                break;
            }
        }

        GameObject formation = Instantiate(enemyFormations[formationIndex], new Vector2(Random.Range(minX, maxX), spawningY), Quaternion.identity);
        EnemySpawner[] spawners = formation.GetComponentsInChildren<EnemySpawner>();

        foreach (EnemySpawner spawner in spawners)
        {
            spawner.enemyPool = enemyPool;
            spawner.enemySpeed = enemySpeed;
            spawner.enemyHealth = enemyHealth;
            spawner.enemyShootCooldown = enemyShootCooldown;
            spawner.enemyBulletSpeed = enemyBulletSpeed;
        }

        formation.transform.DetachChildren();
        Destroy(formation);
    }

    public void SpawnPowerUp()
    {
        GameObject powerUp = Instantiate(powerUpPrefab, new Vector2(Random.Range(minX, maxX), spawningY), Quaternion.identity);
    }
}