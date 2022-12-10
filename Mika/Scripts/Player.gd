extends KinematicBody2D

var speed = 500

func _process(delta):
	pass

func _physics_process(delta):
	var direction = Vector2.ZERO

	if Input.is_action_pressed("ui_left"):
		direction.x = -1
	elif Input.is_action_pressed("ui_right"):
		direction.x = 1
	if Input.is_action_pressed("ui_up"):
		direction.y = -1
	elif Input.is_action_pressed("ui_down"):
		direction.y = 1

	var updated_pos = (direction * delta * speed)
	
	var collision = move_and_collide(updated_pos)
	if (collision):
		print(collision)
