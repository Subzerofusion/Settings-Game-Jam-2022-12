extends RigidBody2D


func _ready():
	$AnimatedSprite.playing = true
	#add_force(Vector2.UP, Vector2.RIGHT * 100)

func _process(delta):
	#Make look right
	
	add_torque(-rotation/delta)
	#make max velocity
	add_force(Vector2.UP, Vector2.RIGHT * delta * 10)
	applied_force.limit_length(10)

