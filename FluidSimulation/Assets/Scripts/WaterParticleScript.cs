using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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
    [SerializeField] float maxClimbAngle;
    [SerializeField] float maxDescendAngle;
    public float currentGroundAngle;
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

    public void UpdateParticlePosition(Vector2 vel)
    {
        UpdateRaycastOrigins();
        collisionInfo.Reset();

        Vector2 tempRayOrigin = raycastOrigins.centre;
        RaycastHit2D hit = Physics2D.Raycast(tempRayOrigin, -Vector2.up, collisionDistance + skinWidth, wallLayer);
        currentGroundAngle = Vector2.Angle(hit.normal, Vector2.up);
        print("ground angle: " + currentGroundAngle);

        if (currentGroundAngle != 0 && vel.y == 0)
        {
            DescendSlope(ref vel);
        }
        if (vel.x != 0f)
        {
            HorizontalCollision();
           
        }

        if (vel.y != 0f ) 
        {
            VerticalCollision();
         
        }

        //vel.x = Mathf.Clamp(vel.x, -clampVelocityX, clampVelocityX);
        //vel.y = Mathf.Clamp(vel.y, -clampVelocityY, clampVelocityY);

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
        raycastOrigins.centre = new Vector2((bounds.min.x + bounds.max.x) / 2, (bounds.min.y + bounds.max.y) / 2);

    }

    void HorizontalCollision()
    {   
        //raycast values :3
        float directionX = Mathf.Sign(velocity.x);
        float rayLength = Mathf.Abs(velocity.x) + skinWidth;
      
        if (velocity.y  == 0 && velocity.x != 0) //moving but encounters a slope
        {
            //slope detection
            Vector2 rayOrigin = raycastOrigins.bottom;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, wallLayer);

            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

            if (hit)
            {
                if (slopeAngle < maxClimbAngle)
                {
                    ClimbSlope(ref velocity, slopeAngle);
                    print("climbing slope");
                }

            }

        }
        else //every other case
        {
            //wall detection
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.left : raycastOrigins.right;

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
            collisionInfo.below = directionY == -1;
            collisionInfo.above = directionY == 1;

            if (currentGroundAngle == 0)
            {
                //print("Hit distance:" + hit.distance);

                if (hit.distance < skinWidth)
                {
                     velocity.y *= -coefRestitutionY;
                     print("normal bounce");

                }
                //print("VERTICAL COLLISION");
            }
            else
            { 
                if (hit.distance < skinWidth && currentGroundAngle != 0)
                {
                    Vector2 normalDirection = hit.normal;
                    Vector2 tangentDirection = (Vector3.Cross(normalDirection, new Vector3(0, 0, 1))).normalized; // fine

                    float magnitude = Vector2.SqrMagnitude(velocity);
                        
                    float angleToTangent = Vector2.Angle(velocity, tangentDirection);
                    float reboundAngle = Mathf.Atan(coefRestitutionY * Mathf.Tan(angleToTangent * Mathf.Deg2Rad)); // correct
                                                                                                                       
                    print("slope bounce");
                    float angleForCartesian = (Mathf.PI / 2) - reboundAngle; // correct
                    print(angleForCartesian * Mathf.Rad2Deg);

                    float magnitudeForCartesian = magnitude * Mathf.Pow(coefRestitutionY, 2);
                    print(magnitudeForCartesian);

                    float lengthX = magnitudeForCartesian * Mathf.Cos(angleForCartesian);
                    float lengthY = magnitudeForCartesian * Mathf.Sin(angleForCartesian);

                    float clampedlengthY = Mathf.Clamp(lengthY, 0, 10); 
          
                    Vector2 tempVector = new Vector2(lengthX, clampedlengthY);

                    print(Vector2.SqrMagnitude(velocity));

                    velocity = tempVector;
  
                }
            }

        }
    }

    void ClimbSlope(ref Vector2 velocity, float slopeAngle)
    {
        float moveDistance = Mathf.Abs(velocity.x);

        float climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

        if (velocity.y <= climbVelocityY)
        {
            velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
            velocity.y = climbVelocityY;
            collisionInfo.below = true;
            collisionInfo.climbingSlope = true;
            collisionInfo.slopeAngle = slopeAngle;
        }
    }

    void DescendSlope(ref Vector2 velocity)
    {
        float directionX = Mathf.Sign(velocity.x);

        Vector2 rayOrigin = raycastOrigins.bottom;

        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, wallLayer);

        if (hit)
        {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if (slopeAngle != 0 && slopeAngle <= maxDescendAngle)
            {
                if (Mathf.Sign(hit.normal.x) == directionX)
                {
                    if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x))
                    {
                        float moveDistance = Mathf.Abs(velocity.x);
                        float descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
                        velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);

                        velocity.y -= descendVelocityY;

                        collisionInfo.slopeAngle = slopeAngle;
                        collisionInfo.descendingSlope = true;
                        collisionInfo.below = true;
                    }
                }
            }
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
                    nearbyParticles += 1; //pressure counter
                }
                else if (Vector2.Distance(PM.particleArray[i].transform.position, transform.position) < viscosityRegion)
                {
                    if (Vector3.Magnitude(velocity) > Vector3.Magnitude(PM.particleArray[i].velocity))
                    {
                        viscosityFactorVariable = 1 / viscosityFactorFixed;
                        PM.particleArray[i].viscosityFactorVariable = viscosityFactorFixed; //viscosity
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
        public Vector2 top, bottom, left, right, centre;
    }

    public struct CollisionInfo
    {
        public bool above, below, left, right;
        public bool climbingSlope, descendingSlope;
        public float slopeAngle;
        public void Reset()
        {
            above = below = false;
            left = right = false;
            climbingSlope = descendingSlope = false;
            slopeAngle = 0;
        }
    }
}
