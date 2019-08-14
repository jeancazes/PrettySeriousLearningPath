using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    //config
    public FixedJoystick Joystick;
    [SerializeField] public bool isMobile = true;
    private float controlThrowX;
    private float controlThrowY;

    [SerializeField] float playerSpeed = 5f;
    [SerializeField] float jumpSpeed = 10f;
    [SerializeField] float climbSpeed = 5f;


    // en fait a priori ... ya pas besoin
    //float playerSpeedRst;
    //float jumpSpeedRst;
    //float climbSpeedRst;

    [SerializeField] float playerSpeedBlob = 2f;
    //[SerializeField] float jumpSpeedBlob = 0f;
    //[SerializeField] float climbSpeedBlob = 0f;


    //gestion de la fatigue
    [SerializeField] public int staminaMax = 1000;
    [SerializeField] public int staminaCurrent;
    [SerializeField] float fatigueFactor = 1f;
    [SerializeField] Vector2 deathkick = new Vector2(5f, 8f);
    [SerializeField] bool blobified = false;
    public float keyPressed;
    [SerializeField] bool blobPeuMarcherEnAvant = true;
    [SerializeField] public float vitesseDeGlisseDuBlob = 3f;
    [SerializeField] public float nombreDeSecondesEnAvantDuBlobMax = 4f;
    public float nombreDeSecondesEnAvantDuBlobRestantCurrent;
    [SerializeField] public float tempsDeLatenceAvantDePouvoirRemarcherEnAvant = 5f;
    float timer = 0f;


    //valeur pour décadrer l'echelle quand on la porte ... ils ont pas trop bon
    [SerializeField] float depX = 0.5f;
    [SerializeField] float depY = 0.5f;
    [SerializeField] float rdepY = 0.5f;
    [SerializeField] float rotZ = 15f;
    [SerializeField] GameObject holdingPoint;
    [SerializeField] Canvas CanvasJoystick;

    //et quand on la pose ...ils sont bons
    [SerializeField] float dep2X = 0.5f;
    [SerializeField] float dep2Y = 0.5f;

    //Var pour Gestion des animss en l'air
    int lesCollisions = 0;


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
        //caching
        myRigidBody = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        myBodyCollider = GetComponent<CapsuleCollider2D>();
        gravityScaleAtStart = myRigidBody.gravityScale;
        myFeet = GetComponent<BoxCollider2D>();
        GameObject lechelle = GameObject.FindGameObjectWithTag("Echelle");

        //init var
        staminaCurrent = staminaMax;
        //playerSpeedRst = playerSpeed;
        //jumpSpeedRst = jumpSpeed;
        //climbSpeedRst = climbSpeed;

        nombreDeSecondesEnAvantDuBlobRestantCurrent = nombreDeSecondesEnAvantDuBlobMax;

       


    }

    // Update is called once per frame
    void Update()
    {
        if (!isAlive)
        {
            return;
        }

        if (blobified)
        {
            BlobMoonWalk();
            FlipSprite();
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

    public void TestMobile()
    {
        if (isMobile)
        {
            CanvasJoystick.enabled = false;
            isMobile = false;
        }
        else
        {
            CanvasJoystick.enabled = true;
            isMobile = true;
        }
    }

    private void BlobMoonWalk()
    {
        if (isMobile)
        {

            controlThrowX = Joystick.Horizontal; // +1 to -1


        }
        else
        {
            controlThrowX = CrossPlatformInputManager.GetAxis("Horizontal"); // +1 to -1
        }


        if (controlThrowX == 0)
        {
            //sliding if static
            myRigidBody.velocity = new Vector2(-1 * vitesseDeGlisseDuBlob, myRigidBody.velocity.y);
            return;
        }
        Debug.Log(" ---------------------------after blob = " + controlThrowX);

        Vector2 playerVelocity = new Vector2(controlThrowX * playerSpeedBlob, myRigidBody.velocity.y);

        bool playerHasHorizontalSpeed = Mathf.Abs(myRigidBody.velocity.x) > Mathf.Epsilon;

        timer += Time.deltaTime;
        //Debug.Log("latence = " + timer);

        //
        // $$$!!!$$$  Ca merde par  là ... je n'arrive pas à le faire avancer de nombreDeSecondesEnAvantDuBlobMax et à devoir attendre tempsDeLatenceAvantDePouvoirRemarcherEnAvant avant de pouvoir recommencer
        // Pour l'instant ça marche par une lutte du Drag v/s la vitesse du joueur
        //
        if (playerHasHorizontalSpeed)
        {
            if (myRigidBody.velocity.x <= 0)
            {
                myRigidBody.velocity = playerVelocity;
            }
            else
            {
                if (blobPeuMarcherEnAvant)
                {
                    if (timer >= tempsDeLatenceAvantDePouvoirRemarcherEnAvant)
                    {
                        timer = 0f;

                        if (nombreDeSecondesEnAvantDuBlobRestantCurrent < nombreDeSecondesEnAvantDuBlobMax)
                        {
                            //Debug.Log("secondes restantes = " + nombreDeSecondesEnAvantDuBlobRestantCurrent);
                            myRigidBody.velocity = playerVelocity;
                            nombreDeSecondesEnAvantDuBlobRestantCurrent += Time.deltaTime;



                        }
                        else
                        {
                            nombreDeSecondesEnAvantDuBlobRestantCurrent = nombreDeSecondesEnAvantDuBlobMax;


                        }
                    }
                }
            }
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

    public void Grab()
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

        if (isMobile)
        {

            float controlThrowY = Joystick.Vertical; // +1 to -1

        }
        else
        {
            float controlThrowY = CrossPlatformInputManager.GetAxis("Vertical"); // +1 to -1
        }

        Vector2 climbVelocity = new Vector2(myRigidBody.velocity.x, controlThrowY * climbSpeed);
        myRigidBody.velocity = climbVelocity;

        myRigidBody.gravityScale = 0;

        bool playerHasVerticalSpeed = Mathf.Abs(myRigidBody.velocity.y) > Mathf.Epsilon;

        myAnimator.SetBool("Climbing", playerHasVerticalSpeed);

    }

    private void Run()
    {

        Debug.Log("------------------Joystick " + Joystick.Horizontal);
        if (isMobile)
        {


            controlThrowX = Joystick.Horizontal; // +1 to -1
            Debug.Log("------------------dans run avant " + controlThrowX);
        }
        else
        {
            controlThrowX = CrossPlatformInputManager.GetAxis("Horizontal"); // +1 to -1
        }

        Debug.Log("------------------dans run après " + controlThrowX);

        Vector2 playerVelocity = new Vector2(controlThrowX * playerSpeed, myRigidBody.velocity.y);
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
        if (!isMobile)
        {
            if (CrossPlatformInputManager.GetButtonDown("Jump"))
            {
                Jumping();
            }
        }



    }

    public void Jumping()
    {
        if (lesCollisions > 0)
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
            Died();
        }


    }

    private void Died()
    {
        isAlive = false;
        NettoyageAnimator();
        myAnimator.SetTrigger("Dying");
        GetComponent<Rigidbody2D>().velocity = deathkick;
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

        if (staminaCurrent <= 0)
        {

            Blobification();
        }

    }

    private void Blobification()
    {
        blobified = true;
        myAnimator.SetBool("Blobified", true);



    }

    public int Stamina()
    {
        return staminaCurrent;
    }

    public void FillingStamina(int staminaBoost)
    {
        keyPressed = 0;

        //deblobification
        blobified = false;
        myAnimator.SetBool("Blobified", false);

        //
        staminaCurrent = Mathf.Clamp((staminaCurrent + staminaBoost), 0, staminaMax);
        //Debug.Log("-----------------------------------------Stamina = " + staminaCurrent);
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
