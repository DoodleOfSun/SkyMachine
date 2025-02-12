using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    }

    // Update is called once per frame
    void Update()
    {
        AdjustingBGMVolume();
        AdjustingSFXVolume();
        PlayingBGM();
    }

    private void PlayingBGM()
    {
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
        bgmSource.volume = bgmSlider.value;
    }

    private void AdjustingSFXVolume()
    {
        sfxSource.volume = sfxSlider.value;
        sfxBulletPro1.volume = sfxSlider.value;
        sfxBulletPro2.volume = sfxSlider.value;
        sfxBulletPro3.volume = sfxSlider.value;
    }
}
