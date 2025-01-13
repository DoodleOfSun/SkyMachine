using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace UnityStandardAssets._2D
{
    public class Camera2DFollow : MonoBehaviour
    {
        public static Camera2DFollow instance;
        public float damping = 1;
        public float lookAheadFactor = 3;
        public float lookAheadReturnSpeed = 0.5f;
        public float lookAheadMoveThreshold = 0.1f;
        public List<GameObject> wave;

        private float offsetZ;
        private Vector3 lastTargetPosition;
        private Vector3 currentVelocity;
        private Vector3 lookAheadPos;
        private Vector3 previousPosition;

        // 처음에는 플레이어를 카메라가 따라가야 하므로 다음과 같이 초기화한다.
        private void Start()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
            //InitCam(target.transform.position);
        }

        // Update is called once per frame
        private void Update()
        {
            //ChasingByTransform(target.transform.position);
        }

        private void InitCam(Vector3 targetPos)
        {
            // 변수 초기화
            lastTargetPosition = targetPos;
            offsetZ = transform.position.z - targetPos.z;
            transform.parent = null;
        }

        private void ChasingByTransform(Vector3 targetPos)
        {
            // 카메라 이동 로직
            float xMoveDelta = targetPos.x - lastTargetPosition.x;

            bool updateLookAheadTarget = Mathf.Abs(xMoveDelta) > lookAheadMoveThreshold;

            if (updateLookAheadTarget)
            {
                lookAheadPos = lookAheadFactor * Vector3.right * Mathf.Sign(xMoveDelta);
            }
            else
            {
                lookAheadPos = Vector3.MoveTowards(lookAheadPos, Vector3.zero, Time.deltaTime * lookAheadReturnSpeed);
            }

            Vector3 aheadTargetPos = targetPos + lookAheadPos + Vector3.forward * offsetZ;
            aheadTargetPos.y = 0f;
            //Vector3 newPos = Vector3.SmoothDamp(transform.position, aheadTargetPos, ref currentVelocity, damping);
            Vector3 newPos = Vector3.SmoothDamp(
                new Vector3(transform.position.x, 0f, -10f),
                new Vector3(aheadTargetPos.x, 0f, -10f),
                ref currentVelocity, damping);

            transform.position = newPos;
            lastTargetPosition = targetPos;
        }
    }
}