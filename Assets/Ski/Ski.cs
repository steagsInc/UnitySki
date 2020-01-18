using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ski : CustomPhysics3D
{
    public Transform body;
    public CameraFollow cam;
    private Rigidbody rigidbody;
    private Material debug_color;

    public float originalFriction = 0.2f;
    public float jumpForce = 20f;

    #region computational value
    private Vector2 firstFingerPos;

    private Vector3 velocity;
    private Vector3 move;
    private float friction;
    private bool grounded = false;
    private bool turning = false;
    private bool skidding = false;
    private float crouchRate = 0;

    private int forwardDirection = 1;
    private Vector3 lastForward = Vector3.forward;
    private Vector3 lastNormal = Vector3.zero;

    #endregion

    private const float minGroundNormalY = .65f;
    private const float shellRadius = 0.01f;

    private void Awake()
    {
        rigidbody = gameObject.GetComponent<Rigidbody>();
        friction = originalFriction;
        debug_color = transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material;
    }

    void FixedUpdate()
    {
        TouchsInputRelative(mousePosPercent());

        computeMovement();
    }

    private void computeMovement()
    {

        velocity.y += 1 * Physics.gravity.y * Time.deltaTime;

        float linearspeed = new Vector3(velocity.x, 0, velocity.z).magnitude;
        

        if (grounded && turning)
        {
            
            Vector3 currentForward = body.forward;

            float dot = Vector3.Dot(currentForward, lastForward);

            int d = dot > 0 ? 1 : -1;

            Vector3 skiddingVelocity = new Vector3(linearspeed * body.right.x, velocity.y, linearspeed * body.right.z) * Mathf.Abs(1 - Mathf.Abs(dot));

            showVector(skiddingVelocity, Color.magenta);

            velocity = airFriction(velocity + skiddingVelocity);
            showVector(velocity, Color.cyan);

            //skidding = Vector2.Angle(new Vector2(velocity.x,velocity.z), new Vector2(body.forward.x, body.forward.z)) > 20 ? true : false;

            /*
            if (skidding) {
                Vector3 skiddingVelocity = new Vector3(linearspeed * body.right.x, velocity.y, linearspeed * body.right.z) * Mathf.Abs(1 - Mathf.Abs(dot));

                showVector(skiddingVelocity, Color.magenta);

                velocity = airFriction(velocity + skiddingVelocity);
                showVector(velocity, Color.cyan);

            }
            else
            {
                Vector3 forwardVelocity = new Vector3(linearspeed * currentForward.x, velocity.y, linearspeed * currentForward.z) * Mathf.Abs(1 - Mathf.Abs(dot));

                velocity = airFriction(velocity * dot + forwardVelocity);
                showVector(velocity, Color.blue);
            }*/

            lastForward = forward();
            
        }
        else
        {
            jumpForward();
        }

        grounded = false;

        move = new Vector3(velocity.x, 0, velocity.z) * Time.deltaTime;

        collision(rigidbody, move);

        rigidbody.position += move;

        move = new Vector3(0, velocity.y, 0) * Time.deltaTime;

        collision(rigidbody, move);

        rigidbody.position += move;
    }

    private Vector2 mousePosPercent()
    {
        Vector2 fingerPos;

        fingerPos.x = (Mathf.Clamp(Input.mousePosition.x, 0, Screen.width)) / (Screen.width) * 100;
        fingerPos.y = (Mathf.Clamp(Input.mousePosition.y, 0, Screen.height)) / (Screen.height) * 100;

        return fingerPos;
    }

    private void TouchsInput(Vector2 pos)
    {

        if (Input.GetMouseButton(0))
        {

            Vector2 move = pos - new Vector2(50, 35);

            //Horizontal
            float percentH = Mathf.Clamp(Mathf.Abs(move.x) / 45, 0, 1);

            //print(percentH);

            Quaternion target = new Quaternion();
            int f = Vector3.Dot(cam.transform.forward, rigidbody.transform.forward) > 0 ? 1 : -1;

            float angle = grounded ? 15f : 30;

            target.eulerAngles = rigidbody.rotation.eulerAngles + new Vector3(0, angle * percentH * move.normalized.x, 0);

            float smooth = grounded ? 2.5f : 5;

            rigidbody.rotation = Quaternion.Slerp(rigidbody.rotation, target, Time.deltaTime * smooth);

            //crouch

            if(move.normalized.y < 0)
            {
                float percentV = Mathf.Clamp(Mathf.Abs(move.y) / 20, 0, 1);

                crouchRate = percentV;
                debug_color.color = Color.Lerp(Color.red, Color.blue, percentV);
            }
            else if (move.normalized.y > 0)
            {
                float percentV = Mathf.Clamp(Mathf.Abs(move.y) / 50, 0, 1);
                debug_color.color = Color.red;

                velocity.y += jumpForce * crouchRate;
                crouchRate = 0;

            }

        }
        else
        {
            crouchRate = 0;
            debug_color.color = Color.Lerp(Color.red, Color.blue, 0);
        }
    }

    private void TouchsInputRelative(Vector2 pos)
    {

        Quaternion target = DirectionToRotation(new Vector3(velocity.x, velocity.z, 0));

        float smooth = grounded ? 2.5f : 5;

        turning = false;

        if (Input.GetMouseButton(0))
        {
            turning = true;
            Vector2 move = pos - new Vector2(50, 35);

            //Horizontal
            float percentH = Mathf.Clamp(Mathf.Abs(move.x) / 45, 0, 1);

            //print(percentH);

            int f = Vector3.Dot(cam.transform.forward, rigidbody.transform.forward) > 0 ? 1 : -1;

            float angle = grounded ? 90f : 30;

            int d = move.normalized.x < 0 ? -1 : 1;

            target.eulerAngles += new Vector3(0, angle * percentH * move.normalized.x, 0);

            skidding = percentH > 0.5 ? true : false;

            //crouch

            if (move.normalized.y < 0)
            {
                float percentV = Mathf.Clamp(Mathf.Abs(move.y) / 20, 0, 1);

                crouchRate = percentV;
                debug_color.color = Color.Lerp(Color.red, Color.blue, percentV);
            }
            else if (move.normalized.y > 0)
            {
                float percentV = Mathf.Clamp(Mathf.Abs(move.y) / 50, 0, 1);
                debug_color.color = Color.red;

                velocity.y += jumpForce * crouchRate;
                crouchRate = 0;

            }

        }
        else
        {
            crouchRate = 0;
            debug_color.color = Color.Lerp(Color.red, Color.blue, 0);
        }

        rigidbody.rotation = Quaternion.Slerp(rigidbody.rotation, target, Time.deltaTime * smooth);
    }

    private Vector3 forward()
    {
        //change direction of forward vector if the character turn back
        if (Mathf.Floor(velocity.magnitude) == 0 && Vector3.Dot(transform.forward*forwardDirection, lastNormal) < 0) forwardDirection = -forwardDirection;

        return transform.forward * forwardDirection;
    }

    //keep direction of velocity when character fly   
    private void jumpForward()
    {
        if (Vector3.Dot(transform.forward * forwardDirection, velocity) < 0) forwardDirection = -forwardDirection;

    }

    private Vector3 airFriction(Vector3 velocity)
    {
        Vector3 localVelocity = body.InverseTransformVector(velocity);

        localVelocity.x *= (1 - friction);

        return body.TransformVector(localVelocity);

    }

    protected override void onCollision(RaycastHit hit)
    {
        lastNormal = hit.normal;
        //transform.rotation = Quaternion.Slerp(rigidbody.rotation,paralleBody(transform, hit.normal),Time.deltaTime * 5);
        velocity = velocityCorrection(velocity, hit.normal);
        if (move.y != 0)
        {
            grounded = groundCollisionDetection(hit.normal);
            if (grounded)
            {
                showVector(lastNormal, Color.green);
                float x = VectorToAngle(body, new Vector2(lastNormal.z, lastNormal.y));
                float z = VectorToAngle(body, new Vector2(lastNormal.x, lastNormal.y));

                Quaternion target = Quaternion.Euler(x, body.rotation.y, z);

                //body.rotation = Quaternion.Slerp(body.rotation, target, Time.deltaTime * 5.0f);
            }
            //velocity += computeGravityWithCollision(hit.normal);
        }
        move = wallCollision(move, hit.distance);

    }

    public Vector3 getVelocity()
    {
        return velocity;
    }

    public void setCamera(CameraFollow cam) { this.cam = cam; }

    public Vector3 resetEntities(Vector3 decal)
    {
        rigidbody.position += decal;
        cam.transform.position += decal;

        return rigidbody.position;
    }

}
