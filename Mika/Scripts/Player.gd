extends KinematicBody2D

var speed = 500

func _physics_process(delta):
	var direction = Vector2.ZERO
	
	if Input.is_action_pressed("ui_left"):
		$Sprite.rotation = 0 
		direction.x = -1
	elif Input.is_action_pressed("ui_right"):
		$Sprite.rotation = PI
		direction.x = 1
	if Input.is_action_pressed("ui_up"):
		$Sprite.rotation = PI * .5
		direction.y = -1
	elif Input.is_action_pressed("ui_down"):
		$Sprite.rotation = PI * 1.5
		direction.y = 1
	var updated_pos = (direction * delta * speed)
	move_and_collide(updated_pos)
