using BulletPro;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class ParrySystem : MonoBehaviour
{
    private float vignetteFadingTransition = 3;      // 비네트 페이드인 시 효과 최대치 1f까지 걸리는 시간
    private float vignetteFadeOutTransition = 6;     // 비네트 페이드아웃 시 효과 최소치 0f까지 걸리는 시간
    private float maxVignette = 0.35f;                   // 최대 비네트 값
    private float radius = 1.3f;
    public Transform bulletPool;                // 총알 오브젝트 풀. 5000개

    private PostProcessVolume darknessVolume;   // 객체의 피사체심도 볼륨 컴포넌트
    private Vignette vignette;

    public GameObject parryVFX;     // 패리 효과 오브젝트

    void Start()
    {
        darknessVolume = GetComponent<PostProcessVolume>();
        darknessVolume.profile.TryGetSettings(out vignette);
        parryVFX = Instantiate(parryVFX);
        parryVFX.SetActive(false);
    }

    void FixedUpdate()
    {
        CursorControl(GameManager.instance.worldMousePos);
        Parry();
    }

    // 해당 좌표로 마우스커서 오브젝트 이동, FixedUpdate
    private void CursorControl(Vector3 pos)
    {
        VignetteControl();
        Vector3 nextPos = Vector3.MoveTowards(transform.position, new Vector3(pos.x, pos.y, 0f), Mathf.Infinity);
        transform.position = nextPos;
    }

    private void VignetteControl()
    {
        // isParryMode가 true일 때 Intensity 값을 서서히 1로 증가
        // 조건 추가, 쿨타임 동안 이하 로직이 발동하지 않음.
        if (Player.instance.isParryAiming && Player.instance.parryCoroutine == null)
        {
            vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, maxVignette, vignetteFadingTransition * Time.fixedDeltaTime);
        }
        // isParryMode가 false일 때 Intensity 값을 서서히 0으로 감소
        else
        {
            vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, 0f, vignetteFadeOutTransition * Time.fixedDeltaTime);
        }

        Vector3 screenPos = Camera.main.WorldToViewportPoint(transform.position);
        vignette.center.value = screenPos;
    }

    private void Parry()
    {
        if (Player.instance.isReadyToParry)
        {

            StartCoroutine(ParryVFX());
            Debug.Log("패링");
            InspectAllBullets(bulletPool);
            Player.instance.isReadyToParry = false;
        }
    }

    private IEnumerator ParryVFX()
    {
        parryVFX.SetActive(true);
        parryVFX.transform.position = this.transform.position;
        yield return new WaitForSeconds(0.5f);
        parryVFX.SetActive(false);
    }


    private void InspectAllBullets(Transform parent)
    {
        // 부모 객체에 적용할 검사 및 실질적인 탄막 처리 로직
        DestroyBulletsInZone(parent);

        // 부모 객체의 모든 자식 객체를 재귀적으로 탐색
        foreach (Transform child in parent)
        {
            InspectAllBullets(child);
        }
    }

    // 패리 구역에 존재하는 탄막을 제거
    private void DestroyBulletsInZone(Transform bullet)
    {
        Vector2 center = transform.position;
        if (bullet.gameObject.activeSelf && bullet.GetComponent<SpriteRenderer>() != null && bullet.GetComponent<SpriteRenderer>().enabled)
        {
            // 총알이 마법진 안에 들어가 있을 때
            if (Vector2.Distance(center, bullet.transform.position) <= radius)
            {
                GameManager.instance.parriedBulletScore++;
                //bullet.gameObject.SetActive(false);
                Bullet bulletScript = bullet.gameObject.GetComponent<Bullet>();
                bulletScript.Die(bullet.gameObject.transform.position);
                Player.instance.EtherFluctuation(Player.instance.parryEtherGaineValue);
            }
        }
    }
}
