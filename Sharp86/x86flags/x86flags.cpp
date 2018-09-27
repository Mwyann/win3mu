// x86flags.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"

typedef unsigned char byte;
typedef unsigned short ushort;
typedef unsigned int uint;

FILE* pFile;

ushort SetFlags = 0x0001 | 0x0004 | 0x0010 | 0x0040 | 0x0080 | 0x0800;

ushort values16[] =
{
	0x0000, 0x0001, 0x0002, 0x0003, 0x0004, 0x0005, 0x0006, 0x0007, 
	0x0008, 0x0009, 0x000A, 0x000B, 0x000C, 0x000D, 0x000E, 0x000F,
	0x3FF0, 0x3FF1, 0x3FF2, 0x3FF3, 0x3FF4, 0x3FF5, 0x3FF6, 0x3FF7,
	0x3FF8,	0x3FF9, 0x3FFA, 0x3FFB, 0x3FFC, 0x3FFD, 0x3FFE, 0x3FFF,
	0x4000, 0x4001, 0x4002, 0x4003, 0x4004, 0x4005, 0x4006, 0x4007,
	0x4008, 0x4009, 0x400A, 0x400B, 0x400C, 0x400D, 0x400E, 0x400F,
	0x7FF0, 0x7FF1, 0x7FF2, 0x7FF3, 0x7FF4, 0x7FF5, 0x7FF6, 0x7FF7,
	0x7FF8,	0x7FF9, 0x7FFA, 0x7FFB, 0x7FFC, 0x7FFD, 0x7FFE, 0x7FFF,
	0x8000, 0x8001, 0x8002, 0x8003, 0x8004, 0x8005, 0x8006, 0x8007,
	0x8008, 0x8009, 0x800A, 0x800B, 0x800C, 0x800D, 0x800E, 0x800F,
	0xBFF0, 0xBFF1, 0xBFF2, 0xBFF3, 0xBFF4, 0xBFF5, 0xBFF6, 0xBFF7,
	0xBFF8,	0xBFF9, 0xBFFA, 0xBFFB, 0xBFFC, 0xBFFD, 0xBFFE, 0xBFFF,
	0xC000, 0xC001, 0xC002, 0xC003, 0xC004, 0xC005, 0xC006, 0xC007,
	0xC008, 0xC009, 0xC00A, 0xC00B, 0xC00C, 0xC00D, 0xC00E, 0xC00F,
	0xFFF0, 0xFFF1, 0xFFF2, 0xFFF3, 0xFFF4, 0xFFF5, 0xFFF6, 0xFFF7,
	0xFFF8,	0xFFF9, 0xFFFA, 0xFFFB, 0xFFFC, 0xFFFD, 0xFFFE, 0xFFFF,
};

byte values8[] =
{
	0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16,
	0x38, 0x39, 0x3a, 0x3b, 0x3c, 0x3d, 0x3e, 0x3f,
	0x40, 0x41, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
	0x78, 0x79, 0x7a, 0x7b, 0x7c, 0x7d, 0x7e, 0x7f,
	0x80, 0x81, 0x81, 0x82, 0x83, 0x84, 0x85, 0x86,
	0xb8, 0xb9, 0xba, 0xbb, 0xbc, 0xbd, 0xbe, 0xbf,
	0xC0, 0xC1, 0xC1, 0xC2, 0xC3, 0xC4, 0xC5, 0xC6,
	0xF8, 0xF9, 0xFa, 0xFb, 0xFc, 0xFd, 0xFe, 0xFf,
};

#define _countof(x) (sizeof(x) / sizeof(x[0]))

ushort add16(ushort inFlags, ushort a, ushort b, ushort* outFlags)
{
	__asm
	{
		push word ptr[inFlags]
		popf

		mov ax, word ptr[a]
		add ax, word ptr[b]

		pushf
		mov esi, dword ptr[outFlags]
		pop word ptr[esi]
	}
}

