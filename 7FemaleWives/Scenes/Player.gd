extends Node2D

var velocity = Vector2()
var sail_angular_speed = 1
var sail_max_angle = PI/6
var desired_sail_angle = 0

var wind_direction = 0
var wind_angular_speed = 0.65

var base_speed = 10
var ship_speed = 0

var desired_ship_direction = Vector2(0,0)
var hull_lerp_factor = 0

func _ready():
	pass # Replace with function body.


func _physics_process(delta):
	
	if Input.is_action_pressed("sail_cw"):
#		if  $Sail.rotation - $Ship.rotation < sail_max_angle:
		$Sail.rotation += sail_angular_speed * delta
		
	elif Input.is_action_pressed("sail_ccw"):
#		if $Sail.rotation - $Ship.rotation > - sail_max_angle:
		$Sail.rotation -= sail_angular_speed * delta
	
	else:
		$Sail.rotation = lerp_angle($Sail.rotation, $Ship/Hull.rotation, 0.5 * delta)
		
	if Input.is_action_pressed("wind_cw"):
		wind_direction += wind_angular_speed * delta
		
	if Input.is_action_pressed("wind_ccw"):
		wind_direction -= wind_angular_speed * delta
		
	set_compass_angle(wind_direction)
	_calculate_ship_direction()
	_calculate_ship_speed()
	_calculate_hull_lerp_factor()
	
	
	
	$Ship/Hull.rotation = lerp_angle($Ship/Hull.rotation, desired_ship_direction.angle(), hull_lerp_factor*delta)
	
	
#	Ship.move_and_slide(desired_ship_direction)
	velocity = Vector2(cos($Ship/Hull.rotation), sin($Ship/Hull.rotation))
	
	
	$Ship.move_and_slide(velocity*ship_speed)
	$Sail.position = $Ship.position
	
	$DirectionArrow.position = $Ship.position
	
	
	
func _calculate_ship_direction():
	var sail_vector = Vector2(cos($Sail.rotation), sin($Sail.rotation))
	var wind_vector = Vector2(cos(wind_direction), sin(wind_direction))
	desired_ship_direction = sail_vector + wind_vector
	$DirectionArrow.rotation = desired_ship_direction.angle()
	
	
func _calculate_ship_speed():
	var modifier = 1 + cos($Sail.rotation - wind_direction)
	$DirectionArrow.scale.x = modifier
	var desired_speed = pow(base_speed, modifier)
	
	if pow(base_speed, 2) / desired_speed > 0.9:
		
		desired_speed = desired_speed * 1.2
	elif desired_speed < 4:
		desired_speed = 0
	ship_speed = desired_speed
	$Ship/UI/SpeedLabel.text = "Current Speed: %d" % int(ship_speed)
	
func _calculate_hull_lerp_factor():
	var hull_vector = Vector2(cos($Ship/Hull.rotation), sin($Ship/Hull.rotation))
	var angle = abs(hull_vector.angle_to(desired_ship_direction))
	if angle <= PI/2:
		hull_lerp_factor = ship_speed/5
	else: 
		hull_lerp_factor = ship_speed/15

func set_compass_angle(a):
	$Ship/UI/WindCompass/Needle.rotation = a
	
	
