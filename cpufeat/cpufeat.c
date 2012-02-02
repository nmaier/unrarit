/*
Copyright (c) 2006,2009 Nils Maier

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

#include <intrin.h>

static unsigned _feat_edx, _feat_ecx;
static void fill() {
	int a[4];
	__cpuid(a, 1);
	_feat_edx = (unsigned)a[3];
	_feat_ecx = (unsigned)a[2];
}

int __stdcall DllMain(void *instance, unsigned long reason, void *reserved)
{
	if (reason == 1) {
		fill();
	}
    return 1;
}

#define EXPORT __stdcall
#define EXPORTED(name) unsigned EXPORT name()

EXPORTED(featuresEDX) {
	return _feat_edx;
}
EXPORTED(featuresECX) {
	return _feat_edx;
}


EXPORTED(hasMMX) {
	return (_feat_edx >> 23) & 1;
}

EXPORTED(hasSSE) {
	return (_feat_edx >> 24) & 3 ? 1 : 0;
}
EXPORTED(hasSSE2) {
	return (_feat_edx >> 25) & 1;
}
EXPORTED(hasHT) {
	return (_feat_edx >> 28) & 1;
}

EXPORTED(hasSSE3) {
	return _feat_ecx & 1;
}
EXPORTED(hasSSSE3) {
	return (_feat_ecx >> 9) & 1;
}

EXPORTED(hasSSE4) {
	return (_feat_ecx >> 19) & 1;
}
EXPORTED(hasSSSE4) {
	return (_feat_ecx >> 20) & 1;
}