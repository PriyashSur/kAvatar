#include"Reme.h"
#include<conio.h>


void Reme::ReadFromMotor()
{
	Serial* readMotor = new Serial("COM3");
	if (readMotor->IsConnected())
	{
		printf("Motor is connected ");
	}
	char incomingData[256] = "";			// don't forget to pre-allocate memory
	//printf("%s\n",incomingData);
	int dataLength = 256;
	int readResult = 0;

	while (readMotor->IsConnected())
	{
		readResult = readMotor->ReadData(incomingData, dataLength);
		printf("Bytes read: (-1 means no data available) %i\n", readResult);

		std::string test(incomingData);

		printf("%s", incomingData);
		test = "";
		Sleep(500);
	}
}

//THE FINALIZE MAIN CODE FOR THE SCAN
void Reme::Start_Inkarne_Motor()
{
	//OPENING THE SERIAL PORT (PORT NAME IS :- COM3)
	Serial* InkarneMotor = new Serial("COM3");
	//CHECK THE MOTOR IS CONNECTED OR NOT 
	if (InkarneMotor->IsConnected())
	{
		printf("Motor is connected ");
	}
	//SEND THE COMMANDS(ONE TIME COMPILATION BEFORE SENDING THE COMMAND COMPILE THE ARDUINO SERVO MOTOR CODE ONCE AND UPLOAD IT INTO THE BOARD)
	//COMMAND FOR THE MOTOR TO ROTATE AND DISPLACE(UP AND DOWN)-1,312,4,7
	InkarneMotor->WriteData("1,312,4,7", 100);
}

void Reme::run(int time, reme_context_t c, reme_image_t volume, reme_image_t aux, reme_image_t depth, reme_viewer_t viewer, reme_sensor_t s)
{
	while (time < 1000 && REME_SUCCESS(reme_sensor_grab(c, s)))
	{
		if (!motor_rotation)break;
		reme_sensor_prepare_images(c, s);
		if (REME_SUCCESS(reme_sensor_track_position(c, s)))
		{
			reme_sensor_update_volume(c, s);
		}
		reme_sensor_get_image(c, s, REME_IMAGE_AUX, aux);
		reme_sensor_get_image(c, s, REME_IMAGE_VOLUME, volume);
		reme_sensor_get_image(c, s, REME_IMAGE_DEPTH, depth);
		reme_viewer_update(c, viewer);

		time += 1;
	}
}


