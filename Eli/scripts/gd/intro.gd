extends Label


# Declare member variables here. Examples:
# var a = 2
# var b = "text"
export var action = ""

# Called when the node enters the scene tree for the first time.
func _ready():
	pass


# Called every frame. 'delta' is the elapsed time since the previous frame.
#func _process(delta):
#	pass

func _input(event):
	if event.is_action_pressed(action):
		visible=false
		
