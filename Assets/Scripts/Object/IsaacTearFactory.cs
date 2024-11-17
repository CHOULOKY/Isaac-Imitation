using System.Collections.Generic;
using UnityEngine;

public class IsaacTearFactory : TearFactory
{
    protected override void Awake()
    {
        base.Awake();
        Debug.Log("IsaacTearFactory Initialized");

        for (int i = 0; i < tearList.Count; i++)
        {
            Debug.Log($"Tear Pool Created: {tearList[i].name}, Size: {poolSize[i]}");
        }
    }

    public override GameObject GetTear(Tears _type, bool _setActive = true)
    {
        GameObject selected = base.GetTear(_type, _setActive);
        Debug.Log($"Tear Retrieved from Pool: {_type}, Active: {selected.activeSelf}");
        return selected;
    }
}
