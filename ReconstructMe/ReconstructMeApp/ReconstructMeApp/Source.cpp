#include"Reme.h"
#include<iostream>

using namespace std;


int main(int argc ,char* argv[])
{

	string userID="2";
    //userID = argv[1];
	Reme* reme = new Reme(userID);
	reme->start_scan();
	//reme->Start_Inkarne_Motor();
	return 0;

}
