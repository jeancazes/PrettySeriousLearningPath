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
    [SerializeField] public int staminaMax = 1000;
    [SerializeField] public int staminaCurrent;
    [SerializeField] Vector2 deathkick = new Vector2(100f, 8f);

    [SerializeField] float depX = 0.5f;
    [SerializeField] float depY = 0.5f;
    [SerializeField] float rotZ = 15f;
    [SerializeField] GameObject holdingPoint;
    [SerializeField] float dep2X = 0.5f;
    [SerializeField] float dep2Y = 0.5f;
    [SerializeField] float fatigueFactor = 1f;
    int lesCollisions = 0;
    public float keyPressed;

    //State
    bool isAlive = true;

    //cached Component references
    Rigidbody2D myRigidBody;
    Animator myAnimator;
    CapsuleCollider2D myBodyCollider;
    BoxCollider2D myFeet;
    float gravityScaleAtStart;
    GameObject lechelle;
    bool grabbed = false;


    // Start is called before the first frame update
    void Start()
    {
        myRigidBody = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        myBodyCollider = GetComponent<CapsuleCollider2D>();
        gravityScaleAtStart = myRigidBody.gravityScale;
        myFeet = GetComponent<BoxCollider2D>();
        GameObject lechelle = GameObject.FindGameObjectWithTag("Echelle");
        staminaCurrent = staminaMax;
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
        Grab();
        GestionAnimations();
        CompteFatigue();
        Die();

        if (Input.GetKeyDown("h"))
        {

            FillingStamina(600);
        }

    }


    //Compter les collisions pour être sur que je ne suis sur rien...
    private void OnCollisionEnter2D(Collision2D collision)
    {
        lesCollisions++;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        lesCollisions--;

    }

    // Pour attrapper et relacher l'echelle
    // des améliorations à apporter (pas toujours bien en place !)

    private void Grab()
    {
        if (grabbed)
        {
            if (CrossPlatformInputManager.GetButtonDown("Interact"))
            {

                grabbed = false;
                GameObject lechelle = GameObject.FindGameObjectWithTag("Echelle");
                lechelle.transform.parent = null;
                lechelle.transform.rotation = Quaternion.identity;
                lechelle.transform.localPosition = new Vector2(transform.position.x - dep2X, transform.position.y - dep2Y);

                lechelle.GetComponent<BoxCollider2D>().enabled = true;
            }
        }


        if (!myBodyCollider.IsTouchingLayers(LayerMask.GetMask("Ladder")))
        {
            return;
        }
        else
        {
            if (CrossPlatformInputManager.GetButtonDown("Interact"))
            {

                grabbed = true;

                GameObject lechelle = GameObject.FindGameObjectWithTag("Echelle");
                lechelle.transform.parent = holdingPoint.transform;
                lechelle.transform.localPosition = new Vector2(depX, depY);
                lechelle.transform.Rotate(new Vector3(0, 0, rotZ));
                lechelle.GetComponent<BoxCollider2D>().enabled = false;
            }


        }
    }

    // pour monter à l'échelle
    // -- on peut toujours pas sauter de l'échelle
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
        //if (!myFeet.IsTouchingLayers(LayerMask.GetMask("Ground")) && !myFeet.IsTouchingLayers(LayerMask.GetMask("Platforms")) && !myFeet.IsTouchingLayers(LayerMask.GetMask("Ladder")) && !myFeet.IsTouchingLayers(LayerMask.GetMask("Objects")) )
        if (lesCollisions <= 0)
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

        
        if (lesCollisions > 0)

        {
            if (CrossPlatformInputManager.GetButtonDown("Jump"))
            {
                Vector2 jumpVelocityToAdd = new Vector2(0f, jumpSpeed);
                myRigidBody.velocity += jumpVelocityToAdd;
                myAnimator.SetBool("Jumping", true);
            }
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

   

    private void CompteFatigue()
    {
        if (Input.anyKey)
        {
            keyPressed++;
        }

        int fatigue = Mathf.RoundToInt(keyPressed * fatigueFactor);
        //mettre ici les malus d'efforts supplémentaires 
        staminaCurrent = staminaMax - fatigue;

    }

    public int Stamina()
    {
        return staminaCurrent;
    }

    public void FillingStamina(int staminaBoost)
    {
        keyPressed = 0;
        staminaCurrent = Mathf.Clamp((staminaCurrent + staminaBoost), 0, staminaMax);
        Debug.Log("-----------------------------------------Stamina = " + staminaCurrent);
    }

    //public void FillingStamina(int staminaBoost)
    //{
    //    Coroutine co;
    //    co = StartCoroutine(Remplissage(staminaBoost));
    //}

    //public IEnumerator Remplissage(int staminaBoost)
    //{

    //    int staminaMaxProv = staminaCurrent + staminaBoost;

    //        while (staminaCurrent<staminaMaxProv)
    //        {
    //        staminaCurrent++;
    //        yield return new WaitForSeconds(.1f);
    //        }

    //}

    

    //public void StopFillingStamina
    //{
    //    StopCoroutine(co);
    //}

    //public IEnumerator Remplissage(staminaBoost)
    //{

    //    print("fatigue key = " + jsuiscrevé);
    //    while (staminaCurrent <= staminaMax)
    //    {

    //        print(keyPressed);
    //        print("i=" + i);
    //        yield return new WaitForSeconds(.1f);

    //    }
    //}
}
