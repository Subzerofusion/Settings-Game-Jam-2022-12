using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Godot;
using Array = Godot.Collections.Array;

namespace Normalfarming.sauce
{
	internal enum SuckMode
	{
		Nothing,
		SuckingAir,
		Clogged,
		Blowing
	};
	
	public class Player : KinematicBody
	{
		[Export] public float MoveSpeed = 10.0f;
		[Export] public float RotateSpeed = 2.0f;
		[Export] public float JumpPower = 6.0f;
		[Export] private float _ejectionForce = 10.0f;
	
		private Vector3 _velocity = Vector3.Zero;
		private float _gravity;
		private SuckMode _suckMode = SuckMode.Nothing;
		private Camera _camera = null;
		private RayCast _cameraRay = null;
		private List<Suckable> _suckingThings = new List<Suckable>();
		private Suckable _clog = null;
		private Node _clogParent = null;
		private Spatial _holdPoint;

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

			_holdPoint = GetNode<Spatial>("Camera/HoldPoint");
			if (_holdPoint is null)
			{
				Debug.Fail("No hold point found on player");
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
					// Nothing to do (passively)
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

		private void ReleaseSuckingThings()
		{
			foreach (var thing in _suckingThings)
			{
				thing.StopSucking();
			}
			_suckingThings.Clear();
		}

		private void HandleSuckRelease()
		{
			switch (_suckMode)
			{
				case SuckMode.Nothing:
					// Can happen, but nothing needs to be done here
					break;
				case SuckMode.SuckingAir:
					ReleaseSuckingThings();
					_suckMode = SuckMode.Nothing;
					break;
				case SuckMode.Clogged:
					// Don't do anything, 'cause we keep holding whatever
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
				Debug.Print("EJECT THAT SHIT!!!");
				_suckMode = SuckMode.Nothing;

				if (_clog is null)
				{
					// Shouldn't happen, but... meh
					return;
				}

				var clogOwner = _clog.Owner ?? _clog;
				_holdPoint.RemoveChild(clogOwner);
				_clogParent.AddChild(clogOwner);
				
				_clog.GlobalTranslation = _holdPoint.GlobalTranslation;
				_clog.Mode = RigidBody.ModeEnum.Rigid;
				_clog.GetNode<CollisionShape>("CollisionShape").Disabled = false;
				_clog.ApplyCentralImpulse(-_camera.GlobalTransform.basis.z * _ejectionForce);
			}
			else
			{
				_suckMode = SuckMode.Blowing;
			}
		}

		private void HandleBlowReleased()
		{
			if (_suckMode == SuckMode.Blowing)
			{
				_suckMode = SuckMode.Nothing;
			}
		}
		
		public void TryCollect(Suckable suckable)
		{
			// See if I can collect the object (i.e., there's container space and I'm sucking air)
			
			// If I can collect it, then delete the object in the world and add it to my inventory
			
			// Otherwise, stop sucking the object
		}

		public void GetClogged(Suckable suckable)
		{
			// Ignore suckables that I'm not actually sucking
			if (!_suckingThings.Contains(suckable))
			{
				return;
			}

			// If I'm already clogged, then ignore it
			if (_suckMode != SuckMode.SuckingAir)
			{
				return;
			}

			// If I'm sucking air, then set myself to clogged and move the object to be a child of ... my gun mount?
			Debug.Print("I'm getting clogged!!");
			_suckMode = SuckMode.Clogged;
			ReleaseSuckingThings();

			_clog = suckable;
			_clog.Mode = RigidBody.ModeEnum.Static;
			_clog.GetNode<CollisionShape>("CollisionShape").Disabled = true;
			var owner = _clog.GetOwnerOrNull<Spatial>() ?? suckable;
			if (owner.GetParentOrNull<Node>() is Node parent)
			{
				parent.RemoveChild(owner);
				_clogParent = parent;
			}
			else
			{
				Debug.Print("No parent, I guess?");
			}

			_holdPoint.AddChild(owner);
			_clog.GlobalTranslation = _holdPoint.GlobalTranslation;
		}
	}
}
