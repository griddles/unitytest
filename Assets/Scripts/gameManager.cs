using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public float arenaX;
    public float arenaY;

    public List<GameObject> normalEnemies;
    public List<GameObject> strongEnemies;
    public List<GameObject> supportEnemies;
    public List<GameObject> bossEnemies;

    public TextMeshProUGUI waveText;

    public bool gameStarted = false;
    public bool gameOver = false;

    public GameObject spawnRoom;
    public GameObject spawnDoor;

    private int wave = 1;

    private int strongCooldown = 5;
    private int supportCooldown = 3;
    private int bossCooldown = 10;

    private List<GameObject> activeEnemies;

    private Cinemachine.CinemachineVirtualCamera virtualCamera;

    void Start()
    {
        virtualCamera = GameObject.FindWithTag("CameraController").GetComponent<Cinemachine.CinemachineVirtualCamera>();
    }

    void FixedUpdate()
    {
        if (GameObject.FindGameObjectsWithTag("Enemy").Length < 1 && gameStarted && !gameOver) 
        {
            WaveEnd();
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
        int normalEnemy1Count = Mathf.Clamp(Random.Range(3, 5 + wave), 0, 12);
        int normalEnemy2Count = Mathf.Clamp(Random.Range(3, 5 + wave), 0, 12);

        // spawn the enemies
        for (int i = 0; i < normalEnemy1Count; i++)
        {
            Instantiate(normalEnemies[normalEnemy1], new Vector3(Random.Range(-arenaX, arenaX), Random.Range(-arenaY, arenaY), 0), Quaternion.identity);
        }
        for (int i = 0; i < normalEnemy2Count; i++)
        {
            Instantiate(normalEnemies[normalEnemy2], new Vector3(Random.Range(-arenaX, arenaX), Random.Range(-arenaY, arenaY), 0), Quaternion.identity);
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
