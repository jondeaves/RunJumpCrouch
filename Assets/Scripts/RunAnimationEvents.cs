using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunAnimationEvents : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GameObject.FindGameObjectWithTag("Player").GetComponent<Animator>().SetBool("IsRunning", true);
    }
}
