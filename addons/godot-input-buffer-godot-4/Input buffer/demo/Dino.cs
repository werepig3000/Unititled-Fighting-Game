using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// The game's brave protagonist.
/// </summary>
public class Dino : KinematicBody2D
{
    /// <summary>
    /// After doing the jump to begin the game, we emit this signal so other nodes in the scene can finish the intro 
    /// animation.
    /// </summary>
    [Signal] private delegate void IntroJumpFinished();
    /// <summary> Emitted after hitting an obstacle </summary>
    [Signal] private delegate void GotHit();

    private enum DinoState
    {
        Idle,
        IntroAnimation,
        Grounded,
        Jumping,
        Ducking,
        Dead
    }

    private static readonly string JUMP_ACTION = "ui_accept";
    private static readonly string DUCK_ACTION = "ui_down";
    private static readonly string RUN_ANIMATION = "Run";

    private StateMachine<DinoState> _stateMachine;
    private AnimationPlayer _animator; [Export] private NodePath _animation_player_path = null;
    private CollisionShape2D _regularHitbox; [Export] private NodePath _regularHitboxPath = null;
    private CollisionShape2D _duckingHitbox; [Export] private NodePath _duckingHitboxPath = null;
    private AudioStreamPlayer _audio; [Export] private NodePath _audioPath = null;
    private Vector2 _velocity;
    private float _gravity;
    private bool _useBuffer = true;
    private float _initialY;

    /// <summary> How many pixels per second squared the dino accelerates towards the ground at normally. </summary>
    [Export] private float _regularGravity = 2400f;
    /// <summary>
    /// How many pixels per second squared the dino accelerates towards the ground at if the player releases the jump 
    /// button while rising.
    /// </summary>
    [Export] private float _shortHopGravity = 4800f;
    /// <summary>
    /// How many pixels per second squared the dino accelerates towards the ground at if the player presses and holds 
    /// the down button.
    /// </summary>
    [Export] private float _fastFallGravity = 9600f;
    /// <summary>
    /// Pixels per second downward the dino moves the moment it jumps.
    /// Recall that the coordinate system has the positive y axis point down, so this should be negative.
    /// </summary>
    [Export] private float _initialJumpSpeed = 800f;

    /// <summary>
    /// Called when the node enters the scene tree for the first time.
    /// </summary>
    public override void _Ready()
    {
        _animator = GetNode<AnimationPlayer>(_animation_player_path);
        _regularHitbox = GetNode<CollisionShape2D>(_regularHitboxPath);
        _duckingHitbox = GetNode<CollisionShape2D>(_duckingHitboxPath);
        _audio = GetNode<AudioStreamPlayer>(_audioPath);
        _initialY = Position.y;

        _stateMachine = new StateMachine<DinoState>(
            new Dictionary<DinoState, StateSpec>
            {
                { DinoState.Idle, new StateSpec(enter: IdleEnter, update: IdlePhysicsProcess) },
                { DinoState.IntroAnimation, new StateSpec(
                    enter: IntroAnimationEnter, update: IntroAnimationPhysicsProcess, exit: IntroAnimationExit) },
                { DinoState.Grounded, new StateSpec(enter: GroundedEnter, update: GroundedPhysicsProcess)},
                { DinoState.Jumping, new StateSpec(enter: JumpingEnter, update: JumpingPhysicsProcess)},
                { DinoState.Ducking, new StateSpec(
                    enter: DuckingEnter, update: DuckingPhysicsProcess, exit: DuckingExit) },
                { DinoState.Dead, new StateSpec(enter: Die)}
            },
            DinoState.Idle
        );

        _animator.Play("Idle + Jump");
    }

    /// <summary>
    /// Called during the physics processing step of the main loop.
    /// </summary>
    /// <param name="delta"> The elapsed time since the previous physics step. </param>
    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

