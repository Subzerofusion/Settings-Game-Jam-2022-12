extends Node2D

var number_of_wives = 6

var velocity = Vector2()
var sail_angular_speed = 0.3
var sail_max_angle = PI/6
var desired_sail_angle = 0

var wind_direction = 0
var wind_angular_speed = 0.9

var hull_angle_boost = 0

var base_speed = 100
var ship_speed = 0

var desired_ship_direction = Vector2(0,0)
var hull_lerp_factor = 0

var push_force = 300

var max_hp = 100.0
var hp = max_hp
var heal_factor = 0.5

func _ready():
#	hp = max_hp
	pass

func _process(_delta):
	if Input.is_action_just_pressed("zoom1"):
		$Ship/Camera2D.zoom = Vector2(1,1)
		
	if Input.is_action_just_pressed("zoom2"):
		$Ship/Camera2D.zoom = Vector2(2,2)
		
	if Input.is_action_just_pressed("zoom3"):
		$Ship/Camera2D.zoom = Vector2(4,4)
		
	if Input.is_action_pressed("wife_update"):
		_wife_update()

func _physics_process(delta):
	

	$Sail.rotation = lerp_angle($Sail.rotation, wind_direction, delta * sail_angular_speed)
		
	if Input.is_action_pressed("wind_cw"):
		wind_direction += wind_angular_speed * delta
		
	if Input.is_action_pressed("wind_ccw"):
		wind_direction -= wind_angular_speed * delta
		

	_calculate_ship_direction()
	_calculate_ship_speed()
	_calculate_hull_lerp_factor()
	
	
	
	print_debug(hull_lerp_factor)
	$Ship.rotation = lerp_angle($Ship.rotation, desired_ship_direction.angle(), (hull_angle_boost + hull_lerp_factor)*delta)
	
	
#	Ship.move_and_slide(desired_ship_direction)
	velocity = Vector2(cos($Ship.rotation), sin($Ship.rotation))
	
	
	$Ship.move_and_slide(velocity*ship_speed)
	$Sail.position = $Ship.position
	
	$DirectionArrow.position = $Ship.position
	
	get_parent().wind_direction = wind_direction
	
	hp += delta*heal_factor
	hp = clamp(hp, 0, max_hp)
	
func _calculate_ship_direction():
	var sail_vector = Vector2(cos($Sail.rotation), sin($Sail.rotation))
	var wind_vector = Vector2(cos(wind_direction), sin(wind_direction))
	desired_ship_direction = sail_vector + wind_vector
	$DirectionArrow.rotation = desired_ship_direction.angle()
	
	
func _calculate_ship_speed():
	var modifier = 1.1 + cos($Sail.rotation - wind_direction)
	$DirectionArrow.scale.x = modifier
	var desired_speed = base_speed * modifier
	
	if base_speed * 2 / desired_speed > 0.9:
		desired_speed = desired_speed * 1.2
		
	elif desired_speed < 4:
		desired_speed = 0
	ship_speed = desired_speed
	
	
func hit():
	
	hp -= 10
	if hp == 0:
		
		get_parent().game_over()
		
	
func _calculate_hull_lerp_factor():
	var hull_vector = Vector2(cos($Ship/Hull.rotation), sin($Ship/Hull.rotation))
	var angle = abs(hull_vector.angle_to(desired_ship_direction))
	if angle <= PI/2:
		hull_lerp_factor = ship_speed/5
	else: 
		hull_lerp_factor = ship_speed/15


func _wife_update():
	if number_of_wives == 6:
		if not $Sounds/SixFemaleWives.playing:
			$Sounds/SixFemaleWives.play()
	elif number_of_wives == 7:
		if not $Sounds/SevenFemaleWives.playing:
			$Sounds/SevenFemaleWives.play()
	


