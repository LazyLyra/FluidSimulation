using System;
using UnityEngine;
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))] 
public class PhysicsSystemPlayerController : MonoBehaviour, IPlayerController
{
    [SerializeField] private ScriptableStats _stats;
    private Rigidbody2D RB;
    private CapsuleCollider2D CC;
    private FrameInput currentFrameInput;
    private Vector2 currentFrameVelocity;
    private bool _cachedQueryStartInColliders;

    private float time;

    private void Awake()
    {
        RB = GetComponent<Rigidbody2D>();
        CC = GetComponent<CapsuleCollider2D>();

        _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        GatherInput();
    }

    private void FixedUpdate()
    {
        CheckCollisions();

        HandleJump();
        HandleDirection();
        HandleGravity();

        ApplyMovement();
    }

    //jumping stuff
    private bool jumpToConsume;
    private bool bufferedJumpUsable;
    private bool endedJumpEarly;
    private bool coyoteUsable;
    private float timeJumpPress;
    private bool HasBufferedJump => bufferedJumpUsable && time < timeJumpPress + _stats.JumpBuffer;
    private bool CanUseCoyote => coyoteUsable && !Grounded && time < frameLeftGround + _stats.CoyoteTime;

    //collision stuff
    private float frameLeftGround = float.MinValue;
    private bool Grounded;

    

    private void GatherInput()
    {
        currentFrameInput = new FrameInput
        {
            JumpDown = Input.GetButtonDown("Jump"),
            JumpHeld = Input.GetButton("Jump"),
            MoveVector = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"))
        };

        if (_stats.SnapInput)
        {
            currentFrameInput.MoveVector.x = Math.Abs(currentFrameInput.MoveVector.x) < _stats.HorizontalDeadZoneThreshold ? 0 : Mathf.Sign(currentFrameInput.MoveVector.x);
            currentFrameInput.MoveVector.y = Math.Abs(currentFrameInput.MoveVector.y) < _stats.VerticalDeadZoneThreshold ? 0 : Mathf.Sign(currentFrameInput.MoveVector.y);
        }

        if (currentFrameInput.JumpDown)
        {
            jumpToConsume = true;
            timeJumpPress = time;
        }
    }

    private void HandleDirection()
    {
        if (currentFrameInput.MoveVector.x == 0)
        {
            var deceleration = Grounded ? _stats.GroundDeceleration : _stats.AirDeceleration;
            currentFrameVelocity.x = Mathf.MoveTowards(currentFrameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
        }
        else
        {
            currentFrameVelocity.x = Mathf.MoveTowards(currentFrameVelocity.x, currentFrameInput.MoveVector.x * _stats.MaxSpeed, _stats.Acceleration * Time.fixedDeltaTime);
        }
    }

    private void HandleGravity()
    {
        if (Grounded && currentFrameVelocity.y <= 0f)
        {
            currentFrameVelocity.y = _stats.GroundingForce;
        }
        else
        {
            var inAirGravity = _stats.FallAcceleration;
            if (endedJumpEarly && currentFrameVelocity.y > 0)
            {
                inAirGravity *= _stats.JumpEndEarlyGravityModifier;
            }

            currentFrameVelocity.y = Mathf.MoveTowards(currentFrameVelocity.y, -_stats.MaxFallSpeed, inAirGravity * Time.deltaTime);
        }
    }
    private void HandleJump()
    {
        if (!endedJumpEarly && !Grounded && !currentFrameInput.JumpHeld && RB.velocity.y > 0)
        {
            endedJumpEarly = true;
        }

        if (!jumpToConsume && !HasBufferedJump)
        {
            return;
        }

        if (Grounded || CanUseCoyote)
        {
            ExecuteJump();
        }

        jumpToConsume = false;

    }
    private void ExecuteJump()
    {
        endedJumpEarly = false;
        timeJumpPress = 0f;
        bufferedJumpUsable = false;
        coyoteUsable = false;
        currentFrameVelocity.y = _stats.JumpPower;
        Jumped?.Invoke();
    }

    private void CheckCollisions()
    {
        Physics2D.queriesStartInColliders = false;

        //ground and ceiling check
        bool groundHit = Physics2D.CapsuleCast(CC.bounds.center, CC.size, CC.direction, 0, Vector2.down, _stats.GrounderDistance, ~_stats.PlayerLayer);
        bool ceilingHit = Physics2D.CapsuleCast(CC.bounds.center, CC.size, CC.direction, 0, Vector2.up, _stats.GrounderDistance, ~_stats.PlayerLayer);
        
        //hit ceiling
        if (ceilingHit)
        {
            currentFrameVelocity.y = Mathf.Min(0, currentFrameVelocity.y);
        }

        if (!Grounded && groundHit)
        {
            Grounded = true;
            coyoteUsable = true;
            bufferedJumpUsable = true;
            endedJumpEarly = false;
            GroundedChanged?.Invoke(true, Mathf.Abs(currentFrameVelocity.y));
        }

        //leave ground

        else if (Grounded && !groundHit)
        {
            Grounded = false;
            frameLeftGround = time;
            GroundedChanged?.Invoke(false, 0);
        }

        Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
    }

    private void ApplyMovement()
    {
       RB.velocity = currentFrameVelocity;
    }
    //structure and interface
    public struct FrameInput
    {
        public bool JumpDown;
        public bool JumpHeld;
        public Vector2 MoveVector;
    }
    public interface IPlayerController
    {
        public event Action<bool, float> GroundChanged;

        public event Action Jumped;
        public Vector2 FrameInput { get; }
    }
}