        _stateMachine.Update(delta);
    }

    // Signal callbacks.
    private void _on_Regular_hitbox_area_entered(Area2D area) => OnObstacleHit();
    private void _on_Ducking_hitbox_area_entered(Area2D area) => OnObstacleHit();
    private void _on_Buffer_control_BufferToggled(bool on) => _useBuffer = on;
    private void _on_Retry_button_pressed()
    {
        Position = new Vector2(Position.x, _initialY);
        InputBuffer.InvalidateAction(JUMP_ACTION);
        _stateMachine.TransitionTo(DinoState.Grounded);
    }

    /// <summary>
    /// Called when colliding with an obstacle.
    /// </summary>
    /// <param name="area"> The obstacle's collider. </param>
    private void OnObstacleHit()
    {
        EmitSignal(nameof(GotHit), new object[0]);
        _stateMachine.TransitionTo(DinoState.Dead);
    }

    // Idle state callbacks.
    private void IdleEnter()
    {
        _animator.Play("Idle + Jump");
    }
    private void IdlePhysicsProcess(float delta)
    {
        bool jumping;
        if (_useBuffer)
        {
            jumping = InputBuffer.IsActionPressBuffered(JUMP_ACTION);
        }
        else
        {
            jumping = Input.IsActionJustPressed(JUMP_ACTION);
        }

        if (jumping)
        {
            _stateMachine.TransitionTo(DinoState.IntroAnimation);
        }
    }

    // Intro animation state callbacks.
    private void IntroAnimationEnter()
    {
        _velocity = _initialJumpSpeed * Vector2.Up;
        _gravity = _regularGravity;
        _audio.Play();
    }
    private void IntroAnimationPhysicsProcess(float delta)
    {
        // Move and detect collision with the ground.
        _velocity += _gravity * delta * Vector2.Down;
        MoveAndSlide(_velocity, Vector2.Up);
        if (IsOnFloor())
        {
            _stateMachine.TransitionTo(DinoState.Grounded);
        }
    }
    private void IntroAnimationExit()
    {
        /*
        Move the dino forward a bit as part of the intro animation. It's defined here instead of in the capital-A 
        Animation in order to only set the position's x component and make sure the dino is still able to jump.
        */
        SceneTreeTween introAnimationDrift = CreateTween();
        introAnimationDrift.TweenProperty(this, "position:x", 154f, 0.7f);
        introAnimationDrift.Play();

        EmitSignal(nameof(IntroJumpFinished), new object[0]);
    }

    // Grounded state callbacks.
    private void GroundedEnter()
    {
        _animator.Play(RUN_ANIMATION);
    }
    private void GroundedPhysicsProcess(float delta)
    {
        bool jumping;
        if (_useBuffer)
        {
            jumping = InputBuffer.IsActionPressBuffered(JUMP_ACTION);
        }
        else
        {
            jumping = Input.IsActionJustPressed(JUMP_ACTION);
        }

        if (jumping)
        {
            _stateMachine.TransitionTo(DinoState.Jumping);
        }
        else if (Input.IsActionPressed(DUCK_ACTION))
        {
            _stateMachine.TransitionTo(DinoState.Ducking);
        }
    }

    // Jumping state callbacks.
    private void JumpingEnter()
    {
        _velocity = _initialJumpSpeed * Vector2.Up;
        _gravity = _regularGravity;
        _animator.Play("Idle + Jump");
        _audio.Play();
    }
    private void JumpingPhysicsProcess(float delta)
    {
        // Increase gravity if the player releases the jump button while rising.
        if (Input.IsActionJustReleased(JUMP_ACTION) && _velocity.Dot(Vector2.Up) > 0)
        {
            _gravity = _shortHopGravity;
        }

        // Reset the gravity once the dino begins falling after a short hop. 
        if (_velocity.Dot(Vector2.Up) < 0)
        {
            _gravity = _regularGravity;
        }

        /*
        Fast fall by pressing the down button.
        With this implementation, the player can cancel a fast fall by releasing the button, but hey, that's how the 
        original worked ¯\_(ツ)_/¯
        */
        if (Input.IsActionPressed("ui_down"))
        {
            _gravity = _fastFallGravity;
        }

        // Move and detect collision with the ground.
        _velocity += _gravity * delta * Vector2.Down;
        MoveAndSlide(_velocity, Vector2.Up);
        if (IsOnFloor())
        {
            _stateMachine.TransitionTo(DinoState.Grounded);
        }
    }

    // Ducking state callbacks.
    private void DuckingEnter()
    {
        _animator.Play("Ducking");
        _regularHitbox.SetDeferred("disabled", true);
        _duckingHitbox.SetDeferred("disabled", false);
    }
    private void DuckingPhysicsProcess(float delta)
    {
        if (!Input.IsActionPressed(DUCK_ACTION))
        {
            _stateMachine.TransitionTo(DinoState.Grounded);
        }
    }
    private void DuckingExit()
    {
        _regularHitbox.SetDeferred("disabled", false);
        _duckingHitbox.SetDeferred("disabled", true);
    }

    // Dead state callback :(
    private void Die()
    {
        _animator.Play("Die");
        _animator.Advance(0);
    }
}
