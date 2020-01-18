using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    private Ski player;

    // Start is called before the first frame update
    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Ski>();
        player.setCamera(this);
    }

    // Update is called once per frame
    void LateUpdate()
    {

        transform.position = Vector3.Lerp(transform.position,player.transform.position,1);

        Vector3 velocity = player.getVelocity();

        Vector2 direction = new Vector2(velocity.x, velocity.z) + new Vector2(transform.forward.x, transform.forward.z);

        CustomPhysics3D.directionalRotationY(transform, direction,2.5f);

    }

   
}
