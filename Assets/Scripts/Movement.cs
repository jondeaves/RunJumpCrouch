using System;
using UnityEngine;

public enum GameState
{
    Tutorial,
    Playing,
    Complete
}

public enum PlayerState
{
    Running,
    Crouching,
    Jumping,
    Falling,
    Hurt
}

[RequireComponent( typeof( CharacterController ) )]
public class Movement : MonoBehaviour
{
    [Tooltip( "How fast player moves during regular movement" )]
    [SerializeField]
    private float m_RunSpeed = 4f;

    [Tooltip( "How fast player moves while crouched" )]
    [SerializeField]
    private float m_CrouchSpeed = 2f;

    [Tooltip( "How far up player jumps" )]
    [SerializeField]
    private float m_JumpPower = 4f;

    private CharacterController m_CharacterController;
    private Animator m_Animator;
    private PlayerState m_PlayerState = PlayerState.Running;
    // TODO: Move to game manager?
    private GameState m_GameState = GameState.Tutorial;

    private Vector3 m_MoveDirection = Vector3.zero;
    private Boolean m_PreviousGroundedState;

    private float m_ColliderCentreStanding = 0.9f;
    private float m_ColliderHeightStanding = 1.8f;

    private float m_ColliderCentreCrouched = 0.6f;
    private float m_ColliderHeightCrouched = 1.1f;

    private float m_ColliderCentreJumping = 1f;
    private float m_ColliderHeightJumping = 1.7f;

    private float m_JumpTime = 0.4f;
    private float m_JumpTimer = 0f;

    private float m_TutorialTimer = 3f;

    private Boolean m_ShouldTurn = false;

    void Start()
    {
        m_CharacterController = GetComponent<CharacterController>();
        m_Animator = GetComponent<Animator>();

        m_PreviousGroundedState = m_CharacterController.isGrounded;
    }

    // Update is called once per frame
    void Update()
    {
        if ( m_GameState == GameState.Tutorial )
        {
            UpdateTutorial();
            return;
        }
        else if ( m_GameState == GameState.Complete )
        {
            UpdateComplete();
            return;
        }

        UpdatePlaying();
    }

    private void UpdateTutorial()
    {
        // Ensures player is grounded from start
        UpdateMovement();

        m_TutorialTimer -= Time.deltaTime;
        Debug.Log( string.Format( "Time is: {0}", (Math.Round( m_TutorialTimer, 0 )).ToString() ) );
        // TODO: Make UI to show this

        if ( m_TutorialTimer <= 0 )
        {
            m_GameState = GameState.Playing;
            m_Animator.SetTrigger( "Start" );
        }
    }

    private void UpdateComplete() { }

    private void UpdatePlaying()
    {
        m_MoveDirection.x = 0f;

        if ( m_PlayerState != PlayerState.Hurt )
        {
            UpdateState();
        }
        UpdateCollider();
        UpdateMovement();
        UpdateTurn();

        m_PreviousGroundedState = m_CharacterController.isGrounded;
    }

    private void UpdateMovement()
    {
        float xMoveSpeed = 0;

        if ( m_PlayerState != PlayerState.Hurt && m_GameState == GameState.Playing )
        {
            xMoveSpeed = m_RunSpeed;
            if ( m_PlayerState == PlayerState.Jumping )
                xMoveSpeed = m_RunSpeed * 1.5f;
            else if ( m_PlayerState == PlayerState.Crouching )
                xMoveSpeed = m_CrouchSpeed;

            m_MoveDirection.x = m_Animator.GetBool( "Forwards" ) ? xMoveSpeed : -xMoveSpeed;
        }

        if ( m_PlayerState != PlayerState.Jumping )
        {
            m_MoveDirection.y = Math.Clamp( m_MoveDirection.y + (Physics.gravity.y * Time.deltaTime * 2f), Physics.gravity.y, 9999f );
        }

        m_CharacterController.Move( m_MoveDirection * Time.deltaTime );
    }

