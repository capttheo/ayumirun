using Godot;
using System;

//states
//Jump1	grace timer
//-FallLongUpside -> JumpShort

public enum PlrState {
	Idle, Jump1, FallJumpLong, FallJump1, FallTouch, JumpLongUp, JumpLongMid, JumpShort, Run, Fall, FallDelayed, ClimbL, ClimbR
};

public class Player : KinematicBody2D
{
	const float GRAV = 800; //~2.5block x 16px x 10m/s x doubled

	int move_speed = 125; //arbitrary
	int jump_force = 300; //
	int jump_up_force = 400; //
	float gravity = GRAV;
	float gravCoeff = 1.0f;


	const float ANIM_RUN_SPEED = 18; //16;
	const float ANIM_JUMP_SPEED = 11;
	const float ANIM_CLIMB_SPEED = 12;
	const float ANIM_TOUCH_SPEED = 16;

	const float JUMP_DX = 1750;
	const float JUMP_NDX = JUMP_DX; // / 2;
	const float JUMP_LONG_DX = 1200;
	const float JUMP_LONG_NDX = JUMP_LONG_DX; // / 2;
	const float ROLL_DX = 750;
	const float ROLL_NDX = ROLL_DX / 3;

	const float TIME_FALL_AFTER_JUMP = 0.8f; //sec
	float timeToFallAfterJump = TIME_FALL_AFTER_JUMP;


	//float jump1fallTimer = 0;
	//const float JUMP1FALL_TIME = 0.5f; //sec	

	//float jumpLongFallTimer = 0;
	//const float JUMPLONGFALL_TIME = 0.5f; //sec

	const float TIME_TO_FALL_ANIM = 0.2f; //sec
	float timeToFallAnim = TIME_TO_FALL_ANIM;

	const float TIME_COYOTE = 0.1f; //sec
	float timeCoyote = TIME_COYOTE;

	const float TIME_COYOTE_JUMP = 0.1f; //sec
	float timeCoyoteJump = 0;

	Vector2 velocity = Vector2.Zero;

	Sprite sprIdle, sprRun, sprJumpLong, sprJumpShort, sprFall, sprClimb;
	CollisionShape2D colFront, colBottom, colRun, colBase, colUpper;
	PlrState plrState = PlrState.Idle;
	AnimationPlayer animPlayer;
	Area2D grabArea;
	KinematicBody2D grabBody;
	RayCast2D rayFloor;

	float headingX = +1.0f; //-1 to the left, +1 to the right

 	public override void _Ready()
 	{	
		sprIdle = GetNode<Sprite>("sprIdle");
 		sprRun = GetNode<Sprite>("sprRun");
		sprJumpLong = GetNode<Sprite>("sprJumpLong");
		sprJumpShort = GetNode<Sprite>("sprJumpShort");
		sprFall = GetNode<Sprite>("sprFall");
		sprClimb = GetNode<Sprite>("sprClimb");
		

		animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		
		colFront = GetNode<CollisionShape2D>("colFront");
		colBottom = GetNode<CollisionShape2D>("colBottom");
		colRun = GetNode<CollisionShape2D>("colRun");
		colBase = GetNode<CollisionShape2D>("colBase");
		colUpper = GetNode<CollisionShape2D>("colUpper");

		grabArea = GetNode<Area2D>("GrabArea");
		grabArea.Connect("body_entered", this, "OnGrab");

		rayFloor = GetNode<RayCast2D>("rayFloor");

		TurnOnSpr(SprObj.SprIdle);
 	}

//	Vector2 climbTarget;
	Vector2 climbOffset = new Vector2(-10, 62); //(-18, 54); //hardcoded -(GrabArea.Pos+ClimbTarget.size/2)
	Vector2 climbFinal = new Vector2(40, -50); //(32, -57); //harddcoded based on animation

	Vector2 climbGlobPos;
	Vector2 GetClimbGlobalPos(Vector2 off)
	{
		climbGlobPos = grabBody.GlobalPosition;
		climbGlobPos.x += off.x * headingX;
		climbGlobPos.y += off.y;
		return climbGlobPos;
	}

