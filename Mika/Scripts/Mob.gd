extends RigidBody2D

var speedModifier = 10

func _ready():
	$AnimatedSprite.playing = true
	angular_damp = 10
	if(Globals.HardMode):
		speedModifier = 1000
	 

func _process(delta):
	#Make look right
	
	var angular = fmod(rotation, PI * 2)
	
	add_torque(-angular/delta/5)
	#make max velocity
	add_force(Vector2.UP, Vector2.RIGHT * delta * speedModifier)
	applied_force.limit_length(10)

