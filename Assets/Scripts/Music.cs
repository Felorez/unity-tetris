using UnityEngine;

public class  Music : MonoBehaviour
{
    public AudioSource clickSound;
    public AudioSource rotateSound;
    public AudioSource dropSound;
    public AudioSource lineClearSound;
    public AudioSource gameOverSound;
    public AudioSource backgroundMusic;
    public AudioSource RestartSound;

    void Start()
    {
        backgroundMusic.Play();
        OnEnable();
    }

    void OnEnable()
    {
        GameProcess.OnGameOver += PlayGameOverSound;
        GameProcess.OnLineClear += PlayLineClearSound;
        GameProcess.OnDrop += PlayDropSound;
        GameProcess.OnRotate += PlayRotateSound;
        GameProcess.OnClick += PlayClickSound;
        GameProcess.OnRestart += PlayRestartSound;
    }

    void OnDisable()
    {
        GameProcess.OnGameOver -= PlayGameOverSound;
        GameProcess.OnLineClear -= PlayLineClearSound;
        GameProcess.OnDrop -= PlayDropSound;
        GameProcess.OnRotate -= PlayRotateSound;
        GameProcess.OnClick -= PlayClickSound;
        GameProcess.OnRestart -= PlayRestartSound;
    }

    void PlayGameOverSound()
    {
        gameOverSound.Play();
    }

    void PlayLineClearSound()
    {
        lineClearSound.Play();
    }

    void PlayDropSound()
    {
        dropSound.Play();
    }

    void PlayRotateSound()
    {
        rotateSound.Play();
    }

    void PlayClickSound()
    {
        clickSound.Play();
    }

    void PlayRestartSound()
    {
        RestartSound.Play();
        OnDisable();
    }
}
