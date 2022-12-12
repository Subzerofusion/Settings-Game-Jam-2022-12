extends Control



func _process(_delta):
	if Input.is_action_just_pressed("fullscreen"):
		OS.window_fullscreen = not OS.window_fullscreen
	
func _ready():
	pass



func _on_Start_Button_pressed():
	$SplashScreen.hide()
	$Story.show()
	$Story/Part1.show()
	$Timer.start()
	yield($Timer, "timeout")
	$Story/Part1/Label2.show()
	$Timer.start()
	yield($Timer, "timeout")
	$Story/Part1/Label3.show()
	$Timer.start()
	yield($Timer, "timeout")
	$Story/Part1/Buttons/Accept.show()
	$Story/Part1/Buttons/Deny.show()





func _on_Deny_pressed():
	$Story/Part1/Buttons/Deny.hide()
	$Story/Part1/Buttons/Accept.hide()
	$Story/Part1/Label.hide()
	$Story/Part1/Label3.hide()
	$Story/Part1/Label2.text = "You lived a long and fulfilling life with your six wives, but you could never find the seventh..."
	$Timer.start()
	yield($Timer, "timeout")
	$Story/Part1/Buttons/Back.show()
	

func _on_Back_pressed():
	$Story/Part1/Label.show()
	$Story/Part1/Label2.text = "While sailing the waters around your home in Coney Island you met the Wind God, who had an offer for you."
	$Story/Part1/Label2.hide()
	$Story/Part1/Buttons/Back.hide()
	$Story.hide()
	$SplashScreen.show()


func _on_Accept_pressed():
	$Story/Part1/yes.play()
	$Story/Part1.hide()
	$Story/Part2.show()
	
	$Timer.start()
	yield($Timer, "timeout")
	$Story/Part2/Label2.show()
	$Timer.start()
	yield($Timer, "timeout")
	$Story/Part2/Label3.show()
	$Timer.start()
	yield($Timer, "timeout")
	$Story/Part2/StartDialogue.show()
	
	yield($Story/Part2/StartDialogue, "pressed")
	$Story/Part2.hide()
	$Story/Part3.show()
	
	$Story/Part3/Portrait.texture = load("res://Sprites/Wives/wife1.png")
	$Story/Part3/WifeName.text = "First Female Wife"
	$Story/Part3/WifeDialogue.text = "Good luck on your quest honey, I love you."
	
	yield($Story/Part3/Continue, "pressed")
	$Story/Part3/Portrait.texture = load("res://Sprites/Wives/wife2.png")
	$Story/Part3/WifeName.text = "Second Female Wife"
	$Story/Part3/WifeDialogue.text = "Be careful out there my love, you are a great spouse and I love you."
	
	yield($Story/Part3/Continue, "pressed")
	$Story/Part3/Portrait.texture = load("res://Sprites/Wives/wife3.png")
	$Story/Part3/WifeName.text = "Third Female Wife"
	$Story/Part3/WifeDialogue.text = "I love you dear, you are a wonderful partner and a generous lover."
	
	yield($Story/Part3/Continue, "pressed")
	$Story/Part3/Portrait.texture = load("res://Sprites/Wives/wife4.png")
	$Story/Part3/WifeName.text = "Fourth Female Wife"
	$Story/Part3/WifeDialogue.text = "I will miss you my darling, you are an amazing provider and I want to spend the rest of my life with you."
	
	yield($Story/Part3/Continue, "pressed")
	$Story/Part3/Portrait.texture = load("res://Sprites/Wives/wife5.png")
	$Story/Part3/WifeName.text = "Fifth Female Wife"
	$Story/Part3/WifeDialogue.text = "I will pray for your safe return my sweetie, I love how much you help around the house and make time for all of us."
	
	yield($Story/Part3/Continue, "pressed")
	$Story/Part3/Portrait.texture = load("res://Sprites/Wives/wife6.png")
	$Story/Part3/WifeName.text = "Sixth Female Wife"
	$Story/Part3/WifeDialogue.text = "I adore you sweetheart, please be careful with the giant sword the Wind God put on your ship for some reason."
	
	$Story/Part3/Continue.text = "Start Game"
	yield($Story/Part3/Continue, "pressed")
	get_tree().change_scene("res://Scenes/Main.tscn")