ushort adc16(ushort inFlags, ushort a, ushort b, ushort* outFlags)
{
	__asm
	{
		push word ptr[inFlags]
		popf

		mov ax, word ptr[a]
		adc ax, word ptr[b]

		pushf
		mov esi, dword ptr[outFlags]
		pop word ptr[esi]
	}
}

ushort sub16(ushort inFlags, ushort a, ushort b, ushort* outFlags)
{
	__asm
	{
		push word ptr[inFlags]
		popf

		mov ax, word ptr[a]
		sub ax, word ptr[b]

		pushf
		mov esi, dword ptr[outFlags]
		pop word ptr[esi]
	}
}

ushort sbb16(ushort inFlags, ushort a, ushort b, ushort* outFlags)
{
	__asm
	{
		push word ptr[inFlags]
		popf

		mov ax, word ptr[a]
		sbb ax, word ptr[b]

			pushf
			mov esi, dword ptr[outFlags]
			pop word ptr[esi]
	}
}


ushort and16(ushort inFlags, ushort a, ushort b, ushort* outFlags)
{
	__asm
	{
		push word ptr[inFlags]
		popf

		mov ax, word ptr[a]
		and ax, word ptr[b]

			pushf
			mov esi, dword ptr[outFlags]
			pop word ptr[esi]
	}
}

ushort or16(ushort inFlags, ushort a, ushort b, ushort* outFlags)
{
	__asm
	{
		push word ptr[inFlags]
		popf

			mov ax, word ptr[a]
			or ax, word ptr[b]

			pushf
			mov esi, dword ptr[outFlags]
			pop word ptr[esi]
	}
}

ushort xor16(ushort inFlags, ushort a, ushort b, ushort* outFlags)
{
	__asm
	{
		push word ptr[inFlags]
		popf

		mov ax, word ptr[a]
		xor ax, word ptr[b]

			pushf
			mov esi, dword ptr[outFlags]
			pop word ptr[esi]
	}
}

ushort inc16(ushort inFlags, ushort a, ushort* outFlags)
{
	__asm
	{
		push word ptr[inFlags]
		popf

		mov ax, word ptr[a]
		inc ax

			pushf
			mov esi, dword ptr[outFlags]
			pop word ptr[esi]
	}
}


ushort dec16(ushort inFlags, ushort a, ushort* outFlags)
{
	__asm
	{
		push word ptr[inFlags]
		popf

			mov ax, word ptr[a]
			dec ax

			pushf
			mov esi, dword ptr[outFlags]
			pop word ptr[esi]
	}
}


ushort neg16(ushort inFlags, ushort a, ushort* outFlags)
{
	__asm
	{
		push word ptr[inFlags]
		popf

		mov ax, word ptr[a]
		neg ax

			pushf
			mov esi, dword ptr[outFlags]
			pop word ptr[esi]
	}
}

ushort not16(ushort inFlags, ushort a, ushort* outFlags)
{
	__asm
	{
		push word ptr[inFlags]
		popf

		mov ax, word ptr[a]
		not ax

			pushf
			mov esi, dword ptr[outFlags]
			pop word ptr[esi]
	}
}


byte add8(ushort inFlags, byte a, byte b, ushort* outFlags)
{
	__asm
	{
		push word ptr[inFlags]
		popf

			mov al, byte ptr[a]
			add al, byte ptr[b]

			pushf
			mov esi, dword ptr[outFlags]
			pop word ptr[esi]
	}
}

byte adc8(ushort inFlags, byte a, byte b, ushort* outFlags)
{
	__asm
	{
		push word ptr[inFlags]
		popf

			mov al, byte ptr[a]
			adc al, byte ptr[b]

			pushf
			mov esi, dword ptr[outFlags]
			pop word ptr[esi]
	}
}

byte sub8(ushort inFlags, byte a, byte b, ushort* outFlags)
{
	__asm
	{
		push word ptr[inFlags]
		popf

			mov al, byte ptr[a]
			sub al, byte ptr[b]

			pushf
			mov esi, dword ptr[outFlags]
			pop word ptr[esi]
	}
}

