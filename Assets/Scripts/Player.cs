using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class Player : MonoBehaviour
{
    //config
    [SerializeField] float playerSpeed = 5f;
    [SerializeField] float jumpSpeed = 10f;
    [SerializeField] float climbSpeed = 5f;
    [SerializeField] Vector2 deathkick = new Vector2(100f,8f);

    //State
    bool isAlive = true;

    //cached Component references
    Rigidbody2D myRigidBody;
    Animator myAnimator;
    CapsuleCollider2D myBodyCollider;
    BoxCollider2D myFeet;
    float gravityScaleAtStart;

    // Start is called before the first frame update
    void Start()
    {
        myRigidBody = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        myBodyCollider = GetComponent<CapsuleCollider2D>();
        gravityScaleAtStart = myRigidBody.gravityScale;
        myFeet = GetComponent<BoxCollider2D>();

    }

    // Update is called once per frame
    void Update()
    {
        if (!isAlive)
        {
            return;
        }


        Run();
        FlipSprite();
        Jump();
        Climb();
        GestionAnimations();
        Die();

    }

    private void Climb()
    {

        if (!myBodyCollider.IsTouchingLayers(LayerMask.GetMask("Ladder")))
        {
            myAnimator.SetBool("Climbing", false);
            myRigidBody.gravityScale = gravityScaleAtStart;
            return;
        }


        float controlthrow = CrossPlatformInputManager.GetAxis("Vertical");
        Vector2 climbVelocity = new Vector2(myRigidBody.velocity.x, controlthrow * climbSpeed);
        myRigidBody.velocity = climbVelocity;

        myRigidBody.gravityScale = 0;

        bool playerHasVerticalSpeed = Mathf.Abs(myRigidBody.velocity.y) > Mathf.Epsilon;

        myAnimator.SetBool("Climbing", playerHasVerticalSpeed);
            
       

    }

    private void Run()
    {
        float controlThrow = CrossPlatformInputManager.GetAxis("Horizontal"); // +1 to -1
        Vector2 playerVelocity = new Vector2(controlThrow * playerSpeed, myRigidBody.velocity.y);
        myRigidBody.velocity = playerVelocity;

        bool playerHasHorizontalSpeed = Mathf.Abs(myRigidBody.velocity.x) > Mathf.Epsilon;

        if (playerHasHorizontalSpeed)
        {
            myAnimator.SetBool("Running", true);
        }
        else
        {
            myAnimator.SetBool("Running", false);
        }

    }

    private void FlipSprite()
    {
        bool playerHasHorizontalSpeed = Mathf.Abs(myRigidBody.velocity.x) > Mathf.Epsilon;

        if (playerHasHorizontalSpeed)
        {
           transform.localScale = new Vector2(Mathf.Sign(myRigidBody.velocity.x), 1f);
        }


    }


    private void GestionAnimations()
    {
        if (!myFeet.IsTouchingLayers(LayerMask.GetMask("Ground")) && !myFeet.IsTouchingLayers(LayerMask.GetMask("Ladder")))
        {
            bool playerHasVerticalSpeed = Mathf.Abs(myRigidBody.velocity.y) > Mathf.Epsilon;

            if (playerHasVerticalSpeed)
            {
                if (myRigidBody.velocity.y <= 0)
                {
                    myAnimator.SetBool("Falling", true);
                    myAnimator.SetBool("Jumping", false);
                }
                else
                {
                    myAnimator.SetBool("Falling", false);
                    myAnimator.SetBool("Jumping", true);
                }
            }

        }
        else
        {
            NettoyageAnimator();
        }

    }

    private void NettoyageAnimator()
    {
        myAnimator.SetBool("Falling", false);
        myAnimator.SetBool("Jumping", false);

    }

    private void Jump()
        {

            if (!myFeet.IsTouchingLayers(LayerMask.GetMask("Ground")))
            {
                return;
            }

            if (CrossPlatformInputManager.GetButtonDown("Jump"))
            {
                Vector2 jumpVelocityToAdd = new Vector2(0f, jumpSpeed);
                myRigidBody.velocity += jumpVelocityToAdd;
                myAnimator.SetBool("Jumping", true);
            }

        }

    public void Die()
    {
        if (myBodyCollider.IsTouchingLayers(LayerMask.GetMask("Enemy")))
        {
            isAlive = false;
            NettoyageAnimator();
            myAnimator.SetTrigger("Dying");
            GetComponent<Rigidbody2D>().velocity = deathkick;

        }

    }




}   
