using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレイヤーの見た目
/// </summary>
public class PlayerView : MonoBehaviour
{
    [SerializeField] private Transform _view;
    [SerializeField] private Transform _face;

    public void SetDirection(float dir)
    {
        if(Mathf.Abs(dir) <= float.Epsilon) { return; }

        dir = Mathf.Sign(dir);
        _face.localPosition = new Vector3(dir * 0.1f, 0.0f, 0.0f);
    }
}