void Reme::start_scan()
{

	//CREATE REME CONTEXT
	reme_context_t c;
	reme_context_create(&c);
	reme_context_set_log_callback(c, reme_default_log_callback, 0);
	// Create a license object
	reme_license_t l;
	reme_license_create(c, &l);
	reme_error_t e = reme_license_authenticate(c, l,license_path);
	// Create options
	reme_options_t o;
	reme_options_create(c, &o);
	reme_context_bind_reconstruction_options(c, o);
	reme_options_set_bool(c, o, "data_integration.use_colors", true);

	//SET BOUNDING BOX VOLUME
	reme_options_set_real(c, o, "volume.minimum_corner.x", -1024);
	reme_options_set_real(c, o, "volume.minimum_corner.y", -2048);
	reme_options_set_real(c, o, "volume.minimum_corner.z", -1024);

	reme_options_set_real(c, o, "volume.maximum_corner.x", 1024);
	reme_options_set_real(c, o, "volume.maximum_corner.y", 1024);
	reme_options_set_real(c, o, "volume.maximum_corner.z", 1024);

	reme_options_set_int(c, o, "volume.resolution.x", 128);
	reme_options_set_int(c, o, "volume.resolution.y", 128);
	reme_options_set_int(c, o, "volume.resolution.z", 128);

	//QUALITY OF THE 3D MESH 
	reme_context_tune_reconstruction_options(c, REME_TUNE_PROFILE_MAX_QUALITY);
	// Compile for OpenCL device using defaults
	reme_context_compile(c);
	// Create a new volume
	reme_volume_t v;
	reme_volume_create(c, &v);
	// Create a new sensor.
	reme_sensor_t s;
	reme_sensor_create(c, "openni;mskinect;file", true, &s);
	reme_sensor_open(c, s);

	//SETTING THE KINECT SENSOR DEFAULT POSITION(REME_SENSOR_POSITION_INFRONT)
	reme_sensor_set_prescan_position(c, s, REME_SENSOR_POSITION_INFRONT);

	// Use color in rendering volume image
	reme_sensor_bind_render_options(c, s, o);
	reme_options_set(c, o, "shade_mode", "SHADE_COLORS");
	reme_sensor_apply_render_options(c, s, o);

	/*CREATE CALIBRATOR USING STANDARD SETTINGS
	reme_calibrator_t calib;
	reme_calibrator_create(c, &calib);*/

	// Create viewer and start reconstruction
	reme_viewer_t viewer;
	reme_viewer_create_image(c, "INKARNE SCANNER", &viewer);
	reme_image_t volume, aux, depth;
	reme_image_create(c, &volume);
	reme_image_create(c, &aux);
	reme_image_create(c, &depth);
	reme_viewer_add_image(c, viewer, aux);
	reme_viewer_add_image(c, viewer, volume);
	reme_viewer_add_image(c, viewer, depth);

	// Perform reconstruction until no more frames are left
	int time = 0;
	bool continue_grabbing = true;
	promise<bool>p;
	auto future = p.get_future();
	thread motor_thread([&p,this]{
		&Reme::Start_Inkarne_Motor;
		p.set_value(true);
	});
	//CHECK WHETHER THE MOTOR THREAD IS ALIVE OR NOT
	auto status = future.wait_for(chrono::milliseconds(0));
	if (status == future_status::ready)
	{
		motor_rotation = false;
	}
	
	//RECONSTRUCTME SCAN 
    /*thread scan_thread(&Reme::run,time, c, volume, aux, depth, viewer, s);
	scan_thread.join();*/
	

	// Close and destroy the sensor, it is not needed anymore
	reme_sensor_close(c, s);
	reme_sensor_destroy(c, &s);
	// Create surface from volume
	reme_surface_t m;
	reme_surface_create(c, &m);
	reme_surface_generate(c, m, v);
	// Remove small unconnected parts
	reme_surface_cleanup(c, m);

	// Fill holes and make mesh watertight 
	reme_surface_poisson(c, m);              

	reme_csg_t csg;
	reme_csg_create(c, v, &csg);

	// Keep the color volume, but clear the geometric volume.
	reme_volume_reset_selectively(c, v, true, false);

	// Insert the watertight mesh
	reme_csg_update_with_surface(c, csg, REME_SET_UNION, m);

	// In case anything is sticking out of the volume (e.g on top), close it now.
	reme_csg_close_volume_borders(c, csg);

	//GENERATING SURFACE
	reme_surface_generate(c, m, v);
	reme_surface_colorize_vertices(c, m, v);
	reme_surface_inpaint_vertices(c, m);

	// Save mesh in .obj format
	reme_surface_save_to_file(c, m, file.c_str());
	// Visualize resulting surface
	reme_viewer_t viewer_surface;
	reme_viewer_create_surface(c, m, "INKARNE SCANNER", &viewer_surface);
	reme_viewer_wait(c, viewer_surface);
	reme_surface_destroy(c, &m);
	// Print pending errors
	reme_context_print_errors(c);
	// Make sure to release all memory acquired
	reme_context_destroy(&c);
	
}

//EXPORTING THE PLY FILE TO OBJ FORMAT
void Reme::exportOBJ()
{
	//CREATE REME CONTEXT
	reme_context_t c;
	reme_context_create(&c);
	//CREATE LICENSE OBJECT FOR LICENSE CHECKING
	reme_license_t l;
	reme_license_create(c, &l);
	reme_error_t e = reme_license_authenticate(c, l,license_path);
	//CREATE REME SURFACE
	reme_surface_t m;
	reme_surface_create(c, &m);
	//LOAD THE PLY FILE WITH SUPPORTED COLORIZE VERTICES
	reme_surface_load_from_file(c, m,file.c_str());
	//CHANGE THE EXTENSION FROM PLY TO OBJ 
	string _file;
	_file = file.substr(0, file.find_last_of("."));
	_file += objExt;
	//EXPORT THE FILE WITH .OBJ FORMAT
	reme_surface_save_to_file(c, m, _file.c_str());
	// Print pending errors
	reme_context_print_errors(c);
	reme_context_destroy(&c);
}




