using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TD3_Framework
{
    [System.Serializable]
    public class MouseLook
    {
        #region 字段和属性
        public float XSensitivity = 2.0f; //水平方向灵敏度
        public float YSensitivity = 2.0f; //垂直方向灵敏度
        public bool clampVerticalRotation = true; //是否需要限制垂直方向的旋转
        public float MinimumX = -90.0f;
        public float MaximumX = 90.0f;
        public bool smooth;
        public float smoothTime = 5.0f;
        public bool lockCursor = true; //是否将鼠标光标锁定在游戏窗口的中心

        private Quaternion m_CharacterTargetRot;
        private Quaternion m_CameraTargetRot;
        private bool m_cursorIsLocked = true;
        #endregion


        #region 公有方法
        public void Init(Transform character, Transform camera)
        {
            m_CharacterTargetRot = character.localRotation;
            m_CameraTargetRot = camera.localRotation;
        }
        public void LookRotation(Transform character, Transform camera)
        {
            float yRot = Input.GetAxis("Mouse X") * XSensitivity;
            float xRot = Input.GetAxis("Mouse Y") * YSensitivity;

            m_CharacterTargetRot *= Quaternion.Euler(0f, yRot, 0f);
            m_CameraTargetRot *= Quaternion.Euler(-xRot, 0f, 0f);

            if (clampVerticalRotation)
                m_CameraTargetRot = ClampRotationAroundXAxis(m_CameraTargetRot);

            if (smooth)
            {
                character.localRotation = Quaternion.Slerp(character.localRotation, m_CharacterTargetRot,
                    smoothTime * Time.deltaTime);
                camera.localRotation = Quaternion.Slerp(camera.localRotation, m_CameraTargetRot,
                    smoothTime * Time.deltaTime);
            }
            else
            {
                character.localRotation = m_CharacterTargetRot;
                camera.localRotation = m_CameraTargetRot;
            }

            UpdateCursorLock();
        }
        public void SetCursorLock(bool value)
        {
            lockCursor = value;
            if (!lockCursor)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
        public void UpdateCursorLock()
        {
            if (lockCursor)
                InternalLockUpdate();
        }
        #endregion

        #region 私有方法
        private void InternalLockUpdate()
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                m_cursorIsLocked = false;
            }else if (Input.GetMouseButtonUp(0))
            {
                m_cursorIsLocked = true;
            }

            if (m_cursorIsLocked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }else if (!m_cursorIsLocked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
        //在X轴上做旋转限制，同理可以在Y轴，Z轴做旋转限制的扩展
        private Quaternion ClampRotationAroundXAxis(Quaternion q)
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
            angleX = Mathf.Clamp(angleX, MinimumX, MaximumX);
            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);
            
            return q;
        }
        #endregion
    }
}

