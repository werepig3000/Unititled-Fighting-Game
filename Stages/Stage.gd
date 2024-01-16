extends Node2D

func _ready() -> void:
	Engine.max_fps = 60

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	$CurrentFPS.text = "Current FPS: " + str(Engine.get_frames_per_second())
