using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private AudioSource _backgroundMusic;
    [SerializeField] private AudioSource _rickrollMusic;
    [SerializeField] private Image[] _rickrollImages;
    [Tooltip("Song length in seconds (AudioSource.clip.length doesn't seem to match song length though?)")]
    [SerializeField] [MinAttribute(0)] private float _songLength = 3.33f;
    [Tooltip("Time in seconds for final image to fade back to original main menu background image (this is subtracted from Song Length)")]
    [SerializeField] [MinAttribute(0)] private float _songEndFadeLength = 0.14f;

    private bool _isRickrolling = false;
    private Coroutine _rickrollCoroutine;
    private int _rickrollImagesSize;
    private float MAX_COLOR_THING = 255F; // yenno from 0 - 255 ye but for some reason the alpha channel is 0 - 1?!?!? hello?!?!?!????? 

    private void Awake()
    {
        _rickrollImagesSize = _rickrollImages.Length;

        if (_songEndFadeLength >= _songLength)
        {
            throw new System.ArgumentException($"MainMenu Error: Song Length ({_songLength}) needs to be greater than Song End Fade Length ({_songEndFadeLength}).");
        }

        foreach (Image img in _rickrollImages)
        {
            if (img == null)
            {
                throw new System.NullReferenceException("MainMenu Error: None/null image(s) under Rickroll Images.");
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.R) && _rickrollImages.Length != 0)
        {
            if (_isRickrolling)
            {
                _backgroundMusic.UnPause();
                _rickrollMusic.Stop();
                _isRickrolling = false;
                StopCoroutine(_rickrollCoroutine);
                SetPotatAlpha(0);
            }
            else
            {
                _backgroundMusic.Pause();
                _rickrollMusic.Play();
                _isRickrolling = true;
                _rickrollCoroutine = StartCoroutine(RickrollCoroutine());
            }
        }
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("Level");
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void MainMenuBtn()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void Quit()
    {
        Debug.Log("Quit");
        Application.Quit(); // Won't work in editor
    }

    private IEnumerator RickrollCoroutine()
    {
        float TotalTime = (_songLength - _songEndFadeLength) * 60 / _rickrollImagesSize;
        float ElapsedTime = 0;
        float LerpValue = 0;

        for (int i = 0; i < _rickrollImagesSize; i++)
        {
            while (ElapsedTime < TotalTime)
            {
                LerpValue = Mathf.Lerp(0, 1, ElapsedTime / TotalTime);
                _rickrollImages[i].color = new Color(MAX_COLOR_THING, MAX_COLOR_THING, MAX_COLOR_THING, LerpValue);
                ElapsedTime += Time.deltaTime;
                yield return null;  
            }

            _rickrollImages[i].color = new Color(MAX_COLOR_THING, MAX_COLOR_THING, MAX_COLOR_THING, 1);
            ElapsedTime = 0;
        }

        TotalTime = _songEndFadeLength * 60;
        ElapsedTime = 0;
        LerpValue = 0;

        while (ElapsedTime < TotalTime)
        {
            LerpValue = Mathf.Lerp(1, 0, ElapsedTime / TotalTime);
            SetPotatAlpha(LerpValue);
            ElapsedTime += Time.deltaTime;
            yield return null;
        }

        SetPotatAlpha(0);
        _isRickrolling = false;
        _backgroundMusic.UnPause();
    }

    private void SetPotatAlpha(float a)
    {
        for (int i = 0; i < _rickrollImagesSize; i++)
        {
            _rickrollImages[i].color = new Color(MAX_COLOR_THING, MAX_COLOR_THING, MAX_COLOR_THING, a);
        }
    }
}
