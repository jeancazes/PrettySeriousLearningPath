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
    [SerializeField] float distanceRayon = 2f;

    //State
    bool isAlive = true;

    //cached Component references
    Rigidbody2D myRigidBody;
    Animator myAnimator;
    CapsuleCollider2D myBodyCollider;
    BoxCollider2D myFeet;
    float gravityScaleAtStart;
    //SpriteRenderer myEchelleVis;

    //essai de grab par raycast (qui tire pas droit ??)
    bool grabbed =false;
    RaycastHit2D hit;
    public Transform holdPoint;

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

        if (CrossPlatformInputManager.GetButtonDown("Fire2"))
        {
            grabbed = false;
            hit.collider.gameObject.transform.position = transform.position;
        }

            if (CrossPlatformInputManager.GetButtonDown("Interact"))
        {
            print("EEE");
            if (!grabbed)
            {
                Physics2D.queriesStartInColliders = false;

                hit = Physics2D.Raycast(transform.position, Vector2.right * transform.localScale.x,distanceRayon);

                if (hit.collider != null)
                {
                    grabbed = true;

                }
                else
                {
                    grabbed = false;
                    hit.collider.gameObject.transform.position = transform.position;

                }



            }

            
        }

        if (grabbed)
            {
                print("grabbing!!!");
                hit.collider.gameObject.transform.position = holdPoint.position;

            }

    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (!myBodyCollider.IsTouchingLayers(LayerMask.GetMask("Ladder")))
    //    {
    //        if (CrossPlatformInputManager.GetButtonDown("Interact"))
    //        {
    //            Destroy(echelle);
    //            myEchelleVis.enabled = true;

    //        }
    //    }


    //}

    private void PoseEchelle(Collider echelle)
    {
        
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.right * transform.localScale.x * distanceRayon);
    }


}   
