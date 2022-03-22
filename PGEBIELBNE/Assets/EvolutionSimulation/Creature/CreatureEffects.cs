using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureEffects : MonoBehaviour
{
    public ParticleSystem biteParticle;

    public void Bite()
    {
        biteParticle.Play();
    }
}
