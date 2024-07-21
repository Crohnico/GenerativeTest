using RootMotion.FinalIK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GT_CharacterController : MonoBehaviour
{
    public Rigidbody characterRigidbody;
    public float moveForce = 10f;
    public float turnTorque = 5f;
    public FullBodyBipedIK ik;

    public Rigidbody[] rigidbodies;
    public Collider[] colliders;

    void Start()
    {
        ToggleRagdoll(false);
    }

    void FixedUpdate()
    {
        float moveInput = Input.GetAxis("Vertical");
        float turnInput = Input.GetAxis("Horizontal");

        characterRigidbody.AddForce(transform.forward * moveInput * moveForce);
        characterRigidbody.AddTorque(transform.up * turnInput * turnTorque);

        // Aquí puedes usar FinalIK para ajustar la posición de las extremidades si es necesario
        ik.solver.SetIKPositionWeight(moveInput);
        ik.solver.SetIKPosition(transform.position + transform.forward * moveInput);
    }

    void ToggleRagdoll(bool isRagdoll)
    {
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = !isRagdoll;
        }

        foreach (Collider col in colliders)
        {
            col.enabled = isRagdoll;
        }

        GetComponent<Animator>().enabled = !isRagdoll;
    }
}
