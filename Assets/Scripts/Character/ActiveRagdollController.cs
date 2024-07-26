using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class ActiveRagdollController : MonoBehaviour
{
    public RagdollBone[] ragdollBones;
    public float forceFactor = 1;
    public float rotateSpeed = 1;
    public float dampingFactor = 1;
    public Transform animatedRoot;
    public Transform ragdollRoot;

    public float minDistance = .1f;

    [Range(0,1)]
    public float weight = .5f;

    void Start()
    {
       // InitializeRagdollBones();
    }

    void FixedUpdate()
    {
        foreach (RagdollBone bone in ragdollBones)
        {
            Vector3 targetPosition = bone.animatedReference.position;
            Quaternion targetRotation = bone.animatedReference.rotation;

            Vector3 positionDifference = targetPosition - bone.rigidbody.position;
            Vector3 force = positionDifference.normalized * positionDifference.magnitude * forceFactor;
            force -= bone.rigidbody.velocity * dampingFactor;
            bone.rigidbody.AddForce(force, ForceMode.Acceleration);


            Quaternion deltaRotation = targetRotation * Quaternion.Inverse(bone.rigidbody.rotation);
            deltaRotation.ToAngleAxis(out float angle, out Vector3 axis);
            if (angle > 180f) angle -= 360f;
            Vector3 torque = angle * axis * rotateSpeed;
            torque -= bone.rigidbody.angularVelocity * dampingFactor;
            bone.rigidbody.AddTorque(torque, ForceMode.Acceleration);

            bone.configurableJoint.targetRotation = Quaternion.Slerp(bone.configurableJoint.targetRotation, deltaRotation, Time.fixedDeltaTime * rotateSpeed);
        }
    }

    public void InitializeRagdollBones()
    {
        List<RagdollBone> ragdollBoneList = new List<RagdollBone>();
        Rigidbody[] ragdollBoneTransforms = ragdollRoot.GetComponentsInChildren<Rigidbody>();


        Transform[] aniamtedTransChield = animatedRoot.GetComponentsInChildren<Transform>();


        foreach (Rigidbody rigid in ragdollBoneTransforms) 
        {

            RagdollBone bone = new RagdollBone();
            string name = rigid.gameObject.name;
            Transform aBone = null;

            rigid.TryGetComponent<ConfigurableJoint>(out ConfigurableJoint cJoint);

            foreach (Transform animatedBone in aniamtedTransChield) 
            {
                if(animatedBone.gameObject.name == name) 
                {
                    aBone = animatedBone;
                }
            }

            bone.name = name;
            bone.rigidbody = rigid;
            rigid.isKinematic = false;
            bone.configurableJoint = cJoint;
            bone.animatedReference = aBone;
            rigid.GetComponent<Collider>().enabled = true;
            ragdollBoneList.Add(bone);
        }
 

        ragdollBones = ragdollBoneList.ToArray();
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(ActiveRagdollController))]
public class ActiveRagdollControllerEditor : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        ActiveRagdollController core = (ActiveRagdollController)target;

        if (GUILayout.Button("Inizialize"))
        {
            core.InitializeRagdollBones();
        }

    }
}
#endif
