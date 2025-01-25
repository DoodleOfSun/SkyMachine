using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class ScoreManager : MonoBehaviour
{
    public Text parriedBullet;
    public Text enemyKilled;
    public Text remainHeart;
    public Text resultScore;
    public Text betaText;
    private float typingSpeed;
    private Coroutine typingCoroutine;
    private int typingOrder;

    // Start is called before the first frame update
    void Start()
    {
        typingSpeed = 0.1f;
        typingCoroutine = null;
        typingOrder = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (typingCoroutine == null && GameManager.instance != null)
        {
            if (typingOrder == 0)
            {
                parriedBullet.text = "Removed Bullet: " + GameManager.instance.parriedBulletScore;
                typingCoroutine = StartCoroutine(TypeText(parriedBullet.text, parriedBullet));
            }
            else if (typingOrder == 1)
            {
                enemyKilled.text = "Killed Enemy : " + GameManager.instance.killedEnemyScore;
                typingCoroutine = StartCoroutine(TypeText(enemyKilled.text, enemyKilled));
            }
            else if (typingOrder == 2)
            {
                remainHeart.text = "Remaining Health : " + GameManager.instance.playerHeartScore;
                typingCoroutine = StartCoroutine(TypeText(remainHeart.text, remainHeart));
            }
            else if (typingOrder == 3)
            {
                int result = (GameManager.instance.killedEnemyScore * 1000) + 
                             (GameManager.instance.playerHeartScore * 30000) + 
                             (GameManager.instance.parriedBulletScore * 100);
                resultScore.text = "Score : " + result;
                typingCoroutine = StartCoroutine(TypeText(resultScore.text, resultScore));
            }
            else if (typingOrder == 4)
            {
                betaText.text = "Stay tuned for updates!";
                typingCoroutine = StartCoroutine(TypeText(betaText.text, betaText));
            }
        }

        // 씬에서 직접 테스트용
        else if(typingCoroutine == null && GameManager.instance == null)
        {
            if (typingOrder == 0)
            {
                parriedBullet.text = "Removed Bullet: " + 1;
                typingCoroutine = StartCoroutine(TypeText(parriedBullet.text, parriedBullet));
            }
            else if (typingOrder == 1)
            {
                enemyKilled.text = "Killed Enemy : " + 1;
                typingCoroutine = StartCoroutine(TypeText(enemyKilled.text, enemyKilled));
            }
            else if (typingOrder == 2)
            {
                remainHeart.text = "Remaining Health : " + 1;
                typingCoroutine = StartCoroutine(TypeText(remainHeart.text, remainHeart));
            }
            else if (typingOrder == 3)
            {
                int result = (1 * 1000) +
                             (1 * 30000) +
                             (1 * 100);
                resultScore.text = "Score : " + result;
                typingCoroutine = StartCoroutine(TypeText(resultScore.text, resultScore));
            }
            else if (typingOrder == 4)
            {
                betaText.text = "Stay tuned for updates!";
                typingCoroutine = StartCoroutine(TypeText(betaText.text, betaText));
            }
        }
    }

    // 텍스트를 한 자 한 자 뜨게 만든다
    private IEnumerator TypeText(string fullText, Text text)
    {
        fullText = fullText + " ";
        text.text = "";  // 시작할 때 텍스트를 초기화

        foreach (char letter in fullText.ToCharArray())
        {
            text.text += letter;  // 한 글자씩 추가
            yield return new WaitForSeconds(typingSpeed);  // 글자 간 시간 간격
        }
        typingCoroutine = null;
        typingOrder++;
    }

    public void ReturnToMainScene()
    {
        TitleManager.targetScene = "MainScene";
        SceneManager.LoadScene("LoadingScene");
    }
}
