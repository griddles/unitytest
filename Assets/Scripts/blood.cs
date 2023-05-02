using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class blood : MonoBehaviour
{
    public ParticleSystem part;

    void Start()
    {
        part = GetComponent<ParticleSystem>();
    }

    void OnParticleCollision(GameObject other)
    {
        if (other.tag == "Player")
        {
            // heal the player
            other.GetComponent<playerMovement>().Heal(3);
        }
    }
}
