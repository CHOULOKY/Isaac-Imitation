using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    public UIManager uiManager;
    public IsaacTearFactory isaacTearFactory;

    private void Awake()
    {
        instance = this;

        uiManager = GetComponentInChildren<UIManager>();
        isaacTearFactory = GetComponentInChildren<IsaacTearFactory>();
    }

    public static GameManager Instance
    {
        get {
            if (instance == null) return null;
            return instance;
        }
    }

    private void Start()
    {
        GameStart();
    }

    public void GameStart()
    {
        uiManager.GameStart();
    }
}
