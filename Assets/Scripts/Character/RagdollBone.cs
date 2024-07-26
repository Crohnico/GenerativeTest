
using UnityEngine;

[System.Serializable]
public class RagdollBone
{
    public string name;
    public Transform animatedReference;
    public Rigidbody rigidbody;
    public ConfigurableJoint configurableJoint;
}
