using RootMotion.FinalIK;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class ActiveRagdollController : MonoBehaviour
{
    public RagdollBone[] ragdollBones;
    public VRIK vrIK;
    public float forceFactor = 1;
    public float rotateSpeed = 1;
    public float dampingFactor = 1;
    public Transform animatedRoot;
    public Transform ragdollRoot;

    public float minDistance = .1f;
    public float reposVelocity;

    public Rigidbody hips;
    public Vector3 hipsOffset;

    [Range(0, 1)]
    public float weight = .5f;

    [Header("Ragdoll Behaviour")]
    public float distanceToDeactivate = .5f;
    public bool isDeactivate = false;
    public float TimeToRactivate = 2f;
    private float timeLimit;
    private float _time;
    public float forwardStep = .25f;
    public float bacwardStep = .1f;
    public float rightStep = .15f;
    private Vector3 previousPosition;

    [Header("Trackers")]
    public List<TrackersActiveRagdoll> trackers = new List<TrackersActiveRagdoll>();

    public float maxDistanceBeforeReposition = 5f; // Nueva variable para la distancia máxima antes de reposicionar

    void Start()
    {
        hipsOffset = hips.position - animatedRoot.transform.position;
        // InitializeRagdollBones();
    }

    private void Update()
    {
        if (!isDeactivate)
        {
            float distance = Vector3.Distance(animatedRoot.transform.position, ragdollRoot.transform.position);

            if (distance > maxDistanceBeforeReposition)
            {
                isDeactivate = true;
                timeLimit = Time.time + TimeToRactivate;
                animatedRoot.transform.position = new Vector3(ragdollRoot.transform.position.x, animatedRoot.transform.position.y, ragdollRoot.transform.position.z);
            }
            else
            {
                animatedRoot.transform.position = Vector3.Slerp(animatedRoot.transform.position, new Vector3(hips.transform.position.x, animatedRoot.transform.position.y, hips.transform.position.z), Time.deltaTime * reposVelocity);
            }

            ragdollRoot.transform.position = animatedRoot.transform.position;
        }
        else
        {
            if (Time.time > timeLimit)
            {
                isDeactivate = false;
            }
        }

        ChageForLocomotion();
    }

    private void ChageForLocomotion() 
    {
        Vector3 currentPosition = trackers[0].tracker.position;
        Vector3 movementDirection = currentPosition - previousPosition;

        if (movementDirection.z < 0)
        {
            UpdateVRIKParameters(bacwardStep);
            vrIK.solver.locomotion.offset = new Vector3(0, 0, -0.2f);
        }
        else if (movementDirection.x != 0) 
        {
            UpdateVRIKParameters(rightStep);
        }
        else if (movementDirection.z > 0)
        {
            UpdateVRIKParameters(forwardStep);
            vrIK.solver.locomotion.offset = new Vector3(0, 0, 0.2f);
        }
        else
        {
            UpdateVRIKParameters(forwardStep);
            vrIK.solver.locomotion.offset = new Vector3(0, 0, 0);
        }

        previousPosition = currentPosition;
    }

    void UpdateVRIKParameters(float threshold)
    {
        if (vrIK != null)
        {
            vrIK.solver.locomotion.stepThreshold = threshold;
        }
    }

    void FixedUpdate()
    {
 
        foreach (RagdollBone bone in ragdollBones)
        {
            if (Vector3.Distance(bone.animatedReference.position, bone.rigidbody.transform.position) > distanceToDeactivate)
            {
                isDeactivate = true;
                timeLimit = Time.time + TimeToRactivate;
                break;
            }


            TrackersActiveRagdoll tracker = trackers.FirstOrDefault(b => b.destiny == bone.rigidbody);

            bool isATracker = tracker != null;

            Vector3 targetPosition =    bone.animatedReference.position;
            Quaternion targetRotation = bone.animatedReference.rotation;

            if (isATracker)
            {
                if (tracker != null)
                {
                    float distance = Vector3.Distance(tracker.tracker.position, tracker.destiny.transform.position);
                    if (distance > .5f)
                    {
                        Vector3 direction = (tracker.destiny.transform.position - tracker.tracker.parent.position).normalized;
                        tracker.tracker.parent.position = Vector3.MoveTowards(tracker.tracker.parent.position, tracker.destiny.transform.position, 10 * Time.deltaTime);
                    }
                }

                targetPosition = tracker.tracker.position;
                targetRotation = tracker.tracker.rotation;
            }

            if (isDeactivate) continue;

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
                if (animatedBone.gameObject.name == name)
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


[System.Serializable]
public class TrackersActiveRagdoll 
{
    public string name;
    public Rigidbody destiny;
    public Transform tracker;
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