byte sbb8(ushort inFlags, byte a, byte b, ushort* outFlags)
{
	__asm
	{
		push word ptr[inFlags]
		popf

			mov al, byte ptr[a]
			sbb al, byte ptr[b]

			pushf
			mov esi, dword ptr[outFlags]
			pop word ptr[esi]
	}
}


byte and8(ushort inFlags, byte a, byte b, ushort* outFlags)
{
	__asm
	{
		push word ptr[inFlags]
		popf

			mov al, byte ptr[a]
			and al, byte ptr[b]

			pushf
			mov esi, dword ptr[outFlags]
			pop word ptr[esi]
	}
}

byte or8(ushort inFlags, byte a, byte b, ushort* outFlags)
{
	__asm
	{
		push word ptr[inFlags]
		popf

			mov al, byte ptr[a]
			or al, byte ptr[b]

			pushf
			mov esi, dword ptr[outFlags]
			pop word ptr[esi]
	}
}

byte xor8(ushort inFlags, byte a, byte b, ushort* outFlags)
{
	__asm
	{
		push word ptr[inFlags]
		popf

			mov al, byte ptr[a]
			xor al, byte ptr[b]

			pushf
			mov esi, dword ptr[outFlags]
			pop word ptr[esi]
	}
}

byte inc8(ushort inFlags, byte a, ushort* outFlags)
{
	__asm
	{
		push word ptr[inFlags]
		popf

			mov al, byte ptr[a]
			inc al

			pushf
			mov esi, dword ptr[outFlags]
			pop word ptr[esi]
	}
}


byte dec8(ushort inFlags, byte a, ushort* outFlags)
{
	__asm
	{
		push word ptr[inFlags]
		popf

			mov al, byte ptr[a]
			dec al

			pushf
			mov esi, dword ptr[outFlags]
			pop word ptr[esi]
	}
}


byte neg8(ushort inFlags, byte a, ushort* outFlags)
{
	__asm
	{
		push word ptr[inFlags]
		popf

			mov al, byte ptr[a]
			neg al

			pushf
			mov esi, dword ptr[outFlags]
			pop word ptr[esi]
	}
}

byte not8(ushort inFlags, byte a, ushort* outFlags)
{
	__asm
	{
		push word ptr[inFlags]
		popf

			mov al, byte ptr[a]
			not al

			pushf
			mov esi, dword ptr[outFlags]
			pop word ptr[esi]
	}
}




ushort shl16(ushort inFlags, ushort a, byte shift, ushort* outFlags)
{
	__asm
	{
		push word ptr[inFlags]
		popf

			mov ax, word ptr[a]
			mov cl, byte ptr[shift]
			shl ax, cl

			pushf
			mov esi, dword ptr[outFlags]
			pop word ptr[esi]
	}
}

byte shl8(ushort inFlags, byte a, byte shift, ushort* outFlags)
{
	__asm
	{
		push word ptr[inFlags]
		popf

			mov al, byte ptr[a]
			mov cl, byte ptr[shift]
			shl al, cl

			pushf
			mov esi, dword ptr[outFlags]
			pop word ptr[esi]
	}
}



ushort rol16(ushort inFlags, ushort a, byte shift, ushort* outFlags)
{
	__asm
	{
		push word ptr[inFlags]
		popf

			mov ax, word ptr[a]
			mov cl, byte ptr[shift]
			rol ax, cl

			pushf
			mov esi, dword ptr[outFlags]
			pop word ptr[esi]
	}
}

byte rol8(ushort inFlags, byte a, byte shift, ushort* outFlags)
{
	__asm
	{
		push word ptr[inFlags]
		popf

			mov al, byte ptr[a]
			mov cl, byte ptr[shift]
			rol al, cl

			pushf
			mov esi, dword ptr[outFlags]
			pop word ptr[esi]
	}
}


