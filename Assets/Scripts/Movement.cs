using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement: MonoBehaviour
{
    private Animator m_Animator;
    private Rigidbody m_Rigidbody;
    private Collider m_Collider;
    private CapsuleCollider m_CapsuleCollider;

    private float m_DistToGround;

    // TODO: Update to check for actual collision?
    public Boolean IsGrounded { get { return Physics.Raycast(transform.position, -Vector3.up, m_DistToGround - 0.55f); } }
    
    private Boolean m_IsCrouched = false;
    private Boolean m_PreviousGroundedState;


    private float m_ColliderCentreStanding = 0.88f;
    private float m_ColliderHeightStanding = 1.8f;

    private float m_ColliderCentreCrouched = 0.46f;
    private float m_ColliderHeightCrouched = 0.9f;

// Start is called before the first frame update
    void Start()
    {
        m_Animator = GetComponent<Animator>();
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Collider = GetComponent<Collider>();
        m_CapsuleCollider = GetComponent<CapsuleCollider>();

        m_DistToGround = m_Collider.bounds.extents.y;
        m_PreviousGroundedState = IsGrounded;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsGrounded && !m_PreviousGroundedState)
        {
            m_Animator.SetTrigger("Land");
        }

        UpdateAnimations();

        m_PreviousGroundedState = IsGrounded;
    }

    private void FixedUpdate()
    {
        m_Rigidbody.velocity = new Vector3(
            (m_IsCrouched ? 100f : 200f) * Time.deltaTime,
            m_Rigidbody.velocity.y,
            m_Rigidbody.velocity.z
            );
        //m_Rigidbody.velocity = (m_IsCrouched ? 100f : 200f) * Time.deltaTime * new Vector3(1f, 0, 0);
    }

    private void UpdateAnimations()
    {
        if (Input.GetKeyUp(KeyCode.UpArrow) && IsGrounded)
        {
            m_Animator.SetTrigger("Jump");
            m_Rigidbody.AddForce(Vector3.up * 250f);

            UpdateCollider(false);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) )
        {
            m_Animator.SetTrigger("Crouch");
            UpdateCollider(true);
            m_IsCrouched = true;
        }
        else if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            // Finish crouch
            m_Animator.SetTrigger("StopCrouch");
            UpdateCollider(false);
            m_IsCrouched = false;
        }
    }

    private void UpdateCollider(bool isCrouched)
    {
        Debug.Log(string.Format("Is crouched: {0}", isCrouched ? "Yes" : "No"));
        if (isCrouched)
        {
            m_CapsuleCollider.height = m_ColliderHeightCrouched;
            m_CapsuleCollider.center = new Vector3(0, m_ColliderCentreCrouched, 0);
        } else
        {
            m_CapsuleCollider.height = m_ColliderHeightStanding;
            m_CapsuleCollider.center = new Vector3(0, m_ColliderCentreStanding, 0);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.tag);
    }

    public void FootL() { }
    public void FootR() { }
    public void Land() { }
}
