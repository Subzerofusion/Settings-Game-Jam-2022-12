extends Node2D


var tile_ids = {}
var map_size = Vector2(96*4, 54*4)

var island_cap = 0.5
var noise 

func _ready():
	
	
	for i in $Sea/Layer1.tile_set.get_tiles_ids():
		tile_ids[$Sea/Layer1.tile_set.tile_get_name(i)] = i
		
	_draw_sea()
	randomize()
	noise = OpenSimplexNoise.new()
	noise.seed = randi()
	noise.octaves = 1
	noise.period = 12
	
	
	make_island()

func make_island():
	for x in map_size.x:
		for y in map_size.y:
			var a = noise.get_noise_2d(x,y)
			if a>island_cap:
				$Islands.set_cell(x, y, tile_ids["island"])
	
	$Islands.update_bitmask_region(Vector2(0,0), Vector2(map_size.x, map_size.y))

func _draw_sea():
	for i in range(0, map_size.x, 2):
		for j in range(0, map_size.y, 2):
			$Sea/Layer1.set_cell(i, j, tile_ids["sea1"])
			$Sea/Layer2.set_cell(i, j, tile_ids["sea2"])