ushort ror16(ushort inFlags, ushort a, byte shift, ushort* outFlags)
{
	__asm
	{
		push word ptr[inFlags]
		popf

			mov ax, word ptr[a]
			mov cl, byte ptr[shift]
			ror ax, cl

			pushf
			mov esi, dword ptr[outFlags]
			pop word ptr[esi]
	}
}

byte ror8(ushort inFlags, byte a, byte shift, ushort* outFlags)
{
	__asm
	{
		push word ptr[inFlags]
		popf

			mov al, byte ptr[a]
			mov cl, byte ptr[shift]
			ror al, cl

			pushf
			mov esi, dword ptr[outFlags]
			pop word ptr[esi]
	}
}

ushort rcr16(ushort inFlags, ushort a, byte shift, ushort* outFlags)
{
	__asm
	{
		push word ptr[inFlags]
		popf

			mov ax, word ptr[a]
			mov cl, byte ptr[shift]
			rcr ax, cl

			pushf
			mov esi, dword ptr[outFlags]
			pop word ptr[esi]
	}
}

byte rcr8(ushort inFlags, byte a, byte shift, ushort* outFlags)
{
	__asm
	{
		push word ptr[inFlags]
		popf

			mov al, byte ptr[a]
			mov cl, byte ptr[shift]
			rcr al, cl

			pushf
			mov esi, dword ptr[outFlags]
			pop word ptr[esi]
	}
}

ushort rcl16(ushort inFlags, ushort a, byte shift, ushort* outFlags)
{
	__asm
	{
		push word ptr[inFlags]
		popf

			mov ax, word ptr[a]
			mov cl, byte ptr[shift]
			rcl ax, cl

			pushf
			mov esi, dword ptr[outFlags]
			pop word ptr[esi]
	}
}

byte rcl8(ushort inFlags, byte a, byte shift, ushort* outFlags)
{
	__asm
	{
		push word ptr[inFlags]
		popf

			mov al, byte ptr[a]
			mov cl, byte ptr[shift]
			rcl al, cl

			pushf
			mov esi, dword ptr[outFlags]
			pop word ptr[esi]
	}
}

ushort shr16(ushort inFlags, ushort a, byte shift, ushort* outFlags)
{
	__asm
	{
		push word ptr[inFlags]
		popf

			mov ax, word ptr[a]
			mov cl, byte ptr[shift]
			shr ax, cl

			pushf
			mov esi, dword ptr[outFlags]
			pop word ptr[esi]
	}
}

byte shr8(ushort inFlags, byte a, byte shift, ushort* outFlags)
{
	__asm
	{
		push word ptr[inFlags]
		popf

			mov al, byte ptr[a]
			mov cl, byte ptr[shift]
			shr al, cl

			pushf
			mov esi, dword ptr[outFlags]
			pop word ptr[esi]
	}
}

ushort sar16(ushort inFlags, ushort a, byte shift, ushort* outFlags)
{
	__asm
	{
		push word ptr[inFlags]
		popf

			mov ax, word ptr[a]
			mov cl, byte ptr[shift]
			sar ax, cl

			pushf
			mov esi, dword ptr[outFlags]
			pop word ptr[esi]
	}
}

byte sar8(ushort inFlags, byte a, byte shift, ushort* outFlags)
{
	__asm
	{
		push word ptr[inFlags]
		popf

			mov al, byte ptr[a]
			mov cl, byte ptr[shift]
			sar al, cl

			pushf
			mov esi, dword ptr[outFlags]
			pop word ptr[esi]
	}
}

uint mul16(ushort inFlags, ushort a, ushort b, ushort* outFlags)
{
	__asm
	{
		push word ptr[inFlags]
		popf

			mov ax, word ptr[a]
			mul word ptr[b]

			pushf
			mov esi, dword ptr[outFlags]
			pop word ptr[esi]

			shl edx, 16
			and eax, 0xFFFF
			or eax, edx
	}
}

uint imul16(ushort inFlags, ushort a, ushort b, ushort* outFlags)
{
	__asm
	{
		push word ptr[inFlags]
		popf

			mov ax, word ptr[a]
			imul word ptr[b]

			pushf
			mov esi, dword ptr[outFlags]
			pop word ptr[esi]

			shl edx, 16
			and eax, 0xFFFF
			or eax, edx
	}
}

