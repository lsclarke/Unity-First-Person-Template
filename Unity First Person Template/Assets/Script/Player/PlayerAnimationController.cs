using System;
using UnityEngine;
using UnityEngine.Windows;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField]
    private Animator animator;
    //Increase performances
    int isWalkHash;

    [SerializeField]
    private PlayerMovement movement;
    [SerializeField]
    private PlayerLedge ledge;
    [SerializeField]
    private PlayerInteract interactions;
    private Vector3 orientationLocation;


    [SerializeField]
    private AnimationText animText;

    public float acc;
    public float dec;

    private float velocityX, velocityZ;

    bool wasJustRunning = false;

    [SerializeField]
    private PlayerHealth health;
    
    private float transitionValue = 0f;
    private float transitionHangValue = 0f;
    private void Awake()
    {
        orientationLocation = transform.localPosition;
    }
    private void Start()
    {
        //Increase performances
        isWalkHash = Animator.StringToHash("isWalking");
        wasJustRunning = false;
    }

    public Animator getAnimator()
    {
        return animator;
    }

    private void LinkAnimatorToPlayer()
    {
        animator.SetBool("On Ground", movement.OnGround());
        animator.SetBool("JumpButtonPressed", movement.jumpButtonPressed);
        animator.SetFloat("Velocity X", velocityX);
        animator.SetFloat("Velocity Z", velocityZ);
        animator.SetBool("isHurt", health.isHurt);
        animator.SetBool("isHanging", ledge.isHanging);
        animator.SetBool("StartHang", ledge.GetStartClimb());
        animator.SetBool("ClimbUp", ledge.GetStartClimbOnTop());
        Taunt();
        Point();

        if (UnityEngine.Input.GetKeyDown("f") && interactions.canInteract)
        {
            HoldItem();
        }

        if (UnityEngine.Input.GetKeyDown("g") && interactions.isInteracting)
        {
            DropItem();
        }

        if (health.isHurt)
        {
            HurtAnimationStart();
        }
        else
        {
            HurtAnimationEnd();
        }

    }

    public void HurtAnimationStart()
    {
        transitionValue += 0.025f;
        animator.SetLayerWeight(2, transitionValue);

        if (transitionValue >= 1f)
        {
            transitionValue = 1f;
        }
    }

    public void HurtAnimationEnd()
    {
        transitionValue -= 0.025f;
        animator.SetLayerWeight(2, transitionValue);

        if(transitionValue <= 0f)
        {
            transitionValue = 0f;
        }
    }

    public void HangAnimationStart()
    {
        animator.SetLayerWeight(3, 1);
    }

    public void HangAnimationEnd()
    {
        animator.SetLayerWeight(3, 0);
    }

    public void HoldItem()
    {
        animator.SetLayerWeight(1,1f);
    }

    public void DropItem()
    {
        animator.SetLayerWeight(1, 0f);
    }

    public void Taunt()
    {
        bool danceButton = UnityEngine.Input.GetKeyDown("q");
        if (danceButton)
        {
            animator.SetTrigger("dance");
        }
    }

    public void Point()
    {
        //Get key input
        bool rmb = UnityEngine.Input.GetKey("e");
        var value = 0f;
        if (rmb)
        {
            value += 0.01f;
            animator.SetLayerWeight(4, value);

            if (value >= 1f)
            {
                value = 1f;
            }
        }
    }

    public void MovementAnimations()
    {

        //Get key input
        bool forwardPressed = UnityEngine.Input.GetKey("w");
        bool backwardPressed = UnityEngine.Input.GetKey("s");
        bool rightPressed = UnityEngine.Input.GetKey("d");
        bool leftPressed = UnityEngine.Input.GetKey("a");
        bool runPressed = UnityEngine.Input.GetKey("left shift");

        if (velocityX == 0f && velocityZ == 0f)
        {
            animText.setCondition("Idle");
        }
        if (velocityX > 0f && velocityX <= 2f)
        {
            animText.setCondition("Walking");
        }
        if (velocityZ > 0f && velocityZ <= 2f)
        {
            animText.setCondition("Walking");
        }
        if (velocityX > 2f && velocityX <= 3f)
        {
            animText.setCondition("Running");
        }
        if (velocityZ > 2f && velocityZ <= 3f)
        {
            animText.setCondition("Running");
        }
        if (velocityX >= 5f)
        {
            animText.setCondition("Running");
        }
        if (velocityZ >= 5f)
        {
            animText.setCondition("Running");
        }

        if (movement.jumpButtonPressed && !movement.OnGround())
        {
            animText.setCondition("Jumping");
        }

        if (runPressed)
        {

            if (forwardPressed)
            {
                velocityZ += Time.deltaTime * acc;
                if (velocityZ >= 5f)
                {
                    velocityZ = 5f;
                }
            }

            if (backwardPressed)
            {
                velocityZ -= Time.deltaTime * acc;

                if (velocityZ <= -5f)
                {
                    velocityZ = -5f;
                }
            }
        }

        if (forwardPressed && velocityZ < 1f)
        {
            velocityZ += Time.deltaTime * acc;
        }

        if (!forwardPressed && velocityZ > 0.0f)
        {
            velocityZ -= Time.deltaTime * dec;

            if (velocityZ < 0.0f)
            {
                velocityZ = 0.0f;   
            }
        }
        //

        if (backwardPressed && velocityZ > -1f)
        {
            velocityZ -= Time.deltaTime * acc;
        }

        if (!backwardPressed && velocityZ < 0.0f)
        {
            velocityZ += Time.deltaTime * dec;

            if (velocityZ > 0.0f)
            {
                velocityZ = 0.0f;
            }
        }


        //

        if (rightPressed && velocityX < 1f)
        {
            velocityX += Time.deltaTime * acc;
        }
        if (!rightPressed && velocityX > 0f)
        {
            velocityX -= Time.deltaTime * dec;
            if (velocityX <= 0.0f)
            {
                velocityX = 0.0f;
            }
        }

        if (leftPressed && velocityX > -1f)
        {
            velocityX -= Time.deltaTime * dec;
        }
        if (!leftPressed && velocityX < 0f)
        {
            velocityX += Time.deltaTime * dec;
            if (velocityX >= 5f)
            {
                velocityX = 5f;
            }
        }

    }
    
    // Update is called once per frame
    void Update()
    {
        LinkAnimatorToPlayer();
        MovementAnimations();
        transform.localPosition = orientationLocation;
    }
}
