#include "../../ReadSTL/ReadSTL/Header.h"
#include "../../ReadSTL/ReadSTL/body.cpp"

extern "C" __declspec(dllexport) void openStl(const char *fname)
{
	read_STL newFile(fname);
}