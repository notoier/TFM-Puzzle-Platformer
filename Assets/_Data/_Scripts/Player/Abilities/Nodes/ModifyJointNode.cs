using System;
using UnityEngine;

[Serializable]
public class ModifyJointNode : ActionNode
{
    [SerializeField] private ModifyJointOperation operation;
    [SerializeField] private Joint2DKind jointType;
    [SerializeField, Min(0)] private int jointIndex;

    [SerializeField] private JointObjectSource ownerSource;
    [SerializeField] private string ownerKey;

    [SerializeField] private JointConnectionSource connectionSource;
    [SerializeField] private string connectedBodyKey;
    [SerializeField] private bool useTargetAsConnectedAnchor = true;

    [SerializeField] private bool applySettings = true;

    [Header("Common Joint Settings")]
    [SerializeField] private bool autoConfigureConnectedAnchor = true;
    [SerializeField] private Vector2 anchor;
    [SerializeField] private Vector2 connectedAnchor;
    [SerializeField] private bool enableCollision;
    [SerializeField] private float breakForce = Mathf.Infinity;
    [SerializeField] private float breakTorque = Mathf.Infinity;

    [Header("Distance Joint Settings")]
    [SerializeField] private bool autoConfigureDistance = true;
    [SerializeField, Min(0f)] private float distance = 1f;
    [SerializeField] private bool maxDistanceOnly;

    [Header("Hinge Joint Settings")]
    [SerializeField] private bool useHingeMotor;
    [SerializeField] private JointMotor2D hingeMotor;
    [SerializeField] private bool useHingeLimits;
    [SerializeField] private JointAngleLimits2D hingeLimits;

    [Header("Spring/Fixed Joint Settings")]
    [SerializeField, Range(0f, 1f)] private float dampingRatio = 0.5f;
    [SerializeField, Min(0f)] private float frequency = 5f;

    [Header("Friction Joint Settings")]
    [SerializeField, Min(0f)] private float frictionMaxForce = 10f;
    [SerializeField, Min(0f)] private float frictionMaxTorque = 10f;

    [Header("Relative Joint Settings")]
    [SerializeField, Min(0f)] private float relativeMaxForce = 10f;
    [SerializeField, Min(0f)] private float relativeMaxTorque = 10f;
    [SerializeField, Min(0f)] private float correctionScale = 0.3f;
    [SerializeField] private bool autoConfigureOffset = true;
    [SerializeField] private Vector2 linearOffset;
    [SerializeField] private float angularOffset;

    [Header("Slider Joint Settings")]
    [SerializeField] private bool autoConfigureAngle = true;
    [SerializeField] private float angle;
    [SerializeField] private bool useSliderMotor;
    [SerializeField] private JointMotor2D sliderMotor;
    [SerializeField] private bool useSliderLimits;
    [SerializeField] private JointTranslationLimits2D sliderLimits;

    [Header("Wheel Joint Settings")]
    [SerializeField] private bool useWheelMotor;
    [SerializeField] private JointMotor2D wheelMotor;
    [SerializeField] private JointSuspension2D suspension;

    public override void Execute(AbilityContext context)
    {
        if (!TryGetOwner(context, out GameObject owner)
            || !TryGetJoint(owner, out Joint2D joint))
        {
            Fail(context);
            return;
        }

        switch (operation)
        {
            case ModifyJointOperation.Enable:
                ConfigureAndEnable(joint);
                break;

            case ModifyJointOperation.Disable:
                joint.enabled = false;
                break;

            case ModifyJointOperation.Connect:
                if (!TryConnect(joint, context))
                {
                    Fail(context);
                    return;
                }
                break;

            case ModifyJointOperation.Disconnect:
                joint.enabled = false;
                joint.connectedBody = null;
                break;

            case ModifyJointOperation.Toggle:
                if (joint.enabled)
                {
                    joint.enabled = false;
                    joint.connectedBody = null;
                    Cancel(context);
                    return;
                }

                if (!TryConnect(joint, context))
                {
                    Fail(context);
                    return;
                }
                break;
        }

        Complete(context);
    }