//DIFFERENT RECONSTRUCTME MODES
void Reme::Selfie_Scan()
{
	reme_context_t c;
	reme_context_create(&c);
	reme_context_set_log_callback(c, reme_default_log_callback, 0);
	// Create a license object
	reme_license_t l;
	reme_license_create(c, &l);
	reme_error_t e = reme_license_authenticate(c, l, license_path);

	// Create options
	reme_options_t o;
	reme_options_create(c, &o);

	// Enable colorization
	reme_context_bind_reconstruction_options(c, o);
	reme_options_set_bool(c, o, "data_integration.use_colors", true);


	//SETTING BOUNDING BOX
	reme_options_set_int(c, o, "volume.minimum_corner.x", -1024);
	reme_options_set_int(c, o, "volume.minimum_corner.y", -1024);
	reme_options_set_int(c, o, "volume.minimum_corner.z", -1024);

	reme_options_set_int(c, o, "volume.maximum_corner.x", 1024);
	reme_options_set_int(c, o, "volume.maximum_corner.y", 1024);
	reme_options_set_int(c, o, "volume.maximum_corner.z", 1024);

	reme_options_set_int(c, o, "volume.resolution.x", 128);
	reme_options_set_int(c, o, "volume.resolution.y", 128);
	reme_options_set_int(c, o, "volume.resolution.z", 128);

	// Compile for OpenCL device using defaults
	reme_context_tune_reconstruction_options(c, REME_TUNE_PROFILE_MID_QUALITY);
	reme_context_compile(c);

	// Create a new volume
	reme_volume_t v;
	reme_volume_create(c, &v);

	// Create a new sensor.
	reme_sensor_t s;
	reme_sensor_create(c, "openni;mskinect;file", true, &s);
	reme_sensor_open(c, s);
	reme_sensor_set_prescan_position(c, s, REME_SENSOR_POSITION_INFRONT);

	// Use color in rendering volume image
	reme_sensor_bind_render_options(c, s, o);
	reme_options_set(c, o, "shade_mode", "SHADE_COLORS");
	reme_sensor_apply_render_options(c, s, o);

	// Create viewer and start reconstruction
	reme_viewer_t viewer;
	reme_viewer_create_image(c, "INKARNE SCANNER", &viewer);
	reme_image_t volume, aux;
	reme_image_create(c, &volume);
	reme_image_create(c, &aux);
	reme_viewer_add_image(c, viewer, aux);
	reme_viewer_add_image(c, viewer, volume);

	// Perform reconstruction until one complete rotation is performed
	float prev_pos[16], cur_pos[16];
	float rotation_axis[4] = { 0, 0, 1, 0 };
	reme_sensor_get_position(c, s, prev_pos);
	float angle;
	float sum_turn_angles = 0.f;
	while (fabs(sum_turn_angles) < 2.f * 3.1415f)
	{
		if (!REME_SUCCESS(reme_sensor_grab(c, s)))
		{
			continue;
		}
		reme_sensor_prepare_images(c, s);
		if (REME_SUCCESS(reme_sensor_track_position(c, s)))
		{
			reme_sensor_update_volume(c, s);
			reme_sensor_get_position(c, s, cur_pos);
			reme_transform_get_projected_angle(c, rotation_axis, prev_pos, cur_pos, &angle);
			sum_turn_angles += angle;
			memcpy(prev_pos, cur_pos, 16 * sizeof(float));
		}
		reme_sensor_get_image(c, s, REME_IMAGE_AUX, aux);
		reme_sensor_get_image(c, s, REME_IMAGE_VOLUME, volume);
		reme_viewer_update(c, viewer);
	}
	// Close and destroy the sensor, it is not needed anymore
	reme_sensor_close(c, s);
	reme_sensor_destroy(c, &s);

	// Create surface from volume
	reme_surface_t m;
	reme_surface_create(c, &m);
	reme_surface_generate(c, m, v);


	// AUTOMATIC POST PROCESSING

	// Remove small unconnected parts
	reme_surface_cleanup(c, m);

	// Fill holes and make mesh watertight
	reme_surface_poisson(c, m);

	reme_csg_t csg;
	reme_csg_create(c, v, &csg);

	// Keep the color volume, but clear the geometric volume.
	reme_volume_reset_selectively(c, v, true, false);

	// Insert the watertight mesh
	reme_csg_update_with_surface(c, csg, REME_SET_UNION, m);

	// In case anything is sticking out of the volume (e.g on top), close it now.
	reme_csg_close_volume_borders(c, csg);

	// Generate the final mesh.
	reme_surface_generate(c, m, v);
	reme_surface_colorize_vertices(c, m, v);
	reme_surface_inpaint_vertices(c, m);

	// Save mesh
	reme_surface_save_to_file(c, m, file.c_str());

	// Visualize resulting surface
	reme_viewer_t viewer_surface;
	reme_viewer_create_surface(c, m, "INKARNE SCAN", &viewer_surface);
	reme_viewer_wait(c, viewer_surface);
	reme_surface_destroy(c, &m);

	// Print pending errors
	reme_context_print_errors(c);

	// Make sure to release all memory acquired
	reme_context_destroy(&c);
}

