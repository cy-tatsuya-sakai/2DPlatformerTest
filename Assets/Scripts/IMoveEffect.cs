using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 移動に影響するやつ。動く床用
/// </summary>
public interface IMoveEffect
{
    public Vector2 effect { get; }
}
