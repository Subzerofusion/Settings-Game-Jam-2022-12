extends RigidBody2D


func _ready():
	$AnimatedSprite.playing = true
	add_force(Vector2.UP, Vector2.RIGHT * 100)

func _integrate_forces(state):
	add_force(Vector2.UP, Vector2.RIGHT/3)