	public void OnGrab(Node body)
	{
		if(body.Name.StartsWith("ClimbL"))
		{
			grabBody = body as KinematicBody2D;
			SwitchTo(PlrState.ClimbL);
		}
		else if(body.Name.StartsWith("ClimbR"))
		{
			grabBody = body as KinematicBody2D;
			SwitchTo(PlrState.ClimbR);
		}
	}


	Vector2 tmpVec= Vector2.Zero;

 	void UpdateHeadingX(float newHeadingX)
 	{
 		if(headingX != newHeadingX)
 		{
 			headingX = newHeadingX;
 			sprIdle.FlipH = (headingX < 0);
			sprRun.FlipH = (headingX < 0);
			sprJumpLong.FlipH = (headingX < 0);
			sprJumpShort.FlipH = (headingX < 0);
			sprFall.FlipH = (headingX < 0);
			sprClimb.FlipH = (headingX < 0);

			tmpVec = colFront.Position;
			tmpVec.x = -tmpVec.x;
			colFront.Position = tmpVec;

			tmpVec = colBottom.Position;
			tmpVec.x = -tmpVec.x;
			colBottom.Position = tmpVec;

			tmpVec = grabArea.Position;
			tmpVec.x = -tmpVec.x;
			grabArea.Position = tmpVec;
			
 		}
 	}

	

	bool PlayerTappedJump()
	{
		if(Input.IsActionJustReleased("jump"))
		{
			timeCoyoteJump = TIME_COYOTE_JUMP;
		}
		return (timeCoyoteJump > 0);
	}

	bool PlayerHoldingUp()
	{
		return Input.IsActionJustReleased("key_up") || Input.IsActionPressed("key_up");
	}

	bool PlayerHoldingDown()
	{
		return Input.IsActionJustReleased("key_down") || Input.IsActionPressed("key_down");
	}

bool AnimFinished(PlrState state)
	{
		switch(state)
		{
			case PlrState.Jump1:
				return sprJumpLong.Frame == 3;
			case PlrState.JumpLongMid:
			case PlrState.JumpLongUp:
				return sprJumpLong.Frame == 7;
			case PlrState.JumpShort:
				return sprJumpShort.Frame == 7;
			case PlrState.ClimbL:
			case PlrState.ClimbR:
				return isClimbFinished;
			case PlrState.FallTouch:
				return sprFall.Frame == 2;
			default:
				GD.Print("## ALERT");
				return true;
		}
	}

	void SlowDownBody(float amount) 
	{
		float oldX = velocity.x;
		velocity.x -= amount;
		if(Math.Sign(oldX) != Math.Sign(velocity.x))
		{
			velocity.x = 0;
		}
	}

	void AdvanceBody(PlrState plrState, float delta)
	{
		switch(plrState)
		{
			case PlrState.Jump1:
				switch(sprJumpLong.Frame)
				{
					//jump1
					case 0:
						break;
					case 1:
						velocity.x += JUMP_DX * delta * headingX;
						break;
					case 2:
						break;
					case 3:
						SlowDownBody(JUMP_NDX * delta * headingX);
						break;
				}
			break;

			case PlrState.JumpLongMid:
				switch(sprJumpLong.Frame)
				{
					//jumpLong
					case 4:
						velocity.x += JUMP_LONG_DX * delta * headingX;
						break;
					case 5:
						break;
					case 6:
						//velocity.x -= JUMP_LONG_NDX * delta * headingX;
						break;
					case 7:
						SlowDownBody(JUMP_LONG_NDX * delta * headingX);
						break;
				}
			break;

			case PlrState.JumpLongUp:
				switch(sprJumpLong.Frame)
				{
					//jumpLong
					case 4:
						velocity.x += JUMP_LONG_DX * delta * headingX;
						break;
					case 5:
						break;
					case 6:
						//velocity.x -= JUMP_LONG_NDX * delta * headingX;
						break;
					case 7:
						SlowDownBody(JUMP_LONG_NDX * delta * headingX);						
						break;
				}
			break;

			case PlrState.JumpShort:
				switch(sprJumpShort.Frame)
				{
					//jumpShort
					case 4:
						velocity.x += ROLL_DX * delta * headingX;
						break;
					case 5:
						break;
					case 6:
						velocity.x -= ROLL_NDX * delta * headingX;
						break;
					case 7:
					case 8:
						velocity.x = 0;
						break;
				}				
			break;
		}
	}

