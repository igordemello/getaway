extends CharacterBody3D

@export_category("Settings Player")
@export var speed = 5.0
@export var jump_force = 4.5

@export_category("Settings Mouse")
@export var mouse_sensitivity := 0.2
@export var camera_limite_down := -80.0
@export var camera_limite_up := 60

@onready var player: CharacterBody3D = $"."
var cam_ver := 0.0

func _ready() -> void:
	Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)

func _input(event: InputEvent) -> void:
	if event is InputEventMouseMotion:
		rotate_y(deg_to_rad(-event.relative.x * mouse_sensitivity))
		
		cam_ver -= event.relative.y * mouse_sensitivity
		cam_ver = clamp(cam_ver, camera_limite_down, camera_limite_up)
		$head/vertical.rotation_degrees.x = cam_ver
		
	if Input.is_action_just_pressed("ui_cancel"):
		Input.set_mouse_mode(Input.MOUSE_MODE_VISIBLE)

func _process(delta: float) -> void:
	print(player.position.y)
	if player.position.y <= -20:
		player.position.y = 0
		player.position.x = 0
		player.position.z = 0

func _physics_process(delta: float) -> void:
	# Add the gravity.
	if not is_on_floor():
		velocity += get_gravity() * delta

	# Handle jump.
	if Input.is_action_just_pressed("ui_accept") and is_on_floor():
		velocity.y = jump_force

	# Get the input direction and handle the movement/deceleration.
	# As good practice, you should replace UI actions with custom gameplay actions.
	var input_dir := Input.get_vector("ui_left", "ui_right", "ui_up", "ui_down")
	var direction := (transform.basis * Vector3(input_dir.x, 0, input_dir.y)).normalized()
	if direction:
		velocity.x = direction.x * speed
		velocity.z = direction.z * speed
	else:
		velocity.x = move_toward(velocity.x, 0, speed)
		velocity.z = move_toward(velocity.z, 0, speed)

	move_and_slide()
