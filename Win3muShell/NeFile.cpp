// NeFile.cpp : Implementation of DllMain.

#include "stdafx.h"
#include "NeFile.h"

CNeFile::CNeFile()
{
	m_pFile = NULL;
}

CNeFile::~CNeFile()
{
	Close();
}

bool CNeFile::Open(const wchar_t* pszFileName)
{
	// Open the file
	FILE* pFile;
	if (_wfopen_s(&pFile, pszFileName, L"rb"))
		return false;

	// Load the file
	if (!Open(pFile))
	{ 
		fclose(pFile);
		return false;
	}

	return true;
}

bool CNeFile::Open(FILE* pFile)
{
	// Read MZHEADER
	MZHEADER mzHeader;
	fread(&mzHeader, sizeof(MZHEADER), 1, pFile);

	// Check signature
	if (mzHeader.signature != ('M' | ('Z' << 8)))
		return NULL;

	// Read NEHEADER
	fseek(pFile, mzHeader.offsetNEHeader, SEEK_SET);
	NEHEADER neHeader;
	fread(&neHeader, sizeof(NEHEADER), 1, pFile);
	if (neHeader.signature != ('N' | ('E' << 8)))
		return NULL;

	// Seek to the resource table
	fseek(pFile, mzHeader.offsetNEHeader + neHeader.ResTableOffset, SEEK_SET);

	// Read resource table
	m_wAlignShift = 0;
	fread(&m_wAlignShift, sizeof(m_wAlignShift), 1, pFile);

	while (true)
	{
		// Resource type
		WORD rtType = 0;
		fread(&rtType, sizeof(rtType), 1, pFile);
		if (rtType == 0)
			break;

		// Create a resource type entry
		RESOURCE_TYPE* rt = new RESOURCE_TYPE(rtType);
		m_ResourceTypes.Add(rt);

		// Entry count
		WORD rtCount = 0;
		fread(&rtCount, sizeof(rtCount), 1, pFile);

		// Reserved
		DWORD dwReserved = 0;
		fread(&dwReserved, sizeof(dwReserved), 1, pFile);

		// Read entries
		for (int i = 0; i < rtCount; i++)
		{
			RESOURCE_ENTRY* pre = new RESOURCE_ENTRY();
			fread(pre, sizeof(*pre), 1, pFile);

			rt->m_entries.Add(pre);

			/*
			if (rtType == (0x8000 | (DWORD)RT_GROUP_ICON))
			{
			HICON hIcon = ExtractIcon(pFile, mzHeader.offsetNEHeader + neHeader.ResTableOffset, re);
			if (hIcon!=NULL)
			return hIcon
			}
			*/
		}
	}

	m_pFile = pFile;

	return true;
}

void CNeFile::Close()
{
	if (m_pFile!=NULL)
	{ 
		fclose(m_pFile);
		m_pFile = NULL;
	}

	m_ResourceTypes.RemoveAll();
}

RESOURCE_TYPE* CNeFile::FindResourceType(WORD rtType)
{
	for (int i = 0; i < m_ResourceTypes.GetSize(); i++)
	{
		if (m_ResourceTypes[i]->m_typeName == rtType)
			return m_ResourceTypes[i];
	}

	return NULL;
}

RESOURCE_ENTRY* CNeFile::FindResourceEntry(WORD rtType, WORD rtName)
{
	RESOURCE_TYPE* prt = FindResourceType(rtType);
	if (prt == NULL)
		return NULL;

	for (int i = 0; i < prt->m_entries.GetSize(); i++)
	{
		if (prt->m_entries[i]->id == rtName)
			return prt->m_entries[i];
	}

	return NULL;
}


bool CNeFile::ExtractIcon(UINT dwSize, HICON* phIconLarge, HICON* phIconSmall)
{
	// Find the group icon
	RESOURCE_TYPE* prt = FindResourceType(0x8000 | (WORD)(size_t)RT_GROUP_ICON);
	if (prt == NULL)
		return NULL;

	// Must have at least one entry
	if (prt->m_entries.GetSize() == 0)
		return NULL;

	// Get the first icon group
	RESOURCE_ENTRY* pEntry = prt->m_entries[0];

	// Seek to the resource 
	SeekResource(pEntry);

	// Read the group directory and find the best entry
	GRPICONDIR grp;
	fread(&grp, sizeof(grp), 1, m_pFile);

	int best = -1;
	CVector<GRPICONDIRENTRY> entries;
	for (int i = 0; i < grp.idCount; i++)
	{
		GRPICONDIRENTRY entry;
		fread(&entry, sizeof(entry), 1, m_pFile);
		entries.Add(entry);

		if (best < 0)
			best = i;
		else
		{
			if (entries[i].IsPreferredSize())
			{
				if (!entries[best].IsPreferredSize() || entries[i].wBitCount > entries[best].wBitCount)
					best = i;
			}
			else
			{
				if (!entries[best].IsPreferredSize() || entries[i].wBitCount > entries[best].wBitCount)
					best = i;
			}
		}
	}

	if (best < 0)
		return NULL;

	// Find the actual icon resource
	RESOURCE_ENTRY* prtIcon = FindResourceEntry(0x8000 | (WORD)(size_t)RT_ICON, 0x8000 | entries[best].nId);

	// Seek to it
	int length = SeekResource(prtIcon);

	// Read it
	BYTE* pMem = (BYTE*)malloc(length);
	fread(pMem, length, 1, m_pFile);

	*phIconLarge = CreateIconFromResourceEx(pMem, length, true, 0x00030000, LOWORD(dwSize), LOWORD(dwSize), 0);
	*phIconSmall = CreateIconFromResourceEx(pMem, length, true, 0x00030000, HIWORD(dwSize), HIWORD(dwSize), 0);

	free(pMem);


	return true;
}

int CNeFile::SeekResource(RESOURCE_ENTRY* pre)
{
	// Seek to start of resource
	fseek(m_pFile, (1 << m_wAlignShift) * pre->offset, SEEK_SET);

	return pre->length * (1 << m_wAlignShift);
}

