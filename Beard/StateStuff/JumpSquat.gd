extends State

@export
var S_HopState: State
@export
var F_HopState: State

var jump_squat = 6
var short_hop_executed = false

func enter()-> void:
	super()

func process_physics(delta: float) -> State:
	var movement = Input.get_axis('left_%s' % [parent.player_id], 'right_%s' % [parent.player_id])

	frame += 1
	if movement != 0:
		parent.get_node("Animations").flip_h = movement < 0
	
	if abs(parent.velocity.x) >= 500:
		#print(frame, "friction: ", parent.velocity.x)
		parent.velocity.x -= movement * ground_resistance * delta
	else:
		#print(frame, " no friction: ", parent.velocity.x)
		parent.velocity.x = movement * move_speed
	parent.move_and_slide()
	
	if Input.is_action_just_released("jump_%s" % [parent.player_id]):
		short_hop_executed = true
	
	if frame == jump_squat:
		frame = 0
		if short_hop_executed == true:
			short_hop_executed = false
			return S_HopState
		else:
			return F_HopState
	else:
		return null

func exit() -> void:
	super()
