using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TD3_Framework
{
    [RequireComponent(typeof(CharacterController))]
    public class ThirdPersonCtrl : MonoBehaviour
    {
        #region 字段和属性
        [SerializeField]
        private bool m_Smooth; //是否平滑过渡旋转
        [SerializeField]
        [Range(0.0f, 10.0f)]
        private float m_SmoothSpeed = 5.0f; //旋转平滑过渡速度
        [SerializeField]
        private float m_StickToGroundForce = 10.0f; //跳跃起来回到地面的力量
        [SerializeField]
        private float m_GravityMultiplier; //重力影响的权重比
        [SerializeField]
        private float m_JumpSpeed;
        [SerializeField]
        private bool m_IsWalking = true;
        [SerializeField]
        private float m_WalkSpeed;
        [SerializeField]
        private float m_RunSpeed;

        private CharacterController m_CharacterController;


        private bool m_Jump = false;
        private bool m_Jumping = false;
        private Vector3 m_MoveDir;
        #endregion

        #region Unity生命周期
        void Start()
        {
            m_CharacterController = GetComponent<CharacterController>();
        }
        void Update()
        {
            RotateView();
        }
        void FixedUpdate()
        {

        }
        #endregion

        #region 方法
        private void RotateView()
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            Vector2 input = new Vector2(horizontal, vertical);


            if (input.sqrMagnitude > 1.0f)
            {
                input.Normalize();
            }
            transform.rotation = Quaternion.RotateTowards(transform.rotation,
                Quaternion.LookRotation(new Vector3(input.x, 0.0f, input.y)), m_SmoothSpeed);
            m_MoveDir.x = m_WalkSpeed * input.x;
            m_MoveDir.z = m_WalkSpeed * input.y;

            if (!m_Jump)
            {
                m_Jump = Input.GetButtonDown("Jump");
            }

            if (m_CharacterController.isGrounded)
            {
                m_MoveDir.y = -m_StickToGroundForce;
                if (m_Jump)
                {
                    m_MoveDir.y = m_JumpSpeed;
                    m_Jump = false;
                    m_Jumping = true;
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

