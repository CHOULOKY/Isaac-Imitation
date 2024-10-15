using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TearFactory : MonoBehaviour
{
    public enum Tears { Basic }
    public abstract GameObject GetTear(Tears _type, bool _setActive = true);
}
