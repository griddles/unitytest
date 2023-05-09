using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    public GameObject playerPrefab;

    public List<GameObject> normalEnemies;
    public List<int> normalEnemyCounts;
    public List<GameObject> strongEnemies;
    public List<int> strongEnemyCounts;
    public List<GameObject> supportEnemies;
    public List<int> supportEnemyCounts;
    public List<GameObject> bossEnemies;

    public TextMeshProUGUI waveText;

    public bool gameStarted = false;
    public bool gameOver = false;

    public GameObject spawnRoom;
    public GameObject spawnDoor;

    public AudioClip deathSound;

    public AudioSource audioSource;

    public Tilemap ground;

    private GameObject player;

    private int wave = 1;

    private int strongCooldown = 5;
    private int supportCooldown = 3;
    private int bossCooldown = 10;

    private Cinemachine.CinemachineVirtualCamera virtualCamera;

    private List<Vector3> spawnPoints;

    void Start()
    {
        virtualCamera = GameObject.FindWithTag("CameraController").GetComponent<Cinemachine.CinemachineVirtualCamera>();
        player = GameObject.FindWithTag("Player");
        audioSource = GetComponent<AudioSource>();

        // get all enemy spawnable points
        spawnPoints = new List<Vector3>();
        for (int n = ground.cellBounds.xMin; n < ground.cellBounds.xMax; n++)
        {
            for (int p = ground.cellBounds.yMin; p < ground.cellBounds.yMax; p++)
            {
                Vector3Int localPlace = new Vector3Int(n, p, (int)ground.transform.position.y);
                Vector3 place = ground.CellToWorld(localPlace);
                if (ground.HasTile(localPlace))
                {
                    //Tile at "place"
                    spawnPoints.Add(place);
                }
                else
                {
                    //No tile at "place"
                }
            }
        }
    }

    

    void FixedUpdate()
    {
        if (GameObject.FindGameObjectsWithTag("Enemy").Length < 1 && gameStarted && !gameOver) 
        {
            WaveEnd();
        }

        if (gameOver)
        {
            if (Input.GetMouseButton(0))
            {
                wave = 1;

                strongCooldown = 5;
                supportCooldown = 3;
                bossCooldown = 10;

                gameOver = false;
                gameStarted = false;

                spawnRoom.SetActive(true);
                spawnDoor.SetActive(false);

                StopAllCoroutines();
                Camera.main.orthographicSize = 7.5f;

                GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
                foreach (GameObject enemy in enemies)
                {
                    Destroy(enemy);
                }

                player.SetActive(true);
                player.transform.position = new Vector3(0, -34, 0);
                player.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
                player.GetComponent<playerMovement>().Heal(500);
                virtualCamera.Follow = player.transform;
            }
        }
    }

    public void WaveStart()
    {
        waveText.text = "Wave " + wave;

        // handle normal enemiess

        // randomly pick 2 enemy types from normalEnemies
        int normalEnemy1 = Random.Range(0, normalEnemies.Count);
        int normalEnemy2 = Random.Range(0, normalEnemies.Count);

        // spawn a random number of each type from 3 to 5 + wave, capped at 12
        int normalEnemy1Count = Mathf.Clamp(Random.Range(1, normalEnemyCounts[normalEnemy1] + wave), 0, normalEnemyCounts[normalEnemy1] * 2);
        int normalEnemy2Count = Mathf.Clamp(Random.Range(1, normalEnemyCounts[normalEnemy2] + wave), 0, normalEnemyCounts[normalEnemy2] * 2);

        // spawn the enemies
        for (int i = 0; i < normalEnemy1Count; i++)
        {
            Vector2 spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
            Instantiate(normalEnemies[normalEnemy1], spawnPoint, Quaternion.identity);
        }
        for (int i = 0; i < normalEnemy2Count; i++)
        {
            Vector2 spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
            Instantiate(normalEnemies[normalEnemy2], spawnPoint, Quaternion.identity);
        }

        // if strongenemycooldown is 0 or lower, give a 75% chance to spawn strong enemies
        if (strongCooldown <= 0 && Random.Range(0, 4) != 0)
        {
            // randomly pick 1 enemy type from strongEnemies
            int strongEnemy1 = Random.Range(0, strongEnemies.Count);
            int strongEnemy1Count = Mathf.Clamp(Random.Range(1, strongEnemyCounts[strongEnemy1] + wave), 0, strongEnemyCounts[strongEnemy1] * 2);
            // spawn the enemies
            for (int i = 0; i < strongEnemy1Count; i++)
            {
                Vector2 spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
                Instantiate(strongEnemies[strongEnemy1], spawnPoint, Quaternion.identity);
            }
            // reset the cooldown
            strongCooldown = 5;
        }
    }

    public void WaveEnd()
    {
        wave += 1;
        strongCooldown -= 1;
        supportCooldown -= 1;
        bossCooldown -= 1;

        WaveStart();
    }

    public void GameEnd()
    {
        gameOver = true;
        // get a list of all enemies
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        // start a coroutine to kill each enemy in rapid succession
        StartCoroutine(KillEnemies(enemies));
        transform.position = virtualCamera.transform.position;
        virtualCamera.Follow = gameObject.transform;
        // start a coroutine that moves this object to the center of the screen
        StartCoroutine(LerpCamera());
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!gameStarted)
        {
            gameStarted = true;
            spawnRoom.SetActive(false);
            spawnDoor.SetActive(true);
            WaveStart();
        }
    }

    IEnumerator KillEnemies(GameObject[] enemies)
    {
        foreach (GameObject enemy in enemies)
        {
            enemy.GetComponent<enemy>().Damage(Vector3.forward, 5, 999);
            yield return new WaitForSecondsRealtime(0.2f);
        }

        // cleanup just in case
        GameObject[] cleanup = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in cleanup)
        {
            Destroy(enemy);
        }
    }

    IEnumerator LerpCamera()
    {
        float t = 0;
        while (t < 10)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, new Vector3(0, 0, 0), t / 10);
            Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, 21.5f, t / 10);
            yield return null;
        }
    }
}
