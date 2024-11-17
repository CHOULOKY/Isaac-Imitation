using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    public IsaacTearFactory isaacTearFactory;
    public MonsterTearFactory monsterTearFactory;


    private void Awake()
    {
        instance = this;

        isaacTearFactory = GetComponentInChildren<IsaacTearFactory>();
        monsterTearFactory = GetComponentInChildren<MonsterTearFactory>();

    }

    public static GameManager Instance
    {
        get
        {
            if (instance == null) return null;
            return instance;
        }
    }

    public void GameOver()
    {
        SceneManager.LoadScene(0);
    }
}
