using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 動く床（適当）
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlatformMover : MonoBehaviour, IMoveEffect
{
    [SerializeField] private Transform  _target;
    [SerializeField] private float      _moveSec;
    [SerializeField] private float      _waitSec;

    private Rigidbody2D _body;
    private Vector2     _beginPos, _endPos;
    private float       _sec;
    private bool        _isMove;
    private Vector2     _velocity;

    public Vector2 effect => _velocity;

    void Awake()
    {
        _body = gameObject.GetComponent<Rigidbody2D>();

        _sec = 0.0f;
        _isMove = true;
        _beginPos = transform.position;
        _endPos   = _target.position;
    }

    void FixedUpdate()
    {
        if(_isMove)
        {
            Move();
        }
        else
        {
            Wait();
        }
    }

    /// <summary>
    /// 移動
    /// </summary>
    private void Move()
    {
        _sec = Mathf.Min(_sec + Time.fixedDeltaTime, _moveSec);

        var t = EasingFunction.EaseInOutCubic(0.0f, 1.0f, _sec / _moveSec);
        var pre = _body.position;
        var pos = Vector2.Lerp(_beginPos, _endPos, t);
        _body.MovePosition(pos);

        _velocity = pos - pre;

        if(_sec >= _moveSec)
        {
            _sec = 0.0f;
            var tmp = _beginPos;
            _beginPos = _endPos;
            _endPos = tmp;
            _isMove = false;
        }
    }

    /// <summary>
    /// 待ち
    /// </summary>
    private void Wait()
    {
        _sec += Time.fixedDeltaTime;
        if(_sec >= _waitSec)
        {
            _sec = 0.0f;
            _isMove = true;
        }
    }
}
