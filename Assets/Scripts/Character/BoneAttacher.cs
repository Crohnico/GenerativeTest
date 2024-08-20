using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneAttacher : MonoBehaviour
{
    [System.Serializable]
    public class BoneDefiner
    {
        public ConfigurableJoint joint;
        public GameObject reference;
        [HideInInspector] public Quaternion initialRotation;
        [Range(0, 1)]
        public float weight = .5f;

        private Rigidbody rigid;
        public void AdjustPosition(float streng) 
        {
            if(rigid == null) rigid = joint.GetComponent<Rigidbody>();

            Vector3 direction = reference.transform.position - joint.transform.position;
            rigid.AddForce(direction * streng, ForceMode.VelocityChange);
            if (rigid.velocity.magnitude > 2) rigid.velocity  = rigid.velocity.normalized*2;
        }

    }


    [Header("Targets")]
    public GameObject leftHandTarget;
    public GameObject rightHandTarget;
    public GameObject leftFootTarget;
    public GameObject rightFootTarget;

    [Header("Head")]
    public BoneDefiner head;

    [Header("Body")]
    public BoneDefiner spine;
    public BoneDefiner hip;

    [Header("Left Arm")]
    public BoneDefiner leftUpperArm;
    public BoneDefiner leftLowerArm;
    public BoneDefiner leftHand;
    [Range(0, 1)]
    public float leftArmWeight = 0.5f;

    [Header("Right Arm")]
    public BoneDefiner rightUpperArm;
    public BoneDefiner rightLowerArm;
    public BoneDefiner rightHand;
    [Range(0, 1)]
    public float rightArmWeight = 0.5f;

    [Header("Left Leg")]
    public BoneDefiner leftUpperLeg;
    public BoneDefiner leftLowerLeg;
    public BoneDefiner leftFoot;
    [Range(0, 1)]
    public float leftLegWeight = 0.5f;

    [Header("Right Leg")]
    public BoneDefiner rightUpperLeg;
    public BoneDefiner rightLowerLeg;
    public BoneDefiner rightFoot;
    [Range(0, 1)]
    public float rightLegWeight = 0.5f;

    [Header("Weight Values")]
    public float minForce = 0;
    public float maxForce = 6;

    public float followStreng = 1;

    void Start()
    {
        SaveInitialRotations();
    }
    void SaveInitialRotations()
    {
        //HEAD
        //  head.initialRotation = head.joint.targetRotation;

        //BODY
        //   spine.initialRotation = spine.joint.targetRotation;
        //   hip.initialRotation = hip.joint.targetRotation;

        //LEFT ARM
        leftUpperArm.initialRotation = leftUpperArm.joint.targetRotation;
        leftLowerArm.initialRotation = leftLowerArm.joint.targetRotation;
        leftHand.initialRotation = leftHand.joint.targetRotation;

        //RIGHT ARM
        rightUpperArm.initialRotation = rightUpperArm.joint.targetRotation;
        rightLowerArm.initialRotation = rightLowerArm.joint.targetRotation;
        rightHand.initialRotation = rightHand.joint.targetRotation;

        // LEFT LEG
        leftUpperLeg.initialRotation = leftUpperLeg.joint.targetRotation;
        leftLowerLeg.initialRotation = leftLowerLeg.joint.targetRotation;
        leftFoot.initialRotation = leftFoot.joint.targetRotation;

        //RIGHT LEG
        rightUpperLeg.initialRotation = rightUpperLeg.joint.targetRotation;
        rightLowerLeg.initialRotation = rightLowerLeg.joint.targetRotation;
        rightFoot.initialRotation = rightFoot.joint.targetRotation;
    }

    void UpdateWeights()
    {
        //LEFT ARM
        leftUpperArm.weight = leftLowerArm.weight = leftHand.weight = leftArmWeight;

        //RIGHT ARM
        rightUpperArm.weight = rightLowerArm.weight = rightHand.weight = rightArmWeight;

        //LEFT LEG
        leftUpperLeg.weight = leftLowerLeg.weight = leftFoot.weight = leftLegWeight;

        //RIGHT LEG
        rightUpperLeg.weight = rightLowerLeg.weight = rightFoot.weight = rightLegWeight;
    }
    private void FixedUpdate()
    {
        UpdateWeights();
        FixPosition();

        UpdateReferences();
    }

    private void FixPosition()
    {
        Check(leftHand.joint.transform, leftHandTarget.transform);
        Check(rightHand.joint.transform, rightHandTarget.transform);

        void Check(Transform joint, Transform reference)
        {
            if (Vector3.Distance(joint.transform.position, reference.transform.position) > .2f)
            {
                Vector3 direction = reference.transform.position - joint.transform.position;
                direction = direction.normalized;
                reference.transform.position = direction * 0.2f + joint.transform.position;
            }
        }
    }

    private void UpdateReferences()
    {
        //HEAD
        //   head.joint.SetTargetRotationLocal(GetLocalRotation(head), head.initialRotation);

        //BODY
        //   spine.joint.SetTargetRotationLocal(GetLocalRotation(spine), spine.initialRotation);
        //   hip.joint.SetTargetRotationLocal(GetLocalRotation(hip), hip.initialRotation);

        //LEFT ARM
        JointLocomotion(leftUpperArm);
        JointLocomotion(leftLowerArm);
        JointLocomotion(leftHand);

        //RIGHT ARM
        JointLocomotion(rightUpperArm);
        JointLocomotion(rightLowerArm);
        JointLocomotion(rightHand);

        // LEFT LEG
        JointLocomotion(leftUpperLeg);
        JointLocomotion(leftLowerLeg);
        JointLocomotion(leftFoot);

        //RIGHT LEG
        JointLocomotion(rightUpperLeg);
        JointLocomotion(rightLowerLeg);
        JointLocomotion(rightFoot);

    }

    private void JointLocomotion(BoneDefiner bone)
    {
        bone.AdjustPosition(followStreng);
        AditiveRotation(bone);

    }

    private void AditiveRotation(BoneDefiner bone) 
    {
        Quaternion targetRotation = bone.joint.GetTargetRotation(GetLocalRotation(bone), bone.initialRotation);
        targetRotation.Normalize();

        Quaternion currentRotation = bone.joint.targetRotation;
        currentRotation.Normalize();

        Quaternion differentialRotation = targetRotation * Quaternion.Inverse(currentRotation);
        differentialRotation.Normalize();

        bone.joint.targetRotation = currentRotation * differentialRotation;
        bone.joint.targetRotation.Normalize();
    }



    Quaternion GetLocalRotation(BoneDefiner bone)
    {
        ConfigurableJoint toLocal = bone.joint;
        GameObject reference = bone.reference;
        float weight = bone.weight;

        float value = Mathf.Lerp(minForce, maxForce, weight);
        int integerPart = Mathf.FloorToInt(value);
        float decimalPart = value - integerPart;

        Quaternion A = IncreaseRotation(Quaternion.Inverse(toLocal.transform.rotation) * reference.transform.rotation, integerPart);
        Quaternion B = IncreaseRotation(Quaternion.Inverse(toLocal.transform.rotation) * reference.transform.rotation, integerPart + 1);

        bone.AdjustPosition(followStreng);

        return Quaternion.Slerp(A, B, decimalPart);
    }

    Quaternion IncreaseRotation(Quaternion originalRotation, float factor)
    {
        float angle;
        Vector3 axis;
        originalRotation.ToAngleAxis(out angle, out axis);
        angle *= factor;
        return Quaternion.AngleAxis(angle, axis);
    }
}