ushort mul8(ushort inFlags, byte a, byte b, ushort* outFlags)
{
	__asm
	{
		push word ptr[inFlags]
		popf

			mov al, byte ptr[a]
			mul byte ptr[b]

			pushf
			mov esi, dword ptr[outFlags]
			pop word ptr[esi]
	}
}

ushort imul8(ushort inFlags, byte a, byte b, ushort* outFlags)
{
	__asm
	{
		push word ptr[inFlags]
		popf

			mov al, byte ptr[a]
			imul byte ptr[b]

			pushf
			mov esi, dword ptr[outFlags]
			pop word ptr[esi]
	}
}

uint div16(ushort inFlags, uint a, ushort b, ushort* outFlags)
{
	__asm
	{
		push word ptr[inFlags]
		popf

		mov ax, word ptr[a]
		mov dx, word ptr[a+2]
		div word ptr[b]

			pushf
			mov esi, dword ptr[outFlags]
			pop word ptr[esi]

		shl edx, 16
		and eax, 0xFFFF
		or eax, edx
	}
}

ushort div8(ushort inFlags, ushort a, byte b, ushort* outFlags)
{
	__asm
	{
		push word ptr[inFlags]
		popf

			mov ax, word ptr[a]
			div byte ptr[b]

			pushf
			mov esi, dword ptr[outFlags]
			pop word ptr[esi]
	}
}

uint idiv16(ushort inFlags, uint a, ushort b, ushort* outFlags)
{
	__asm
	{
		push word ptr[inFlags]
		popf

			mov ax, word ptr[a]
			mov dx, word ptr[a + 2]
			idiv word ptr[b]

			pushf
			mov esi, dword ptr[outFlags]
			pop word ptr[esi]

			shl edx, 16
			and eax, 0xFFFF
			or eax, edx
	}
}

ushort idiv8(ushort inFlags, ushort a, byte b, ushort* outFlags)
{
	__asm
	{
		push word ptr[inFlags]
		popf

		mov ax, word ptr[a]
		idiv byte ptr[b]

			pushf
			mov esi, dword ptr[outFlags]
			pop word ptr[esi]
	}
}

typedef ushort(*PFNBINARYOP16)(ushort inFlags, ushort a, ushort b, ushort* outFlags);
typedef ushort(*PFNUNARYOP16)(ushort inFlags, ushort a, ushort* outFlags);
typedef byte(*PFNBINARYOP8)(ushort inFlags, byte a, byte b, ushort* outFlags);
typedef byte(*PFNUNARYOP8)(ushort inFlags, byte a, ushort* outFlags);
typedef ushort(*PFNSHIFTOP16)(ushort inFlags, ushort a, byte shift, ushort* outFlags);
typedef byte(*PFNSHIFTOP8)(ushort inFlags, byte a, byte shift, ushort* outFlags);
typedef uint(*PFNMULOP16)(ushort inFlags, ushort a, ushort b, ushort* outFlags);
typedef ushort(*PFNMULOP8)(ushort inFlags, byte a, byte b, ushort* outFlags);
typedef uint(*PFNDIVOP16)(ushort inFlags, uint a, ushort b, ushort* outFlags);
typedef ushort(*PFNDIVOP8)(ushort inFlags, ushort a, byte b, ushort* outFlags);


void Run(PFNBINARYOP16 pfn, const char* pszOpName)
{
	for (int i = 0; i < _countof(values16); i++)
	{
		for (int j = 0; j < _countof(values16); j++)
		{
			ushort a = values16[i];
			ushort b = values16[j];
			for (int f = 0; f < 2; f++)
			{
				ushort flagIn = f ? SetFlags : 0;
				ushort flagOut;
				ushort r = pfn(flagIn, a, b, &flagOut);
				fprintf(pFile, "%s %.4x %.4x %.4x %.4x %.4x\n", pszOpName, flagIn, a, b, r, flagOut);
			}
		}
	}
}


