extends RigidBody2D


func _ready():
	$AnimatedSprite.playing = true
	angular_damp = 10
	#add_force(Vector2.UP, Vector2.RIGHT * 100)

func _process(delta):
	#Make look right
	
	var angular = fmod(rotation, PI * 2)
	
	add_torque(-angular/delta/5)
	#make max velocity
	add_force(Vector2.UP, Vector2.RIGHT * delta * 10)
	applied_force.limit_length(10)

