extends Label

func _ready():
	pass # Replace with function body.

func _process(delta):
	self.text = str(Globals.Score)
	self.rect_scale.x = 5
	self.rect_scale.y = 5