void Run(PFNUNARYOP16 pfn, const char* pszOpName)
{
	for (int i = 0; i < _countof(values16); i++)
	{
		ushort a = values16[i];
		for (int f = 0; f < 2; f++)
		{
			ushort flagIn = f ? SetFlags : 0;
			ushort flagOut;
			ushort r = pfn(flagIn, a, &flagOut);
			fprintf(pFile, "%s %.4x %.4x %.4x %.4x\n", pszOpName, flagIn, a, r, flagOut);
		}
	}
}

void Run(PFNBINARYOP8 pfn, const char* pszOpName)
{
	for (int i = 0; i < _countof(values8); i++)
	{
		for (int j = 0; j < _countof(values8); j++)
		{
			byte a = values8[i];
			byte b = values8[j];
			for (int f = 0; f < 2; f++)
			{
				ushort flagIn = f ? SetFlags : 0;
				ushort flagOut;
				byte r = pfn(flagIn, a, b, &flagOut);
				fprintf(pFile, "%s %.4x %.2x %.2x %.2x %.4x\n", pszOpName, flagIn, a, b, r, flagOut);
			}
		}
	}
}

void Run(PFNUNARYOP8 pfn, const char* pszOpName)
{
	for (int i = 0; i < _countof(values8); i++)
	{
		byte a = values8[i];
		for (int f = 0; f < 2; f++)
		{
			ushort flagIn = f ? SetFlags : 0;
			ushort flagOut;
			byte r = pfn(flagIn, a, &flagOut);
			fprintf(pFile, "%s %.4x %.2x %.2x %.4x\n", pszOpName, flagIn, a, r, flagOut);
		}
	}
}

void RunShiftOp(PFNSHIFTOP16 pfn, const char* pszOpName)
{
	for (int i = 0; i < _countof(values16); i++)
	{
		for (int b = 0; b <= 16; b++)
		{
			ushort a = values16[i];

			for (int f = 0; f < 2; f++)
			{
				ushort flagIn = f ? SetFlags : 0;
				ushort flagOut;
				ushort r = pfn(flagIn, a, b, &flagOut);
				fprintf(pFile, "%s %.4x %.4x %.2x %.4x %.4x\n", pszOpName, flagIn, a, b, r, flagOut);
			}
		}
	}
}

void RunShiftOp(PFNSHIFTOP8 pfn, const char* pszOpName)
{
	for (int i = 0; i < _countof(values8); i++)
	{
		for (int b = 0; b <= 8; b++)
		{
			byte a = values8[i];

			for (int f = 0; f < 2; f++)
			{
				ushort flagIn = f ? SetFlags : 0;
				ushort flagOut;
				byte r = pfn(flagIn, a, b, &flagOut);
				fprintf(pFile, "%s %.4x %.2x %.2x %.2x %.4x\n", pszOpName, flagIn, a, b, r, flagOut);
			}
		}
	}
}

void Run(PFNMULOP16 pfn, const char* pszMulOpName)
{
	for (int i = 0; i < _countof(values16); i++)
	{
		for (int j = 0; j < _countof(values16); j++)
		{
			ushort a = values16[i];
			ushort b = values16[j];
			for (int f = 0; f < 2; f++)
			{
				ushort flagIn = f ? SetFlags : 0;
				ushort flagOut;
				uint r = pfn(flagIn, a, b, &flagOut);
				fprintf(pFile, "%s %.4x %.4x %.4x %.8x %.4x\n", pszMulOpName, flagIn, a, b, r, flagOut);
			}
		}
	}
}

void Run(PFNMULOP8 pfn, const char* pszOpName)
{
	for (int i = 0; i < _countof(values8); i++)
	{
		for (int j = 0; j < _countof(values8); j++)
		{
			byte a = values8[i];
			byte b = values8[j];
			for (int f = 0; f < 2; f++)
			{
				ushort flagIn = f ? SetFlags : 0;
				ushort flagOut;
				ushort r = pfn(flagIn, a, b, &flagOut);
				fprintf(pFile, "%s %.4x %.2x %.2x %.4x %.4x\n", pszOpName, flagIn, a, b, r, flagOut);
			}
		}
	}
}

