using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    //movement
    [SerializeField] private float walkSpeed, runSpeed;
    [SerializeField] private float CwalkSpeed, CrunSpeed;
    [SerializeField] private float runBuildUp;
    [SerializeField] private KeyCode runKey;
    [SerializeField] private KeyCode crouchKey;
    private bool crouchCool = true;
    private Vector3 moveDirection = Vector3.zero;
    public float speed = 6.0F;
    public float gravity = 20.0F;

    //crouch
    [SerializeField] private bool crouch = false;
    //public Animator anim;
    //private enum State { idle, crouch, sprint }

    private State state;
    private CharacterController charController;

    //jump
    [SerializeField] private AnimationCurve jumpFalloff;
    [SerializeField] private float jumpMultiplier;
    [SerializeField] private KeyCode jumpKey;
    private bool isJumping;

    //dialogue
    [SerializeField] private KeyCode interactKey;
    private bool inDialogueRange;
    private DialogueTrigger currentDialogue;
    public bool dialogueOn;

    //cam&claw
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform debugHitPointTransform;
    [SerializeField] private Transform clutchTransform;
    [SerializeField] private float rayLength;
    [SerializeField] private KeyCode clutchKey;
    private Vector3 clutchPosition;
    private float clutchSize;

    //gunShooting&Ray
    private int gunDamage = 1;
    private float fireRate = .75f;
    private float gunRange = 50f;
    private float hitForce = 500f;
    [SerializeField] private Transform gunEnd;

    private WaitForSeconds shotDuration = new WaitForSeconds(.07f);
    private float nextFire;
    [SerializeField] private ParticleSystem part;
    private void Awake()
    {
        state = State.Normal;
        charController = GetComponent<CharacterController>();
        clutchTransform.gameObject.SetActive(false);
        //anim = GetComponent<Animator>();
    }

    //Update & StateMachine
    private void Update()
    {
        //currentState();
        //anim.SetInteger("state", (int)state);
        switch (state)
        {
            default:
            case State.Normal:
                PlayerMovement();
                normalMove();
                clutchClaw();
                checkCrouch();
                break;
            case State.Crouch:
                PlayerMovement();
                crouchMove();
                checkUnCrouch();
                break;
            case State.Clutching:
                clutchClawMove();
                break;
            case State.ClutchThrown:
                HandleThrow();
                PlayerMovement();
                normalMove();
                break;
        }

        if (!inDialogue())
        {
            dialogueOn = false;
            //dialogue
            if (inDialogueRange == true && Input.GetKeyDown(interactKey))
            {
                currentDialogue.ActivateDialogue();
            }

            //shoot
            if (Input.GetKeyDown(KeyCode.Mouse0) && Time.time > nextFire)
            {
                nextFire = Time.time + fireRate;
                StartCoroutine(ShotEffect());
                Vector3 rayOrigin = playerCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));
                RaycastHit hit;

                /*if (Physics.Raycast(rayOrigin, playerCamera.transform.forward, out hit, gunRange))
                {
                    if (hit.rigidbody != null)
                    {
                        hit.rigidbody.AddForce(-hit.normal * hitForce);
                    }
                }*/
                if (Physics.SphereCast(rayOrigin, 0.5f, playerCamera.transform.forward, out hit, gunRange))
                {
                    if (hit.rigidbody != null)
                    {
                        hit.rigidbody.AddForce(-hit.normal * hitForce);
                    }
                }
            }
        }
        else
        {
            dialogueOn = true;
        }

    }
    /*private void FixedUpdate()
    {
        switch (state)
        {
            default:
            case State.Normal:
                PlayerMovement();
                normalMove();
                clutchClaw();
                checkCrouch();
                break;
            case State.Crouch:
                PlayerMovement();
                crouchMove();
                checkUnCrouch();
                break;
            case State.Clutching:
                clutchClawMove();
                break;
            case State.ClutchThrown:
                HandleThrow();
                PlayerMovement();
                normalMove();
                break;

        }
    }*/

    //states
    private enum State
    { 
        Normal, Clutching, Crouch, ClutchThrown,
    }

    /*private void currentState()
    {
        if (state == State.idle)
        {
            checkCrouch();
            normalMove();
        }

        if (state == State.crouch)
        {
            checkUnCrouch();
            crouchMove();
        }
    }*/

    void checkCrouch()
    {
        if (!inDialogue())
        {
            if (Input.GetKeyDown(crouchKey) && crouchCool == true)
            {
                //state = State.crouch;
                crouchCool = false;
            }
            if (Input.GetKeyUp(crouchKey))
            {
                crouchCool = true;
            }
        }
    }
    void checkUnCrouch()
    {
        if (!inDialogue())
        {
            if (Input.GetKeyDown(crouchKey) && crouchCool == true)
            {
                //state = State.idle;
                crouchCool = false;
            }
            if (Input.GetKeyUp(crouchKey))
            {
                crouchCool = true;
            }
        }
    }

    private void PlayerMovement()
    {
        //moveV2
        if (!inDialogue())
        {
            charController.Move(moveDirection * Time.deltaTime);
            if (charController.isGrounded)
            {
                moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
                moveDirection = transform.TransformDirection(moveDirection);
                moveDirection *= speed;
                JumpInput();
            }
            else if (charController.isGrounded == false)
            {
                moveDirection.x = Input.GetAxis("Horizontal") * speed;
                moveDirection.z = Input.GetAxis("Vertical") * speed;

                moveDirection = transform.TransformDirection(moveDirection);
            }

            moveDirection.y -= gravity * Time.deltaTime;
        } 
    }
    //moveV1
    /*
    float horizInput = Input.GetAxis(horizontalInputName);
    float vertInput = Input.GetAxis(verticalInputName);

    Vector3 forwardMovement = transform.forward * vertInput;
    Vector3 rightMovement = transform.right * horizInput;

    charController.SimpleMove(Vector3.ClampMagnitude(forwardMovement + rightMovement, 1.0f) * movementSpeed);
        
        JumpInput();
        */
    private void normalMove()
    {
        if (Input.GetKey(runKey))
        {
            speed = Mathf.Lerp(speed, runSpeed, Time.deltaTime * runBuildUp);
        }
        else
        {
           speed = Mathf.Lerp(speed, walkSpeed, Time.deltaTime * runBuildUp);
        }
    }
    //crouch needs update
    private void crouchMove()
    {
        if (Input.GetKey(runKey))
        {
            speed = Mathf.Lerp(speed, CrunSpeed, Time.deltaTime * runBuildUp);
        }
        else
        {
            speed = Mathf.Lerp(speed, CwalkSpeed, Time.deltaTime * runBuildUp);
        }
    }

    private void JumpInput()
    {
        if (Input.GetKeyDown(jumpKey) && !isJumping)
        {
            isJumping = true;
            StartCoroutine(JumpEvent());
        }
    }

    private IEnumerator JumpEvent()
    {
        float timeInAir = 0.0f;
        do
        {
            float jumpForce = jumpFalloff.Evaluate(timeInAir);
            charController.Move(Vector3.up * jumpForce * jumpMultiplier * Time.deltaTime);
            timeInAir += Time.deltaTime;
            yield return null;
        } while (!charController.isGrounded && charController.collisionFlags != CollisionFlags.Above);

        isJumping = false;
    }

    //claw
    private void clutchClaw()
    {
        if (!inDialogue())
        {
            if (Input.GetKeyDown(clutchKey))
            {
                if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward * rayLength, out RaycastHit raycasathit))
                {
                    debugHitPointTransform.position = raycasathit.point;
                    clutchPosition = raycasathit.point;
                    clutchSize = 0f;
                    clutchTransform.gameObject.SetActive(true);
                    clutchTransform.localScale = Vector3.zero;
                    state = State.ClutchThrown;
                }
            }
        }
    }

    private void HandleThrow()
    {
        clutchTransform.LookAt(clutchPosition);

        float clutchThrowSpeed = 70f;
        clutchSize += clutchThrowSpeed * Time.deltaTime;
        clutchTransform.localScale = new Vector3(1, 1, clutchSize);

        if (clutchSize >= Vector3.Distance(transform.position, clutchPosition)) 
        {
            state = State.Clutching;
        }
    }
    private void clutchClawMove()
    {
        clutchTransform.LookAt(clutchPosition);

        Vector3 clutchDir = (clutchPosition - transform.position).normalized;
        float clutchSpeedMin = 10f;
        float clutchSpeedMax = 40f;
        float clutchSpeedMultiplier = 2f;
        float clutchSpeed = Mathf.Clamp(Vector3.Distance(transform.position, clutchPosition), clutchSpeedMin, clutchSpeedMax);
        
        //move chara
        charController.Move(clutchDir * clutchSpeed *clutchSpeedMultiplier* Time.deltaTime);

        float reachClutchPos = 2f;
        if (Vector3.Distance(transform.position, clutchPosition) < reachClutchPos)
        {
            StopClutch();
        }
        else if (Input.GetKeyDown(clutchKey))
        {
            StopClutch();
        }
    }

    private void StopClutch()
    {
        clutchTransform.gameObject.SetActive(false);
        state = State.Normal;
    }

    //gun
    private IEnumerator ShotEffect()
    {
        part.Play();
        yield return shotDuration;
        part.Stop();
    }

    //trigger
    private void OnTriggerEnter(Collider other)
    {   
        currentDialogue = other.gameObject.GetComponent<DialogueTrigger>();
        if (other.gameObject.CompareTag("Dialogue"))
        {
            inDialogueRange = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Dialogue"))
        {
            inDialogueRange = false;
            currentDialogue = null;
        }
    }

    //dialogue
    private bool inDialogue()
    {
        if (currentDialogue != null)
            return currentDialogue.DialogueActive();
        else
            return false;
    }
}
