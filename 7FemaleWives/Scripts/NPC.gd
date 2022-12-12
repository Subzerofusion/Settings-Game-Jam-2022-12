extends Node2D

var velocity = Vector2()
var sail_angular_speed = 1
var sail_max_angle = PI/6
var desired_sail_angle = 0

var wind_direction = 0

var base_speed = 28
var ship_speed = 0

var desired_ship_direction = Vector2(0,0)
var hull_lerp_factor = 0

var player_position = Vector2(0,0)
var escape_direction = Vector2(0,0)
var desired_sail_direction = 0
var running_away = false
var sight_radius = 2000
var bullet_range = 2500

var boost_velocity = Vector2(0, 0)
var boost_loss = 1
var boost_force = 600

var health = 3
var alive = true
var dying = false

var cannon_offset = 50
onready var bullet_scene = load("res://Scenes/Bullet.tscn")


func _physics_process(delta):
	
	if alive:
		if ($Ship.position - player_position).length() < sight_radius:
			running_away = true
		else:
#			print(($Ship.position - player_position).length()

			running_away = false
		

		if running_away:
			$Sail.rotation = lerp_angle($Sail.rotation, escape_direction.angle(), 0.5 * delta)
		else:
			$Sail.rotation = lerp_angle($Sail.rotation, wind_direction, 0.5 * delta)
		
		
		

		_calculate_ship_direction()
		_calculate_ship_speed()
		_calculate_hull_lerp_factor()
		
		
		
		
		$Ship.rotation = lerp_angle($Ship.rotation, desired_ship_direction.angle(), hull_lerp_factor*delta)
		
		
	#	Ship.move_and_slide(desired_ship_direction)
		velocity = Vector2(cos($Ship.rotation), sin($Ship.rotation))
		
		
		$Ship.move_and_slide(velocity*ship_speed + boost_velocity+velocity*ship_speed)
		
		boost_velocity = lerp(boost_velocity, Vector2(0,0), delta*boost_loss)
		$Sail.position = $Ship.position
		
		$DirectionArrow.position = $Ship.position
		_cannon_stuff(delta)
		
		wind_direction = get_parent().wind_direction 
	
	elif dying:
		$DespawnTimer.start()
		dying = false
		yield($DespawnTimer, "timeout")
		get_parent().npcs.erase(self)
		queue_free()
		

func spawn(pos):
	$Ship.position = pos
	$Ship.rotation = get_parent().wind_direction
	show()

	
func _calculate_ship_direction():
	var sail_vector = Vector2(cos($Sail.rotation), sin($Sail.rotation))
	var wind_vector = Vector2(cos(wind_direction), sin(wind_direction))
	desired_ship_direction = sail_vector + wind_vector
	
	
	
	escape_direction = -(player_position - $Ship.position).normalized()
	
	desired_sail_angle = (escape_direction - wind_vector).angle()
	
	
	
#	$DirectionArrow.rotation = escape_direction.angle()
	
func _calculate_ship_speed():
	var modifier = 1.1 + clamp(cos($Sail.rotation - wind_direction), 0, 1)
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


func _cannon_stuff(delta):
	$Cannon.position = $Ship.position + velocity * cannon_offset
	var angle_to_player = escape_direction.angle() - PI
	if abs(angle_to_player) - abs($Ship.rotation) < 1.91986:
		$Cannon.rotation = lerp_angle($Cannon.rotation, angle_to_player, 0.5*delta)
	else:
		$Cannon.rotation = lerp_angle($Cannon.rotation, $Ship.rotation, 0.5*delta)
	$Cannon.rotation = clamp($Cannon.rotation, $Ship.rotation - 1.91986, $Ship.rotation + 1.91986)
	$Cannon.rotation = angle_to_player
	
func _on_Area2D_area_entered(area):
	
	if area.name == "SwordArea":
		health -= 1
		if health == 2:
			$Ship/Hull.texture = load("res://Sprites/Enemy/Enemy2.png")
			play_sound("hurt")
		elif health == 1:
			$Ship/Hull.texture = load("res://Sprites/Enemy/Enemy3.png")
			play_sound("hurt")
		elif health == 0:
			$Ship/Hull.texture = load("res://Sprites/Enemy/Enemy4.png")
			play_sound("dead")
			alive = false
			dying = true
			$Ship/Collision.queue_free()
			$Sail.texture = load("res://Sprites/Enemy/EnemySailBroken.png")
			$Cannon.texture = load("res://Sprites/Enemy/Cannon2.png")
			get_parent().on_NPC_death()
		
		if alive:
			boost_velocity = escape_direction*boost_force
			


func _on_ShootTimer_timeout():
	if $Ship.position.distance_to(player_position) <= bullet_range:
		var kappa = int(abs(rad2deg($Cannon.rotation)))%360 #int(abs(rad2deg($Cannon.rotation - $Ship.rotation))) % 360
		kappa = int(abs(rad2deg($Cannon.position.angle_to_point($Ship.position) - $Cannon.rotation))) % 360
		if (kappa <= 150 or kappa >= 210):
			var bullet = bullet_scene.instance()
			add_child(bullet)
			var cannon_vector = Vector2(cos($Cannon.rotation), sin($Cannon.rotation))
			bullet.position = $Cannon.position + cannon_vector * cannon_offset * 1.3
		
		
			bullet.shoot(cannon_vector)
	
			
	
func _set_arrow(angle):
	$DirectionArrow.rotation=angle
	$DirectionArrow.show()
	
func play_sound(type):
	var r = randf()
	if type=="hurt":
		if r<0.5:
			$Sounds/BingBong1.play()
		elif r<0.95:
			$Sounds/BingBong2.play()
		elif r<=1:
			$Sounds/YellowFoams.play()
	elif type == "dead":
		if r<0.5:
			$Sounds/FuckYaLife.play()
		elif r<=1:
			$Sounds/SteveJobs.play()
			
			
			
			
			
			
			
			
			
			
			
			
			
			
			
			
