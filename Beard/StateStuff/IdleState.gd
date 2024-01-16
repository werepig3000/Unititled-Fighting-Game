extends State

@export
var SquatState: State
@export
var RunState: State
@export
var JabState: State


func enter()-> void:
	super()
	
func process_input(event: InputEvent) -> State:
	if Input.is_action_pressed("right_%s" % [parent.player_id]) and Input.is_action_pressed("left_%s" % [parent.player_id]):
		return null
	if Input.is_action_pressed("right_%s" % [parent.player_id]) or Input.is_action_pressed("left_%s" % [parent.player_id]):
		return RunState
	if Input.is_action_just_pressed("attack_%s" % [parent.player_id]):
		return JabState
	if Input.is_action_just_pressed("jump_%s" % [parent.player_id]):
		return SquatState
	return null

func process_physics(delta: float) -> State:
	parent.velocity.y += gravity * delta
	parent.velocity.x -= parent.velocity.x * ground_resistance * 30 * delta
	parent.move_and_slide()
	return null

func exit() -> void:
	super()
