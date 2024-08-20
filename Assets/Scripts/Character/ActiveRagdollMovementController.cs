using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveRagdollMovementController : MonoBehaviour
{
    public Rigidbody head;
    public Rigidbody hips;

    public float targetHeight = 1.5f;
    public float positionCorrectionStrength = 5f; 
    public float balanceStrength = 10f;

    private Vector3 initialHeadOffset;
    private float groundLevel;

    void Start()
    {
        initialHeadOffset = head.position - hips.position;
        groundLevel = hips.position.y - targetHeight;
    }

    void FixedUpdate()
    {
        MaintainHipHeight();
        AlignHeadAboveHips();
    }

    void MaintainHipHeight()
    {
        float currentHeight = hips.position.y;
        float heightDifference = targetHeight - (currentHeight - groundLevel);
        hips.velocity += Vector3.up * heightDifference * positionCorrectionStrength;
    }

    void AlignHeadAboveHips()
    {
        Vector3 desiredHeadPosition = hips.position + initialHeadOffset;
        Vector3 positionError = desiredHeadPosition - head.position;


        head.velocity += positionError * positionCorrectionStrength;

        Quaternion headRotationError = Quaternion.Inverse(head.rotation) * Quaternion.LookRotation(Vector3.forward, Vector3.up);
        Vector3 headTorque = new Vector3(headRotationError.x, headRotationError.y, headRotationError.z) * balanceStrength;

        head.AddTorque(headTorque, ForceMode.VelocityChange);
    }
}