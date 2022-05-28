using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureEffects : MonoBehaviour
{
    public ParticleSystem biteParticle;
    public ParticleSystem eatParticle;
    public ParticleSystem drinkParticle;

    public void Bite()
    {
        biteParticle.Play();
    }

    public void Eat()
    {
        eatParticle.Play();
    }

    public void Drink()
    {
        drinkParticle.Play();
    }
}
