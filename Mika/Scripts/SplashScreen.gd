extends Node2D

func _ready():
	pass # Replace with function body.

func _process(delta):
	$Sprite.position.x -= delta * 100
	if($Sprite.position.x < 500):
		$Sprite.position.x = 1280


func _on_TextureButton_pressed():
	get_tree().change_scene("res://Scenes/Tutorial.tscn")
	pass # Replace with function body.
