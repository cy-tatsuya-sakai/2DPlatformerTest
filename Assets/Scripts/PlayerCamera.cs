using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// とりあえずX方向追従
/// </summary>
public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private Camera     _cam;
    [SerializeField] private Transform  _target;

    void LateUpdate()
    {
        var pos = _cam.transform.position;
        pos.x = _target.position.x;
        _cam.transform.position = pos;
    }
}
