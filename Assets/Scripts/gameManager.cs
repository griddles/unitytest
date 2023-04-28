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

    private int wave = 1;

    private int strongCooldown = 5;
    private int supportCooldown = 3;
    private int bossCooldown = 10;

    private List<GameObject> activeEnemies;

    void Start()
    {
        WaveStart();
    }

    void FixedUpdate()
    {
        if (GameObject.FindGameObjectsWithTag("Enemy").Length < 1) 
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

        Debug.Log(normalEnemy1 + " | " + normalEnemy2);

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
}
