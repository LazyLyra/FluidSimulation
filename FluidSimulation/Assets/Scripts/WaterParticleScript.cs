using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

public class WaterParticleScript : MonoBehaviour
{
    [Header("Physics")]
    public Vector2 velocity;
    [SerializeField] float gravity;
    //wall physics
    [SerializeField] float coefRestitutionX;
    [SerializeField] float coefRestitutionY;
    //particle physics
    [SerializeField] float clampVelocityX;
    [SerializeField] float clampVelocityY;

    public float coefResistutionParticles;
    [SerializeField] float collisionDistance;
    [Header("Pressure")]
    [SerializeField] float pressuredistanceConstant;
    [SerializeField] float pressureparticlesConstant;
    [SerializeField] float pressureIncreaseRegion;

    [Header("Viscosity")]
    [SerializeField] float viscosityRegion;
    [SerializeField] float viscosityFactorFixed;
    [SerializeField] float viscosityFactorVariable;

    [Header("Raycasting")]
    //for wall collisions
    [SerializeField] float skinWidth; 
    public LayerMask wallLayer;

    [Header("References")]
    public CircleCollider2D CC;
    public ParticleManager PM;
    RaycastOrigins raycastOrigins;
    CollisionInfo collisionInfo;

    // Start is called before the first frame update
    void Start()
    {
        CC = GetComponent<CircleCollider2D>();
        PM = GameObject.FindGameObjectWithTag("Manager").GetComponent<ParticleManager>();
    }

    // Update is called once per frame
    void Update()
    {

        velocity.y += gravity * Time.deltaTime;

        UpdateParticlePosition(velocity * Time.deltaTime);

    }

    //handle updating position

    private void UpdateParticlePosition(Vector2 vel)
    {
        UpdateRaycastOrigins();
        collisionInfo.Reset();

        if (vel.x != 0f)
        {
            HorizontalCollision();
        }

        if (vel.y != 0f) 
        {
            VerticalCollision();
        }

        vel.x = Mathf.Clamp(vel.x, -clampVelocityX, clampVelocityX);
        vel.y = Mathf.Clamp(vel.y, -clampVelocityY, clampVelocityY);

        transform.Translate(vel);
    }

    //RAYCASTING FOR WALLS
    void UpdateRaycastOrigins()
    {
        Bounds bounds = CC.bounds;
        bounds.Expand(skinWidth * -2f);

        raycastOrigins.top = new Vector2(transform.position.x, bounds.max.y);
        raycastOrigins.bottom = new Vector2(transform.position.x, bounds.min.y);
        raycastOrigins.left = new Vector2(bounds.min.x, transform.position.y);
        raycastOrigins.right = new Vector2(bounds.max.x, transform.position.y);

    }

    void HorizontalCollision()
    {
        float directionX = Mathf.Sign(velocity.x);
        float rayLength = Mathf.Abs(velocity.x) + skinWidth;

        Vector2 rayOrigin = (directionX == -1)?raycastOrigins.left:raycastOrigins.right;

        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, wallLayer);

        Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

        if (hit)
        {

            collisionInfo.left = directionX == -1;
            collisionInfo.right = directionX == 1;

            //print("Hit distance:" + hit.distance);

            if (hit.distance < skinWidth)
            {
                velocity.x *= -coefRestitutionX;
            }
            //print("VERTICAL COLLISION");
        }
    }

    void VerticalCollision()
    {
        float directionY = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + skinWidth;

        Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottom : raycastOrigins.top;

        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, wallLayer);

        Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);

        if (hit)
        {

            collisionInfo.below= directionY == -1;
            collisionInfo.above = directionY == 1;

            //print("Hit distance:" + hit.distance);

            if (hit.distance < skinWidth)
            {
                velocity.y *= -coefRestitutionY;
            }
            //print("VERTICAL COLLISION");
        }
    }

    //collision with other particles

    public void HandleCollision(WaterParticleScript collidingParticle)
    {
        float distance = Vector2.Distance(collidingParticle.transform.position, transform.position);


        if (distance < collisionDistance && distance > 0)
        {
            print("we collided !");

            int nearbyParticles = 0;

            for (int i = 0; i < PM.spawnNumber; i++)
            {
                if (Vector2.Distance(PM.particleArray[i].transform.position, transform.position) < pressureIncreaseRegion)
                {
                    nearbyParticles += 1;
                }
                else if (Vector2.Distance(PM.particleArray[i].transform.position, transform.position) < viscosityRegion)
                {
                    if (Vector3.Magnitude(velocity) > Vector3.Magnitude(PM.particleArray[i].velocity))
                    {
                        viscosityFactorVariable = 1 / viscosityFactorFixed;
                        PM.particleArray[i].viscosityFactorVariable = viscosityFactorFixed;
                    }
                    else
                    {
                        viscosityFactorVariable = viscosityFactorFixed;
                        PM.particleArray[i].viscosityFactorVariable = 1 / viscosityFactorFixed;
                    }
                }
            }

                Vector2 AB = collidingParticle.transform.position - transform.position;
            // direction * affect from nearby particles * affect from collision distance
            //velocity = velocity + (-1 * AB.normalized * coefResistutionParticles) * (pressureparticlesConstant * nearbyParticles) * (pressuredistanceConstant * (1 / distance)) * viscosityFactorVariable;
            velocity = velocity + (-1 * AB.normalized * coefResistutionParticles);

            // same but for the other particle lol
            //collidingParticle.velocity = collidingParticle.velocity + (AB.normalized * coefResistutionParticles) * (pressureparticlesConstant * nearbyParticles) * (pressuredistanceConstant * (1 / distance)) * viscosityFactorVariable;
            collidingParticle.velocity = collidingParticle.velocity + (AB.normalized * coefResistutionParticles);
        }
    }



    struct RaycastOrigins
    {
        public Vector2 top, bottom, left, right;
    }

    public struct CollisionInfo
    {
        public bool above, below, left, right;

        public void Reset()
        {
            above = below = false;
            left = right = false;
        }
    }
}
