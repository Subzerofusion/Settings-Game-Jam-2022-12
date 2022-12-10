using System;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using Array = Godot.Collections.Array;

namespace NormalFarming.sauce
{
	internal enum SuckMode
	{
		Nothing,
		SuckingAir,
		Clogged,
		Unclogging,
		Blowing
	};
	
	public class Player : KinematicBody
	{
		[Export] public float MoveSpeed = 10.0f;
		[Export] public float RotateSpeed = 2.0f;
		[Export] public float JumpPower = 6.0f;
	
		private Vector3 _velocity = Vector3.Zero;
		private float _gravity;
		private SuckMode _suckMode = SuckMode.Nothing;
		private Camera _camera = null;
		private RayCast _cameraRay = null;
		private List<Suckable> _suckingThings = new List<Suckable>();

		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			_gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");

			_camera = GetNode<Camera>("Camera");
			_cameraRay = GetNode<RayCast>("Camera/RayCast");
			if (_camera is null || _cameraRay is null)
			{
				Debug.Fail("Missing Camera or RayCast on the Player");
			}
		}

		public override void _PhysicsProcess(float delta)
		{
			//
			// Looking around
			//
			var rotateActual = RotateSpeed * delta;
			var lookUp = Input.GetAxis("camera_down", "camera_up");
			_camera.RotateX(lookUp * rotateActual);
			var lookSide = Input.GetAxis("camera_right", "camera_left");
			RotateY(lookSide * rotateActual);
		
			//
			// Movement
			//
			var inputVector = Input.GetVector("player_move_left", "player_move_right","player_move_forward", "player_move_backward");
			var direction = Transform.basis.x * inputVector.x + Transform.basis.z * inputVector.y;

			// Don't go too fast, eh?
			if (direction.Length() > 1)
			{
				direction = direction.Normalized();
			}

			_velocity.x = direction.x * MoveSpeed;
			_velocity.z = direction.z * MoveSpeed;
			if (Input.IsActionJustPressed("player_jump"))
			{
				_velocity.y = JumpPower;
			}
			_velocity.y -= _gravity * delta;
			_velocity = MoveAndSlide(_velocity, Vector3.Up);
			
			//
			// Sucking and Blowing
			//
			if (Input.IsActionJustPressed("suck"))
			{
				HandleSuckPress();
			} else if (Input.IsActionJustReleased("suck"))
			{
				HandleSuckRelease();
			}

			if (Input.IsActionJustPressed("blow"))
			{
				HandleBlowPressed();
			} else if (Input.IsActionJustReleased("blow"))
			{
				HandleBlowReleased();
			}
			
			DoGunUpdate(delta);
		}

		void DoGunUpdate(float delta)
		{
			switch (_suckMode)
			{
				case SuckMode.Nothing:
					// Freebie -- nothing to do if nothing is happening!
					break;
				case SuckMode.SuckingAir:
					if (!_cameraRay.IsColliding())
					{
						break;
					}

					if (!(_cameraRay.GetCollider() is Suckable suckable))
					{
						break;
					}
					
					suckable.BeSucked(_camera);
					_suckingThings.Add(suckable);
					
					break;
				case SuckMode.Clogged:
					// TODO: drop whatever we're holding, politely
					_suckMode = SuckMode.Nothing;
					break;
				case SuckMode.Unclogging:
					break;
				case SuckMode.Blowing:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void HandleSuckPress()
		{
			switch (_suckMode)
			{
				case SuckMode.Nothing:
					_suckMode = SuckMode.SuckingAir;
					break;
				case SuckMode.SuckingAir:
					// This shouldn't happen... :(
					break;
				case SuckMode.Clogged:
					// Can't suck if you're clogged...
					// TODO: Play a sound?
					break;
				case SuckMode.Blowing:
					// Safe to ignore, probably
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void HandleSuckRelease()
		{
			switch (_suckMode)
			{
				case SuckMode.Nothing:
					// Can happen, but nothing needs to be done here
					break;
				case SuckMode.SuckingAir:
					foreach (var thing in _suckingThings)
					{
						thing.StopSucking();
					}
					_suckingThings.Clear();
					_suckMode = SuckMode.Nothing;
					break;
				case SuckMode.Clogged:
					// Don't do anything, 'cause we keep holding whatever
					break;
				case SuckMode.Unclogging:
					break;
				case SuckMode.Blowing:
					// Safe to ignore, probably
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void HandleBlowPressed()
		{
			if (_suckMode == SuckMode.Clogged)
			{
				// TODO: Eject that shit hard!!
				_suckMode = SuckMode.Unclogging;
			}
		}

		private void HandleBlowReleased()
		{
			if (_suckMode == SuckMode.Blowing)
			{
				// TODO: Tell anything we're sucking that we stopped sucking it
				_suckMode = SuckMode.Nothing;
			}
		}
	}
}