void Run(PFNDIVOP16 pfn, const char* pszOpName)
{
	for (int i = 0; i < _countof(values16); i++)
	{
		for (int j = 0; j < _countof(values16); j++)
		{
			ushort a = values16[i];
			ushort b = values16[j];
			for (uint k = 0; k < 2; k++)
			{
				for (int f = 0; f < 2; f++)
				{
					ushort flagIn = f ? SetFlags : 0;
					ushort flagOut;
					uint factor = (uint)a * (uint)b + k;
					__try
					{
						uint r = pfn(flagIn, factor, b, &flagOut);
						fprintf(pFile, "%s %.4x %.8x %.4x %.8x %.4x\n", pszOpName, flagIn, factor, b, r, flagOut);
					}
					__except (1)
					{
						fprintf(pFile, "%s %.4x %.8x %.4x ????\n", pszOpName, flagIn, factor, b);
					}
				}
			}
		}
	}
}

void Run(PFNDIVOP8 pfn, const char* pszOpName)
{
	for (int i = 0; i < _countof(values8); i++)
	{
		for (int j = 0; j < _countof(values8); j++)
		{
			byte a = values8[i];
			byte b = values8[j];
			for (uint k = 0; k < 2; k++)
			{
				for (int f = 0; f < 2; f++)
				{
					ushort flagIn = f ? SetFlags : 0;
					ushort flagOut;
					ushort factor = (ushort)a * (ushort)b + k;
					__try
					{
						ushort r = pfn(flagIn, factor, b, &flagOut);
						fprintf(pFile, "%s %.4x %.4x %.2x %.4x %.4x\n", pszOpName, flagIn, factor, b, r, flagOut);
					}
					__except (1)
					{
						fprintf(pFile, "%s %.4x %.4x %.4x ????\n", pszOpName, flagIn, factor, b);
					}
				}
			}
		}
	}
}

int main()
{
	pFile = fopen("aludata.txt", "wt");
	Run(add16, "add16");
	Run(adc16, "adc16");
	Run(sub16, "sub16");
	Run(sbb16, "sbb16");
	Run(and16, "and16");
	Run(or16, "or16");
	Run(xor16, "xor16");
	Run(inc16, "inc16");
	Run(dec16, "dec16");
	Run(neg16, "neg16");
	Run(not16, "not16");
	Run(add8, "add8");
	Run(adc8, "adc8");
	Run(sub8, "sub8");
	Run(sbb8, "sbb8");
	Run(and8, "and8");
	Run(or8, "or8");
	Run(xor8, "xor8");
	Run(inc8, "inc8");
	Run(dec8, "dec8");
	Run(neg8, "neg8");
	Run(not8, "not8");
	RunShiftOp(shl16, "shl16");
	RunShiftOp(shl8, "shl8");
	RunShiftOp(shr16, "shr16");
	RunShiftOp(shr8, "shr8");
	RunShiftOp(sar16, "sar16");
	RunShiftOp(sar8, "sar8");
	RunShiftOp(rcr16, "rcr16");
	RunShiftOp(rcr8, "rcr8");
	RunShiftOp(rcl16, "rcl16");
	RunShiftOp(rcl8, "rcl8");
	RunShiftOp(ror16, "ror16");
	RunShiftOp(ror8, "ror8");
	RunShiftOp(rol16, "rol16");
	RunShiftOp(rol8, "rol8");
	Run(mul16, "mul16");
	Run(imul16, "imul16");
	Run(mul8, "mul8");
	Run(imul8, "imul8");
	Run(div16, "div16");
	Run(idiv16, "idiv16");
	Run(div8, "div8");
	Run(idiv8, "idiv8");
	fclose(pFile);
}

