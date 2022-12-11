extends Control


var wind_direction = 0
var wind_angular_speed = 0.65

onready var npc_scene = load("res://Scenes/NPC.tscn")

var showing_map = false
var npcs = []
var center
func _ready():
	center = Vector2($Map.map_size.x * 100 / 2, $Map.map_size.y * 100 / 2)
	$MapCamera.position=center
	$Player/Ship.position = center
	
	

func _physics_process(delta):
	for npc in npcs:
		npc.player_position = $Player/Ship.position
		
	if Input.is_action_just_pressed("map"):
		if showing_map:
			hide_map()
		else:
			show_map()
	
		
		
		
	_update_UI()
	
func show_map():
	
	showing_map = true
	$Map/Sea.hide()
	$MapCamera.current=true
	
func hide_map():
	showing_map = false
	$Map/Sea.show()
	$Player/Ship/Camera2D.current = true
	

func _update_compass_angle():
	$UILayer/UI/WindCompass/Needle.rotation = wind_direction
	
func _update_speed():
	$UILayer/UI/SpeedLabel.text = "Base Speed: %d\nCurrent Speed: %d" % [int($Player.base_speed), int($Player.ship_speed)]
	
func _update_UI():
	_update_compass_angle()
	_update_speed()
	
func on_NPC_death():
	print_debug("somebody died")

func reward_go_faster():
	$Player.base_speed += 5

func _on_EnemySpawnTimer_timeout():
	var rang = 200
	var m = 500
	
	
	var alpha = deg2rad(randi() % 360)
	var dir = Vector2(cos(alpha), sin(alpha))
	var dis = (randi() % rang) + m
	var pos = dir * dis
	while not $Map.check_pos(pos):
		alpha = deg2rad(randi() % 360)
		dir = Vector2(cos(alpha), sin(alpha))
		dis = (randi() % rang) + m
		pos = dir * dis
		pos += $Player/Ship.position
		
	var npc = npc_scene.instance()
	add_child(npc)
	npc.spawn(pos)
	npcs.append(npc)
