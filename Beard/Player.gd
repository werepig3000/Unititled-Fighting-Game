extends CharacterBody2D
class_name Player

@onready
var animations = $Animations

@onready
var state_machine = $FSM

@export
var player_id = 1

func _ready():
	state_machine.init(self)

func _unhandled_input(event: InputEvent) -> void:
	state_machine.process_input(event)

func _physics_process(delta: float) -> void:
	state_machine.process_physics(delta)

func _process(delta: float) -> void:
	state_machine.process_frame(delta)
