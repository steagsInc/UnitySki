using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomPhysics3D : MonoBehaviour
{

    private bool useGravity = true;

    //Constantes
    private const float minMoveDistance = 0.001f;
    private const float shellRadius = 0.02f;
    private const float minGroundNormalY = .65f;
    protected const float G = 6.67384e-11f;

    protected Vector3 velocityClamp(Vector3 velocity, float max)
    {

        velocity.x = Mathf.Clamp(velocity.x, -max, max);
        velocity.y = Mathf.Clamp(velocity.y, -max, max);
        velocity.z = Mathf.Clamp(velocity.z, -max, max);

        return velocity;
    }

    protected Vector3 walkingSpeedtoVelocity(float speed)
    {
        Vector2 move = Vector2.zero;

        move.x = Input.GetAxis("Horizontal");

        return move * speed;
    }

    protected float jump(float jumpForce)
    {

        if (Input.GetButtonDown("Jump"))
        {
            return jumpForce;
        }

        return 0;
    }



    protected Vector2 pointDirection(Vector2 point1, Vector2 point2)
    {
        return new Vector2(point2.x - point1.x, point2.y - point1.y);
    }

    protected Vector2 directionalVelocity(Vector2 direction, float speed)
    {

        return direction * speed;

    }

    public static float VectorToAngle(Transform t, Vector2 direction)
    {
        Quaternion target = Quaternion.Euler(0, 0, 0);

        float rad = t.rotation.eulerAngles.x * Mathf.Rad2Deg;

        Vector3 currentDirection = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0);

        Vector3 aim = new Vector3(direction.x, direction.y, 0).normalized;

        float angle = Mathf.Atan2(aim.x, aim.y) * 180 / Mathf.PI;

        return angle;

    }

    public static void directionalRotationX(Transform t, Vector2 direction, float smooth = 5.0f)
    {
        Quaternion target = Quaternion.Euler(0, 0, 0);

        float rad = t.rotation.eulerAngles.x * Mathf.Rad2Deg;

        Vector3 currentDirection = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0);

        Vector3 aim = new Vector3(direction.x, direction.y, 0).normalized;

        float angle = Mathf.Atan2(aim.x, aim.y) * 180 / Mathf.PI;

        target = Quaternion.Euler(angle, 0, 0);

        t.rotation = Quaternion.Slerp(t.rotation, target, Time.deltaTime * smooth);

        if (Mathf.Floor(t.rotation.eulerAngles.x) == target.eulerAngles.x)
        {
            t.rotation = target;
        }

    }

    public static void directionalRotationY(Transform t, Vector2 direction, float smooth = 5.0f)
    {
        Quaternion target = Quaternion.Euler(0, 0, 0);

        float rad = t.rotation.eulerAngles.y * Mathf.Rad2Deg;

        Vector3 currentDirection = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0);

        Vector3 aim = new Vector3(direction.x, direction.y, 0).normalized;

        float angle = Mathf.Atan2(aim.x, aim.y) * 180 / Mathf.PI;

        target = Quaternion.Euler(0, angle, 0);

        t.rotation = Quaternion.Slerp(t.rotation, target, Time.deltaTime * smooth);

        if (Mathf.Floor(t.rotation.eulerAngles.y) == target.eulerAngles.y)
        {
            t.rotation = target;
        }

    }

    public static Quaternion DirectionToRotation(Vector2 direction)
    {
        Quaternion target = Quaternion.Euler(0, 0, 0);

        Vector3 aim = new Vector3(direction.x, direction.y, 0).normalized;

        float angle = Mathf.Atan2(aim.x, aim.y) * 180 / Mathf.PI;

        target = Quaternion.Euler(0, angle, 0);

        return target;

    }

    public static void directionalRotationZ(Transform t, Vector2 direction,float smooth = 5.0f)
    {
        Quaternion target = Quaternion.Euler(0, 0, 0);

        float rad = t.rotation.eulerAngles.z * Mathf.Rad2Deg;

        Vector3 currentDirection = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0);

        Vector3 aim = new Vector3(direction.x, direction.y, 0).normalized;

        float angle = Mathf.Atan2(aim.x, aim.y) * 180 / Mathf.PI;

        target = Quaternion.Euler(0, 0, angle);

        print(target.eulerAngles);

        t.rotation = Quaternion.Slerp(t.rotation, target, Time.deltaTime * smooth);

        if (Mathf.Floor(t.rotation.eulerAngles.z) == target.eulerAngles.z)
        {
            t.rotation = target;
        }

    }

    protected void collision(Rigidbody rigidbody, Vector3 direction)
    {
        float distance = direction.magnitude;

        if (distance > minMoveDistance)
        {
            RaycastHit hit;

            bool collision = rigidbody.SweepTest(direction,out hit, distance + shellRadius,QueryTriggerInteraction.Ignore);

            if (collision)
            {
                onCollision(hit);
            }

        }
    }

    protected bool groundCollisionDetection(Vector3 normal)
    {
        if (normal.y > minGroundNormalY)
        {
            groundCollsionEnter(normal);
            return true;
        }
        return false;
    }

    protected Vector3 wallCollision(Vector3 move, float hitDistance)
    {
        float distance = move.magnitude;

        float modifiedDistance = hitDistance - shellRadius;
        distance = modifiedDistance < distance ? modifiedDistance : distance;

        return move.normalized * distance;
    }

    protected Vector3 calculateForward(Transform body, Vector3 normal)
    {

        return Vector3.Cross(body.right, normal);
    }

    protected Vector3 velocityCorrection(Vector3 velocity, Vector3 normal)
    {
        float projection = Vector3.Dot(velocity, normal);

        if (projection < 0)
        {
            velocity = velocity - projection * normal;
        }

        return velocity;
    }

    protected Vector3 velocityCorrectionForward(Transform body,Vector3 velocity, Vector3 normal)
    {
        float projection = Vector3.Dot(velocity, normal);

        //showVector(velocity, Color.red);
        //showVector(normal, Color.blue);

        normal = new Vector3(normal.x * Mathf.Abs(body.forward.x), normal.y, normal.z * Mathf.Abs(body.forward.z));

        if (projection < 0)
        {
            velocity = velocity - projection * normal;
        }

        return velocity;
    }

    protected Vector3 velocityCorrectionForwardAlternative(Transform body, Vector3 velocity, Vector3 normal)
    {
        float projection = Vector3.Dot(velocity, normal);

        showVector(velocity, Color.red);
        showVector(normal, Color.blue);

        float normalMagnitude = new Vector3(normal.x, 0, normal.z).magnitude;

        float forwardDot = Vector3.Dot(body.forward, normal);

        int normalDirectionX = normal.x > 0 ? 1 : -1;
        int normalDirectionZ = normal.z > 0 ? 1 : -1;

        normal = new Vector3(normalDirectionX * Mathf.Abs(body.forward.x) * normalMagnitude * Mathf.Abs(forwardDot), normal.y, normalDirectionZ * Mathf.Abs(body.forward.z) * normalMagnitude * Mathf.Abs(forwardDot));

        if (projection < 0)
        {
            velocity = velocity - projection * normal;
        }

        return velocity;
    }

    protected Vector3 computeGravityWithCollision(Vector3 normal)
    {
        Vector3 velocity = Vector3.zero;

        showVector(normal,Color.red);
        showVector(-Physics.gravity, Color.blue);

        Vector2 gravity = -Physics.gravity;
        Vector2 normal2d = normal;

        //velocity.x += (Physics.gravity.y * Mathf.Sin(Vector2.Angle(gravity, normal2d))) * Time.deltaTime;

        normal2d = new Vector2(normal.z,normal.y);

        //velocity.z += (Physics.gravity.y * Mathf.Sin(Vector2.Angle(gravity, normal2d))) * Time.deltaTime;

        return velocity;
    }

    protected Quaternion paralleBody(Transform body, Vector3 normal)
    {
        Vector3 rotation = new Vector3(body.rotation.eulerAngles.x + (90-Vector3.Angle(body.forward, normal)), 0, body.rotation.eulerAngles.z + 90 -Vector3.Angle(body.right, normal));

        Quaternion q = new Quaternion();

        q.eulerAngles = rotation;

        return q;
    }

    protected virtual Vector3 groundCollsionEnter(Vector3 normal)
    {
        return Vector2.zero;
    }

    protected virtual void onCollision(RaycastHit hit)
    {

    }

    #region debug

    protected void showVector(Vector3 v, Color c, float t = 0f)
    {
        Debug.DrawLine(transform.position, new Vector3(v.x, v.y, v.z) + transform.position, c, t, false);
    }

    protected void showRay(Vector3 pos, Vector3 dir, Color c)
    {
        Debug.DrawRay(pos, dir, c, 0f);
    }

    #endregion
}