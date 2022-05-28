using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySimulation
{
    /// <summary>
    /// De/Activates the animations setted on the inspector.
    /// Used to activate walking and flying creatures animations.
    /// </summary>
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
