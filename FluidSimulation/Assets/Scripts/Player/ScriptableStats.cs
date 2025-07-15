using UnityEngine;

[CreateAssetMenu(menuName = "Tarodev/Scriptable Stats")]
public class ScriptableStats : ScriptableObject
{
    [Header("Movement")]
    public float MaxSpeed = 8f;
    public float Acceleration = 120f;
    public float GroundDeceleration = 60f;
    public float AirDeceleration = 30f;

    [Header("Jumping")]
    public float JumpPower = 16f;
    public float CoyoteTime = 0.1f;
    public float JumpBuffer = 0.1f;
    public float JumpEndEarlyGravityModifier = 3f;

    [Header("Gravity")]
    public float FallAcceleration = 110f;
    public float MaxFallSpeed = 20f;
    public float GroundingForce = -1.5f;

    [Header("Input")]
    public float HorizontalDeadZoneThreshold = 0.05f;
    public float VerticalDeadZoneThreshold = 0.05f;
    public bool SnapInput = true;

    [Header("Layers")]
    public LayerMask PlayerLayer;
    public float GrounderDistance = 0.05f;
}