    private void UpdateState()
    {
        // If jumping
        if ( m_PlayerState == PlayerState.Jumping )
        {
            m_JumpTimer += Time.deltaTime;

            if ( m_JumpTimer >= m_JumpTime )
            {
                m_JumpTimer = 0f;
                m_PlayerState = PlayerState.Falling;
            }

        }

        // If we aren't grounded (i.e. jumping or falling) then we don't want to be able to do anything
        if ( !m_CharacterController.isGrounded && m_PlayerState != PlayerState.Crouching && m_PlayerState != PlayerState.Jumping )
        {
            m_Animator.SetTrigger( "Falling" );
            m_PlayerState = PlayerState.Falling;

            return;
        }

        // If we have just become grounded, time to land
        if ( m_CharacterController.isGrounded && m_PlayerState == PlayerState.Falling )
        {
            m_Animator.SetTrigger( "Land" );
            m_PlayerState = PlayerState.Running;
        }

        if ( Input.GetKeyUp( KeyCode.UpArrow ) && m_PlayerState == PlayerState.Running )
        {
            m_Animator.SetTrigger( "Jump" );
            m_PlayerState = PlayerState.Jumping;
            m_MoveDirection.y = m_JumpPower;
        }
        else if ( Input.GetKeyDown( KeyCode.DownArrow ) && m_PlayerState == PlayerState.Running )
        {
            m_Animator.SetTrigger( "Crouch" );
            m_PlayerState = PlayerState.Crouching;
        }
        else if ( Input.GetKeyUp( KeyCode.DownArrow ) && m_PlayerState == PlayerState.Crouching )
        {
            // Finish crouch
            m_Animator.SetTrigger( "StopCrouch" );
            m_PlayerState = PlayerState.Running;
        }
    }

    private void UpdateCollider()
    {
        switch ( m_PlayerState )
        {
            case PlayerState.Running:
                m_CharacterController.height = m_ColliderHeightStanding;
                m_CharacterController.center = new Vector3( 0, m_ColliderCentreStanding, 0 );
                break;
            case PlayerState.Crouching:
                m_CharacterController.height = m_ColliderHeightCrouched;
                m_CharacterController.center = new Vector3( 0, m_ColliderCentreCrouched, 0 );
                break;
            case PlayerState.Jumping:
            case PlayerState.Falling:
                m_CharacterController.height = m_ColliderHeightJumping;
                m_CharacterController.center = new Vector3( 0, m_ColliderCentreJumping, 0 );
                break;
        }
    }

    private void UpdateTurn()
    {
        if ( !m_ShouldTurn )
            return;

        Vector3 targetDirection = (transform.position - new Vector3( m_Animator.GetBool( "Forwards" ) ? 10 : -10, 0, 0 )) - transform.position;

        float singleStep = 6f * Time.deltaTime;
        Vector3 newDirection = Vector3.RotateTowards( transform.forward, targetDirection, singleStep, 0.0f );
        transform.rotation = Quaternion.LookRotation( newDirection );

        if ( newDirection.z > -0.01f )
        {
            m_PlayerState = PlayerState.Running;
            m_ShouldTurn = false;
            m_Animator.SetBool( "Forwards", !m_Animator.GetBool( "Forwards" ) );
        }
    }

    public void Turn()
    {
        m_ShouldTurn = true;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if ( hit.gameObject.tag.Equals( "Obstacle" ) && m_PlayerState != PlayerState.Hurt )
        {
            // TODO: Don't repeat until we've left the object?
            // Or maybe move player away from object
            m_PlayerState = PlayerState.Hurt;
            m_Animator.SetTrigger( "Hit" );
        }
        else if ( hit.gameObject.tag.Equals( "Goal" ) )
        {
            // TODO: UI to show completion
            m_GameState = GameState.Complete;
            m_Animator.SetTrigger( "Win" );
        }
    }

    public void FootL() { }
    public void FootR() { }
    public void Land() { }

}
