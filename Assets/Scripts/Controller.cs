using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    [Header("- Move")]
    public bool enableMove = true;
    public bool velocityMode = true;
    public float moveVelocity = 5.5f;
    public float stopSpeed = 5;
    public float horizontalSpeed = 1.5f;
    public float turnSmoothness = 5f;

    public GameObject target;
    public Transform player;

    public Vector3 moveDirAcceleration;
    public Rigidbody pelvis;

    private float horizontal => Input.GetAxis("Horizontal");
    private float vertical => Input.GetAxis("Vertical");
    private Vector3 inputs;

    public ConfigurableJoint rootJoint;

    private void FixedUpdate()
    {
        if (!enableMove) return;

        inputs = new Vector3(horizontal, 0, vertical);
        Move();
        // Rotate(); // No rotamos en esta lógica
    }

    private void Move()
    {

        target.transform.position += inputs;

        Vector3 direction = (pelvis.transform.right * horizontal + pelvis.transform.forward * vertical).normalized;

        if (Vector3.Distance(target.transform.position, pelvis.transform.position) > 1)
        {
            target.transform.position = pelvis.transform.position + direction * 1;
        }

        pelvis.velocity += new Vector3(direction.x * horizontalSpeed, 0, direction.z * horizontalSpeed);

        if (pelvis.velocity.magnitude > moveVelocity)
        {
            pelvis.velocity = pelvis.velocity.normalized * moveVelocity;
        }


        if (vertical == 0 && horizontal == 0)
        {
            Vector3 targetPosition = pelvis.transform.position + Vector3.up * 1;
            target.transform.position = Vector3.Lerp(target.transform.position, targetPosition, Time.fixedDeltaTime * stopSpeed);
            pelvis.velocity = Vector3.Lerp(pelvis.velocity, Vector3.zero, Time.fixedDeltaTime * stopSpeed);
        }
    }
}