    public override AbilityValidationResult Validate()
    {
        if (jointIndex < 0)
            return AbilityValidationResult.Invalid("Joint index cannot be negative.");

        if (ownerSource == JointObjectSource.ContextGameObject
            && string.IsNullOrWhiteSpace(ownerKey))
        {
            return AbilityValidationResult.Incomplete(
                "Modify Joint node needs an owner context key.");
        }

        if ((operation == ModifyJointOperation.Connect
             || operation == ModifyJointOperation.Toggle)
            && connectionSource == JointConnectionSource.ContextGameObject
            && string.IsNullOrWhiteSpace(connectedBodyKey))
        {
            return AbilityValidationResult.Incomplete(
                "Modify Joint node needs a connected body context key.");
        }

        if (!applySettings)
            return AbilityValidationResult.Complete();

        if (breakForce < 0f || breakTorque < 0f)
            return AbilityValidationResult.Invalid(
                "Joint break force and torque cannot be negative.");

        if (distance < 0f || frequency < 0f)
            return AbilityValidationResult.Invalid(
                "Joint distance and frequency cannot be negative.");

        return AbilityValidationResult.Complete();
    }

    private void ConfigureAndEnable(Joint2D joint)
    {
        if (applySettings)
            ApplySettings(joint);

        joint.enabled = true;
    }

    private bool TryConnect(Joint2D joint, AbilityContext context)
    {
        if (!TryGetConnectedBody(
                context,
                out Rigidbody2D connectedBody,
                out Transform connectedTarget))
        {
            return false;
        }

        ConfigureAndEnable(joint);
        joint.connectedBody = connectedBody;
        ApplyConnectedTargetAnchor(
            joint,
            connectedBody,
            connectedTarget);
        return true;
    }

    private void ApplySettings(Joint2D joint)
    {
        if (joint is AnchoredJoint2D anchoredJoint)
        {
            anchoredJoint.autoConfigureConnectedAnchor = autoConfigureConnectedAnchor;
            anchoredJoint.anchor = anchor;
            if (!autoConfigureConnectedAnchor)
                anchoredJoint.connectedAnchor = connectedAnchor;
        }

        joint.enableCollision = enableCollision;
        joint.breakForce = breakForce;
        joint.breakTorque = breakTorque;

        switch (joint)
        {
            case DistanceJoint2D distanceJoint:
                distanceJoint.autoConfigureDistance = autoConfigureDistance;
                if (!autoConfigureDistance)
                    distanceJoint.distance = distance;
                distanceJoint.maxDistanceOnly = maxDistanceOnly;
                break;

            case HingeJoint2D hingeJoint:
                hingeJoint.useMotor = useHingeMotor;
                hingeJoint.motor = hingeMotor;
                hingeJoint.useLimits = useHingeLimits;
                hingeJoint.limits = hingeLimits;
                break;

            case SpringJoint2D springJoint:
                springJoint.autoConfigureDistance = autoConfigureDistance;
                if (!autoConfigureDistance)
                    springJoint.distance = distance;
                springJoint.dampingRatio = dampingRatio;
                springJoint.frequency = frequency;
                break;

            case FixedJoint2D fixedJoint:
                fixedJoint.dampingRatio = dampingRatio;
                fixedJoint.frequency = frequency;
                break;

            case FrictionJoint2D frictionJoint:
                frictionJoint.maxForce = frictionMaxForce;
                frictionJoint.maxTorque = frictionMaxTorque;
                break;

            case RelativeJoint2D relativeJoint:
                relativeJoint.maxForce = relativeMaxForce;
                relativeJoint.maxTorque = relativeMaxTorque;
                relativeJoint.correctionScale = correctionScale;
                relativeJoint.autoConfigureOffset = autoConfigureOffset;
                if (!autoConfigureOffset)
                {
                    relativeJoint.linearOffset = linearOffset;
                    relativeJoint.angularOffset = angularOffset;
                }
                break;

            case SliderJoint2D sliderJoint:
                sliderJoint.autoConfigureAngle = autoConfigureAngle;
                if (!autoConfigureAngle)
                    sliderJoint.angle = angle;
                sliderJoint.useMotor = useSliderMotor;
                sliderJoint.motor = sliderMotor;
                sliderJoint.useLimits = useSliderLimits;
                sliderJoint.limits = sliderLimits;
                break;

            case WheelJoint2D wheelJoint:
                wheelJoint.useMotor = useWheelMotor;
                wheelJoint.motor = wheelMotor;
                wheelJoint.suspension = suspension;
                break;
        }
    }

