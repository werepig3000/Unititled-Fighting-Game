extends CharacterBody2D
class_name Beard

func _process(delta):
	pass

func _physics_process(delta):
	if velocity.x == 0:
		$Animations.play("Idle")
