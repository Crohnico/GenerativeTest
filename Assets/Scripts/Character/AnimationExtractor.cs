using System.Collections.Generic;
using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class AnimationExtractor : MonoBehaviour
{
    public AnimationClip clip;
    public Transform rootBone;

    public List<BodyPartLayers> animationLayers = new List<BodyPartLayers>();
    public List<BoneAnimationData> boneAnimationDataList = new List<BoneAnimationData>();

    public GameObject rHand;
    public GameObject lHand;

    public GameObject rLowerArm;
    public GameObject lLowerArm;

    public GameObject rUpperArm;
    public GameObject lUpperArm;


    public GameObject head;
    public GameObject lfoot;
    public GameObject rfoot;
    public GameObject torso;

    public Dictionary<string, GameObject> map = new Dictionary<string, GameObject>();

    private void Awake()
    {
        map.Add("Hand_L", lHand);
        map.Add("Elbow_L", lLowerArm);
        map.Add("Shoulder_L", lUpperArm);
        map.Add("Hand_R", rHand);
        map.Add("Elbow_R", rLowerArm);
        map.Add("Shoulder_R", rUpperArm);
        map.Add("Head", head);
        map.Add("Ankle_R", rfoot);
        map.Add("Ankle_L", lfoot);
        map.Add("Spine_03", torso);
    }

    public void Init()
    {
        if (clip == null || rootBone == null)
        {
            Debug.LogError("Clip or rootBone is not assigned.");
            return;
        }
        boneAnimationDataList.Clear();


        StartCoroutine(ExtractAnimationDataCoroutine(clip, rootBone));
    }

    private List<string> EnumToTransform(BodyPartLayers part)
    {
        switch (part) 
        {
            case BodyPartLayers.Head:
                return new List<string>() { "Head" };
            case BodyPartLayers.LeftArm:
                return new List<string>() { "Hand_L", "Elbow_L", "Shoulder_L" };
            case BodyPartLayers.RightArm:
                return new List<string>() { "Hand_R", "Elbow_R", "Shoulder_R" };
            case BodyPartLayers.LeftLeg:
                return new List<string>() { "Ankle_L" };
            case BodyPartLayers.RightLeg:
                return new List<string>() { "Ankle_R" };
            case BodyPartLayers.Torso:
                return new List<string>() { "Spine_03"};
        }

        return new List<string>() {};
    }

    private IEnumerator ExtractAnimationDataCoroutine(AnimationClip clip, Transform rootBone)
    {
        Transform[] allBones = rootBone.GetComponentsInChildren<Transform>();
        List<string> validBones = new List<string>();

        foreach (BodyPartLayers layer in animationLayers)
        {
            validBones.AddRange(EnumToTransform(layer));
        }

        boneAnimationDataList.Clear();
        foreach (Transform bone in allBones)
        {
            if (validBones.Contains(bone.name))
            {
                BoneAnimationData boneData = new BoneAnimationData(bone);
                boneData.initialLocalRotation = bone.localRotation;
                boneAnimationDataList.Add(boneData);
            }
        }

        float frameRate = clip.frameRate;
        float clipLength = clip.length;
        float timeStep = 1.0f / frameRate;
        float currentTime = 0f;

        while (currentTime <= clipLength)
        {
            clip.SampleAnimation(gameObject, currentTime);
            foreach (BoneAnimationData boneData in boneAnimationDataList)
            {
                Transform bone = boneData.boneTransform;
                GameObject reference = map[bone.name];
                reference.transform.position = bone.position;
                reference.transform.rotation = bone.rotation;
                boneData.position.Add(reference.transform.localPosition);
                boneData.rotations.Add(reference.transform.localRotation);
            }

            currentTime += timeStep;

            yield return new WaitForEndOfFrame();
        }

        clip.SampleAnimation(gameObject, clipLength);
        foreach (BoneAnimationData boneData in boneAnimationDataList)
        {
            Transform bone = boneData.boneTransform;
            GameObject reference = map[bone.name];
            reference.transform.position = bone.position;
            reference.transform.rotation = bone.rotation;
            boneData.position.Add(reference.transform.localPosition);
            boneData.rotations.Add(reference.transform.localRotation);
        }

        // TODO: Save in scriptable.
        Debug.Log("Animation data extracted for " + boneAnimationDataList.Count + " bones.");
    }
}

[System.Serializable]
public class BoneAnimationData
{
    public Transform boneTransform;
    public Quaternion initialLocalRotation;
    public List<Quaternion> rotations = new List<Quaternion>();
    public List<Vector3> position = new List<Vector3>();

    public BoneAnimationData(Transform boneTransform)
    {
        this.boneTransform = boneTransform;
    }
}

public enum BodyPartLayers 
{
    Head,
    RightArm,
    LeftArm,
    Torso,
    RightLeg,
    LeftLeg
}


#if UNITY_EDITOR

[CustomEditor(typeof(AnimationExtractor))]
public class AnimationExtractorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        AnimationExtractor core = (AnimationExtractor)target;

        if (GUILayout.Button("Extract"))
        {
            core.Init();
        }
    }
}
#endif