    private bool TryGetOwner(AbilityContext context, out GameObject owner)
    {
        owner = null;

        if (ownerSource == JointObjectSource.Self)
        {
            owner = context?.actor;
            return owner != null;
        }

        return context != null
               && context.TryGetGameObject(ownerKey, out owner)
               && owner != null;
    }

    private bool TryGetConnectedBody(
        AbilityContext context,
        out Rigidbody2D connectedBody,
        out Transform connectedTarget)
    {
        connectedBody = null;
        connectedTarget = null;

        if (connectionSource == JointConnectionSource.World)
            return true;

        GameObject bodyObject;
        if (connectionSource == JointConnectionSource.Self)
        {
            bodyObject = context?.actor;
        }
        else if (context == null
                 || !context.TryGetGameObject(connectedBodyKey, out bodyObject))
        {
            return false;
        }

        if (bodyObject == null)
            return false;

        connectedTarget = bodyObject.transform;
        connectedBody = bodyObject.GetComponentInParent<Rigidbody2D>();
        return connectedBody != null;
    }

    private void ApplyConnectedTargetAnchor(
        Joint2D joint,
        Rigidbody2D connectedBody,
        Transform connectedTarget)
    {
        if (!useTargetAsConnectedAnchor
            || connectedBody == null
            || connectedTarget == null
            || joint is not AnchoredJoint2D anchoredJoint)
        {
            return;
        }

        anchoredJoint.autoConfigureConnectedAnchor = false;
        anchoredJoint.connectedAnchor =
            connectedBody.transform.InverseTransformPoint(
                connectedTarget.position);
    }

    private bool TryGetJoint(GameObject owner, out Joint2D selectedJoint)
    {
        selectedJoint = null;
        Joint2D[] joints = owner.GetComponents<Joint2D>();
        int matchingIndex = 0;

        foreach (Joint2D joint in joints)
        {
            if (!MatchesSelectedType(joint))
                continue;

            if (matchingIndex == jointIndex)
            {
                selectedJoint = joint;
                return true;
            }

            matchingIndex++;
        }

        return false;
    }

    private bool MatchesSelectedType(Joint2D joint)
    {
        return jointType switch
        {
            Joint2DKind.Distance => joint is DistanceJoint2D,
            Joint2DKind.Hinge => joint is HingeJoint2D,
            Joint2DKind.Spring => joint is SpringJoint2D,
            Joint2DKind.Fixed => joint is FixedJoint2D,
            Joint2DKind.Friction => joint is FrictionJoint2D,
            Joint2DKind.Relative => joint is RelativeJoint2D,
            Joint2DKind.Slider => joint is SliderJoint2D,
            Joint2DKind.Wheel => joint is WheelJoint2D,
            _ => false
        };
    }
}

public enum ModifyJointOperation
{
    Enable,
    Disable,
    Connect,
    Disconnect,
    Toggle
}

public enum Joint2DKind
{
    Distance,
    Hinge,
    Spring,
    Fixed,
    Friction,
    Relative,
    Slider,
    Wheel
}

public enum JointObjectSource
{
    Self,
    ContextGameObject
}

public enum JointConnectionSource
{
    World,
    Self,
    ContextGameObject
}
