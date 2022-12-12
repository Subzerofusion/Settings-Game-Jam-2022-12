extends Control



func _process(_delta):
	if Input.is_action_just_pressed("fullscreen"):
		OS.window_fullscreen = not OS.window_fullscreen
	



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
	$Story/Part1/Accept.show()
	$Story/Part1/Deny.show()



func _on_Accept_pressed():
	$Story/Part1/yes.play()
	


func _on_Deny_pressed():
	$Story/Part1/Deny.hide()
	$Story/Part1/Accept.hide()
	$Story/Part1/no.play()
	$Story/Part1/Label.hide()
	$Story/Part1/Label3.hide()
	$Story/Part1/Label2.text = "You lived a long fulfilling life with your six wives, but never could find the seventh..."
	$Timer.start()
	yield($Timer, "timeout")
	
	
	
