using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class PlayerMovement : NetworkBehaviour
{

    public float speed = 500f;

    private Rigidbody playerRigidBody;

    // Start is called before the first frame update
    void Start()
    {
        playerRigidBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!IsLocalPlayer)
        {
            return;
        }

        MovePlayer();
    }

    void MovePlayer()
    {
        float mH = Input.GetAxis("Horizontal");
        float mV = Input.GetAxis("Vertical");
        playerRigidBody.velocity = new Vector3(mH * (Time.deltaTime * speed), playerRigidBody.velocity.y, mV * (Time.deltaTime * speed));
    }
}
