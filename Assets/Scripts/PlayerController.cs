using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lib.State;

/// <summary>
/// プレイヤーの入力情報
/// </summary>
public class PlayerInputInfo
{
    public bool jump;
    public int  move;
}

/// <summary>
/// プレイヤー
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    private const float JUMP_GRAVITY    = 32.0f;
    private const float FALL_GRAVITY    = 32.0f;
    private const float JUMP_SPEED      = 16.0f;
    private const float MOVE_SPEED      = 8.0f;
    private const float FALL_SPEED_MAX  = 16.0f;
    private const float COYOTE_SEC      = 0.3f;     // コヨーテタイム

    [SerializeField] private PlayerView      _view;
    [SerializeField] private ContactFilter2D _filterFloor;      // 床判定
    [SerializeField] private ContactFilter2D _filterCeiling;    // 天井判定

    private Rigidbody2D         _body;
    private FiniteStateMachine  _state;
    private PlayerInputInfo     _inputInfo = new PlayerInputInfo();
    private float               _speedX, _speedY;
    private float               _coyoteSec;
    private bool                _isJumpOK = false;

    void Awake()
    {
        _body = gameObject.GetComponent<Rigidbody2D>();
        _body.centerOfMass = Vector2.zero;
        _state = new FiniteStateMachine(State_Ground);
    }

    void Update()
    {
        // とりあえず入力取る
        _isJumpOK = _isJumpOK || Input.GetKey(KeyCode.Space) == false;
        _inputInfo.jump = Input.GetKey(KeyCode.Space) && _isJumpOK;

        _inputInfo.move = 0;
        if(Input.GetKey(KeyCode.LeftArrow))
        {
            _inputInfo.move = -1;
        }
        else if(Input.GetKey(KeyCode.RightArrow))
        {
            _inputInfo.move = 1;
        }
    }

    void FixedUpdate()
    {
        _state.Update();
    }

    /// <summary>
    /// 地上
    /// </summary>
    private void State_Ground(FiniteStateMachine.Case c)
    {
        switch(c)
        {
            case FiniteStateMachine.Case.Enter:
            {
                _coyoteSec = 0.0f;
                _isJumpOK = false;
            }
            break;
            case FiniteStateMachine.Case.Exec:
            {
                var dt  = Time.fixedDeltaTime;
                var pos = _body.position;
                var dir = Vector2.right;    // 移動方向
                var ofs = Vector2.zero;     // 動く床等々の移動量

                _speedX = MOVE_SPEED * _inputInfo.move;
                _speedY -= FALL_GRAVITY * dt;
                _speedY = Mathf.Max(_speedY, -FALL_SPEED_MAX);

                // 地面
                {
                    var hitList = new List<ContactPoint2D>();
                    var hitNum  = _body.GetContacts(_filterFloor, hitList);
                    if(hitNum > 0)
                    {
                        var n = Vector2.zero;
                        for(int i = 0; i < hitNum; i++)
                        {
                            // 地面の法線は平均を取る
                            n = hitList[i].normal;
                        }
                        n.Normalize();

                        dir = new Vector2(n.y, -n.x);   // 地面の接線方向に移動する
                        _speedY = 0.0f;                 // 重力加速を止める

                        // 床が移動していたら速度を加算する
                        for(int i = 0; i < hitNum; i++)
                        {
                            var mover = hitList[i].rigidbody.GetComponent<IMoveEffect>();
                            if(mover == null) { continue; }
                            ofs += mover.effect;
                            break;  // GetContactsは同じコライダの組み合わせで複数の衝突を返すので、一旦最初のエフェクトだけ適用する…
                        }

                        _coyoteSec = COYOTE_SEC;
                    }
                }

                // 壁
                {
                    var hitList = new List<ContactPoint2D>();
                    var hitNum  = _body.GetContacts(hitList);
                    if(hitNum > 0)
                    {
                        var hit = hitList[0];
                        var n = hit.normal;
                        if(Vector2.Dot(n, dir * _speedX) < -0.9f) // 適当な角度の壁にぶつかった
                        {
                            // 壁にぶつかっているので速度をゼロに変更
                            _speedX = 0.0f;
                        }
                    }
                }

                // ジャンプ
                _coyoteSec -= dt;
                if(_coyoteSec >= 0.0f && _inputInfo.jump)
                {
                    _state.ChangeState(State_Jump);
                }

                var vel = new Vector2(0.0f, _speedY) + (dir * _speedX);
                _body.MovePosition(pos + (vel * dt) + ofs);
                _view.SetDirection(_speedX);
            }
            break;
            case FiniteStateMachine.Case.Exit:
            {

            }
            break;
        }
    }

    /// <summary>
    /// ジャンプ中
    /// </summary>
    private void State_Jump(FiniteStateMachine.Case c)
    {
        switch(c)
        {
            case FiniteStateMachine.Case.Enter:
            {
                _speedY = JUMP_SPEED;   // ジャンプの初速
                _isJumpOK = true;
            }
            break;
            case FiniteStateMachine.Case.Exec:
            {
                float dt = Time.fixedDeltaTime;

                _speedX = _inputInfo.move * MOVE_SPEED;
                _speedY -= JUMP_GRAVITY * dt;

                // 下降開始
                if(_speedY <= 0.0f)
                {
                    _state.ChangeState(State_Ground);
                }

                // 天井 & ジャンプキャンセル
                if(_body.IsTouching(_filterCeiling) || _inputInfo.jump == false)
                {
                    _speedY = 0.0f;
                    _state.ChangeState(State_Ground);
                }

                var vel = new Vector2(_speedX, _speedY);
                _body.MovePosition(_body.position + vel * dt);
                _view.SetDirection(_speedX);
            }
            break;
            case FiniteStateMachine.Case.Exit:
            {

            }
            break;
        }
    }
}
