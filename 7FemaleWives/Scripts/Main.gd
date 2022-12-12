extends Control


var wind_direction = 0
var wind_angular_speed = 0.65

onready var npc_scene = load("res://Scenes/NPC.tscn")
onready var boss_scene = load("res://Scenes/Boss.tscn")

var boss
var boss_active = false

var despawn_range = 5000

var showing_map = false
var npcs = []
var max_npcs = 15

var Enemy_spawn_range = 2000
var minimum_spawn_distance = 2500

var number_of_kills = 0
var seventh_wife_threshold = 10

var paused = true
var story_active = false

var returning_home = false
var listening = false

var center
var coney_island_position = Vector2()
var instagram_position = Vector2()
var instagram_active = false

var first_time = true

func _ready():
	pause()
	center = Vector2($Map.map_size.x * 100 / 2, $Map.map_size.y * 100 / 2)
	$MapCamera.position=center
	coney_island_position = center + Vector2(350, 320)
	$Player/Ship.position = coney_island_position
	

	
	
	
func _process(_delta):
	if Input.is_action_just_pressed("map"):
		
		if showing_map:
			hide_map()
		elif not paused:
			show_map()
			
			
	if Input.is_action_just_pressed("pause"):
		
		if paused:
			close_pause_menu()
			$UILayer/Menu/Notification.hide()
		else:
			open_pause_menu()
			
	if Input.is_action_just_pressed("fullscreen"):
		OS.window_fullscreen = not OS.window_fullscreen
			
	if Input.is_action_just_pressed("debug"):
		if number_of_kills != seventh_wife_threshold :
			number_of_kills -= 1
		on_NPC_death()
	
		
	

func _physics_process(delta):
	var to_remove = []
	for npc in npcs:
		if npc.get_node("Ship").position.distance_to($Player/Ship.position) > despawn_range:
			to_remove.append(npc)
		npc.player_position = $Player/Ship.position
	if boss_active:
		boss.player_position = $Player/Ship.position
		
	for npc in to_remove:
		npcs.erase(npc)
		npc.queue_free()
		
	if returning_home and coney_island_position.distance_to($Player/Ship.position) < 500:
		returning_home = false
		pause()
		$UILayer/Menu/Notification.show()
		$UILayer/Menu/Notification/Label.text = "After you brought home your seventh female wife, the Wind God came to you once again, with one clear message:"
		$UILayer/Menu/Notification/Continue.text = "Listen"
		listening = true
	
		instagram_position = $Map.generate_instagram()
		instagram_active = true
	
	if instagram_active and instagram_position.distance_to($Player/Ship.position) < 500:
		instagram_active = false
		pause()
		$UILayer/Menu/Notification.show()
		$UILayer/Menu/Notification/Label.text = "The Wind God thanks you for reaching him at his Instagram, and tells you that you are now the new Wind God!\nYOU WIN!"
		$UILayer/Menu/Notification/Continue.text = "Free Play"
		
		
	_update_UI()
	
func pause():
	
	paused = true
	get_tree().paused = true
		
func unpause():
	
	paused = false
	get_tree().paused = false
	$UILayer/UI.show()

func open_pause_menu():
	hide_map()
	pause()
	$UILayer/Menu/Pause.show()
	var n = $Player.number_of_wives
	$UILayer/Menu/Pause/Kills.text = "Ships sunk: %d/%d" % [number_of_kills, seventh_wife_threshold]
	$UILayer/Menu/Pause/Kills.show()
	
	
	

func close_pause_menu():
	unpause()
	
	$UILayer/Menu/Pause.hide()
	$UILayer/Menu/Pause/Hint.hide()
	hide_map()
	
	if listening:
		$Sounds/GoToMyInstagram.play()
		listening = false
		
	if first_time:
		$Sounds/Live.play()
		first_time = false
	

func show_map():
	pause()
	showing_map = true
	$Map/Sea.hide()
	$Map/SeventhWifeIcon.show()
	
	$Player/Ship/MapIcon.show()
	$UILayer/UI/WindCompass.hide()
	$UILayer/UI/SpeedLabel.hide()
	$UILayer/UI/MaxHP.hide()
	$UILayer/UI/StatsLabel.hide()
	
	$UILayer/UI/ConeyIsland.show()
	
	if boss_active:
		boss.get_node("Ship/MapIcon").show()
	
	for npc in npcs:
		npc.get_node("Ship/MapIcon").show()
		
	$MapCamera.current=true
	
func hide_map():
	unpause()
	showing_map = false
	$Map/Sea.show()
	$Map/SeventhWifeIcon.hide()
	
	$Player/Ship/MapIcon.hide()
	$UILayer/UI/WindCompass.show()
	$UILayer/UI/SpeedLabel.show()
	$UILayer/UI/MaxHP.show()
	$UILayer/UI/StatsLabel.show()
	
	$UILayer/UI/ConeyIsland.hide()
	
	for npc in npcs:
		npc.get_node("Ship/MapIcon").hide()
	
	if boss_active:
		boss.get_node("Ship/MapIcon").hide()
	
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
			$Player.base_speed += 15
			notif = notif % "Base Speed"
		elif r == 1:
			$Player.sail_angular_speed += 0.4
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
		pause()
		$UILayer/Menu/Notification.show()
		$UILayer/Menu/Notification/Label.text = "You found a hint about the location of the seventh wife in the wreckage! Check your map to see it."
		$UILayer/Menu/Notification/Continue.text = "Continue"
		spawn_seventh_wife()
	
func _on_EnemySpawnTimer_timeout():
	if len(npcs) < max_npcs:
		var rang = Enemy_spawn_range
		var m = minimum_spawn_distance
		
		
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

func on_Boss_death():
	
	$Player.number_of_wives += 1
	if $Player.number_of_wives > 7:
		$Player.base_speed += 200
	
	boss_active = false
	$Player.hp = 100
	$Player/Sounds/SevenFemaleWives.play()
	$Delay.start()
	yield($Delay, "timeout")
	pause()
	$UILayer/Menu/Notification.show()
	$UILayer/Menu/Notification/Label.text = "You rescued your seventh female wife from the wreckage! Bring her home to Coney Island so you can introduce her to the rest of your wives"
	$UILayer/Menu/Notification/Continue.text = "Continue"
	returning_home = true




func spawn_seventh_wife():
	boss = boss_scene.instance()
	add_child(boss)
	var seventh_wife_location = $Map.select_seventh_wife_spawn_location($Player/Ship.position)
	boss.spawn(seventh_wife_location)
	
	boss_active=true
	


func _on_Continue_pressed():
	if paused:
		close_pause_menu()
		$UILayer/Menu/Notification.hide()

func game_over():
	$UILayer/Menu/Notification.show()
	$UILayer/Menu/Notification/Label.text = "You died, leaving behind %d female widows." % $Player.number_of_wives
	$UILayer/Menu/Notification/Continue.text = "Respawn"
	$Player/Ship.position = coney_island_position
	$Player/Ship.rotation = 0
	$Player.hp = $Player.max_hp
	pause()
	


func _on_Quit_pressed():
	$UILayer/Menu/Pause/Hint.show()
