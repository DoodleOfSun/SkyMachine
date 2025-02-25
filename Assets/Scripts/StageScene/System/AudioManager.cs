using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[System.Serializable]
public class AudioClipEntry
{
    public string key;
    public AudioClip clip;
}


public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    public Slider bgmSlider;    // 배경음 슬라이더
    public Slider sfxSlider;    // 효과음 슬라이더

    public AudioSource bgmSource;  // 배경음 오디오 소스
    public AudioSource sfxSource;  // 효과음 오디오 소스

    // 에셋에서 지원하는 오디오 소스
    // 효과음에 속한다.
    public AudioSource sfxBulletPro1;
    public AudioSource sfxBulletPro2;
    public AudioSource sfxBulletPro3;

    public List<AudioClipEntry> bgmList = new List<AudioClipEntry>();
    public List<AudioClipEntry> sfxList = new List<AudioClipEntry>();

    private Dictionary<string, AudioClip> bgmDictionary = new Dictionary<string, AudioClip>();  // 스테이지별 배경음 딕셔너리
    private Dictionary<string, AudioClip> sfxDictionary = new Dictionary<string, AudioClip>();  // 스테이지별 효과음 딕셔너리

    private Coroutine bgmDelayCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        // 리스트를 딕셔너리로 변환
        foreach (var entry in bgmList)
        {
            bgmDictionary[entry.key] = entry.clip;
        }

        foreach (var entry in sfxList)
        {
            sfxDictionary[entry.key] = entry.clip;
        }

        // 이전 씬에서 저장해둔 값으로 초기화
        if (bgmSlider != null && sfxSlider != null)
        {
            bgmSlider.value = AudioValueSaver.instance.bgmValue;
            sfxSlider.value = AudioValueSaver.instance.sfxValue;
        }

        bgmSource.volume = AudioValueSaver.instance.bgmValue;
        sfxSource.volume = AudioValueSaver.instance.sfxValue;


        bgmDelayCoroutine = null;
    }

    // Update is called once per frame
    void Update()
    {
        CheckingBGMDelay();
        PlayingBGMStage();
        AdjustingBGMVolume();
        AdjustingSFXVolume();
        SavingAudioValue();
    }

    private void PlayingBGMStage()
    {



        if (SceneManager.GetActiveScene().name == "ScoreScene")
        {
            bgmSource.Pause();
            return;
        }
        // 컷신일때 재생 안하는 로직
        /*
        if (!CutsceneManager.instance.isCutscene)
        {
            // 1스테이지
            if (GameManager.instance.currentSceneName == "Stage1")
            {
                // 1스테이지 bgm
                if (WaveManager.instance.waveCount <= 8 && bgmSource.clip != bgmDictionary["Stage1"])
                {
                    bgmSource.clip = bgmDictionary["Stage1"];
                    bgmSource.Play();
                }

                // 1스테이지 보스 bgm
                else if (WaveManager.instance.waveCount >= 9 && bgmSource.clip != bgmDictionary["Stage1Boss"])
                {
                    bgmSource.clip = bgmDictionary["Stage1Boss"];
                    bgmSource.Play();
                }
            }
        }
        */

        // 컷신이어도 재생하는 로직
        // 스타트 컷씬
        if (SceneManager.GetActiveScene().name == "StartCutscene" || SceneManager.GetActiveScene().name == "MainScene")
        {
            if (bgmSource.clip != bgmDictionary["Moter"])
            {
                bgmSource.clip = bgmDictionary["Moter"];
                bgmSource.Play();
            }
        }

        else if (GameManager.instance.currentSceneName.Contains("Stage"))
        {
            // 1스테이지
            if (GameManager.instance.currentSceneName == "Stage1")
            {
                // 1스테이지 bgm
                if (WaveManager.instance.waveCount <= 8 && bgmSource.clip != bgmDictionary["Stage1"])
                {
                    bgmSource.clip = bgmDictionary["Stage1"];
                    bgmSource.Play();
                }

                // 1스테이지 보스 bgm
                else if (WaveManager.instance.waveCount >= 9 && bgmSource.clip != bgmDictionary["Stage1Boss"])
                {
                    bgmSource.clip = bgmDictionary["Stage1Boss"];
                    bgmSource.Play();
                }
            }
        }
    }


    private void CheckingBGMDelay()
    {
        // bgm 재생시간이 끝난 경우
        if (bgmSource.clip != null && bgmSource.time >= bgmSource.clip.length - 0.7f)
        {
            if (bgmDelayCoroutine == null)
            {
                Debug.Log("bgm 딜레이 코루틴 발동");
                bgmDelayCoroutine = StartCoroutine(DelayingBGM());
            }
        }

        /*
        else if(bgmSource.clip != null && bgmSource.time < bgmSource.clip.length)
        {
            Debug.Log("현재 시간 " + bgmSource.time);
        }*/

    }
    
    private IEnumerator DelayingBGM()
    {
        bgmSource.Pause();
        yield return new WaitForSeconds(2f);
        bgmSource.time = 0f;
        bgmSource.Play();
        bgmDelayCoroutine = null;
    }

    public void PlayingSFX(string sfxName)
    {
        if (sfxDictionary.ContainsKey(sfxName))
        {
            sfxSource.clip = sfxDictionary[sfxName];  // 딕셔너리에서 SFX 찾기
            sfxSource.Play();  // SFX 재생
        }
        else
        {
            Debug.LogWarning("효과음이 없습니다: " + sfxName);
        }
    }

    private void AdjustingBGMVolume()
    {
        if (bgmSlider != null)
        {
            bgmSource.volume = bgmSlider.value;
        }
    }

    private void AdjustingSFXVolume()
    {
        if (sfxSlider != null)
        {
            sfxSource.volume = sfxSlider.value;
            if (sfxBulletPro1 != null && sfxBulletPro2 != null && sfxBulletPro3 != null)
            {
                sfxBulletPro1.volume = sfxSlider.value;
                sfxBulletPro2.volume = sfxSlider.value;
                sfxBulletPro3.volume = sfxSlider.value;
            }
        }
    }

    private void SavingAudioValue()
    {
        AudioValueSaver.instance.bgmValue = bgmSource.volume;
        AudioValueSaver.instance.sfxValue = sfxSource.volume;
    }
}