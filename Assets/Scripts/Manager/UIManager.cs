using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    private FadeController fadeController;

    public float fadeDuration = 2;

    private void Awake()
    {
        fadeController = GetComponentInChildren<FadeController>();
    }

    public void GameStart(float _fadeDuration = -1)
    {
        _fadeDuration = _fadeDuration <= 0 ? fadeDuration : _fadeDuration;

        StartCoroutine(fadeController.FadeOutCoroutine(null, fadeDuration));
    }
}
