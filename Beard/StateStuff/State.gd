# Virtual base class for all states.
extends Node
class_name State

@export
var animation_name: String
var run_acceleration = 50
var move_speed: float = 1500
var air_speed: float =  600
var ground_resistance = 1
var air_resistance = 1.2
var gravity: float = 8000
var prejump_velocity: int
var frame = 0
var parent: Player 
var FastFall: int = 1000

func enter() -> void:
	parent.get_node("Animations/AnimationPlayer").play(animation_name)

func exit() -> void:
	frame = 0
	parent.get_node("Animations/AnimationPlayer").stop()
	
func process_input(event: InputEvent) -> State:
	return null

func process_frame(delta: float):
	return null

func process_physics(delta: float) -> State:
	return null
