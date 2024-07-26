using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.Demos {

	/// <summary>
	/// Making a character hold on to a target and swing about it while maintaining his animation.
	/// </summary>
	public class PendulumExample : MonoBehaviour {

		[Tooltip("The master weight of this script.")]
		[Range(0f, 1f)] public float weight = 1f;

		[Tooltip("Multiplier for the distance of the root to the target.")]
		public float hangingDistanceMlp = 1.3f;

		[Tooltip("Where does the root of the character land when weight is blended out?")]
		[HideInInspector] public Vector3 rootTargetPosition;

		[Tooltip("How is the root of the character rotated when weight is blended out?")]
		[HideInInspector] public Quaternion rootTargetRotation;

        public Transform leftHandTarget;
        public Transform rightHandTarget;
        public Transform leftFootTarget;
        public Transform rightFootTarget;
        public Transform pelvisTarget;
        public Transform bodyTarget;
        public Transform headTarget;
        public Vector3 pelvisDownAxis = Vector3.right;

        private FullBodyBipedIK ik;
		private Quaternion rootRelativeToPelvis;
		private Vector3 pelvisToRoot;
		private float lastWeight;

		void Start() {
			ik = GetComponent<FullBodyBipedIK>();


			// Remember the rotation of the root relative to the pelvis
			rootRelativeToPelvis = Quaternion.Inverse(pelvisTarget.rotation) * transform.rotation;

			// Remember the position of the root relative to the pelvis
			pelvisToRoot = Quaternion.Inverse(ik.references.pelvis.rotation) * (transform.position - ik.references.pelvis.position);

			rootTargetPosition = transform.position;
			rootTargetRotation = transform.rotation;

			lastWeight = weight;
		}

		void LateUpdate() {
			// Set effector weights
			if (weight > 0f) {
				ik.solver.leftHandEffector.positionWeight = weight;
				ik.solver.leftHandEffector.rotationWeight = weight;
			} else {
				rootTargetPosition = transform.position;
				rootTargetRotation = transform.rotation;

				if (lastWeight > 0f) {
					ik.solver.leftHandEffector.positionWeight = 0f;
					ik.solver.leftHandEffector.rotationWeight = 0f;
				}
			}

			lastWeight = weight;
			if (weight <= 0f) return;

			transform.position = Vector3.Lerp(rootTargetPosition, pelvisTarget.position + pelvisTarget.rotation * pelvisToRoot * hangingDistanceMlp, weight);
			transform.rotation = Quaternion.Lerp(rootTargetRotation, pelvisTarget.rotation * rootRelativeToPelvis, weight);

	
			ik.solver.leftHandEffector.position = leftHandTarget.position;
			ik.solver.leftHandEffector.rotation = leftHandTarget.rotation;


			Vector3 dir = ik.references.pelvis.rotation * pelvisDownAxis;

			Quaternion rightArmRot = Quaternion.FromToRotation(dir, rightHandTarget.position - headTarget.position);

			ik.references.rightUpperArm.rotation = Quaternion.Lerp(Quaternion.identity, rightArmRot, weight) * ik.references.rightUpperArm.rotation;
			
			Quaternion leftLegRot = Quaternion.FromToRotation(dir, leftFootTarget.position - bodyTarget.position);
			ik.references.leftThigh.rotation = Quaternion.Lerp(Quaternion.identity, leftLegRot, weight) * ik.references.leftThigh.rotation;
			
			Quaternion rightLegRot = Quaternion.FromToRotation(dir, rightFootTarget.position - bodyTarget.position);
			ik.references.rightThigh.rotation = Quaternion.Lerp(Quaternion.identity, rightLegRot, weight) * ik.references.rightThigh.rotation;
		}
	}
}
