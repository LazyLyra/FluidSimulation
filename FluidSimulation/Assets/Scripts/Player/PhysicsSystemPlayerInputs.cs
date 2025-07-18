using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;
[RequireComponent(typeof(PhysicsSystemPlayerController))]
public class PhysicsSystemPlayerInputs : MonoBehaviour
{
    public Vector3 velocity;
    [SerializeField] float MoveSpeed;

    [SerializeField] float gravity;
    
    public PhysicsSystemPlayerController controlScript;

    // Start is called before the first frame update
    void Start()
    {
        controlScript = GetComponent<PhysicsSystemPlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (controlScript.collisionInfo.above || controlScript.collisionInfo.below)
        {
            velocity.y = 0;
        }

        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        velocity.x = input.x * MoveSpeed;
        
        velocity.y += gravity * Time.deltaTime;

        controlScript.Move(velocity * Time.deltaTime);
    }
}