	enum SprObj { SprRun, SprIdle, SprJumpLong, SprFall, SprJumpShort, SprClimb };

	void TurnOnSpr(SprObj spr)
	{
		sprRun.Visible = (spr == SprObj.SprRun);
		sprIdle.Visible = (spr == SprObj.SprIdle);
		sprJumpLong.Visible = (spr == SprObj.SprJumpLong);
		sprFall.Visible = (spr == SprObj.SprFall);
		sprJumpShort.Visible = (spr == SprObj.SprJumpShort);
		sprClimb.Visible = (spr == SprObj.SprClimb);
		
	}


	void SwitchTo(PlrState newState)
	{
		GD.Print(newState.ToString());
		if(plrState != newState)
		{
			switch(newState)
			{
				case PlrState.Idle:
					TurnOnSpr(SprObj.SprIdle);
					animPlayer.Stop(); //TODO idle animation
					velocity.x = 0; //Stopping in every Idle switch
					break;
				case PlrState.Run:
					TurnOnSpr(SprObj.SprRun);
					animPlayer.Play("run", -1, ANIM_RUN_SPEED);
					break;
				case PlrState.Jump1:
					TurnOnSpr(SprObj.SprJumpLong);
					animPlayer.Play("jump1", -1, ANIM_JUMP_SPEED);
					break;
				case PlrState.JumpLongMid:
					TurnOnSpr(SprObj.SprJumpLong);
					animPlayer.Play("jumplong", -1, ANIM_JUMP_SPEED);
					break;
				case PlrState.JumpLongUp:
					TurnOnSpr(SprObj.SprJumpLong);
					animPlayer.Play("jumplong", -1, ANIM_JUMP_SPEED);
					break;
				case PlrState.JumpShort:
					TurnOnSpr(SprObj.SprJumpShort);
					animPlayer.Play("jumpshort", -1, ANIM_JUMP_SPEED);
					break;
				case PlrState.FallJump1:
					//do nothing
					break;
				case PlrState.Fall:
				case PlrState.FallJumpLong:
					sprFall.Frame = 0;
					TurnOnSpr(SprObj.SprFall);
					break;
				case PlrState.FallDelayed:
					timeToFallAnim = TIME_TO_FALL_ANIM;
					break;
				case PlrState.ClimbL:
					TurnOnSpr(SprObj.SprClimb);
					gravCoeff = 0.0f;
					velocity.x = 0;
					velocity.y = 0;
					GlobalPosition = GetClimbGlobalPos(climbOffset);
					isClimbFinished = false;
					animPlayer.Play("climbl", -1, ANIM_CLIMB_SPEED);
					break;
				case PlrState.ClimbR:
					TurnOnSpr(SprObj.SprClimb);
					gravCoeff = 0.0f;
					velocity.x = 0;
					velocity.y = 0;
					GlobalPosition = GetClimbGlobalPos(climbOffset);
					isClimbFinished = false;
					animPlayer.Play("climbr", -1, ANIM_CLIMB_SPEED);
					break;
				case PlrState.FallTouch:
					animPlayer.Play("touch", -1, ANIM_TOUCH_SPEED);
					break;

			}
			plrState = newState;
		}
	}

	bool isClimbFinished = false;

	void OnClimbFinished()
	{
		isClimbFinished = true;
	}

	bool NeedAutoJump()
	{
		return rayFloor.IsColliding();
	}

	

