using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TD3_Framework
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(AudioSource))]
    public class FirstPersonCtrl : MonoBehaviour
    {
        #region 字段和属性
        [SerializeField] private MouseLook m_MouseLook;
        [SerializeField] private float m_WalkSpeed;
        [SerializeField] private float m_RunSpeed;
        [SerializeField] private float m_JumpSpeed;
        [SerializeField] private float m_StickToGroundForce = 10.0f; //跳跃起来回到地面的力量
        [SerializeField] private float m_GravityMultiplier; //重力影响的权重比

        private CharacterController m_CharacterController;
        private Camera m_Camera;
        private AudioSource m_AudioSource;

        private Vector3 m_MoveDir = Vector3.zero;
        private bool m_Jump;
        private bool m_Jumping;
        private bool m_PreviouslyGrounded; //上一帧角色是否在地上
        #endregion

        #region Unity生命周期
        void Start()
        {
            m_CharacterController = GetComponent<CharacterController>();
            m_Camera = Camera.main;
            m_AudioSource = GetComponent<AudioSource>();

            m_MouseLook.Init(m_CharacterController.transform, m_Camera.transform);
        }
        void Update()
        {
            RotateView(); //总是视野旋转在前Update中执行

            if (!m_Jump)
            {
                m_Jump = Input.GetButtonDown("Jump");
            }
            if (!m_PreviouslyGrounded && m_CharacterController.isGrounded) //上一帧在空中，当前帧在地面上
            {
                m_MoveDir.y = 0f;
                m_Jumping = false;
            }
            if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded) //上一帧在地面上, 当前帧在空中, 没有跳跃
            {
                m_MoveDir.y = 0f;
            }

            m_PreviouslyGrounded = m_CharacterController.isGrounded;
        }
        void FixedUpdate()
        {
            MoveAndJumpView(); //总是移动跳跃在后, FixedUpdate中执行
        }
        #endregion

        #region 方法
        //控制主角与摄像机的旋转（摄像机在Y轴上的旋转跟随角色， 摄像机又能沿着自身X轴旋转）
        private void RotateView()
        {
            m_MouseLook.LookRotation(transform, m_Camera.transform);
        }
        private void MoveAndJumpView()
        {
            float speed = Input.GetKey(KeyCode.LeftShift) ? m_RunSpeed : m_WalkSpeed;
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            Vector2 input = new Vector2(horizontal, vertical);
            if (input.sqrMagnitude > 1)
            {
                input.Normalize(); //单位化
            }
            Vector3 desiredMove = transform.forward * input.y + transform.right * input.x;
            RaycastHit hitInfo;
            Physics.SphereCast(transform.position, m_CharacterController.radius,
                Vector3.down, out hitInfo,
                m_CharacterController.height / 2f, Physics.AllLayers,
                QueryTriggerInteraction.Ignore);
            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

            m_MoveDir.x = desiredMove.x * speed;
            m_MoveDir.z = desiredMove.z * speed;

            if (m_CharacterController.isGrounded)
            {
                m_MoveDir.y = -m_StickToGroundForce;
                if (m_Jump)
                {
                    m_MoveDir.y = m_JumpSpeed;
                    m_Jump = false;  //不可以再跳跃
                    m_Jumping = true;  //跳跃中
                }
            }
            else
            {
                m_MoveDir += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime; //重力-9.8
            }
            m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);
        }
        #endregion
    }
}


