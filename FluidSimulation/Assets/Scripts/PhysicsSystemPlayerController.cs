using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsSystemPlayerController : MonoBehaviour
{
    [Header("Motion Variables")]
    [SerializeField] Vector2 velocity;
    [SerializeField] Vector2 acceleration;

    [Header("Force Variables")]
    [SerializeField] float mass;
    [SerializeField] Vector2 totalForce;
    [SerializeField] float gravity;

    [Header("Checking Variables")]
    [SerializeField] float checkRadius;
    public LayerMask WhatIsGround;
    [SerializeField] Transform feetPos;
    [SerializeField] bool Grounded;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        //ground check
        Grounded = Physics2D.OverlapCircle(feetPos.position, checkRadius, WhatIsGround);

        //movement
        ApplyForce(Vector2.down * gravity * mass);

        acceleration = totalForce / mass;

        velocity += acceleration * Time.deltaTime;

        transform.position += (Vector3)(velocity * Time.deltaTime);

        Vector2 newPos = (Vector2)transform.position + velocity * Time.deltaTime;

        RaycastHit2D hit = Physics2D.Linecast(transform.position, newPos);
        if (hit.collider != null)
        {
            print("hit something");
            velocity = Vector2.zero;
            newPos = hit.point;
        }

        transform.position = newPos;

        totalForce = Vector2.zero;
    }

    public void ApplyForce(Vector2 force)
    {
        totalForce += force;
    }
}
