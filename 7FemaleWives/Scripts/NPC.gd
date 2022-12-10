extends Node2D

var velocity = Vector2()
var sail_angular_speed = 1
var sail_max_angle = PI/6
var desired_sail_angle = 0

var wind_direction = 0

var base_speed = 30
var ship_speed = 0

var desired_ship_direction = Vector2(0,0)
var hull_lerp_factor = 0

var player_position = Vector2(0,0)
var escape_direction = Vector2(0,0)
var desired_sail_direction = 0
var running_away = true
var sight_radius = 1000

func _ready():
	pass # Replace with function body.

func _physics_process(delta):
	
	if $Ship.position.distance_to(player_position) < sight_radius:
		running_away = true
	else:
		running_away = false
	

	if running_away:
		$Sail.rotation = lerp_angle($Sail.rotation, escape_direction.angle(), 1.5 * delta)
		
	else:
		$Sail.rotation = lerp_angle($Sail.rotation, wind_direction, 0.5 * delta)
	
	
	

	_calculate_ship_direction()
	_calculate_ship_speed()
	_calculate_hull_lerp_factor()
	
	
	
	
	$Ship.rotation = lerp_angle($Ship.rotation, desired_ship_direction.angle(), hull_lerp_factor*delta)
	
	
#	Ship.move_and_slide(desired_ship_direction)
	velocity = Vector2(cos($Ship.rotation), sin($Ship.rotation))
	
	
	$Ship.move_and_slide(velocity*ship_speed)
	$Sail.position = $Ship.position
	
	$DirectionArrow.position = $Ship.position
	
	wind_direction = get_parent().wind_direction 
	
	
	
func _calculate_ship_direction():
	var sail_vector = Vector2(cos($Sail.rotation), sin($Sail.rotation))
	var wind_vector = Vector2(cos(wind_direction), sin(wind_direction))
	desired_ship_direction = sail_vector + wind_vector
	
	
	
	escape_direction = -(player_position - $Ship.position)
	desired_sail_angle = (escape_direction - wind_vector).angle()
	
	
	
	$DirectionArrow.rotation = escape_direction.angle()
	
func _calculate_ship_speed():
	var modifier = 1.1 + cos($Sail.rotation - wind_direction)
	$DirectionArrow.scale.x = modifier
	var desired_speed = base_speed * modifier
	
	if base_speed * 2 / desired_speed > 0.9:
		desired_speed = desired_speed * 1.2
		
	elif desired_speed < 4:
		desired_speed = 0
	ship_speed = desired_speed
	
	
	
	
func _calculate_hull_lerp_factor():
	var hull_vector = Vector2(cos($Ship/Hull.rotation), sin($Ship/Hull.rotation))
	var angle = abs(hull_vector.angle_to(desired_ship_direction))
	if angle <= PI/2:
		hull_lerp_factor = ship_speed/5
	else: 
		hull_lerp_factor = ship_speed/15


	