	void ChooseJump2()
	{
		if(PlayerTappedJump() || PlayerHoldingUp()) //Jump1 -> jump or up pressed ->JumpLongUp
		{
			velocity.y = -jump_up_force;
			SwitchTo(PlrState.JumpLongUp);
		}
		else if(PlayerHoldingDown()) //Jump1 -> down pressed -> JumpShort
		{
//			velocity.y = -jump_force;
			SwitchTo(PlrState.JumpShort);
		}
		else //Jump1 -> nothing pressed -> JumpMid
		{
			velocity.y = -jump_force;
			SwitchTo(PlrState.JumpLongMid);
		}
	}

	bool IsOnFloorCoyote()
	{
		return IsOnFloor() || timeCoyote > 0;
	}

	void UpdateCoyoteTimers(float delta)
	{
		if(IsOnFloor())
		{
			timeCoyote = TIME_COYOTE;
		}
		else if(timeCoyote > -1)
		{
			timeCoyote -= delta;
		}

		if(Input.IsActionJustReleased("jump"))
		{
			timeCoyoteJump = TIME_COYOTE_JUMP;
		}
		else if(timeCoyoteJump > -1)
		{
			timeCoyoteJump -= delta;
		}

		if(plrState != PlrState.FallDelayed)
		{
			timeToFallAnim = TIME_TO_FALL_ANIM;
		}

		if(plrState != PlrState.Jump1 && plrState != PlrState.JumpLongMid
			&& plrState != PlrState.JumpLongUp && plrState != PlrState.JumpShort)
		{
			timeToFallAfterJump = TIME_FALL_AFTER_JUMP;
		}
	}

	bool debugCol = true;

