extends Node

export(PackedScene) var mob_scene

func _process(delta):
	pass

func _ready():
	for item in [1,2,3,4,5]:
		add_mob()
		
	$MobTimer.connect("timeout", self, "_on_Timer_timeout")
	$MobTimer.start()

func add_mob():
	var mob_spawn_location = get_node("LeftPath/MobSpawnLocation")
	var mob = mob_scene.instance()
	mob_spawn_location.offset = randi()
	mob.position = mob_spawn_location.position
	add_child(mob)

func _on_Timer_timeout():
	var newTime = $MobTimer.wait_time *0.99
	if(newTime <= 0.5):
		newTime = 0.5
	$MobTimer.wait_time = newTime
	add_mob()
	

#Killbox
func _on_Area2D_body_entered(body):
	body.queue_free()

#Score Zone
func _on_ScoreZone_body_entered(body):
	if(body.name != "Player"):
		body.queue_free()
		Globals.Score += 1
		
