#include "Header.h"
#include <vector>

void main()
{
	string fname = "C:/Users/Christian/Documents/Visual Studio 2015/Projects/Fordypningsprosjekt/models/boundary geometry.STL";
	//read_STL open = read_STL(fname);
	read_STL open(fname);
	vector<vector<double>> myVec = open.getVec();
}