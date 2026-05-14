using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BearColorChallengeManager : MonoBehaviour
{
    public GameObject Sprint;
    [Header("Pillows")]
    public ColorPillow[] pillows;

    [Header("Bear Eye Lights")]
    public Light[] bearEyeLights;

    [Header("UI")]
    public GameObject challengeUI;
    public TMP_Text progressText;
    public TMP_Text timerText;
    public TMP_Text colorText;

    [Header("Sounds")]
    public AudioSource audioSource;
    public AudioClip tickSound;
    public AudioClip correctSound;
    public AudioClip wrongSound;

    [Header("Settings")]
    public int totalRounds = 5;
    public float answerTime = 5f;
    public float successLightTime = 2f;
    public float failLightTime = 1f;

    [Header("Game Colors")]
    public Color pinkColor = Color.magenta;
    public Color blueColor = Color.blue;
    public Color orangeColor = new Color(1f, 0.5f, 0f);
    public Color whiteColor = Color.white;
    public Color yellowColor = Color.yellow;

    [Header("Result Colors")]
    public Color greenSuccess = Color.green;
    public Color redFail = Color.red;

    private List<string> availableColors = new List<string>();

    private string targetColor;
    private int currentRound = 0;
    private float currentTimer;

    private bool challengeActive = false;
    private bool canAnswer = false;

    private int lastSecond;

    void Start()
    {
        if (challengeUI != null)
            challengeUI.SetActive(false);

        availableColors.Clear();

        foreach (ColorPillow pillow in pillows)
        {
            if (!availableColors.Contains(pillow.pillowColor))
            {
                availableColors.Add(pillow.pillowColor);
            }
        }

        TurnEyes(false);
        UpdateUI();
    }

    void Update()
    {
        if (!challengeActive) return;
        if (!canAnswer) return;

        currentTimer -= Time.deltaTime;

        int currentSecond = Mathf.CeilToInt(currentTimer);

        if (timerText != null)
        {
            timerText.text = "Time: " + currentSecond.ToString();
        }

        if (currentSecond != lastSecond && currentSecond > 0)
        {
            PlaySound(tickSound);
            lastSecond = currentSecond;
        }

        if (currentTimer <= 0)
        {
            StartCoroutine(WrongAnswer());
        }
    }

    public void StartChallenge()
    {
        if (challengeActive) return;

        challengeActive = true;
        canAnswer = true;
        currentRound = 0;

        if (challengeUI != null)
            challengeUI.SetActive(true);

        TurnEyes(true);

        UpdateUI();
        PickNewColor();
    }

    void PickNewColor()
    {
        targetColor = availableColors[Random.Range(0, availableColors.Count)];

        currentTimer = answerTime;
        lastSecond = Mathf.CeilToInt(currentTimer);

        SetBearEyeColor(targetColor);

        if (colorText != null)
            colorText.text = "Color: " + targetColor;
    }

    public void CheckPillow(string pillowColor)
    {
        if (!challengeActive) return;
        if (!canAnswer) return;

        if (pillowColor == targetColor)
        {
            StartCoroutine(CorrectAnswer());
        }
        else
        {
            StartCoroutine(WrongAnswer());
        }
    }

    IEnumerator CorrectAnswer()
    {
        canAnswer = false;

        currentRound++;
        UpdateUI();

        SetBearEyeColor("Success");

        PlaySound(correctSound);

        if (colorText != null)
            colorText.text = "Correct!";

        if (timerText != null)
            timerText.text = "";

        yield return new WaitForSeconds(successLightTime);

        if (currentRound >= totalRounds)
        {
            FinishChallenge();
        }
        else
        {
            canAnswer = true;
            PickNewColor();
        }
    }

    IEnumerator WrongAnswer()
    {
        canAnswer = false;

        currentRound = 0;
        UpdateUI();

        SetBearEyeColor("Fail");

        PlaySound(wrongSound);

        if (colorText != null)
            colorText.text = "Wrong!";

        if (timerText != null)
            timerText.text = "";

        yield return new WaitForSeconds(failLightTime);

        canAnswer = true;
        PickNewColor();
    }

    void FinishChallenge()
    {
        challengeActive = false;
        canAnswer = false;

        if (progressText != null)
            progressText.text = "Completed!";

        if (timerText != null)
            timerText.text = "";

        if (colorText != null)
            colorText.text = "";

        if (audioSource != null)
            audioSource.Stop();

        Sprint.SetActive(true);
    }

    void UpdateUI()
    {
        if (progressText != null)
        {
            progressText.text = currentRound + " / " + totalRounds;
        }
    }

    void TurnEyes(bool state)
    {
        foreach (Light eye in bearEyeLights)
        {
            if (eye != null)
                eye.enabled = state;
        }
    }

    void SetBearEyeColor(string colorName)
    {
        Color finalColor = Color.white;

        if (colorName == "Pink")
            finalColor = pinkColor;

        else if (colorName == "Blue")
            finalColor = blueColor;

        else if (colorName == "Orange")
            finalColor = orangeColor;

        else if (colorName == "White")
            finalColor = whiteColor;

        else if (colorName == "Yellow")
            finalColor = yellowColor;

        else if (colorName == "Success")
            finalColor = greenSuccess;

        else if (colorName == "Fail")
            finalColor = redFail;

        foreach (Light eye in bearEyeLights)
        {
            if (eye != null)
                eye.color = finalColor;
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}