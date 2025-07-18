using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;
[RequireComponent(typeof(PhysicsSystemPlayerController))]
public class PhysicsSystemPlayerInputs : MonoBehaviour
{
    public Vector3 velocity;

    [Header("Jumping")]
    [SerializeField] float jumpHeight;
    [SerializeField] float timeToJumpApex;
    [SerializeField] float jumpVelocity;
    
    [Header("Movement")]
    [SerializeField] float gravity;
    [SerializeField] float MoveSpeed;
    float velocityXsmoothing;

    [SerializeField] float accelerationTimeAir;
    [SerializeField] float accelerationTimeGround;

    [Header("References")]
    public PhysicsSystemPlayerController controlScript;

    // Start is called before the first frame update
    void Start()
    {
        controlScript = GetComponent<PhysicsSystemPlayerController>();

        gravity = -(2 * jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        print("Gravity:" + gravity + "Jump Velocity:" + jumpVelocity);
    }

    // Update is called once per frame
    void Update()
    {
        if (controlScript.collisionInfo.above || controlScript.collisionInfo.below)
        {
            velocity.y = 0;
        }

        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (Input.GetKeyDown(KeyCode.Space) && controlScript.collisionInfo.below)
        {
            velocity.y = jumpVelocity;
        }


        float targetVelocityX = input.x * MoveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXsmoothing, (controlScript.collisionInfo.below)?accelerationTimeGround:accelerationTimeAir);
        velocity.y += gravity * Time.deltaTime;

        controlScript.Move(velocity * Time.deltaTime);
    }
}
