using Godot;
using System;

public partial class Player : CharacterBody2D
{
	private AnimatedSprite2D _sprite;

	private bool isWinged = false;
	private bool isFurling = false;
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
		bool unfurlPressed = Input.IsActionJustPressed("unfurl");

		velocity = ApplyGravity(velocity, delta);

		if (jumpReleased && IsOnFloor())
		{
			velocity = Jump(velocity);
		}

		velocity = Walk(direction.X, velocity);

		Velocity = velocity;
		MoveAndSlide();

		UpdateAnimation(direction.X, jumpHeld, unfurlPressed);
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
			case "winged_walk_start":
				_sprite.Play("winged_walk_loop");
				break;
			case "walk_end":
				_sprite.Play("idle");
				break;
			case "winged_walk_end":
				_sprite.Play("winged_idle");
				break;
			case "jump_start":
				_sprite.Play("jump_loop");
				break;
			case "walk_to_jump_walk":
				_sprite.Play("jump_walk");
				break;
			case "unfurl":
				if (isFurling)
				{
					isFurling = false;
					isWinged = false;
					_sprite.Play("idle");
				}
				else
				{
					_sprite.Play("winged_idle");
				}
				break;
		}
	}

	private void UpdateAnimation(float inputX, bool jumpHeld, bool unfurlPressed)
	{
		bool isMoving = inputX != 0;

		if (isMoving)
		{
			_sprite.FlipH = inputX < 0;
		}

		string idleAnim = isWinged ? "winged_idle" : "idle";
		string walkStartAnim = isWinged ? "winged_walk_start" : "walk_start";
		string walkLoopAnim = isWinged ? "winged_walk_loop" : "walk_loop";
		string walkEndAnim = isWinged ? "winged_walk_end" : "walk_end";

		string currentAnimation = _sprite.Animation;
		bool isWalking = currentAnimation == walkStartAnim || currentAnimation == walkLoopAnim;
		bool isCharging = currentAnimation == "jump_charge" || currentAnimation == "jump_walk" || currentAnimation == "walk_to_jump_walk";
		bool isInJumpAnimation = isCharging || currentAnimation == "jump_start" || currentAnimation == "jump_loop";

		if (!IsOnFloor())
		{
			if (isCharging)
			{
				_sprite.Play("jump_start");
			}
			else if (currentAnimation != "jump_start" && currentAnimation != "jump_loop")
			{
				_sprite.Play("jump_loop");
			}

			return;
		}

		if (jumpHeld)
		{
			if (isMoving && isWalking)
			{
				_sprite.Play("walk_to_jump_walk");
			}
			else if (!(isMoving && currentAnimation == "walk_to_jump_walk"))
			{
				string chargeAnimation = isMoving ? "jump_walk" : "jump_charge";

				if (currentAnimation != chargeAnimation)
				{
					_sprite.Play(chargeAnimation);

					if (currentAnimation == "jump_walk" || currentAnimation == "walk_to_jump_walk")
					{
						_sprite.Frame = _sprite.SpriteFrames.GetFrameCount(chargeAnimation) - 1;
					}
				}
			}

			return;
		}

		if (isInJumpAnimation)
		{
			_sprite.Play(isMoving ? walkLoopAnim : idleAnim);
			return;
		}

		if (unfurlPressed && !isMoving)
		{
			if (currentAnimation == "idle")
			{
				isFurling = false;
				isWinged = true;
				_sprite.Play("unfurl");
				return;
			}
			else if (currentAnimation == "winged_idle")
			{
				isFurling = true;
				_sprite.PlayBackwards("unfurl");
				return;
			}
		}

		bool isIdleOrEnding = currentAnimation == idleAnim || currentAnimation == walkEndAnim || currentAnimation == "unfurl";

		if (isMoving && isIdleOrEnding)
		{
			_sprite.Play(walkStartAnim);
		}
		else if (!isMoving && isWalking)
		{
			_sprite.Play(walkEndAnim);
		}
	}
}
