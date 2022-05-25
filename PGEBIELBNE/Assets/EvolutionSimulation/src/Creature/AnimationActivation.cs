using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySimulation
{
    public class AnimationActivation : MonoBehaviour
    {
        public Animator[] animators;
        public void Activation(bool mode)
        {
            foreach (Animator ator in animators)
                ator.enabled = mode;
        }
    }
}
