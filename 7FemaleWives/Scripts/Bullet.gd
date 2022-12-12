extends KinematicBody2D


# Declare member variables here. Examples:
# var a = 2
# var b = "text"
var speed = 10
var direction = Vector2(0,0)
var active = false
var collision
var despawning = false
var alpha = 1
# Called when the node enters the scene tree for the first time.
func _physics_process(delta):
	if active:
		collision = move_and_collide(direction*speed)
		if collision:
			active = false
			
			if collision.collider.get_parent().name == "Player":
				collision.collider.get_parent().hit()
			despawning = true
			
	elif despawning:
		alpha = lerp(alpha, 0, delta* 5)
		modulate = Color(1,1,1,alpha)
		if alpha <= 0.1:
			queue_free()

func shoot(dir):
	$AudioStreamPlayer2D.play()
	direction = dir.normalized()
	active = true
