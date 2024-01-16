extends State

@export
var IdleState: State
@export
var RunState: State
@export
var JabState: State


func enter()-> void:
	super()	

func process_physics(delta: float) -> State:
	parent.velocity.y += gravity * delta
	var movement = Input.get_axis('left_%s' % [parent.player_id], 'right_%s' % [parent.player_id])
	
	if !parent.is_on_floor():
		if abs(parent.velocity.x) >= air_speed:
			parent.velocity.x -= parent.velocity.x * air_resistance * delta
			if sign(movement) != sign(parent.velocity.x):
				parent.velocity.x += movement * air_speed * 20 * delta
				
		else:
			parent.velocity.x -= parent.velocity.x * air_resistance * delta
			parent.velocity.x += movement * air_speed * 20 * delta
	
	parent.move_and_slide()

	if Input.is_action_just_pressed("attack_%s" % [parent.player_id]):
		return JabState
	
	if Input.is_action_just_pressed("down_%s" % [parent.player_id]):
		parent.velocity.y = FastFall
	
	if parent.is_on_floor():
		if movement != 0:
			return RunState
		return IdleState
	return null

func exit() -> void:
	super()
