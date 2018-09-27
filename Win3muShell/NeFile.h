// NeFile.h : Declaration of the CIconHandler

#pragma once
#include "resource.h"       // main symbols

#pragma pack(2)

struct MZHEADER
{
	WORD signature;
	WORD extraBytes;
	WORD pages;
	WORD relocationItems;
	WORD headerSize;
	WORD minimumAllocation;
	WORD maximumAllocation;
	WORD initialSS;
	WORD initialSP;
	WORD checkSum;
	WORD initialIP;
	WORD initialCS;
	WORD relocationTable;
	WORD overlay;
	WORD res1;
	WORD res2;
	WORD res3;
	WORD res4;
	WORD res5;
	WORD res6;
	WORD res7;
	WORD res8;
	WORD res9;
	WORD res10;
	WORD res11;
	WORD res12;
	WORD res13;
	WORD res14;
	WORD res15;
	WORD res16;
	WORD offsetNEHeader;
};


struct NEHEADER
{
	WORD signature;          //"NE"
	BYTE MajLinkerVersion;     //The major linker version
	BYTE MinLinkerVersion;     //The minor linker version
	WORD EntryTableOffset;   //Offset of entry table, see below
	WORD EntryTableLength;   //Length of entry table in bytes
	DWORD FileLoadCRC;          //UNKNOWN - PLEASE ADD INFO
	BYTE ProgFlags;            //Program flags, bitmapped
	BYTE ApplFlags;            //Application flags, bitmapped
	BYTE AutoDataSegIndex;     //The automatic data segment index
	WORD InitHeapSize;       //The intial local heap size
	WORD InitStackSize;      //The inital stack size
	DWORD EntryPoint;           //CS:IP entry point, CS is index into segment table
	DWORD InitStack;            //SS:SP inital stack pointer, SS is index into segment table
	WORD SegCount;           //Number of segments in segment table
	WORD ModRefs;            //Number of module references (DLLs)
	WORD NoResNamesTabSiz;   //Size of non-resident names table, in bytes (Please clarify non-resident names table)
	WORD SegTableOffset;     //Offset of Segment table
	WORD ResTableOffset;     //Offset of resources table
	WORD ResidNamTable;      //Offset of resident names table
	WORD ModRefTable;        //Offset of module reference table
	WORD ImportNameTable;    //Offset of imported names table (array of counted strings, terminated with string of length 00h)
	DWORD OffStartNonResTab;    //Offset from start of file to non-resident names table
	WORD MovEntryCount;      //Count of moveable entry point listed in entry table
	WORD FileAlnSzShftCnt;   //File alligbment size shift count (0=9(default 512 BYTE pages))
	WORD nResTabEntries;     //Number of resource table entries
	BYTE targOS;           //Target OS
	BYTE OS2EXEFlags;          //Other OS/2 flags
	WORD retThunkOffset;     //Offset to return thunks or start of gangload area - what is gangload?
	WORD segrefthunksoff;    //Offset to segment reference thunks or size of gangload area
	WORD mincodeswap;        //Minimum code swap area size
	WORD expctwinver;        //Expected windows version
};

struct RESOURCE_ENTRY
{
	WORD offset;
	WORD length;
	WORD flags;
	WORD id;
	WORD handle;
	WORD usage;
};

struct RESOURCE_TYPE
{
	RESOURCE_TYPE(WORD typeName)
	{
		m_typeName = typeName;
	}

	WORD m_typeName;
	CVector<RESOURCE_ENTRY*, SOwnedPtr> m_entries;
};

struct GRPICONDIR
{
	WORD idReserved;
	WORD idType;
	WORD idCount;
};


#pragma pack(push, 1)
struct GRPICONDIRENTRY
{
	BYTE bWidth;
	BYTE bHeight;
	BYTE bColorCount;
	BYTE bReserved;
	WORD wPlanes;
	WORD wBitCount;
	DWORD dwBytesInRes;
	WORD nId;

	bool IsPreferredSize() { return bWidth == 32 && bHeight == 32; }
};
#pragma pack(pop)


class CNeFile
{
public:
	CNeFile();
	~CNeFile();

	bool Open(const wchar_t* pszFileName);
	bool Open(FILE* pFile);
	void Close();

	RESOURCE_TYPE* FindResourceType(WORD rtType);
	RESOURCE_ENTRY* FindResourceEntry(WORD rtType, WORD rtName);

	bool ExtractIcon(UINT dwSize, HICON* phIconLarge, HICON* phIconSmall);

	int SeekResource(RESOURCE_ENTRY* pre);
	
	WORD m_wAlignShift;
	FILE* m_pFile;
	CVector<RESOURCE_TYPE*, SOwnedPtr> m_ResourceTypes;
};