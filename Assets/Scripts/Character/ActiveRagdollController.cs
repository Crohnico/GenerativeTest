using RootMotion.FinalIK;
using System;
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
    public float xoffset = 0;
    public float yzoffset = 0;

    public Rigidbody pelvis;
    public Rigidbody head;
    public Transform hipsReference;
    public Transform headReference;
    public float antigravityStreng = 1;

    public Transform COMP;

    [Header("Ground")]
    public LayerMask ignoreMask;
    public bool isGrounded;
    public float footsSpacing = .25f;

    public float stepDistance = .25f;

    private Vector3 hitPoint;
    private Vector3 leftFootPos;
    private Vector3 rightFootPos;

    private Vector3 CenterOfMassPoint;

    private Dictionary<string, int> boneNameToIndex;
    public AnimationExtractor extractor;
    public float animationSpeed = 1f;
    private float animationTime = 0f;

    public int frameIndex = 0;

    public Dictionary<string, GameObject> map = new Dictionary<string, GameObject>();


    void Start()
    {
        foreach (RagdollBone b in ragdollBones)
        {
            b.SetUp();
        }

        boneNameToIndex = new Dictionary<string, int>();
        for (int i = 0; i < ragdollBones.Length; i++)
        {
            boneNameToIndex[ragdollBones[i].name] = i;
        }
        SetUpReferences();
    }

    void SetUpReferences()
    {
     //   map.Add("Hand_L", Lhand);
     //   map.Add("Elbow_L", LLower);
     //   map.Add("Shoulder_L", LUpper);
     //   map.Add("Hand_R", RHand);
     //   map.Add("Elbow_R", RLower);
     //   map.Add("Shoulder_R", RUpper);
    }

   

    void FixedUpdate()
    {
        isGrounded = CheckGround();
        CenterOfMass();
     
        return;
        pelvis.velocity += Vector3.up * antigravityStreng;
        head.velocity += Vector3.up * antigravityStreng;
    }

    public void InitializeRagdollBones()
    {
        List<RagdollBone> ragdollBoneList = new List<RagdollBone>();
        Rigidbody[] ragdollBoneTransforms = GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody rigid in ragdollBoneTransforms)
        {
            RagdollBone bone = new RagdollBone();
            string name = rigid.gameObject.name;
            bone.name = name;

            rigid.gameObject.TryGetComponent<ConfigurableJoint>(out ConfigurableJoint cJoint);
            bone.rigidbody = rigid;
            bone.configurableJoint = cJoint;

            if (rigid.TryGetComponent(out Collider col)) col.enabled = true;
            rigid.isKinematic = false;
            ragdollBoneList.Add(bone);
        }

        ragdollBones = ragdollBoneList.ToArray();

        foreach (RagdollBone b in ragdollBones)
        {
            b.positionSpring = xoffset;
            b.positionDamper = yzoffset;
            b.SetUp();
        }
    }

    bool CheckGround()
    {
        if (Physics.Raycast(pelvis.position, Vector3.down, out RaycastHit hit, 1, ~ignoreMask))
        {
            hitPoint = hit.point;

            if (Physics.Raycast(pelvis.position - pelvis.transform.forward * footsSpacing, -Vector3.up, out RaycastHit rightHit, 1, ~ignoreMask))
            {
                rightFootPos = rightHit.point;
            }
            if (Physics.Raycast(pelvis.position + pelvis.transform.forward * footsSpacing, -Vector3.up, out RaycastHit leftHit, 1, ~ignoreMask))
            {
                leftFootPos = leftHit.point;
            }

            return true;
        }
        return false;
    }

    public void PlayAnimationShoot()
    {
        StartCoroutine(PlayAnim());
    }

    private IEnumerator PlayAnim()
    {
        frameIndex = 0;


        if (extractor.boneAnimationDataList == null || extractor.boneAnimationDataList.Count == 0 || ragdollBones == null || ragdollBones.Length == 0)
        {
            yield break;
        }

        while (true)
        {
            foreach (BoneAnimationData boneData in extractor.boneAnimationDataList)
            {

                if (frameIndex >= boneData.rotations.Count) break;

                if (boneNameToIndex.TryGetValue(boneData.boneTransform.name, out int index))
                {
                    RagdollBone bone = ragdollBones[index];

                    if (boneData.rotations.Count > 0 && boneData.rotations.Count > 0)
                    {
                        if (bone.configurableJoint != null)
                        {
                          //  GameObject reference = map[bone.configurableJoint.transform.name];
                            //  reference.transform.localPosition = boneData.position[frameIndex];
                            //  reference.transform.localRotation = boneData.rotations[frameIndex];                           
                        }
                    }
                }

            }

            yield return new WaitForSeconds(0.5f);
            frameIndex++;
        }
    }
    void OnDrawGizmos()
    {
        if (isGrounded)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(hitPoint, 0.1f);

            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(rightFootPos, 0.1f);
            Gizmos.DrawSphere(leftFootPos, 0.1f);
        }
    }

    public void EnableCollider()
    {
        foreach (var item in ragdollBones)
        {
            if (item.rigidbody.TryGetComponent(out Collider col))
                col.enabled = true;
        }
    }

    void CenterOfMass()
    {
        Vector3 centersOfmass = Vector3.zero;
        float massSumatory = 0;

        foreach (RagdollBone bone in ragdollBones)
        {
            if (centersOfmass == Vector3.zero) centersOfmass = (bone.rigidbody.mass * bone.rigidbody.transform.position);
            else centersOfmass += (bone.rigidbody.mass * bone.rigidbody.transform.position);

            massSumatory += bone.rigidbody.mass;
        }

        CenterOfMassPoint = centersOfmass / massSumatory;

        COMP.position = CenterOfMassPoint;
    }
}

[System.Serializable]
public class TrackersActiveRagdoll
{
    public Transform tracker;
    public Rigidbody destiny;
}


#if UNITY_EDITOR

[CustomEditor(typeof(ActiveRagdollController))]
public class ActiveRagdollControllerEditor : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        ActiveRagdollController core = (ActiveRagdollController)target;


        if (GUILayout.Button("PlayAnimation"))
        {
            core.PlayAnimationShoot();
        }

        if (GUILayout.Button("Inizialize"))
        {
            core.InitializeRagdollBones();
        }

        if (GUILayout.Button("Active Colliders"))
        {
            core.EnableCollider();
        }

    }
}
#endif
