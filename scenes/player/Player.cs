using Godot;
using System;

public partial class Player : CharacterBody2D
{
	private AnimatedSprite2D _sprite;
	public const float Speed = 300.0f;
	public const float JumpVelocity = -400.0f;

	public override void _Ready()
	{
		_sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		_sprite.AnimationFinished += OnAnimationFinished;
		_sprite.Play("idle");
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		bool jumpHeld = Input.IsActionPressed("ui_accept");
		bool jumpReleased = Input.IsActionJustReleased("ui_accept");

		velocity = ApplyGravity(velocity, delta);

		if (jumpReleased && IsOnFloor())
		{
			velocity = Jump(velocity);
		}

		velocity = Walk(direction.X, velocity);

		Velocity = velocity;
		MoveAndSlide();

		UpdateAnimation(direction.X, jumpHeld);
	}

	private Vector2 ApplyGravity(Vector2 velocity, double delta)
	{
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		return velocity;
	}

	private Vector2 Jump(Vector2 velocity)
	{
		velocity.Y = JumpVelocity;

		return velocity;
	}

	private Vector2 Walk(float inputX, Vector2 velocity)
	{
		if (inputX != 0)
		{
			velocity.X = inputX * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(velocity.X, 0, Speed);
		}

		return velocity;
	}

	private void OnAnimationFinished()
	{
		switch(_sprite.Animation)
		{
			case "walk_start":
				_sprite.Play("walk_loop");
				break;
			case "walk_end":
				_sprite.Play("idle");
				break;
		}
	}

	private void UpdateAnimation(float inputX, bool jumpHeld)
	{
		bool isMoving = inputX != 0;

		if (isMoving)
		{
			_sprite.FlipH = inputX < 0;
		}

		string currentAnimation = _sprite.Animation;
		bool isInJumpAnimation = currentAnimation == "jump_start" || currentAnimation == "jump_loop";

		if (!IsOnFloor())
		{
			if (currentAnimation != "jump_loop")
			{
				_sprite.Play("jump_loop");
			}

			return;
		}

		if (jumpHeld)
		{
			if (currentAnimation != "jump_start")
			{
				_sprite.Play("jump_start");
			}

			return;
		}

		if (isInJumpAnimation)
		{
			_sprite.Play(isMoving ? "walk_start" : "idle");
			return;
		}

		bool isIdleOrEnding = currentAnimation == "idle" || currentAnimation == "walk_end";
		bool isWalking = currentAnimation == "walk_start" || currentAnimation == "walk_loop";

		if (isMoving && isIdleOrEnding)
		{
			_sprite.Play("walk_start");
		}
		else if (!isMoving && isWalking)
		{
			_sprite.Play("walk_end");
		}
	}
}
