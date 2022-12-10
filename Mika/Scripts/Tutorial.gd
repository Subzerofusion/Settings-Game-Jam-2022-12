extends Node2D

var offset = 1
var enableWipe = false

func _ready():
	$ReadTimer.connect("timeout", self, "on_ReadTimeOver")
	$WipeTimer.connect("timeout", self, "on_WipeTimerOver")
	$ReadTimer.start()

func on_ReadTimeOver():
	enableWipe = true
	$WipeTimer.start()

func on_WipeTimerOver():
	get_tree().change_scene("res://Scenes/World.tscn")

func _process(delta):
	if(enableWipe):
		offset = offset * 1.1
		$Splash.position.x += offset
