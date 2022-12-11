extends Control


var wind_direction = 0
var wind_angular_speed = 0.65

onready var npc_scene = load("res://Scenes/NPC.tscn")
var despawn_range = 8000


var showing_map = false
var npcs = []
var max_npcs = 15

var number_of_kills = 0
var seventh_wife_threshold = 10

var center
func _ready():
	center = Vector2($Map.map_size.x * 100 / 2, $Map.map_size.y * 100 / 2)
	$MapCamera.position=center
	$Player/Ship.position = center + Vector2(350, 320)
	
func _process(_delta):
	if Input.is_action_just_pressed("map"):
		if showing_map:
			hide_map()
		else:
			show_map()
	if Input.is_action_just_pressed("die"):
		on_NPC_death()

func _physics_process(delta):
	var to_remove = []
	for npc in npcs:
		if npc.get_node("Ship").position.distance_to($Player/Ship.position) > despawn_range:
			to_remove.append(npc)
		npc.player_position = $Player/Ship.position
		
	
	for npc in to_remove:
		npcs.erase(npc)
		npc.queue_free()
		
		
		
	_update_UI()
	
func show_map():
	
	showing_map = true
	$Map/Sea.hide()
	$Player/Ship/MapIcon.show()
	$UILayer/UI/WindCompass.hide()
	$UILayer/UI/SpeedLabel.hide()
	$UILayer/UI/MaxHP.hide()
	$UILayer/UI/StatsLabel.hide()
	
	$UILayer/UI/ConeyIsland.show()
	for npc in npcs:
		npc.get_node("Ship/MapIcon").show()
		get_tree().paused = true
	$MapCamera.current=true
	
func hide_map():
	showing_map = false
	$Map/Sea.show()
	$Player/Ship/MapIcon.hide()
	$UILayer/UI/WindCompass.show()
	$UILayer/UI/SpeedLabel.show()
	$UILayer/UI/MaxHP.show()
	$UILayer/UI/StatsLabel.show()
	
	$UILayer/UI/ConeyIsland.hide()
	for npc in npcs:
		npc.get_node("Ship/MapIcon").hide()
		get_tree().paused = false
	$Player/Ship/Camera2D.current = true
	
	
	
func _update_UI():
	$UILayer/UI/WindCompass/Needle.rotation = wind_direction
	$UILayer/UI/SpeedLabel.text = "Current Speed: %d" % int($Player.ship_speed)
	$UILayer/UI/StatsLabel.text = "BaseSpeed: %d\nSailSpeed: %.1f\nRegen: %.1f" % [$Player.base_speed, $Player.sail_angular_speed, $Player.heal_factor]
	$UILayer/UI/MaxHP/HP.rect_size.y = $Player.hp * 2
	
func on_NPC_death():
	number_of_kills += 1
	
	if number_of_kills != seventh_wife_threshold:
		var notif = "The Wind God is pleased,\nhe grants you %s"
		
		var r = randi() % 3
		
		if r == 0:
			$Player.base_speed += 10
			notif = notif % "Base Speed"
		elif r == 1:
			$Player.sail_angular_speed += 0.1
			notif = notif % "Sail Speed"
		elif r == 2:
			$Player.heal_factor += 0.5
			notif = notif % "Healing"
		
		$UILayer/UI/NotificationLabel.text = notif
		$UILayer/UI/NotificationLabel.show()
		$UILayer/UI/NotificationLabel/Timer.start()
		yield($UILayer/UI/NotificationLabel/Timer, "timeout")
		$UILayer/UI/NotificationLabel.hide()
	else:
		$UILayer/UI/NotificationLabel.text = "In the wreckage you find a hint of the location\nof the Seventh Female Wife!"
		$UILayer/UI/NotificationLabel.show()
		$UILayer/UI/NotificationLabel/Timer.start()
		yield($UILayer/UI/NotificationLabel/Timer, "timeout")
		$UILayer/UI/NotificationLabel.hide()
		spawn_seventh_wife()
	

func _on_EnemySpawnTimer_timeout():
	if len(npcs) < max_npcs:
		var rang = 3000
		var m = 2000
		
		
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

func spawn_seventh_wife():
	pass
