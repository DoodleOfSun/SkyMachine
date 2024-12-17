using JetBrains.Annotations;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace UnityStandardAssets._2D
{
    public class Camera2DFollow : MonoBehaviour
    {
        public float damping = 1;
        public float lookAheadFactor = 3;
        public float lookAheadReturnSpeed = 0.5f;
        public float lookAheadMoveThreshold = 0.1f;
        public Transform target;

        private float offsetZ;
        private Vector3 lastTargetPosition;
        private Vector3 currentVelocity;
        private Vector3 lookAheadPos;
        private Vector3 previousPosition;

        // 처음에는 플레이어를 카메라가 따라가야 하므로 다음과 같이 초기화한다.
        private void Start()
        {
            InitCam(target.transform.position);
        }


        // Update is called once per frame
        private void Update()
        {
            ChasingByTransform(target.transform.position);
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

        /*
        private void Zooming(Transform target)
        {
            Vector3 velocity = player.position - previousPosition;

            // 타겟과 플레이어 사이의 방향 벡터 계산
            Vector3 directionToTarget = target.position - player.position;

            // 두 벡터의 내적을 계산하여 가까워지고 있는지 판단
            float dotProduct = Vector3.Dot(velocity.normalized, directionToTarget.normalized);
            //Debug.Log(dotProduct);

            if (dotProduct > 0)
            {
                Camera.main.orthographicSize = Mathf.Max(minCameraSize, Camera.main.orthographicSize - zoomingEachFrame * Time.deltaTime);
            }
            else if (dotProduct < 0)
            {
                Camera.main.orthographicSize = Mathf.Min(maxCameraSize, Camera.main.orthographicSize + zoomingEachFrame * Time.deltaTime);
            }

            // 플레이어의 현재 위치를 다음 프레임에서 이전 위치로 사용하기 위해 저장
            previousPosition = player.position;
        }
        
        private void ZoomingWhenNotLockOn()
        {
            Camera.main.orthographicSize = Mathf.Max(minCameraSize, Camera.main.orthographicSize - zoomingEachFrame * Time.deltaTime);
        }
        */
    }
}