	//1. apply gravity
	//2. adjust velocity.x,y
	//3. velocity = move and slide (velocity)
	public override void _PhysicsProcess(float delta)
	{
		UpdateCoyoteTimers(delta);

		//gravity
		//if(enableGrav)
		//{
			velocity.y += gravCoeff * gravity * delta;
		//}

		var dirX = Input.GetActionStrength("key_right") - Input.GetActionStrength("key_left");

		//process state
		switch(plrState)
		{
			case PlrState.Idle:
				if(NeedAutoJump())
				{
					velocity.y = -jump_force;
					SwitchTo(PlrState.Jump1);
				}
				else if(!IsOnFloorCoyote()) //TODO fall if no floor (dont remember what does it means)
				{
					SwitchTo(PlrState.FallDelayed);
				}
				else if(PlayerTappedJump()) //Idle -> jump key pressed -> Jump1
				{
					velocity.y = -jump_force;
					SwitchTo(PlrState.Jump1);
				}
				else if(dirX != 0) //Idle -> dir keys pressed -> Run
				{
					UpdateHeadingX(dirX);
					SwitchTo(PlrState.Run);
				}
				break;

			case PlrState.Run:
				if(!IsOnFloorCoyote()) //Run -> off ground -> stop and Fall (delayed)
				{
					SwitchTo(PlrState.FallDelayed);
				}
				else if(PlayerTappedJump()) //Run -> jump key pressed -> stop and Jump1
				{
					velocity.x = 0;
					velocity.y = -jump_force;
					SwitchTo(PlrState.Jump1);
				}
				else if(dirX == 0) //Run -> dir keys not pressed -> Idle
				{
					velocity.x = 0;
					SwitchTo(PlrState.Idle);
				}
				else //Run -> dir keys pressed -> Run to dir
				{
					UpdateHeadingX(dirX);
					velocity.x = dirX * move_speed;
				}
				break;

			case PlrState.Jump1:
				AdvanceBody(PlrState.Jump1, delta);
				if(AnimFinished(PlrState.Jump1)) //reached last frame
				{
					if(IsOnFloor()) //Jump1 -> on ground -> choose next jump phase
					{
						ChooseJump2();
					}
					else //Jump1 -> off ground -> Jump1Fall
					{
						if((timeToFallAfterJump -= delta) < 0)
						{
							SwitchTo(PlrState.FallJump1);
						}

					}

				}
				break;

			case PlrState.JumpLongUp:
				AdvanceBody(PlrState.JumpLongUp, delta);
				if(AnimFinished(PlrState.JumpLongUp))
				{
					if(IsOnFloor())
					{
						SwitchTo(PlrState.Idle);
					}
					else
					{
						if((timeToFallAfterJump -= delta) < 0)
						{
							SwitchTo(PlrState.FallJumpLong);
						}
					}
				}
				break;

			case PlrState.JumpLongMid:
				AdvanceBody(PlrState.JumpLongMid, delta);
				if(AnimFinished(PlrState.JumpLongMid))
				{
					if(IsOnFloor())
					{
						SwitchTo(PlrState.Idle);
					}
					else
					{
						if((timeToFallAfterJump -= delta) < 0)
						{
							SwitchTo(PlrState.FallJumpLong);
						}
					}
				}
				break;

			case PlrState.JumpShort:
				AdvanceBody(PlrState.JumpShort, delta);
				if(AnimFinished(PlrState.JumpShort))
				{
					if(IsOnFloor())
					{
						SwitchTo(PlrState.Idle);
					}
					else
					{
						SwitchTo(PlrState.Fall);
					}
				}
				break;

			case PlrState.FallDelayed:
				timeToFallAnim -= delta;
				if(timeToFallAnim < 0)
				{
					velocity.x = 0;
					SwitchTo(PlrState.Fall);
				}
				break;				

			case PlrState.Fall:
				if(IsOnFloor()) //Fall -> on ground -> Idle
				{
					gravCoeff = 1.0f;
					SwitchTo(PlrState.Idle);
				}
				else //Fall -> off ground -> Fall
				{
					//do nothing except gravity
				}
				break;

			case PlrState.ClimbL:
				if(AnimFinished(PlrState.ClimbL))
				{
					GlobalPosition = GetClimbGlobalPos(climbFinal + climbOffset);
					sprClimb.Offset = sprClimb.Offset + new Vector2(190, 0);
					gravCoeff = 1.0f;
					velocity.x = 0;
					velocity.y = 0;
					timeCoyoteJump = -1;
					timeCoyote = TIME_COYOTE;
					Input.IsActionJustReleased("jump"); //clear jump
					SwitchTo(PlrState.Idle);
				}
				else
				{
					GlobalPosition = GetClimbGlobalPos(climbOffset);

				}
				break;

			case PlrState.ClimbR:			
				if(AnimFinished(PlrState.ClimbR))
				{
					GlobalPosition = GetClimbGlobalPos(climbFinal + climbOffset);
					sprClimb.Offset = sprClimb.Offset + new Vector2(190, 0);
					GD.Print(GlobalPosition);
					gravCoeff = 1.0f;
					velocity.x = 0;
					velocity.y = 0;
					timeCoyoteJump = -1;
					timeCoyote = TIME_COYOTE;
					Input.IsActionJustReleased("jump"); //clear jump
					SwitchTo(PlrState.Idle);
				}
				else
				{
					GlobalPosition = GetClimbGlobalPos(climbOffset);
				}
				break;

			case PlrState.FallJump1:
				if(IsOnFloor()) //Fall -> on ground -> Roll Short
				{
					SwitchTo(PlrState.JumpShort);
				}
				break;

			case PlrState.FallJumpLong:
				if(IsOnFloor()) //Fall -> on ground -> Idle
				{
					SwitchTo(PlrState.FallTouch);
				}
				break;

			case PlrState.FallTouch:
				if(AnimFinished(PlrState.FallTouch))
				{
					SwitchTo(PlrState.Idle);
				}	
				break;


		}

		//perform move
		velocity = MoveAndSlide(velocity, Vector2.Up);

		//cleanup

		switch(plrState)
		{
			case PlrState.Jump1:
			case PlrState.JumpLongMid:
			case PlrState.JumpLongUp:
			case PlrState.JumpShort:
				if(!IsOnFloor() && (IsOnWall() || IsOnCeiling())) //Jump* -> hit wall/ceil -> stop and Fall
				{
					animPlayer.Stop(); //only for jump anims!
					colFront.Disabled = true;
					colBottom.Disabled = true;
					velocity.x = 0;
					
					if(velocity.y < 0)
					{
						gravCoeff = 2.0f; //Damp jumping speed if hit wall
					}
					SwitchTo(PlrState.Fall);
				}
			break;
		}
	}
};
