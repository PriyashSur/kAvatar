#ifndef REME_H
#define REME_h
#include<reconstructmesdk\reme.h>
#include<string.h>
#include<algorithm>
#include<vector>
#include<sstream>
#include<fstream>
#include<ctime>

using namespace std;

class Reme
{
	string file="C:\\InkarneInternal\\kAvatar\\Processingfolder\\";
	//string plyExt = ".ply";
	string objExt = ".obj";
	char* license_path = "";
	
public:
	Reme(string userId)
	{
		file += userId+objExt;
	}
	~Reme()
	{

	}

	//THE FINALIZE MAIN CODE FOR THE SCAN
	void start_scan();
private:
	//EXPORTING MODULE OF OBJ FORMAT
	void exportOBJ();
	//DIFFERENT MODE OF RECONSTRUCTME MODULE
	void Selfie_Scan();
	void Normal_Scan();
	void selfie_timed();
	void selfie_start_scan_backup();
};

















#endif