void Reme::Normal_Scan()
{
	reme_context_t c;
	reme_context_create(&c);
	reme_context_set_log_callback(c, reme_default_log_callback, 0);
	// Create a license object
	reme_license_t l;
	reme_license_create(c, &l);
	reme_error_t e = reme_license_authenticate(c, l, license_path);
	// Create options
	reme_options_t o;
	reme_options_create(c, &o);
	reme_context_bind_reconstruction_options(c, o);
	reme_options_set_bool(c, o, "data_integration.use_colors", true);

	reme_options_set_int(c, o, "volume.minimum_corner.x", -1024);
	reme_options_set_int(c, o, "volume.minimum_corner.y", -1024);
	reme_options_set_int(c, o, "volume.minimum_corner.z", -1024);

	reme_options_set_int(c, o, "volume.maximum_corner.x", 1024);
	reme_options_set_int(c, o, "volume.maximum_corner.y", 1024);
	reme_options_set_int(c, o, "volume.maximum_corner.z", 1024);

	reme_options_set_int(c, o, "volume.resolution.x", 128);
	reme_options_set_int(c, o, "volume.resolution.y", 128);
	reme_options_set_int(c, o, "volume.resolution.z", 128);


	reme_context_tune_reconstruction_options(c, REME_TUNE_PROFILE_MID_QUALITY);
	// Compile for OpenCL device using defaults
	reme_context_compile(c);
	// Create a new volume
	reme_volume_t v;
	reme_volume_create(c, &v);
	// Create a new sensor.
	reme_sensor_t s;
	reme_sensor_create(c, "openni;mskinect;file", true, &s);
	reme_sensor_open(c, s);

	//reme_sensor_set_prescan_position(c, s, REME_SENSOR_POSITION_INFRONT);

	// Use color in rendering volume image
	reme_sensor_bind_render_options(c, s, o);
	reme_options_set(c, o, "shade_mode", "SHADE_COLORS");
	reme_sensor_apply_render_options(c, s, o);

	// Create viewer and start reconstruction
	reme_viewer_t viewer;
	reme_viewer_create_image(c, "INKARNE SCANNER", &viewer);
	reme_image_t volume, aux, depth;
	reme_image_create(c, &volume);
	reme_image_create(c, &aux);
	reme_image_create(c, &depth);
	reme_viewer_add_image(c, viewer, aux);
	reme_viewer_add_image(c, viewer, volume);
	reme_viewer_add_image(c, viewer, depth);

	// Perform reconstruction until no more frames are left
	int time = 0;
	while (time < 100 && REME_SUCCESS(reme_sensor_grab(c, s)))
	{
		reme_sensor_prepare_images(c, s);
		if (REME_SUCCESS(reme_sensor_track_position(c, s)))
		{
			reme_sensor_update_volume(c, s);
		}
		reme_sensor_get_image(c, s, REME_IMAGE_AUX, aux);
		reme_sensor_get_image(c, s, REME_IMAGE_VOLUME, volume);
		reme_sensor_get_image(c, s, REME_IMAGE_DEPTH, depth);
		reme_viewer_update(c, viewer);

		time += 1;
	}

	// Close and destroy the sensor, it is not needed anymore
	reme_sensor_close(c, s);
	reme_sensor_destroy(c, &s);
	// Create surface from volume
	reme_surface_t m;
	reme_surface_create(c, &m);
	reme_surface_generate(c, m, v);
	// Remove small unconnected parts
	reme_surface_cleanup(c, m);

	// Fill holes and make mesh watertight
	reme_surface_poisson(c, m);

	reme_csg_t csg;
	reme_csg_create(c, v, &csg);
	reme_volume_reset_selectively(c, v, true, false);
	reme_csg_update_with_surface(c, csg, REME_SET_UNION, m);
	reme_csg_close_volume_borders(c, csg);
	reme_surface_generate(c, m, v);
	reme_surface_colorize_vertices(c, m, v);
	reme_surface_inpaint_vertices(c, m);

	// Save mesh
	reme_surface_save_to_file(c, m, file.c_str());
	// Visualize resulting surface
	reme_viewer_t viewer_surface;
	reme_viewer_create_surface(c, m, "This is ReconstructMeSDK", &viewer_surface);
	reme_viewer_wait(c, viewer_surface);
	reme_surface_destroy(c, &m);
	// Print pending errors
	reme_context_print_errors(c);
	// Make sure to release all memory acquired
	reme_context_destroy(&c);
}

