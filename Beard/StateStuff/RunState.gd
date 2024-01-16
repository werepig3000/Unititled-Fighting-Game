extends State

@export
var IdleState: State
@export
var SquatState: State
@export
var JabState: State
@export
var initial_dash = 1750

func enter()-> void:
	super()
	
func process_input(event: InputEvent) -> State:
	if Input.is_action_just_pressed('jump_1') and parent.is_on_floor():
		return SquatState
	if Input.is_action_just_pressed("attack_%s" % [parent.player_id]):
		return JabState
	return null
	
	
func process_physics(delta: float) -> State:
	var direction = Input.get_axis('left_%s' % [parent.player_id], 'right_%s' % [parent.player_id])
	
	if direction != 0:
		parent.get_node("Animations").flip_h = direction < 0
	else:
		return IdleState
	parent.velocity.y += gravity * delta
	frame += 1
	if frame <= 2:
		parent.velocity.x = initial_dash * sign(direction) #if on controller this will be variable so need to change for controller support FIXED
	elif abs(parent.velocity.x) > move_speed:
		parent.velocity.x -= sign(direction) * ground_resistance * 3000 * delta
	else:
		parent.velocity.x = direction *  move_speed
	
	if Input.is_action_pressed("left_%s" % [parent.player_id]) and parent.velocity.x > 0:
		parent.velocity.x += direction * move_speed
	if Input.is_action_pressed("right_%s" % [parent.player_id]) and parent.velocity.x < 0:
		parent.velocity.x += direction * move_speed
	
	parent.move_and_slide()
	return null

func exit() -> void:
	super()
