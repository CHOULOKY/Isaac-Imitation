using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Codes_player; // PlayerHead가 있는 네임스페이스 참조

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public PlayerHead playerhead; // PlayerHead 클래스 사용

    void Awake()
    {
        instance = this;
    }
}
