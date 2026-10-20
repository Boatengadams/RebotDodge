using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameObject block;
    public float max;
    public Transform spawnPoint;
    public float SpawnRate;

    bool gameStarted = false;
    bool gameOver = false;

    public GameObject tapText;
    public TextMeshProUGUI scoreText;
    public AudioClip startMusicClip;
    public AudioClip readySound;
    public AudioClip gameOverSound;
    int score = 0;

    private AudioSource startMusic;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        score = 0;
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }

        if (startMusicClip == null)
        {
            startMusicClip = Resources.Load<AudioClip>("Audio/Fameye GenZ");
        }

        if (gameOverSound == null)
        {
            gameOverSound = Resources.Load<AudioClip>("Audio/ROBOT_DODGE!");
        }

        if (readySound == null)
        {
            readySound = Resources.Load<AudioClip>("Audio/ROBOT READY");
        }

        if (tapText == null)
        {
            Debug.LogError("Tap To Start is not assigned on GameManager.", this);
        }

        if (startMusicClip == null)
        {
            Debug.LogError("Could not load Fameye GenZ.mp3. It must be assigned or stored at Assets/Resources/Audio/Fameye GenZ.mp3.", this);
        }

        // Keep the music source on the always-active GameManager. The prompt
        // is disabled when play starts, so placing the source there can make
        // playback stop with the UI object.
        startMusic = GetComponent<AudioSource>();
        if (startMusic == null)
        {
            startMusic = gameObject.AddComponent<AudioSource>();
        }

        startMusic.clip = startMusicClip;
        startMusic.loop = true;
        startMusic.playOnAwake = false;
        startMusic.spatialBlend = 0f;
        startMusic.volume = 1f;
        startMusic.mute = false;

    }

    void Update()
    {
        Pointer pointer = Pointer.current;
        if (pointer != null && pointer.press.wasPressedThisFrame)
        {
            if (gameOver)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            else if (!gameStarted)
            {
                gameStarted = true;
                if (readySound != null && startMusic != null)
                {
                    startMusic.PlayOneShot(readySound);
                }
                else if (readySound == null)
                {
                    Debug.LogError("Could not load ROBOT READY.mp3 from Assets/Resources/Audio.", this);
                }

                if (tapText != null)
                {
                    tapText.SetActive(false);
                }

                StartCoroutine(BeginGameplayAfterReady());
            }
        }
        
    }

    private void StartSpawning(){
        InvokeRepeating("SpawnBlock", 0.5f, SpawnRate);
    }

    private IEnumerator BeginGameplayAfterReady()
    {
        if (readySound != null)
        {
            yield return new WaitForSecondsRealtime(readySound.length);
        }

        if (gameOver)
        {
            yield break;
        }

        StartSpawning();
        PlayGameplayMusic();
    }

    private void SpawnBlock(){

        Vector3 spawnPos = spawnPoint.position;
        spawnPos.x = Random.Range(-max, max);
        Instantiate(block, spawnPos, Quaternion.identity);

        score ++;
        scoreText.text = score.ToString();

    }

    public void PlayGameplayMusic()
    {
        if (startMusic == null || startMusicClip == null)
        {
            return;
        }

        if (startMusicClip.loadState == AudioDataLoadState.Unloaded)
        {
            startMusicClip.LoadAudioData();
        }

        startMusic.time = 0f;
        startMusic.Play();
    }

    public void StopGameplayMusic()
    {
        if (startMusic != null)
        {
            startMusic.Stop();
        }
    }

    public void EndGame()
    {
        if (gameOver)
        {
            return;
        }

        gameOver = true;
        CancelInvoke(nameof(SpawnBlock));
        StopGameplayMusic();

        if (gameOverSound != null && startMusic != null)
        {
            startMusic.PlayOneShot(gameOverSound);
        }
        else if (gameOverSound == null)
        {
            Debug.LogError("Could not load ROBOT_DODGE!.mp3 from Assets/Resources/Audio.", this);
        }

        if (scoreText != null)
        {
            scoreText.text = $"<b><color=#FF5C7A>GAME OVER</color></b>\n<size=65%>FINAL SCORE</size>\n<size=140%><color=#FFE66D>{score}</color></size>\n<size=55%><color=#56F6FF>TAP TO RETRY</color></size>";
            scoreText.fontSize = 100f;
            scoreText.alignment = TextAlignmentOptions.Center;
            scoreText.rectTransform.anchoredPosition = Vector2.zero;
            scoreText.rectTransform.sizeDelta = new Vector2(1000f, 800f);
            scoreText.transform.SetAsLastSibling();
            scoreText.alpha = 0f;
            scoreText.transform.localScale = Vector3.one * 0.94f;
            StartCoroutine(AnimateGameOver(scoreText));
        }
    }

    private IEnumerator AnimateGameOver(TextMeshProUGUI text)
    {
        const float duration = 0.24f;
        float elapsed = 0f;
        Transform textTransform = text.transform;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            float eased = 1f - Mathf.Pow(1f - progress, 3f);
            text.alpha = eased;
            textTransform.localScale = Vector3.one * Mathf.Lerp(0.94f, 1f, eased);
            yield return null;
        }

        text.alpha = 1f;
        textTransform.localScale = Vector3.one;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
