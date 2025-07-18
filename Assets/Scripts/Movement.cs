using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public static Movement instance;

    private void Awake()
    {
        instance = this;
    }

    public float moveSpeed;

    public Animator anim;

    //public float pickupRange = 1.5f;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 moveInput = new Vector3(0f, 0f, 0f);
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        //Debug.Log(moveInput);

        moveInput.Normalize();

        transform.position += moveInput * moveSpeed * Time.deltaTime;

        /* if (moveInput != Vector3.zero)
        {
            anim.SetBool("isMoving", true);
        }
        else
        {
            anim.SetBool("isMoving", false);
        } */
    }
}