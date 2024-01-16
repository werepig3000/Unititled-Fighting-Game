extends State

@export
var IdleState: State
@export
var RunState: State
@export
var FallState: State
@export
var SquatState: State


@export var cancel_frames = 17
@export var max_frames = 25
var was_in_air = false

func enter()-> void:
	super()
	
func process_physics(delta):
	var movement = Input.get_axis('left_%s' % [parent.player_id], 'right_%s' % [parent.player_id])
	frame += 1

	if parent.velocity.y > -200:
		if Input.is_action_just_pressed("down_%s" % [parent.player_id]):
			print("FastFall")
			parent.velocity.y = FastFall
	
	if parent.is_on_floor() == false:
		was_in_air = true
	print(was_in_air)
	parent.velocity.y += gravity * delta
	if parent.is_on_floor():
		parent.velocity.x -= parent.velocity.x * ground_resistance * 30 * delta
	else:
		if abs(parent.velocity.x) >= air_speed:
			parent.velocity.x -= parent.velocity.x * air_resistance * 2 * delta
		if sign(movement) != sign(parent.velocity.x):
			parent.velocity.x += movement * air_speed * 10 * delta
				
		else:
			parent.velocity.x -= parent.velocity.x * air_resistance * delta
			parent.velocity.x += movement * air_speed * 10 * delta
	
	parent.move_and_slide()
	
	if was_in_air:
		if parent.is_on_floor():
			was_in_air = false
			return IdleState
	
	if frame == cancel_frames:
		if parent.is_on_floor():
			if movement != 0:
				return RunState
			elif Input.is_action_just_pressed("jump_%s" % [parent.player_id]):
				return SquatState
	
	if frame == max_frames:
		if parent.is_on_floor():
			if movement != 0:
				return RunState
			else:
				return IdleState
		else:
			return FallState
	return null

func process_frame(delta) -> State:
	return null
	
func exit() -> void:
	super()
