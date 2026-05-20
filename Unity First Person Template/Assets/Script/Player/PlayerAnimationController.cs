using System;
using UnityEngine;
using UnityEngine.Windows;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField]
    private Animator animator;

    [SerializeField]
    private PlayerMovement movement;

    // Update is called once per frame
    void Update()
    {
        animator.SetFloat("X Velocity", Mathf.Abs(movement.getMoveSpeed()));
        animator.SetFloat("Y Velocity", Mathf.Abs(movement.getRigidbody().linearVelocity.z));

        animator.SetFloat("Input Y", movement.getMoveDirection().z);

        animator.SetFloat("Input X", movement.getMoveDirection().x);

        animator.SetBool("On Ground", movement.OnGround());

    }
}
