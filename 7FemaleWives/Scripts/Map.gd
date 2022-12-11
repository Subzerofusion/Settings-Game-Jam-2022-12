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


func make_islands():
	for x in map_size.x:
		for y in map_size.y:
			var a = noise.get_noise_2d(x,y)
			if a>island_cap:
				$Islands.set_cell(x, y, tile_ids["island"])
	
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
