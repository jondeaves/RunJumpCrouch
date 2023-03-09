using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnAnimationEvent : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GameObject.FindGameObjectWithTag( "Player" ).GetComponent<Movement>().Turn();
    }
}
