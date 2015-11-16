#pragma once
#include "Header.h"
#include <vector>
#include <iostream>
#include <fstream>

read_STL::read_STL(string fname)
{
	//!!
	//don't forget ios::binary
	//!!
	ifstream myFile(
		fname.c_str(), ios::in | ios::binary);

	char header_info[80] = "";
	char nTri[4];
	unsigned long nTriLong;

	//read 80 byte header
	if (myFile) {
		myFile.read(header_info, 80);
		cout << "header: " << header_info << endl;
	}
	else {
		cout << "error" << endl;
	}

	//read 4-byte ulong
	if (myFile) {
		myFile.read(nTri, 4);
		nTriLong = *((unsigned long*)nTri);
		cout << "n Tri: " << nTriLong << endl;
	}
	else {
		cout << "error" << endl;
	}

	//now read in all the triangles
	for (int i = 0; i < nTriLong; i++) {

		char facet[50];

		if (myFile) {

			//read one 50-byte triangle
			myFile.read(facet, 50);

			//populate each point of the triangle
			//using v3::v3(char* bin);
			//facet + 12 skips the triangle's unit normal
			v3(facet + 12);
			v3(facet + 24);
			v3(facet + 36);

		}
	}
}

void read_STL::v3(char* facet)
{
	char f1[4] = { facet[0],
		facet[1],facet[2],facet[3] };

	char f2[4] = { facet[4],
		facet[5],facet[6],facet[7] };

	char f3[4] = { facet[8],
		facet[9],facet[10],facet[11] };

	float xx = *((float*)f1);
	float yy = *((float*)f2);
	float zz = *((float*)f3);

	vector<double> coords;
	coords.push_back(double(xx));
	coords.push_back(double(yy));
	coords.push_back(double(zz));

	vec.push_back(coords);

}
vector<vector<double>> read_STL::getVec()
{
	return vec;
}