//WORKING SELFIE MODE WITH TIMER SETTINGS 
//UNLIMITED NO OF ROTATION 
void Reme::selfie_timed()
{
	reme_context_t c;
	reme_context_create(&c);
	reme_context_set_log_callback(c, reme_default_log_callback, 0);
	// Create a license object
	reme_license_t l;
	reme_license_create(c, &l);
	reme_error_t e = reme_license_authenticate(c, l, license_path);

	// Create options
	reme_options_t o;
	reme_options_create(c, &o);

	// Enable colorization
	reme_context_bind_reconstruction_options(c, o);
	reme_options_set_bool(c, o, "data_integration.use_colors", true);


	//SETTING BOUNDING BOX
	reme_options_set_int(c, o, "volume.minimum_corner.x", -1024);
	reme_options_set_int(c, o, "volume.minimum_corner.y", -1024);
	reme_options_set_int(c, o, "volume.minimum_corner.z", -1024);

	reme_options_set_int(c, o, "volume.maximum_corner.x", 1024);
	reme_options_set_int(c, o, "volume.maximum_corner.y", 1024);
	reme_options_set_int(c, o, "volume.maximum_corner.z", 1024);

	reme_options_set_int(c, o, "volume.resolution.x", 128);
	reme_options_set_int(c, o, "volume.resolution.y", 128);
	reme_options_set_int(c, o, "volume.resolution.z", 128);

	// Compile for OpenCL device using defaults
	reme_context_tune_reconstruction_options(c, REME_TUNE_PROFILE_MID_QUALITY);
	reme_context_compile(c);

	// Create a new volume
	reme_volume_t v;
	reme_volume_create(c, &v);

	// Create a new sensor.
	reme_sensor_t s;
	reme_sensor_create(c, "openni;mskinect;file", true, &s);
	reme_sensor_open(c, s);
	reme_sensor_set_prescan_position(c, s, REME_SENSOR_POSITION_INFRONT);

	// Use color in rendering volume image
	reme_sensor_bind_render_options(c, s, o);
	reme_options_set(c, o, "shade_mode", "SHADE_COLORS");
	reme_sensor_apply_render_options(c, s, o);

	// Create viewer and start reconstruction
	reme_viewer_t viewer;
	reme_viewer_create_image(c, "INKARNE SCANNER", &viewer);
	reme_image_t volume, aux;
	reme_image_create(c, &volume);
	reme_image_create(c, &aux);
	reme_viewer_add_image(c, viewer, aux);
	reme_viewer_add_image(c, viewer, volume);

	// Perform reconstruction until one complete rotation is performed
	float prev_pos[16], cur_pos[16];
	float rotation_axis[4] = { 0, 0, 1, 0 };
	reme_sensor_get_position(c, s, prev_pos);
	float angle;
	float sum_turn_angles = 0.f;
	int time = 0;
	while (time < 1400 && REME_SUCCESS(reme_sensor_grab(c, s)))
	{
		reme_sensor_prepare_images(c, s);
		if (REME_SUCCESS(reme_sensor_track_position(c, s)))
		{
			reme_sensor_update_volume(c, s);
			reme_sensor_get_position(c, s, cur_pos);
			reme_transform_get_projected_angle(c, rotation_axis, prev_pos, cur_pos, &angle);
			sum_turn_angles += angle;
			memcpy(prev_pos, cur_pos, 16 * sizeof(float));
		}
		reme_sensor_get_image(c, s, REME_IMAGE_AUX, aux);
		reme_sensor_get_image(c, s, REME_IMAGE_VOLUME, volume);
		reme_viewer_update(c, viewer);

		time += 1;
	}
	// Close and destroy the sensor, it is not needed anymore
	reme_sensor_close(c, s);
	reme_sensor_destroy(c, &s);

	// Create surface from volume
	reme_surface_t m;
	reme_surface_create(c, &m);
	reme_surface_generate(c, m, v);


	// AUTOMATIC POST PROCESSING

	// Remove small unconnected parts
	reme_surface_cleanup(c, m);

	// Fill holes and make mesh watertight
	reme_surface_poisson(c, m);

	reme_csg_t csg;
	reme_csg_create(c, v, &csg);

	// Keep the color volume, but clear the geometric volume.
	reme_volume_reset_selectively(c, v, true, false);

	// Insert the watertight mesh
	reme_csg_update_with_surface(c, csg, REME_SET_UNION, m);

	// In case anything is sticking out of the volume (e.g on top), close it now.
	reme_csg_close_volume_borders(c, csg);

	// Generate the final mesh.
	reme_surface_generate(c, m, v);
	reme_surface_colorize_vertices(c, m, v);
	reme_surface_inpaint_vertices(c, m);

	// Save mesh
	reme_surface_save_to_file(c, m, file.c_str());

	// Visualize resulting surface
	reme_viewer_t viewer_surface;
	reme_viewer_create_surface(c, m, "INKARNE SCAN", &viewer_surface);
	reme_viewer_wait(c, viewer_surface);
	reme_surface_destroy(c, &m);

	// Print pending errors
	reme_context_print_errors(c);

	// Make sure to release all memory acquired
	reme_context_destroy(&c);
}

