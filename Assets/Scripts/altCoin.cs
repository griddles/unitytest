using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class altCoin : MonoBehaviour
{
    public GameObject coin;
    public float coinForce;
    public float spread;
    public float maxCoins;
    public AudioClip coinFlip;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        int coinCount = GameObject.FindGameObjectsWithTag("Coin :)").Length;
        // if the player right clicks, spawn a coin at the player and give it coinForce towards the mouse
        if (Input.GetMouseButtonDown(1) && coinCount < maxCoins)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 direction = mousePos - transform.position;
            // rotate the direction vector by a random amount
            direction = Quaternion.Euler(0, 0, Random.Range(-spread, spread)) * direction;
            direction.Normalize();
            GameObject newCoin = Instantiate(coin, transform.position, Quaternion.identity);
            // get the player
            GameObject player = GameObject.FindWithTag("Player");
            newCoin.GetComponent<Rigidbody2D>().velocity = (coinForce * new Vector2(direction.x, direction.y)) + player.GetComponent<Rigidbody2D>().velocity;

            audioSource.PlayOneShot(coinFlip);
        }
    }
}
