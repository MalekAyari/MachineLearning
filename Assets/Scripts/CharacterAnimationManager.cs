using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimationManager : MonoBehaviour
{
    public Animator animator;
    public CharacterController characterController;
    public MoveToTarget moveToTarget;
    public float moveSpeed;
    public Transform characterModel;
    public float rotationSpeed;

    private float moveX;
    private float moveZ;
    private Vector3 velocity;
    private float gravity = -9.81f;
    private float fallSpeed;

    private void Update()
    {
        moveX = moveToTarget.moveX;
        moveZ = moveToTarget.moveZ;
        UpdateAnimatorParameters();
    }

    private void FixedUpdate()
    {
        if (characterController.isGrounded)
        {
            fallSpeed = 0f;
        }
        else
        {
            fallSpeed += gravity * Time.fixedDeltaTime;
        }

        Vector3 direction = new Vector3(moveX, 0, moveZ);

        if (direction.magnitude > 1f)
        {
            direction.Normalize();
        }

        velocity = new Vector3(direction.x, fallSpeed, direction.z);
        characterController.Move(velocity * Time.fixedDeltaTime * moveSpeed);

        if (moveX != 0 || moveZ != 0)
        {
            if (direction.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                characterModel.rotation = Quaternion.Slerp(characterModel.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);
            }
        }
    }


    public void OnResetEpisode()
    {
        transform.localPosition = new Vector3(1.236f, 0, 4.981f);
        velocity = Vector3.zero;
        characterController.enabled = true;
    }

    void UpdateAnimatorParameters()
    {
        bool isIdle = Mathf.Approximately(moveX, 0f) && Mathf.Approximately(moveZ, 0f);
        animator.SetBool("isIdle", isIdle);
        animator.SetBool("isWalkingForward", !isIdle);
        animator.SetFloat("WalkSpeed", moveSpeed);
    }
}