void Reme::selfie_start_scan_backup()
{

	//SAVE FILE DATA BASED ON CURRENT TIME
	/*time_t t;
	t = time(0);
	struct tm* now;
	now = localtime(&t);
	const char* ext = ".ply";
	string filename = "E:\\InkarneInternal\\Processingfolder\\";
	int day = now->tm_mday;
	int mon = now->tm_mon;
	int year = now->tm_year + 1900;
	int hh = now->tm_hour;
	int mm = now->tm_min;
	int sec = now->tm_sec + 1;
	stringstream ss;
	ss << day << "-" << mon << "-" << year << "_" << hh << ":" << mm << ":" << sec;
	filename.append("3D_SCAN_");
	filename.append(ss.str());
	filename.append(ext);

*/

	//RECONSTRUCTME INITIALIZE DATA
	reme_context_t c;
	reme_context_create(&c);
	reme_context_set_log_callback(c, reme_default_log_callback, 0);

	//Create a license object
	reme_license_t l;
	reme_license_create(c, &l);
	reme_error_t e = reme_license_authenticate(c, l,license_path);

	// Create options
	reme_options_t o;
	reme_options_create(c, &o);

	// Enable colorization
	reme_context_bind_reconstruction_options(c, o);
	reme_options_set_bool(c, o, "data_integration.use_colors", true);


	//SETTING BOUNDING BOX
	reme_options_set_int(c, o, "volume.minimum_corner.x", -1024);
	reme_options_set_int(c, o, "volume.minimum_corner.y", -1024);
	reme_options_set_int(c, o, "volume.minimum_corner.z", -1024);

	reme_options_set_int(c, o, "volume.maximum_corner.x", 1024);
	reme_options_set_int(c, o, "volume.maximum_corner.y", 1024);
	reme_options_set_int(c, o, "volume.maximum_corner.z", 1024);

	reme_options_set_int(c, o, "volume.resolution.x", 128);
	reme_options_set_int(c, o, "volume.resolution.y", 128);
	reme_options_set_int(c, o, "volume.resolution.z", 128);

	// Compile for OpenCL device using defaults
	reme_context_tune_reconstruction_options(c, REME_TUNE_PROFILE_MAX_QUALITY);
	reme_context_compile(c);

	// Create a new volume
	reme_volume_t v;
	reme_volume_create(c, &v);

	// Create a new sensor.
	reme_sensor_t s;
	reme_sensor_create(c, "openni;mskinect;file", true, &s);
	reme_sensor_open(c, s);
	reme_sensor_set_prescan_position(c, s, REME_SENSOR_POSITION_INFRONT);

	// Use color in rendering volume image
	reme_sensor_bind_render_options(c, s, o);
	reme_options_set(c, o, "shade_mode", "SHADE_COLORS");
	reme_sensor_apply_render_options(c, s, o);

	// Create viewer and start reconstruction
	reme_viewer_t viewer;
	reme_viewer_create_image(c, "INKARNE SCANNER", &viewer);
	reme_image_t volume, aux;
	reme_image_create(c, &volume);
	reme_image_create(c, &aux);
	reme_viewer_add_image(c, viewer, aux);
	reme_viewer_add_image(c, viewer, volume);

	// Perform reconstruction until one complete rotation is performed(FOR SELFIE SCAN 360 ONE ROTATION)
	float prev_pos[16], cur_pos[16];
	float rotation_axis[4] = { 0, 0, 1, 0 };
	reme_sensor_get_position(c, s, prev_pos);
	float angle;
	float sum_turn_angles = 0.f;
	int time = 0;
	while (time < 5000 && REME_SUCCESS(reme_sensor_grab(c, s)))
	{
		reme_sensor_prepare_images(c, s);
		if (REME_SUCCESS(reme_sensor_track_position(c, s)))
		{
			reme_sensor_update_volume(c, s);
			reme_sensor_get_position(c, s, cur_pos);
			reme_transform_get_projected_angle(c, rotation_axis, prev_pos, cur_pos, &angle);
			sum_turn_angles += angle;
			memcpy(prev_pos, cur_pos, 16 * sizeof(float));
		}
		reme_sensor_get_image(c, s, REME_IMAGE_AUX, aux);
		reme_sensor_get_image(c, s, REME_IMAGE_VOLUME, volume);
		reme_viewer_update(c, viewer);

		time += 1;
	}
	// Close and destroy the sensor, it is not needed anymore
	reme_sensor_close(c, s);
	reme_sensor_destroy(c, &s);

	// Create surface from volume
	reme_surface_t m;
	reme_surface_create(c, &m);
	reme_surface_generate(c, m, v);


	// AUTOMATIC POST PROCESSING

	// Remove small unconnected parts
	reme_surface_cleanup(c, m);

	// Fill holes and make mesh watertight
	reme_surface_poisson(c, m);

	reme_csg_t csg;
	reme_csg_create(c, v, &csg);

	// Keep the color volume, but clear the geometric volume.
	reme_volume_reset_selectively(c, v, true, false);

	// Insert the watertight mesh
	reme_csg_update_with_surface(c, csg, REME_SET_UNION, m);

	// In case anything is sticking out of the volume (e.g on top), close it now.
	reme_csg_close_volume_borders(c, csg);

	// Generate the final mesh.
	reme_surface_generate(c, m, v);
	reme_surface_colorize_vertices(c, m, v);
	reme_surface_inpaint_vertices(c, m);

	// Save mesh

	reme_surface_save_to_file(c, m, file.c_str());

	// Visualize resulting surface
	reme_viewer_t viewer_surface;
	reme_viewer_create_surface(c, m, "INKARNE SCAN", &viewer_surface);
	reme_viewer_wait(c, viewer_surface);
	reme_surface_destroy(c, &m);

	// Print pending errors
	reme_context_print_errors(c);

	// Make sure to release all memory acquired
	reme_context_destroy(&c);

}

