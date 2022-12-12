extends Node2D


var tile_ids = {}
var map_size = Vector2(96*4, 54*4)

var island_cap = 0.5
var noise 
var coney_island = [[1,1,1,1,1,1],
					[1,1,1,1,1,1],
					[0,1,1,1,1,0],
					[0,0,0,0,0,0]]
var house_pos = Vector2(3,2)
var seventh_wife_radius = 1000
var instagram_location = Vector2(200, 200)
func _ready():
	
	
	for i in $Sea/Layer1.tile_set.get_tiles_ids():
		tile_ids[$Sea/Layer1.tile_set.tile_get_name(i)] = i
		
	_draw_sea()
	randomize()
	noise = OpenSimplexNoise.new()
	noise.seed = randi()
	noise.octaves = 1
	noise.period = 12
	
	
	make_islands()
	_draw_border()
	$Sea/AnimationPlayer.play("seamovement")

func check_pos(pos):
	var tile_pos = $Islands.world_to_map(pos)
	
	if tile_pos.x < 0 or tile_pos.x > map_size.x or tile_pos.y < 0 or tile_pos.y > map_size.y:
		return false
	return $Islands.get_cell(tile_pos.x, tile_pos.y) == -1

func select_seventh_wife_spawn_location(player_pos):
	var x = randi() % int(map_size.x - 20) + 10
	var y = randi() % int(map_size.y - 20) + 10
	var world_pos = $Islands.map_to_world(Vector2(x, y))
	while not(check_pos(world_pos)) or player_pos.distance_to(world_pos) < seventh_wife_radius:
		x = randi() % int(map_size.x - 20) + 10
		y = randi() % int(map_size.y - 20) + 10
		world_pos = $Islands.map_to_world(Vector2(x, y))
	$SeventhWifeIcon.position = world_pos
	return world_pos

func _on_border(x, y):
	var border = 50
	return x > border and x < map_size.x - border and y > border and y < map_size.y - border

func make_islands():
	
	
	
	var min_loc = Vector2(0,0)
	var m = 999999999
	for x in map_size.x:
		for y in map_size.y:
			var a = noise.get_noise_2d(x,y)
			if a>island_cap:
				$Islands.set_cell(x, y, tile_ids["island"])
			elif a < m and not _on_border(x, y):
				m = a
				min_loc = Vector2(x, y)
	
	instagram_location = min_loc
	print(min_loc)
	$Instagram.position = $Islands.map_to_world(min_loc)
	
	var center = Vector2(int(map_size.x/2), int(map_size.y/2))
	for y in len(coney_island):
		for x in len(coney_island[0]):
			if coney_island[y][x] == 1:
				 $Islands.set_cell(center.x + x, center.y + y, tile_ids["island"])
			else:
				$Islands.set_cell(center.x + x, center.y + y, -1)
		
	$Islands.update_bitmask_region(Vector2(0,0), Vector2(map_size.x, map_size.y))
	var house_coord = center + house_pos 
	$Islands.set_cell(house_coord.x, house_coord.y, tile_ids["coney_island"])
	$Islands.set_cell(house_coord.x+1, house_coord.y, -1)
	
func _draw_sea():
	
		
	for i in range(0, map_size.x, 2):
		for j in range(0, map_size.y, 2):
			$Sea/Layer1.set_cell(i, j, tile_ids["sea1"])
			$Sea/Layer2.set_cell(i, j, tile_ids["sea2"])
			
	
		
func _draw_border():
	for i in range(map_size.x):
		$Islands.set_cell(i, 0, -1)
		$Islands.set_cell(i, 1, -1)
		$Islands.set_cell(i, map_size.y-2, -1)
		$Islands.set_cell(i, map_size.y-1, -1)

		if i % 2 == 0:
			$Islands.set_cell(i, 0, tile_ids["rock"])
			$Islands.set_cell(i, map_size.y-2, tile_ids["rock"])
			
	for j in range(map_size.y):
		$Islands.set_cell(0, j, -1)
		$Islands.set_cell(1, j, -1)
		$Islands.set_cell(map_size.x-2,j, -1)
		$Islands.set_cell(map_size.x-1,j, -1)

		if j % 2 == 0:
			$Islands.set_cell(0, j, tile_ids["rock"])
			$Islands.set_cell(map_size.x-2, j,tile_ids["rock"])	


func generate_instagram():
	$Instagram.show()
	return $Instagram.position
#	var map_pos = instagram_location
#	var out_border = 50
#	var thickness = 6
#	var radius = 10
#
#	var x_min = map_pos.x - out_border
#	var x_max = map_pos.x + out_border
#	var y_min = map_pos.y - out_border
#	var y_max = map_pos.y + out_border
#	print(x_min, x_max)
#
#	for x in range(x_min, x_max):
#		for y in range(y_min, y_max):
#			if  x <= x_min + thickness or x >= x_max - thickness* 2 and y <= y_min + thickness or y >= y_max - thickness:
#				$Islands.set_cell(x, y, tile_ids["island"])
#
#			elif map_pos.distance_to(Vector2(x,y)) >= radius - thickness/2 and map_pos.distance_to(Vector2(x,y)) <= radius - thickness/2:
#				$Islands.set_cell(x, y, tile_ids["island"])
#
#			else:
#				$Islands.set_cell(x, y, -1)
#	$Islands.update_bitmask_region(Vector2(0,0), Vector2(map_size.x, map_size.y))
#	pass

