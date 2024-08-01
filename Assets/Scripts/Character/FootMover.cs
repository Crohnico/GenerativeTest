
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class FootMover : MonoBehaviour
{
    public ActiveRagdollController controller;

    public ConfigurableJoint RightLegJoint;
    public ConfigurableJoint LeftLegJoint;

    public Transform RightFoot;
    public Transform LeftFoot;

    // Variables para controlar el equilibrio
    public float BalanceThreshold = 0.1f; // Umbral para determinar si se ha perdido el equilibrio
    public float StepHeight = 0.5f; // Altura a la que se levanta la pierna
    public float LiftDuration = 0.5f; // Duración del levantamiento de la pierna
    private float liftTimer = 0f;

    private bool isLiftingRightLeg = false;
    private bool isLiftingLeftLeg = false;

    private Quaternion rightLegInitialRotation;
    private Quaternion leftLegInitialRotation;

    private void Start()
    {
        rightLegInitialRotation = RightLegJoint.targetRotation;
        leftLegInitialRotation = LeftLegJoint.targetRotation;
    }

    private void Update()
    {
        CheckBalance();
        UpdateLegPosition();
    }

    void CheckBalance()
    {

        if (controller.isGrounded)
        {
            float centerOfMassY = controller.COMP.position.y;
            float leftFootY = LeftFoot.position.y;
            float rightFootY = RightFoot.position.y;

            if (Mathf.Abs(centerOfMassY - leftFootY) > BalanceThreshold || Mathf.Abs(centerOfMassY - rightFootY) > BalanceThreshold)
            {
                if (leftFootY < rightFootY)
                {
                    LiftRightLeg();
                }
                else
                {
                    LiftLeftLeg();
                }
            }
            else
            {
                isLiftingRightLeg = false;
                isLiftingLeftLeg = false;
            }
        }
    }

    void LiftRightLeg()
    {
        if (!isLiftingRightLeg)
        {
            isLiftingRightLeg = true;
            liftTimer = 0f;
        }

        liftTimer += Time.deltaTime;
        float liftProgress = liftTimer / LiftDuration;

        RightLegJoint.targetRotation = Quaternion.Slerp(rightLegInitialRotation,
            Quaternion.Euler(rightLegInitialRotation.eulerAngles.x + StepHeight, rightLegInitialRotation.eulerAngles.y, rightLegInitialRotation.eulerAngles.z), liftProgress);

        if (liftProgress >= 1f)
        {
            isLiftingRightLeg = false;
            liftTimer = 0f;
        }
    }

    void LiftLeftLeg()
    {
        if (!isLiftingLeftLeg)
        {
            isLiftingLeftLeg = true;
            liftTimer = 0f;
        }

        liftTimer += Time.deltaTime;
        float liftProgress = liftTimer / LiftDuration;
      
        LeftLegJoint.targetRotation = Quaternion.Slerp(leftLegInitialRotation,
            Quaternion.Euler(leftLegInitialRotation.eulerAngles.x + StepHeight, leftLegInitialRotation.eulerAngles.y, leftLegInitialRotation.eulerAngles.z), liftProgress);


     
        if (liftProgress >= 1f)
        {
            isLiftingLeftLeg = false;
            liftTimer = 0f; 
        }
    }

    void UpdateLegPosition()
    {/*
       
        if (!isLiftingRightLeg)
        {
            RightLegJoint.targetRotation = Quaternion.Slerp(RightLegJoint.targetRotation, rightLegInitialRotation, Time.deltaTime * 5f);
        }

        if (!isLiftingLeftLeg)
        {
            LeftLegJoint.targetRotation = Quaternion.Slerp(LeftLegJoint.targetRotation, leftLegInitialRotation, Time.deltaTime * 5f);
        }*/
    }
}


#if UNITY_EDITOR

[CustomEditor(typeof(FootMover))]
public class FootMoverEditor : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        FootMover core = (FootMover)target;

    }
}
#endif

