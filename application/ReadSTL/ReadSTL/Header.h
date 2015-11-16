#pragma once
#include <iostream>
#include <vector>

using namespace std;

class read_STL
{
public:
	read_STL(string fname);
	void v3(char* facet);
	vector<vector <double>> getVec();
private:
	vector<vector <double>> vec;
};





