#include <windows.h>
#include <psapi.h>
#include <stdio.h>
#include <stdlib.h>
#include <time.h>

CHAR cmd[MAX_PATH];
STARTUPINFO si;
PROCESS_INFORMATION pi;
FILETIME createTime, endTime, userTime, kernelTime;
SYSTEMTIME sysKernelTime, sysUserTime;
PROCESS_MEMORY_COUNTERS pmc;
double userSec, kernelSec, blockSec;
double timeLimit, memoLimit;
int beginSec;

int main(int argc, char* argv[]) {
	if(argc < 4) {
		printf("arguments needed");
		return 1;
	}

	strcat(cmd, argv[1]);
	sscanf(argv[2], "%lf", &timeLimit);
	sscanf(argv[3], "%lf", &memoLimit);
	
	if(argc == 5){
		strcat(cmd, " ");
		strcat(cmd, argv[4]);
	}
	
	memset(&si, 0, sizeof(si));
	memset(&pi, 0, sizeof(pi));
	si.cb=sizeof(si);
	si.dwFlags = STARTF_USESTDHANDLES;
	if(!CreateProcess(0, cmd,0,0,FALSE,CREATE_NO_WINDOW,0,0,&si,&pi)){
		printf("Failed to create process\n%s", argv[1]);
		return 0;
	}
	beginSec = clock();
	while(1){
		if(WaitForSingleObject(pi.hProcess, 0) == WAIT_OBJECT_0) break;
		GetProcessMemoryInfo(pi.hProcess, &pmc, sizeof(pmc)); 
		GetProcessTimes(pi.hProcess, &createTime, &endTime, &kernelTime, &userTime);
		FileTimeToSystemTime(&kernelTime, &sysKernelTime);
		FileTimeToSystemTime(&userTime, &sysUserTime);
		userSec = sysUserTime.wHour * 3600000 + sysUserTime.wMinute * 60000 + sysUserTime.wSecond * 1000 + sysUserTime.wMilliseconds;
		kernelSec = sysKernelTime.wHour * 3600000 + sysKernelTime.wMinute * 60000 + sysKernelTime.wSecond * 1000 + sysKernelTime.wMilliseconds;
		blockSec = (double)(clock() - beginSec) - userSec - kernelSec;
		if(userSec > timeLimit || kernelSec > timeLimit || blockSec > 1500.0){
			TerminateProcess(pi.hProcess, 0);
			WaitForSingleObject(pi.hProcess, INFINITE);
			CloseHandle(pi.hProcess);
			CloseHandle(pi.hThread);
			
			if(userSec > timeLimit){
				printf("Time Limit Exceeded");
				return 0;
			}else if(kernelSec > timeLimit){
				printf("Time Limit Exceeded");
				return 0;
			}else{
				printf("Process blocked");
				return 0;
			}
		}
		
		Sleep(5);
	}
	
	DWORD exitCode;
	GetExitCodeProcess(pi.hProcess, &exitCode);
	if(exitCode && exitCode != STILL_ACTIVE){
		printf("Runtime Error\n%u\n", exitCode);
		return 0;
	}
	
	GetProcessMemoryInfo(pi.hProcess, &pmc, sizeof(pmc)); 
	GetProcessTimes(pi.hProcess, &createTime, &endTime, &kernelTime, &userTime);
	FileTimeToSystemTime(&userTime, &sysUserTime);
	
	userSec = sysUserTime.wHour * 3600000 + sysUserTime.wMinute * 60000 + sysUserTime.wSecond * 1000 + sysUserTime.wMilliseconds;
	
	TerminateProcess(pi.hProcess, 0);
	WaitForSingleObject(pi.hProcess, INFINITE);
	CloseHandle(pi.hProcess);
	CloseHandle(pi.hThread);
	
	double m = (double)pmc.PeakWorkingSetSize / 1024.0 / 1024.0;
	if(m > memoLimit){
		printf("Memory Out\n");
	}else{
		printf("Pass\n");
	}
	
	printf("%g\n", userSec);
	printf("%g", m);
	return 0;
}
