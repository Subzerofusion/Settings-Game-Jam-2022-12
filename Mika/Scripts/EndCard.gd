extends Node2D

func _ready():
	$Remaining/RemainingTime.text = str(Globals.RemainingTime)


func _on_TextureButton_pressed():
	Globals.Score = 0
	Globals.HardMode = false
	Globals.ScoreLimit = 10
	get_tree().change_scene("res://Scenes/Tutorial.tscn")


func _on_HardMode_pressed():
	Globals.Score = 0
	Globals.HardMode = true
	Globals.ScoreLimit = 1000
	get_tree().change_scene("res://Scenes/Tutorial.tscn")
