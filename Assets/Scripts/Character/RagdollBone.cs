
using UnityEngine;

[System.Serializable]
public class RagdollBone
{
    public string name;
    public Rigidbody rigidbody;
    public ConfigurableJoint configurableJoint;

    [HideInInspector] public Vector3 initLocalPosition;
    [HideInInspector] public Quaternion initLocalRotation;

    [Header("Slerp Drive Settings")]
    public float positionSpring = 500f;
    public float positionDamper = 0;

    public float slerpDriveMaximumForce = Mathf.Infinity;

    public void SetUp()
    {
        if (configurableJoint)
        {
            initLocalPosition = configurableJoint.transform.localPosition;
            initLocalRotation = configurableJoint.targetRotation;

            JointDrive slerpDrive = new JointDrive();
            slerpDrive.positionSpring = positionSpring;
            slerpDrive.positionDamper = positionDamper;
            slerpDrive.maximumForce = slerpDriveMaximumForce;

            configurableJoint.angularXDrive = slerpDrive;
            configurableJoint.angularYZDrive = slerpDrive;
            configurableJoint.rotationDriveMode = RotationDriveMode.XYAndZ;
        }

    }

    public void Deactive()
    {
        if (configurableJoint)
        {
            JointDrive slerpDrive = new JointDrive();
            slerpDrive.positionSpring = 25;
            slerpDrive.positionDamper = 0;
            slerpDrive.maximumForce = slerpDriveMaximumForce;

            configurableJoint.angularXDrive = slerpDrive;
            configurableJoint.angularYZDrive = slerpDrive;
            configurableJoint.rotationDriveMode = RotationDriveMode.XYAndZ;
        }
    }
}
