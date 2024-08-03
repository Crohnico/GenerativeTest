using UnityEngine;

public static class ConfigurableJointExtensions
{
	/// <summary>
	/// Sets a joint's targetRotation to match a given local rotation.
	/// The joint transform's local rotation must be cached on Start and passed into this method.
	/// </summary>
	public static void SetTargetRotationLocal (this ConfigurableJoint joint, Quaternion targetLocalRotation, Quaternion startLocalRotation)
	{
		if (joint.configuredInWorldSpace) {
			Debug.LogError ("SetTargetRotationLocal should not be used with joints that are configured in world space. For world space joints, use SetTargetRotation.", joint);
		}
		SetTargetRotationInternal (joint, targetLocalRotation, startLocalRotation, Space.Self);
	}
	
	/// <summary>
	/// Sets a joint's targetRotation to match a given world rotation.
	/// The joint transform's world rotation must be cached on Start and passed into this method.
	/// </summary>
	public static void SetTargetRotation (this ConfigurableJoint joint, Quaternion targetWorldRotation, Quaternion startWorldRotation)
	{
		if (!joint.configuredInWorldSpace) {
			Debug.LogError ("SetTargetRotation must be used with joints that are configured in world space. For local space joints, use SetTargetRotationLocal.", joint);
		}
		SetTargetRotationInternal (joint, targetWorldRotation, startWorldRotation, Space.World);
	}
	
	static void SetTargetRotationInternal (ConfigurableJoint joint, Quaternion targetRotation, Quaternion startRotation, Space space)
	{

		var right = joint.axis;
		var forward = Vector3.Cross (joint.axis, joint.secondaryAxis).normalized;
		var up = Vector3.Cross (forward, right).normalized;
		Quaternion worldToJointSpace = Quaternion.LookRotation (forward, up);
		
		Quaternion resultRotation = Quaternion.Inverse (worldToJointSpace);

		if (space == Space.World) {
			resultRotation *= startRotation * Quaternion.Inverse (targetRotation);
		} else {
			resultRotation *= Quaternion.Inverse (targetRotation) * startRotation;
		}

		resultRotation *= worldToJointSpace;
		
		joint.targetRotation = resultRotation;
	}
	
	/// <summary>
	/// Adjust ConfigurableJoint settings to closely match CharacterJoint behaviour
	/// </summary>
	public static void SetupAsCharacterJoint (this ConfigurableJoint joint)
	{
		joint.xMotion = ConfigurableJointMotion.Locked;
		joint.yMotion = ConfigurableJointMotion.Locked;
		joint.zMotion = ConfigurableJointMotion.Locked;
		joint.angularXMotion = ConfigurableJointMotion.Limited;
		joint.angularYMotion = ConfigurableJointMotion.Limited;
		joint.angularZMotion = ConfigurableJointMotion.Limited;
		joint.breakForce = Mathf.Infinity;
		joint.breakTorque = Mathf.Infinity;
		
		joint.rotationDriveMode = RotationDriveMode.Slerp;
		var slerpDrive = joint.slerpDrive;
		slerpDrive.mode = JointDriveMode.Position;
		slerpDrive.maximumForce = Mathf.Infinity;
		joint.slerpDrive = slerpDrive;
	}
}