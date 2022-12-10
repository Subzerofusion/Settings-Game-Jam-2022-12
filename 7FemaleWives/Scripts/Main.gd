extends Control


var wind_direction = 0
var wind_angular_speed = 0.65


func _ready():
	var p = Vector2($Map.map_size.x * 100 / 2, $Map.map_size.y * 100 / 2)
	$Player/Ship.position = p
	$NPC/Ship.position = p
	$NPC/Ship.position.x += 100
	pass # Replace with function body.

func _physics_process(delta):
	$NPC.player_position = $Player/Ship.position

	_update_UI()
	
func _update_compass_angle():
	$CanvasLayer/UI/WindCompass/Needle.rotation = wind_direction
	
func _update_speed():
	$CanvasLayer/UI/SpeedLabel.text = "Current Speed: %d" % int($Player.ship_speed)
	
func _update_UI():
	_update_compass_angle()
	_update_speed()
