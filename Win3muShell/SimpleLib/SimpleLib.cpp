//////////////////////////////////////////////////////////////////////////
//
// SimpleLib Version 1.0
// Copyright (C) 1998-2007 Topten Software.  All Rights Reserved
//
// For documentation, updates, disclaimers etc... please see:
//		http://www.toptensoftware.com/simplelib
//
// Special thanks to Nick Maher of GrofSoft for original map implementation
//		http://www.grofsoft.com
//
// This code has been released for use "as is".  Any redistribution or
// modification however is strictly prohibited.   See the readme.txt file
// for complete terms and conditions.
//
//////////////////////////////////////////////////////////////////////////

/////////////////////////////////////////////////////////////////////////////
// Don't compile this file - its included automatically at the end of SimpleLib.h
/////////////////////////////////////////////////////////////////////////////



// Search helpers

namespace Simple
{

template <class T, class TKey>
int slxFind(TKey key, const T* lo, const T* hi,
					int (__cdecl *pfnCompare)(const T& a, TKey b))
{
    const T* pos = lo;
	while (pos<hi)
	{
		if (pfnCompare(*pos, key)==0)
		{
			return int(pos-lo);
		}
		pos++;
	}

	return -1;
}


// Perform a simple linear search on an array
// Assumes items are sorted and early aborts
template <class T, class TKey>
bool slxLinearSearch(TKey key, const T* lo, const T* hi,
					int (__cdecl *pfnCompare)(const T& a, TKey b), int& iPosition)
{
    const T* pos = lo;
	while (pos<=hi)
	{
		int iCompare=pfnCompare(*pos, key);
		if (iCompare<0)
		{
			pos++;;
		}
		else if (iCompare==0)
		{
			// found
			iPosition=int(pos-lo);
			return true;
		}
		else
		{
			// not found, but should have by now - this is the insert pos
			iPosition=int(pos-lo);
			return false;
		}
	}

	// Not found!
	iPosition=int(hi-lo) + 1;
	return false;
}

// Perform a binary search on an array
template <class T, class TKey>
bool slxQuickSearch(TKey key, const T* base, int iSize,
				int (__cdecl *pfnCompare)(const T& a, TKey b), int& iPosition)
{
	if (iSize<1)
	{
		iPosition=0;
		return 0;
	}
	if (!base)
		return 0;

	// Setup hi/lo pointers
    const T* lo = base;
    const T* hi = base + (iSize-1); // initialize limits

	// Search
	while(lo<hi)
	{
		int size = int(hi - lo) + 1;
		if (size<=8)
		{
			// use a linear search for small lists.
			bool bRetv=slxLinearSearch<T,TKey>(key, lo, hi, pfnCompare, iPosition);
			iPosition+=int(lo-base);
			return bRetv;
		}

        const T* mid = lo + (size / 2);

		int iCompare=pfnCompare(*mid, key);

		if (iCompare<0)
		{
			lo = mid;
		}
		else if (iCompare>0)
		{
			hi = mid;
		}
		else
		{
			iPosition=int(mid-base);
			return true;
		}
	}

	return slxLinearSearch<T,TKey>(key, lo, hi, pfnCompare, iPosition);
}

template <class T, class TKey>
int slxFindEx(TKey key, void* c, const T* lo, const T* hi,
					int (__cdecl *pfnCompare)(void* c, const T& a, TKey b))
{
    const T* pos = lo;
	while (pos<hi)
	{
		if (pfnCompare(c, *pos, key)==0)
		{
			return int(pos-lo);
		}
		pos++;
	}

	return -1;
}


// Perform a simple linear search on an array
// Assumes items are sorted and early aborts
template <class T, class TKey>
bool slxLinearSearchEx(TKey key, void* c, const T* lo, const T* hi,
					int (__cdecl *pfnCompare)(void* c, const T& a, TKey b), int& iPosition)
{
    const T* pos = lo;
	while (pos<=hi)
	{
		int iCompare=pfnCompare(c, *pos, key);
		if (iCompare<0)
		{
			pos++;;
		}
		else if (iCompare==0)
		{
			// found
			iPosition=int(pos-lo);
			return true;
		}
		else
		{
			// not found, but should have by now - this is the insert pos
			iPosition=int(pos-lo);
			return false;
		}
	}

	// Not found!
	iPosition=int(hi-lo) + 1;
	return false;
}

// Perform a binary search on an array
template <class T, class TKey>
bool slxQuickSearchEx(TKey key, void* c, const T* base, int iSize,
				int (__cdecl *pfnCompare)(void* c, const T& a, TKey b), int& iPosition)
{
	if (iSize<1)
	{
		iPosition=0;
		return 0;
	}
	if (!base)
		return 0;

	// Setup hi/lo pointers
    const T* lo = base;
    const T* hi = base + (iSize-1); // initialize limits

	// Search
	while(lo<hi)
	{
		int size = int(hi - lo) + 1;
		if (size<=8)
		{
			// use a linear search for small lists.
			bool bRetv=slxLinearSearchEx<T,TKey>(key, c, lo, hi, pfnCompare, iPosition);
			iPosition+=int(lo-base);
			return bRetv;
		}

        const T* mid = lo + (size / 2);

		int iCompare=pfnCompare(c, *mid, key);

		if (iCompare<0)
		{
			lo = mid;
		}
		else if (iCompare>0)
		{
			hi = mid;
		}
		else
		{
			iPosition=int(mid-base);
			return true;
		}
	}

	return slxLinearSearchEx<T,TKey>(key, c, lo, hi, pfnCompare, iPosition);
}


/////////////////////////////////////////////////////////////////////////////
// Implementation of CString

// Constructor
template <class T>
CString<T>::CString()
{
	SetHeader(NULL);
}

// Constructor
template <class T>
CString<T>::CString(const CString<T>& Other)
{
	m_psz=Other.m_psz;
	if (m_psz)
	{
		GetHeader()->m_iRef++;
	}
}

// Constructor
template <class T>
CString<T>::CString(const CAnyString& Other)
{
	SetHeader(NULL);
	Assign(Other.As<T>());
}

// Constructor
template <class T>
CString<T>::CString(const T* psz, int iLen)
{
	SetHeader(NULL);
	Assign(psz, iLen);
}

// Destructor
template <class T>
CString<T>::~CString()
{
	Empty();
}

// Assignment operator
template <class T>
CString<T>& CString<T>::operator=(const CString<T>& Other)
{
	Assign(Other);
	return *this;
}

// Assignment operator
template <class T>
CString<T>& CString<T>::operator=(const T* psz)
{
	Assign(psz,-1);
	return *this;
}


// T* operator
template <class T>
CString<T>::operator const T* () const
{
	return m_psz;
}

// Return NULL terminated string (use when passing to vararg functions under gcc)
template <class T>
const T* CString<T>::sz() const
{
	return m_psz;
}

// FreeExtra
template <class T>
void CString<T>::FreeExtra()
{
	// Get header, quit if none
	CHeader* pHeader=GetHeader();
	if (!pHeader)
		return;

	// Work out length if invalid
	if (pHeader->m_iLength<0)
		pHeader->m_iLength=len(m_psz);

	// Get new buffer if shared...
	if (pHeader->m_iRef>1)
		GetBuffer(pHeader->m_iLength+1);

	// If using excessive memory, shrink...
	if (pHeader->m_iLength+16<pHeader->m_iMemSize)
	{
		// Work out new length
		int iNewBufSize=pHeader->m_iLength+1;

		// Reallocate
		pHeader=(CHeader*)realloc(pHeader, sizeof(CHeader)+sizeof(T)*iNewBufSize);
		if (!pHeader)
			return;

		// Store new size...
		pHeader->m_iMemSize=iNewBufSize;

		// Store new mem pointer
		SetHeader(pHeader);
	}
}

// GetBuffer
template <class T>
T* CString<T>::GetBuffer(int iBufSize)
{
	CHeader* pHeader=GetHeader();

	if (iBufSize<0)
	{
		iBufSize=pHeader->m_iLength;
		if (iBufSize<0)
			iBufSize=GetLength()+1;
	}

	// Copy on write...
	if (pHeader && pHeader->m_iRef>1)
	{

		// Allocate new header
		pHeader=(CHeader*)malloc(sizeof(CHeader)+sizeof(T)*iBufSize);
		if (!pHeader)
			return NULL;

		// Copy from original string
		memcpy(pHeader, GetHeader(), sizeof(CHeader)+sizeof(T)*min(GetHeader()->m_iMemSize, iBufSize));

		// Release original string
		GetHeader()->m_iRef--;

		// Setup new header
		pHeader->m_iMemSize=iBufSize;
		pHeader->m_iRef=1;
		pHeader->m_iLength=-1;
		SetHeader(pHeader);

		// Done
		return m_psz;
	}

	// Check if need to resize
	if (pHeader && iBufSize<=pHeader->m_iMemSize)
		{
		pHeader->m_iLength=-1;
		return m_psz;
		}

	// Alloc/Grow buffer...
	if (pHeader)
		{
		pHeader=(CHeader*)realloc(pHeader, sizeof(CHeader)+sizeof(T)*iBufSize);
		if (!pHeader)
			return NULL;
		}
	else
		{
		pHeader=(CHeader*)malloc(sizeof(CHeader)+sizeof(T)*iBufSize);
		if (!pHeader)
			return NULL;
		}

	// Store new buffer
	SetHeader(pHeader);
	pHeader->m_iMemSize=iBufSize;
	pHeader->m_sz[iBufSize]=0;
	pHeader->m_iRef=1;

	// Invalidate length
	pHeader->m_iLength=-1;

	// Done!
	return m_psz;
}

// GrowBuffer
template <class T>
T* CString<T>::GrowBuffer(int iNewSize)
{
	// If no buffer allocate at requested size
	if (!m_psz)
		return GetBuffer(iNewSize);

	// Copy on write?
	if (GetHeader()->m_iRef>1)
		return GetBuffer(iNewSize);

	// Quit if already big enough
	if (iNewSize<=GetHeader()->m_iMemSize)
		return m_psz;

	// Instead of just growing by a little bit, double the buffer size
	// (to save lots of tiny reallocs when appending)
	int iDoubleSize=GetHeader()->m_iMemSize*2;
	if (iDoubleSize>iNewSize)
		iNewSize=iDoubleSize;

	// Resize the buffer, but maintain the current length
	int iLen=GetHeader()->m_iLength;
	GetBuffer(iNewSize);
	GetHeader()->m_iLength=iLen;

	return m_psz;
}

// operator[]
template <class T>
const T& CString<T>::operator[] (int iPos)
{
	ASSERT(m_psz);
	ASSERT(iPos>=0 && iPos<GetHeader()->m_iMemSize);
	if (GetHeader()->m_iRef>1)
	{
		GetBuffer(-1);
	}
	return m_psz[iPos];
}

// Empty
template <class T>
void CString<T>::Empty()
{
	if (!m_psz)
		return;

	if (GetHeader()->m_iRef>1)
	{
		GetHeader()->m_iRef--;
	}
	else
	{
		free(GetHeader());
	}
	m_psz=NULL;
}

// IsEmpty
template <class T>
bool CString<T>::IsEmpty() const
{
	return GetLength()==0;
}

// len
template <class T>
int CString<T>::len(const T* psz)
{
	if (!psz)
		return 0;

	const T* p=psz;
	while (p[0])
		p++;

	return int(p-psz);
}

// copy
template <class T>
void CString<T>::copy(T* pszDest, const T* pszSrc, int iLen)
{
	memcpy(pszDest, pszSrc, sizeof(T)*iLen);
}



template <class T>
bool CString<T>::Assign(const CString<T>& Other)
{
	Empty();
	m_psz=Other.m_psz;
	if (m_psz)
		GetHeader()->m_iRef++;
	return true;
}

// Assign
template <class T>
bool CString<T>::Assign(const TAlt* psz, int iLen)
{
	return Assign(t2t<T,TAlt>(psz,iLen));
}

// Assign
template <class T>
bool CString<T>::Assign(const T* psz, int iLen)
{
	// Clear old value
	Empty();

	// Quit if NULL string
	if (!psz)
		return true;

	// Auto length?
	if (iLen<0)
		iLen=len(psz);

	if (!GetBuffer(iLen))
		return false;

	// Copy new value in
	copy(m_psz, psz, iLen);
	m_psz[iLen]=L'\0';

	// Store new text size
	GetHeader()->m_iLength=iLen;

	return true;
}

// GetLength
template <class T>
int CString<T>::GetLength() const
{
	CHeader* pHeader=GetHeader();
	if (!pHeader)
		return 0;
	if (pHeader->m_iLength<0)
		pHeader->m_iLength=len(m_psz);
	return pHeader->m_iLength;
}


// Replace
template <class T>
bool CString<T>::Replace(int iPos, int iOldLen, const T* psz, int iNewLen)
{
	// Quit if nothing to do
	if (!iOldLen && !iNewLen)
		return true;

	// Auto length?
	if (iNewLen<0)
		{
		iNewLen = psz ? len(psz) : 0;
		}

	// Append?
	if (iPos<0)
		iPos=GetLength();

	// Check position in range
	ASSERT(iPos<=GetLength());

	// Replace entire RHS?
	if (iOldLen<0)
		iOldLen=GetLength()-iPos;

	// Check in range
	ASSERT(iPos + iOldLen <= GetLength());

	// Work out new required size
	int iNewTextSize = GetLength() + iNewLen - iOldLen;
	ASSERT(iNewTextSize>=0);

	int iLength=GetLength();

	// Reallocate buffer
	if (!GrowBuffer(iNewTextSize))
		return false;

	// Move trailing characters
	int iTrailingChars = iLength - (iPos + iOldLen);
	if (iTrailingChars && iOldLen!=iNewLen)
		{
		memmove(m_psz + iPos + iNewLen, m_psz + iPos + iOldLen, iTrailingChars * sizeof(T));
		}

	// Copy new characters
	if (iNewLen)
		{
		memcpy(m_psz + iPos, psz, iNewLen * sizeof(T));
		}

	// Update size/position
	m_psz[iNewTextSize]=L'\0';
	GetHeader()->m_iLength=iNewTextSize;
	return true;
}

// Append
template <class T>
bool CString<T>::Append(const T* psz, int iLen)
{
	return Replace(GetLength(), 0, psz, iLen);
}

// Append
template <class T>
bool CString<T>::Append(T ch)
{
	return Append(&ch, 1);
}

// Insert
template <class T>
bool CString<T>::Insert(int iPos, const T* psz, int iLen)
{
	return Replace(iPos, 0, psz, iLen);
}

// Delete
template <class T>
bool CString<T>::Delete(int iPos, int iLen)
{
	return Replace(iPos, iLen, NULL, 0);
}

// operator+=
template <class T>
CString<T>& CString<T>::operator+=(const T* psz)
{
	Append(psz);
	return *this;
}

// operator+=
template <class T>
CString<T>& CString<T>::operator+=(T ch)
{
	Append(ch);
	return *this;
}

template <class T>
CString<T> CString<T>::ToUpper()
{
	if (!m_psz)
		return NULL;

	CString<T> copy(*this);
	SChar<T>::ToUpper(copy.GetBuffer(-1));
	return copy;
}

template <class T>
CString<T> CString<T>::ToLower()
{
	if (!m_psz)
		return NULL;

	CString<T> copy(*this);
	SChar<T>::ToLower(copy.GetBuffer(-1));
	return copy;
}

template <class T>
int CString<T>::Compare(const T* psz)
{
	return ::Compare(*this, psz);
}

template <class T>
CString<T> CString<T>::Left(int iCount)
{
	return Simple::Left<T>(*this, iCount);
}

template <class T>
CString<T> CString<T>::Right(int iCount)
{
	return Simple::Right<T>(*this, iCount);
}

template <class T>
CString<T> CString<T>::SubStr(int iFrom, int iCount)
{
	return Simple::SubStr<T>(m_psz, iFrom, iCount);
}

template <class T>
CString<T> CString<T>::Mid(int iFrom, int iCount)
{
	return Simple::SubStr<T>(m_psz, iFrom, iCount);
}

template <class T>
int CString<T>::Find(const T* psz, int startOffset = 0)
{
	if (psz == NULL)
		return -1;

	// Get search string length
	int srcLen = SChar<T>::Length(psz);
	if (srcLen == 0)
		return startOffset;

	// Find it
	int stopPos = GetLength() - srcLen;
	for (int i = startOffset; i <= stopPos; i++)
	{
		if (SChar<T>::Compare(m_psz + i, psz, srcLen) == 0)
			return i;
	}

	return -1;
}

template <class T>
int CString<T>::FindI(const T* psz, int startOffset = 0)
{
	if (psz == NULL)
		return -1;

	// Get search string length
	int srcLen = SChar<T>::Length(psz);
	if (srcLen == 0)
		return startOffset;

	// Find it
	int stopPos = GetLength() - srcLen;
	for (int i = startOffset; i <= stopPos; i++)
	{
		if (SChar<T>::CompareI(m_psz + i, psz, srcLen) == 0)
			return i;
	}

	return -1;
}

template <class T>
CString<T> CString<T>::Replace(const T* find, const T* replace, int maxReplacements = -1, int startOffset = 0)
{
	int findLen = SChar<T>::Length(find);
	int replaceLen = SChar<T>::Length(replace);

	CString<T> strNew = *this;

	while (true)
	{
		// Find it
		int foundPos = strNew.Find(find, startOffset);
		if (foundPos < 0)
			break;

		// Replace it
		strNew.Replace(foundPos, findLen, replace, replaceLen);

		// Continue searching after
		startOffset = foundPos + replaceLen;

		// Limit replacements
		maxReplacements--;
		if (maxReplacements == 0)
			break;
	}

	return strNew;
}

template <class T>
CString<T> CString<T>::ReplaceI(const T* find, const T* replace, int maxReplacements = -1, int startOffset = 0)
{
	int findLen = SChar<T>::Length(find);
	int replaceLen = SChar<T>::Length(replace);

	CString<T> strNew = *this;

	while (true)
	{
		// Find it
		int foundPos = strNew.FindI(find, startOffset);
		if (foundPos < 0)
			break;

		// Replace it
		strNew.Replace(foundPos, findLen, replace, replaceLen);

		// Continue searching after
		startOffset = foundPos + replaceLen;

		// Limit replacements
		maxReplacements--;
		if (maxReplacements == 0)
			break;
	}

	return strNew;
}

template <class T>
bool CString<T>::StartsWith(const T* find)
{
	if (m_psz == NULL)
		return false;
	return SChar<T>::Compare(m_psz, find, SChar<T>::Length(find)) == 0;
}

template <class T>
bool CString<T>::StartsWithI(const T* find)
{
	if (m_psz == NULL)
		return false;
	return SChar<T>::CompareI(m_psz, find, SChar<T>::Length(find)) == 0;
}

template <class T>
bool CString<T>::EndsWith(const T* find)
{
	int findLen = SChar<T>::Length(find);
	int startPos = GetLength() - findLen;
	if (startPos < 0)
		return false;
	return SChar<T>::Compare(m_psz + startPos, find, findLen) == 0;
}

template <class T>
bool CString<T>::EndsWithI(const T* find)
{
	int findLen = SChar<T>::Length(find);
	int startPos = GetLength() - findLen;
	if (startPos < 0)
		return false;
	return SChar<T>::CompareI(m_psz + startPos, find, findLen) == 0;
}


/////////////////////////////////////////////////////////////////////////////
// CAnyString

inline CAnyString::CAnyString(const char* psz, int iLen)
{
	m_strA.Assign(psz, iLen);
}
inline CAnyString::CAnyString(const wchar_t* psz, int iLen)
{
	m_strW.Assign(psz, iLen);
}
inline CAnyString::CAnyString(const CAnsiString& str)
{
	m_strA=str;
}
inline CAnyString::CAnyString(const CUniString& str)
{
	m_strW=str;
}
inline CAnyString::CAnyString(const CAnyString& other)
{
	m_strW=other.m_strW;
	m_strA=other.m_strA;
}
inline CAnyString::operator const char*() const
{ 
	return As<char>();
}
inline CAnyString::operator const wchar_t*() const
{
	return As<wchar_t>();
}



/////////////////////////////////////////////////////////////////////////////
// Substring functions

// Mid
template <class T>
CString<T> Mid(const T* psz, int iStart, int iLength)
{
	if (!psz)
		return NULL;

	int iLen=CString<T>::len(psz);

	if (iStart>iLen)
		return NULL;

	if (iStart<0)
		iStart=iLen+iStart;

	if (iLength<0)
		iLength=iLen-iStart;

	if (iStart+iLength>iLen)
		iLength=iLen-iStart;

	return CString<T>(psz+iStart, iLength);
}

template <class T>
CString<T> SubStr(const T* psz, int iStart, int iLength)
{
	return Mid<T>(psz, iStart, iLength);
}

// Left
template <class T>
CString<T> Left(const T* psz, int iLength)
{
	return Mid(psz, 0, iLength);
}

// Right
template <class T>
CString<T> Right(const T* psz, int iLength)
{
	if (!psz)
		return NULL;

	int iLen=CString<T>::len(psz);

	if (iLength>iLen)
		iLength=iLen;

	return CString<T>(psz+iLen-iLength);
}

template <class T>
CString<T> Repeat(const T* psz, int iCount)
{
	int iElementLen=CString<T>::len(psz);

	if (iCount==0 || iElementLen==0)
		return SChar<T>::EmptyString();


	CString<T> strBuf;
	T* pszStart=strBuf.GetBuffer(iElementLen*iCount);

	T* p=pszStart;

	memcpy(p, psz, sizeof(T)*iElementLen);
	p+=iElementLen;

	int iDone=1;
	while (iDone*2<=iCount)
	{
		memcpy(p, pszStart, (p-pszStart)*sizeof(T));
		p+=p-pszStart;
		iDone*=2;
	}

	if (iDone<iCount)
	{
		memcpy(p, pszStart, (iCount-iDone)*iElementLen*sizeof(T));
		p+=(iCount-iDone)*iElementLen;
	}
	
	return strBuf;
}


inline bool IsEmptyString(const wchar_t* psz)
{
	return psz==NULL || psz[0]==L'\0';
}

inline bool IsEmptyString(const char* psz)
{
	return psz==NULL || psz[0]=='\0';
}

inline bool IsEqualString(const wchar_t* p1, const wchar_t* p2)
{
	return p1==p2 || (p1!=NULL && p2!=NULL && Compare(p1, p2)==0);
}

inline bool IsEqualString(const char* p1, const char* p2)
{
	return p1==p2 || (p1!=NULL && p2!=NULL && Compare(p1, p2)==0);
}

inline bool IsEqualStringI(const wchar_t* p1, const wchar_t* p2)
{
	return p1==p2 || (p1!=NULL && p2!=NULL && CompareI(p1, p2)==0);
}

inline bool IsEqualStringI(const char* p1, const char* p2)
{
	return p1==p2 || (p1!=NULL && p2!=NULL && CompareI(p1, p2)==0);
}






/////////////////////////////////////////////////////////////////////////////
// CString Format functions

#ifdef _MSC_VER

#if (_MSC_VER < 1300)

template <>
inline CString<wchar_t> Format(const wchar_t* format, va_list args)
{
    int iLen = 1024;

	CString<wchar_t> buf;
    while (_vsnwprintf( buf.GetBuffer(iLen), iLen, format, args )<0)
		iLen*=2;

	return buf;
}



// VC6 Ansi Format
template <>
inline CString<char> Format(const char* format, va_list args)
{
    int iLen = 1024;

	CString<char> buf;
    while (_vsnprintf( buf.GetBuffer(iLen), iLen, format, args )<0)
		iLen*=2;

	return buf;
}

#elif (_MSC_VER < 1400)

// VS03 Unicode Format
template <>
inline CString<wchar_t> Format(const wchar_t* format, va_list args)
{
    int iLen = _vscwprintf(format, args)+1;

	CString<wchar_t> buf;
    _vsnwprintf( buf.GetBuffer(iLen), iLen, format, args);

	return buf;
}

// VS03 Ansi Format
template <>
inline CString<char> Format(const char* format, va_list args)
{
    int iLen = _vscprintf(format, args)+1;

	CString<char> buf;
    _vsnprintf( buf.GetBuffer(iLen), iLen, format, args );

	return buf;
}

#else

// VS05 Unicode Format
template <>
inline CString<wchar_t> Format(const wchar_t* format, va_list args)
{
    int iLen = _vscwprintf(format, args)+1;

	CString<wchar_t> buf;
    _vswprintf_p( buf.GetBuffer(iLen), iLen, format, args);
	return buf;
}


// VS05 Ansi Format
template <>
inline CString<char> Format(const char* format, va_list args)
{
    int iLen = _vscprintf(format, args)+1;

	CString<char> buf;
    _vsprintf_p( buf.GetBuffer(iLen), iLen, format, args);

	return buf;
}

#endif	// VS05

#else	// MSVC

// Normalize MSVC format specifiers to standard C++
//		%[n]s -> %[n]ls
//		%[n]S -> %[n]hs
//		%c -> %lc
//		%C -> %hc
//      %I64 -> %ll

inline CUniString MsvcToGccFormatSpec(const wchar_t* p)
{
	CUniString str;
	while (p[0])
	{
		if (p[0]!='%')
		{
			str+=*p++;
			continue;
		}

		// Copy the %
		str+=*p++;

		// % Literal
		if (p[0]=='%')
		{
			str+=*p++;
			continue;
		}

		// Ignore flags, precision and width
		while ((p[0]>='0' && p[0]<='9') || p[0]=='+' || p[0]=='-' || p[0]==' ' || p[0]=='#' || p[0]=='.' || p[0]=='*')
		{
			str+=*p++;
		}

		// String
		if (p[0]=='s')
		{
			str+=L"ls";
			p++;
		}
		else if (p[0]=='S')
		{
			str+=L"hs";
			p++;
		}
		else if (p[0]=='c')
		{
			str+=L"lc";
			p++;
		}
		else if (p[0]=='C')
		{
			str+=L"hc";
			p++;
		}
		else if (p[0]=='I' && p[1]=='6' && p[2]=='4')
		{
			str+=L"ll";
			p+=3;
		}
		else
		{
			str+=*p++;
		}
	}
	return str;
}

// GNU/SUN Unicode Format
template <>
inline CString<wchar_t> Format(const wchar_t* format, va_list args)
{
	CUniString strNormalized=Simple::MsvcToGccFormatSpec(format);
	int iLen=5;
	while (true)
	{
		CString<wchar_t> buf;
		if (vswprintf(buf.GetBuffer(iLen), iLen+1, strNormalized.sz(), args)>=0)
			return buf;
		iLen*=2;
	}
}


// GNU/SUN Ansi Format
template <>
inline CString<char> Format(const char* format, va_list args)
{
	// Sun compiler core dumps if passing null as first param to vsnprintf, so pass short
	// buffer to calculate length
	char tmp[2];
	va_list args2;
	va_copy(args2, args);
    int iLen = vsnprintf(tmp, _countof(tmp), format, args2);
	va_end(args2);

	// Now do the actual formatting...
	CString<char> buf;
    vsnprintf( buf.GetBuffer(iLen), iLen+1, format, args);

	return buf;
}

#endif	// !MSVC

template <>
inline CString<wchar_t> Format(const wchar_t* format, ...)
{
    va_list  args;
    va_start(args, format);

	CString<wchar_t> strRetv=Format(format, args);

	va_end(args);

	return strRetv;
}

template <>
inline CString<char> Format(const char* format, ...)
{
    va_list  args;
    va_start(args, format);

	CString<char> strRetv=Format(format, args);

	va_end(args);

	return strRetv;
}



/////////////////////////////////////////////////////////////////////////////
// Implementation of CVector

#define VECDATAPTR(x) (void*)(((char*)m_pData) + sizeof(m_pData[0])*(x))

// Constructor
template <class T, class TSem, class TArg>
CVector<T,TSem,TArg>::CVector()
{
	m_pData=NULL;
	m_iSize=0;
	m_iMemSize=0;
}

// Destructor
template <class T, class TSem, class TArg>
CVector<T,TSem,TArg>::~CVector()
{
	RemoveAll();
	if (m_pData)
		free(m_pData);
}

// Reallocate memory
template <class T, class TSem, class TArg>
void CVector<T,TSem,TArg>::GrowTo(int iRequiredSize)
{
	// Quit if already big enough
	if (iRequiredSize<=m_iMemSize)
		return;

	// Work out how big to make it
	int iNewSize=iRequiredSize*2;
	if (iNewSize<16)
		iNewSize=16;

	if (m_pData)
	{
		// Reallocate memory
		ASSERT(m_iMemSize!=0);
		m_pData=(T*)realloc(m_pData, iNewSize*sizeof(T));
	}
	else
	{
		// Allocate memory
		ASSERT(m_iMemSize==0);
		m_pData=(T*)malloc(iNewSize*sizeof(T));
	}

	// Store new sizes
	m_iMemSize=iNewSize;
}

// Set size...
template <class T, class TSem, class TArg>
void CVector<T,TSem,TArg>::SetSize(int iRequiredSize, const TArg& val)
{
	GrowTo(iRequiredSize);
	while (GetSize()<iRequiredSize)
		Add(val);
	while (GetSize()>iRequiredSize)
		Pop();
}

// Release extra memory
template <class T, class TSem, class TArg>
void CVector<T,TSem,TArg>::FreeExtra()
{
	// Quit if no extra memory allocated
	if (m_iMemSize==m_iSize)
		return;

	// Free or realloc memory...
	if (m_iSize==0)
	{
		free(m_pData);
		m_pData=NULL;
	}
	else
	{
		m_pData=(T*)realloc(m_pData, m_iSize*sizeof(T));
	}

	// Store new memory size
	m_iMemSize=m_iSize;
}

// InsertAt
template <class T, class TSem, class TArg>
void CVector<T,TSem,TArg>::InsertAt(int iPosition, const T& val)
{
	InsertAtInternal(iPosition, &val, 1);
}

template <class T, class TSem, class TArg> template <class TSem2, class TArg2>
void CVector<T,TSem,TArg>::Add(CVector<T, TSem2, TArg2>& vec)
{
	InsertAtInternal(GetSize(), vec.GetBuffer(), vec.GetSize());
}

template <class T, class TSem, class TArg> template <class TSem2, class TArg2>
void CVector<T,TSem,TArg>::InsertAt(int iPosition, CVector<T, TSem2, TArg2>& vec)
{
	InsertAtInternal(iPosition, vec.GetBuffer(), vec.GetSize());
}

template <class T, class TSem, class TArg>
void CVector<T,TSem,TArg>::Swap(CVector<T, TSem, TArg>& other)
{
	int tempSize = m_iSize;
	m_iSize = other.m_iSize;
	other.m_iSize = tempSize;

	int tempMemSize = m_iMemSize;
	m_iMemSize = other.m_iMemSize;
	other.m_iMemSize = tempMemSize;

	T* tempData = m_pData;
	m_pData = other.m_pData;
	other.m_pData = tempData;
}


// ReplaceAt
template <class T, class TSem, class TArg>
void CVector<T,TSem,TArg>::ReplaceAt(int iPosition, const T& val)
{
	ASSERT(iPosition>=0 && iPosition<GetSize());

	TSem::OnRemove(m_pData[iPosition], this);

	Destructor(m_pData+iPosition);

	Constructor(m_pData+iPosition, TSem::OnAdd(val, this));
}

// Swap
template <class T, class TSem, class TArg>
void CVector<T,TSem,TArg>::Swap(int iPosA, int iPosB)
{
	ASSERT(iPosA>=0 && iPosA<GetSize());
	ASSERT(iPosB>=0 && iPosB<GetSize());

	// Redundant?
	if (iPosA==iPosB)
		return;

	// Swap it
	T temp=m_pData[iPosA];
	Destructor(m_pData+iPosA);
	Constructor(m_pData+iPosA, m_pData[iPosB]);
	Destructor(m_pData+iPosB);
	Constructor(m_pData+iPosB, temp);
}

// Move
template <class T, class TSem, class TArg>
void CVector<T,TSem,TArg>::Move(int iFrom, int iTo)
{
	ASSERT(iFrom>=0 && iFrom<GetSize());
	ASSERT(iTo>=0 && iTo<GetSize());

	// Redundant?
	if (iFrom==iTo)
		return;

	T temp=m_pData[iFrom];
	Destructor(m_pData+iFrom);
	if (iTo<iFrom)
	{
		memmove(VECDATAPTR(iTo+1), VECDATAPTR(iTo), (iFrom-iTo)*sizeof(T));
	}
	else
	{
		memmove(VECDATAPTR(iFrom), VECDATAPTR(iFrom+1), (iTo-iFrom)*sizeof(T));
	}
	Constructor(m_pData+iTo, temp);
}

// Insert at a position
template <class T, class TSem, class TArg>
void CVector<T,TSem,TArg>::InsertAtInternal(int iPosition, const T* pVal, int iCount)
{
	if (iCount<1)
		return;

	ASSERT(iPosition>=0);
	ASSERT(iPosition<=GetSize());

	// Make sure have room
	GrowTo(m_iSize+iCount);

	// Shuffle memory
	if (iPosition<m_iSize)
		memmove(VECDATAPTR(iPosition+iCount), VECDATAPTR(iPosition), (m_iSize-iPosition)*sizeof(T));

	// Store pointer
	for (int i=0; i<iCount; i++)
	{
		Constructor(m_pData+iPosition+i, TSem::OnAdd(*(pVal+i), this));
	}

	// Update size
	m_iSize+=iCount;
}

// Add
template <class T, class TSem, class TArg>
inline int CVector<T,TSem,TArg>::Add(const T& val)
{
	// Grow if necessary
	if (m_iSize+1>m_iMemSize)
		GrowTo(m_iSize+1);

	Constructor(m_pData+m_iSize, TSem::OnAdd(val, this));
	m_iSize++;
	return m_iSize-1;
}

// Remove a particular item
template <class T, class TSem, class TArg>
int CVector<T,TSem,TArg>::Remove(const TArg& val)
{
	int iPos=Find(val);
	if (iPos>=0)
		RemoveAt(iPos);
	return iPos;
}

// RemoveAt
template <class T, class TSem, class TArg>
inline void CVector<T,TSem,TArg>::RemoveAt(int iPosition)
{
	ASSERT(iPosition>=0);
	ASSERT(iPosition<GetSize());

	TSem::OnRemove(m_pData[iPosition], this);
	Destructor(m_pData+iPosition);

	// Shuffle memory
	if (iPosition<GetSize()-1)
		memmove(VECDATAPTR(iPosition), VECDATAPTR(iPosition+1), (m_iSize-iPosition-1)*sizeof(T));

	// Update size
	m_iSize--;
}

// RemoveAt
template <class T, class TSem, class TArg>
void CVector<T,TSem,TArg>::RemoveAt(int iPosition, int iCount)
{
	// Quit if nothing to do!
	if (iCount==0)
		return;

	ASSERT(iPosition>=0);
	ASSERT(iPosition<GetSize());
	ASSERT(iPosition+iCount-1<GetSize());
	ASSERT(m_iSize-iCount>=0);

	for (int i=0; i<iCount; i++)
	{
		TSem::OnRemove(m_pData[iPosition+i], this);
		Destructor(m_pData+iPosition+i);
	}

	// Shuffle emory
	if (iPosition+iCount<GetSize())
		memmove(VECDATAPTR(iPosition), VECDATAPTR(iPosition+iCount), (m_iSize-iPosition-iCount)*sizeof(T));

	// Update size
	m_iSize-=iCount;
}

// DetachAt
template <class T, class TSem, class TArg>
inline T CVector<T,TSem,TArg>::DetachAt(int iPosition)
{
	ASSERT(iPosition>=0);
	ASSERT(iPosition<GetSize());

	T temp=GetAt(iPosition);

	TSem::OnDetach(m_pData[iPosition], this);
	Destructor(m_pData+iPosition);

	// Shuffle memory
	if (iPosition<GetSize()-1)
		memmove(VECDATAPTR(iPosition), VECDATAPTR(iPosition+1), (m_iSize-iPosition-1)*sizeof(T));

	// Update size
	m_iSize--;

	return temp;
}

template <class T, class TSem, class TArg>
void CVector<T,TSem,TArg>::Detach(const TArg& val)
{
	int iIndex=Find(val);
	ASSERT(iIndex>=0);
	DetachAt(iIndex);
}

template <class T, class TSem, class TArg>
void CVector<T,TSem,TArg>::DetachAll()
{
	for (int i=GetSize()-1; i>=0; i--)
		DetachAt(i);
}


// RemoveAll
template <class T, class TSem, class TArg>
void CVector<T,TSem,TArg>::RemoveAll()
{
	if (m_iSize)
	{
		RemoveAt(0, m_iSize);
		m_iSize=0;
//		FreeExtra();		// Removed as performance enhancement when frequent adding/remove all
	}
}

// GetAt
template <class T, class TSem, class TArg>
inline T& CVector<T,TSem,TArg>::GetAt(int iPosition) const
{
	ASSERT(iPosition>=0);
	ASSERT(iPosition<GetSize());

	return m_pData[iPosition];
}

// operator[]
template <class T, class TSem, class TArg>
inline T& CVector<T,TSem,TArg>::operator[](int iPosition) const
{
	return GetAt(iPosition);
}

// GetBuffer
template <class T, class TSem, class TArg>
inline T* CVector<T,TSem,TArg>::GetBuffer() const
{
	return m_pData;
}

// GetSize
template <class T, class TSem, class TArg>
inline int CVector<T,TSem,TArg>::GetSize() const
{
	return m_iSize;
}

// Find (linear)
template <class T, class TSem, class TArg>
int CVector<T,TSem,TArg>::Find(const TArg& val, int iStartAfter) const
{
	// Find an item
	for (int i=iStartAfter+1; i<m_iSize; i++)
	{
		if (TSem::Compare(m_pData[i], val)==0)
			return i;
	}

	// Not found
	return -1;
}


// QuickSort
template <class T, class TSem, class TArg>
void CVector<T,TSem,TArg>::QuickSort()
{
	QuickSort(TSem::Compare);
}

typedef int (__cdecl *PFNQSORTCOMPARE)(const void*, const void*);


// QuickSort
template <class T, class TSem, class TArg>
void CVector<T,TSem,TArg>::QuickSort(int (__cdecl *pfnCompare)(const T& a, const T& b))
{
	if (IsEmpty())
		return;

	qsort(m_pData, m_iSize, sizeof(T), (PFNQSORTCOMPARE)pfnCompare);
}


#if defined(_MSC_VER) && (_MSC_VER>=1400)

extern "C"
{
typedef int (*PFNQSORTCOMPAREEX)(void*, const void*, const void*);
}
// QuickSort
template <class T, class TSem, class TArg>
void CVector<T,TSem,TArg>::QuickSort(int (__cdecl *pfnCompare)(void* ctx, const T& a, const T& b), void* ctx)
{
	if (IsEmpty())
		return;

	qsort_s(m_pData, m_iSize, sizeof(T), (PFNQSORTCOMPAREEX)pfnCompare, ctx);
}

#endif

// QuickSearch
template <class T, class TSem, class TArg>
bool CVector<T,TSem,TArg>::QuickSearch(const TArg& key, int& iPosition) const
{
	return QuickSearch(key, TSem::Compare, iPosition);
}


// QuickSearch
template <class T, class TSem, class TArg>
bool CVector<T,TSem,TArg>::QuickSearch(const TArg& key, int (__cdecl *pfnCompare)(const T& a, const TArg& b), int& iPosition) const
{
	return Simple::slxQuickSearch<T,const TArg&>(key, m_pData, m_iSize, pfnCompare, iPosition);
}

// QuickSearchEx
template <class T, class TSem, class TArg>
bool CVector<T,TSem,TArg>::QuickSearch(const TArg& key, void* ctx, int (__cdecl *pfnCompare)(void* ctx, const T& a, const TArg& b), int& iPosition) const
{
	return Simple::slxQuickSearchEx<T,const TArg&>(key, ctx, m_pData, m_iSize, pfnCompare, iPosition);
}

template <class T, class TSem, class TArg> template <class TKey>
bool CVector<T,TSem,TArg>::QuickSearchKey(TKey key, int (__cdecl *pfnCompare)(const T& a, TKey b), int& iPosition) const
{
	return slxQuickSearch<T, TKey>(key, GetBuffer(), GetSize(), pfnCompare, iPosition);
}

template <class T, class TSem, class TArg> template <class TKey>
bool CVector<T,TSem,TArg>::QuickSearchKey(TKey key, void* ctx, int (__cdecl *pfnCompare)(void* ctx, const T& a, TKey b), int& iPosition) const
{
	return slxQuickSearchEx<T, TKey>(key, ctx, GetBuffer(), GetSize(), pfnCompare, iPosition);
}


template <class T, class TSem, class TArg> template <class TKey>
int CVector<T,TSem,TArg>::FindKey(TKey key, int (__cdecl *pfnCompare)(const T& a, TKey b), int iStartAfter) const
{
	return slxFind(key, GetBuffer()+iStartAfter+1, GetBuffer()+GetSize(), pfnCompare);
}

template <class T, class TSem, class TArg> template <class TKey>
int CVector<T,TSem,TArg>::FindKey(TKey key, void* ctx, int (__cdecl *pfnCompare)(void* ctx, const T& a, TKey b), int iStartAfter) const
{
	return slxFindEx(key, ctx, GetBuffer()+iStartAfter+1, GetBuffer()+GetSize(), pfnCompare);
}


// IsEmpty
template <class T, class TSem, class TArg>
inline bool CVector<T,TSem,TArg>::IsEmpty() const
{
	return GetSize()==0;
}

// Push
template <class T, class TSem, class TArg>
inline void CVector<T,TSem,TArg>::Push(const TArg& val)
{
	Add(val);
}

// Pop
template <class T, class TSem, class TArg>
inline bool CVector<T,TSem,TArg>::Pop(T& val)
{
	if (m_iSize==0)
		return false;

	// Update size
	m_iSize--;

	val=m_pData[m_iSize];

	TSem::OnDetach(m_pData[m_iSize], this);
	Destructor(m_pData+m_iSize);

	return true;
}

// Pop
template <class T, class TSem, class TArg>
inline T CVector<T,TSem,TArg>::Pop()
{
	ASSERT(!IsEmpty());
	return DetachAt(GetSize()-1);
}

// Top
template <class T, class TSem, class TArg>
inline T& CVector<T,TSem,TArg>::Top() const
{
	ASSERT(!IsEmpty());
	return GetAt(GetSize()-1);
}

// Top
template <class T, class TSem, class TArg>
inline bool CVector<T,TSem,TArg>::Top(T& val) const
{
	if (IsEmpty())
		return false;
	val=Top();
	return true;
}

// Enqueue
template <class T, class TSem, class TArg>
inline void CVector<T,TSem,TArg>::Enqueue(const TArg& val)
{
	Add(val);
}

// Dequeue
template <class T, class TSem, class TArg>
inline T CVector<T,TSem,TArg>::Dequeue()
{
	ASSERT(!IsEmpty());
	return DetachAt(0);
}

// Dequeue
template <class T, class TSem, class TArg>
inline bool CVector<T,TSem,TArg>::Dequeue(T& val)
{
	if (IsEmpty())
		return false;

	val=Dequeue();
	return true;
}

// Peek
template <class T, class TSem, class TArg>
inline T& CVector<T,TSem,TArg>::Peek() const
{
	ASSERT(!IsEmpty());
	return GetAt(0);
}

// Peek
template <class T, class TSem, class TArg>
inline bool CVector<T,TSem,TArg>::Peek(T& val) const
{
	if (IsEmpty())
		return false;
	val=Peek();
	return true;
}


#undef VECDATAPTR

/////////////////////////////////////////////////////////////////////////////
// CSortedVector implementation

// Constructor
template <class T, class TSem, class TArg>
CSortedVector<T,TSem,TArg>::CSortedVector()
{
	m_pfnCompare=NULL;
	m_bAllowDuplicates=true;
#if defined(_MSC_VER) && (_MSC_VER>=1400)
	m_ctx=NULL;
	m_pfnCompareEx=NULL;
#endif

	// Setup default sort order
	Resort(NULL, true);
};

// Destructor
template <class T, class TSem, class TArg>
CSortedVector<T,TSem,TArg>::~CSortedVector()
{
};

// Add
template <class T, class TSem, class TArg>
int CSortedVector<T,TSem,TArg>::Add(const TArg& val)
{
#if defined(_MSC_VER) && (_MSC_VER>=1400)
	if (m_pfnCompareEx)
	{
		int iPos;
		if (m_vec.QuickSearch(val, m_ctx, m_pfnCompareEx, iPos))
		{
			if (!m_bAllowDuplicates)
				return -1-iPos;		// position of existing item = -retv-1;

			// Insert duplicate after existing entries...
			while (iPos<GetSize() && m_pfnCompareEx(m_ctx, val, GetAt(iPos))==0)
				iPos++;
		}

		m_vec.InsertAt(iPos, val);
		return iPos;
	}

#endif
	int iPos;
	if (m_vec.QuickSearch(val, m_pfnCompare, iPos))
	{
		if (!m_bAllowDuplicates)
			return -1-iPos;		// position of existing item = -retv-1;

		// Insert duplicate after existing entries...
		while (iPos<GetSize() && m_pfnCompare(val, GetAt(iPos))==0)
			iPos++;
	}

	m_vec.InsertAt(iPos, val);
	return iPos;
}

// GetSize
template <class T, class TSem, class TArg>
int CSortedVector<T,TSem,TArg>::GetSize() const
{
	return m_vec.GetSize();
}

// Remove
template <class T, class TSem, class TArg>
int CSortedVector<T,TSem,TArg>::Remove(const TArg& val)
{
#if defined(_MSC_VER) && (_MSC_VER>=1400)
	if (m_pfnCompareEx)
	{
		int iPos;
		m_vec.QuickSearch(val, m_ctx, m_pfnCompareEx, iPos);
		if (iPos>=0)
			RemoveAt(iPos);
		return -1;
	}
#endif

	int iPos;
	m_vec.QuickSearch(val, m_pfnCompare, iPos);
	if (iPos>=0)
		RemoveAt(iPos);
	return -1;
}

// RemoveAt
template <class T, class TSem, class TArg>
void CSortedVector<T,TSem,TArg>::RemoveAt(int iPosition)
{
	m_vec.RemoveAt(iPosition);
}

// DetachAt
template <class T, class TSem, class TArg>
T CSortedVector<T,TSem,TArg>::DetachAt(int iPosition)
{
	return m_vec.DetachAt(iPosition);
}

// RemoveAll
template <class T, class TSem, class TArg>
void CSortedVector<T,TSem,TArg>::RemoveAll()
{
	m_vec.RemoveAll();
}

// GetAt
template <class T, class TSem, class TArg>
const T& CSortedVector<T,TSem,TArg>::GetAt(int iPosition) const
{
	return m_vec.GetAt(iPosition);
}

// operator[]
template <class T, class TSem, class TArg>
const T& CSortedVector<T,TSem,TArg>::operator[](int iPosition) const
{
	return m_vec.GetAt(iPosition);
}

// GetBuffer
template <class T, class TSem, class TArg>
const T* CSortedVector<T,TSem,TArg>::GetBuffer() const
{
	return m_vec.GetBuffer();
}

// IsEmpty
template <class T, class TSem, class TArg>
bool CSortedVector<T,TSem,TArg>::IsEmpty() const
{
	return GetSize()==0;
}

// QuickSearch
template <class T, class TSem, class TArg>
bool CSortedVector<T,TSem,TArg>::QuickSearch(const TArg& key, int& iPosition) const
{
#if defined(_MSC_VER) && (_MSC_VER>=1400)
	if (m_pfnCompareEx)
		return m_vec.QuickSearch(key, m_ctx, m_pfnCompareEx, iPosition);
#endif

	return m_vec.QuickSearch(key, m_pfnCompare, iPosition);
}

// Find (uses linear search)
template <class T, class TSem, class TArg>
int CSortedVector<T,TSem,TArg>::Find(const TArg& val, int iStartAfter) const
{
	return m_vec.Find(val, iStartAfter);
}

// Find (uses quick search but matches prototype of CVector::Find
template <class T, class TSem, class TArg>
int CSortedVector<T,TSem,TArg>::Find(const TArg& key) const
{
	// Quick search doesn't work if duplicates are allowed.
	if (m_bAllowDuplicates)
	{
		return Find(key, -1);
	}

#if defined(_MSC_VER) && (_MSC_VER>=1400)
	if (m_pfnCompareEx)
	{
		int iPos;
		if (m_vec.QuickSearch(key, m_ctx, m_pfnCompareEx, iPos))
			return iPos;
		else
			return -1;
	}
#endif

	int iPos;
	if (m_vec.QuickSearch(key, m_pfnCompare, iPos))
		return iPos;
	else
		return -1;
}

// Resort the vertor. Specify NULL for pfnCompare to use TSem::Compare
template <class T, class TSem, class TArg>
void CSortedVector<T,TSem,TArg>::Resort(int (__cdecl *pfnCompare)(const T& a, const T& b), bool bAllowDuplicates)
{
	// If turning off allow duplicates, verify vector is empty
	ASSERT(!m_bAllowDuplicates || bAllowDuplicates || IsEmpty());

#if defined(_MSC_VER) && (_MSC_VER>=1400)
	m_pfnCompareEx=NULL;
#endif
	m_bAllowDuplicates=bAllowDuplicates;
	if (pfnCompare!=NULL)
		m_pfnCompare=pfnCompare;
	else
#ifdef _MSC_VER
		m_pfnCompare=TSem::Compare<T>;
#else
		m_pfnCompare=TSem::Compare;
#endif
	m_vec.QuickSort(m_pfnCompare);
}

#if defined(_MSC_VER) && (_MSC_VER>=1400)
// Resort the vertor with Ex comparer
template <class T, class TSem, class TArg>
void CSortedVector<T,TSem,TArg>::Resort(void* ctx, int (__cdecl *pfnCompare)(void* ctx, const T& a, const T& b), bool bAllowDuplicates)
{
	// If turning off allow duplicates, verify vector is empty
	ASSERT(!m_bAllowDuplicates || bAllowDuplicates || IsEmpty());

	m_pfnCompare=NULL;
	m_pfnCompareEx=pfnCompare;
	m_ctx=ctx;
	m_bAllowDuplicates=bAllowDuplicates;
	m_vec.QuickSort(m_pfnCompareEx, m_ctx);
}
#endif



/////////////////////////////////////////////////////////////////////////////
// CGrid

// Constructor
template <class T, class TSem, class TArg>
CGrid<T,TSem,TArg>::CGrid(int iWidth, int iHeight) :
	m_iHeight(iHeight)
{
	SetSize(iWidth, iHeight);
};

// SetSize
template <class T, class TSem, class TArg>
void CGrid<T,TSem,TArg>::SetSize(int iWidth, int iHeight, const TArg& val)
{
	// Adjust width of existing rows
	for (int i=0; i<m_Columns.GetSize(); i++)
	{
		m_Columns[i]->SetSize(iHeight, val);
	}

	// Create new rows
	while (iWidth>m_Columns.GetSize())
	{
		m_Columns.Add(new CColumn(iWidth, val));
	}

	// Delete extra rows
	while (iWidth<m_Columns.GetSize())
	{
		m_Columns.Pop();
	}
}

template <class T, class TSem, class TArg>
void CGrid<T,TSem,TArg>::InsertColumn(int iPosition, const TArg& val)
{
	m_Columns.InsertAt(iPosition, new CColumn(m_iHeight, val));
}

template <class T, class TSem, class TArg>
void CGrid<T,TSem,TArg>::RemoveColumn(int iPosition)
{
	m_Columns.RemoveAt(iPosition);
}

template <class T, class TSem, class TArg>
void CGrid<T,TSem,TArg>::InsertRow(int iPosition, const TArg& val)
{
	for (int i=0; i<m_Columns.GetSize(); i++)
	{
		m_Columns[i]->InsertAt(iPosition, val);
	}
	m_iHeight++;
}

template <class T, class TSem, class TArg>
void CGrid<T,TSem,TArg>::RemoveRow(int iPosition)
{
	for (int i=0; i<m_Columns.GetSize(); i++)
	{
		m_Columns[i]->RemoveAt(iPosition);
	}
	m_iHeight--;
}

template <class T, class TSem, class TArg>
void CGrid<T,TSem,TArg>::RemoveAll()
{
	m_Columns.RemoveAll();
	m_iHeight=0;
}

template <class T, class TSem, class TArg>
int CGrid<T,TSem,TArg>::GetWidth()
{
	return m_Columns.GetSize();
}

template <class T, class TSem, class TArg>
int CGrid<T,TSem,TArg>::GetHeight()
{
	return m_iHeight;
}

template <class T, class TSem, class TArg>
typename CGrid<T,TSem,TArg>::CColumn& CGrid<T,TSem,TArg>::operator[](int x)
{
	return *m_Columns[x];
}


/////////////////////////////////////////////////////////////////////////////
// CLinkedList

#ifndef _SIMPLELIB_NO_LINKEDLIST_MULTICHAIN
#define template_linkedlist template <class T, class TSem, CChain<T> T::* pMember>
#define CLinkedList_template CLinkedList<T,TSem,pMember>
#define chainmember(x) ((x)->*pMember)
#else
#define template_linkedlist template <class T, class TSem>
#define CLinkedList_template CLinkedList<T,TSem>
#define chainmember(x) ((x)->m_Chain)
#endif

template_linkedlist
CLinkedList_template::CLinkedList()
{
	m_pFirst=NULL;
	m_pCurrent=NULL;
	m_iSize=0;
	m_iIterPos=-1;
	m_pIterElem=NULL;
}

template_linkedlist
CLinkedList_template::~CLinkedList()
{
	RemoveAll();
}

template_linkedlist
void CLinkedList_template::Prepend(T* p)
{
#ifdef _DEBUG
	ASSERT(chainmember(p).m_pPrev==NULL);
	ASSERT(chainmember(p).m_pNext==NULL);
	ASSERT(!Contains(p));
#endif

	// Semantics...
	TSem::OnAdd(p, this);

	if (m_pFirst)
		{
		// Save the last element pointer
		chainmember(p).m_pPrev=chainmember(m_pFirst).m_pPrev;

		// Link the old first to the new first...
		chainmember(m_pFirst).m_pPrev=p;
		}
	else
		{
		// Set the last element as the new element
		chainmember(p).m_pPrev=p;
		}


	// Insert at front
	chainmember(p).m_pNext=m_pFirst;
	m_pFirst=p;

	// Update iterate position
	if (m_pIterElem)
	{
		m_pIterElem=GetPrevious(m_pIterElem);
	}

	m_iSize++;
}


template_linkedlist
void CLinkedList_template::Add(_CLinkedList& OtherList)
{

	if (!OtherList.m_pFirst)
		return;

	if (m_pFirst)
		{
		T* pOtherFirst=OtherList.GetFirst();
		T* pOtherLast=OtherList.GetLast();
		T* pThisFirst=GetFirst();
		T* pThisLast=GetLast();

		chainmember(pThisFirst).m_pPrev=pOtherLast;
		chainmember(pOtherFirst).m_pPrev=pThisLast;
		chainmember(pThisLast).m_pNext=pOtherFirst;
		}
	else
		{
		m_pFirst=OtherList.m_pFirst;
		}

	// Update size...
	m_iSize+=OtherList.m_iSize;

	// Reset other list
	OtherList.m_pFirst=NULL;
	OtherList.m_iSize=0;
	OtherList.m_pCurrent=NULL;
	OtherList.m_pIterElem=NULL;
}

// Insert an element at end of start of a list
template_linkedlist
void CLinkedList_template::Add(T* p)
{
#ifdef _DEBUG
	ASSERT(chainmember(p).m_pPrev==NULL);
	ASSERT(chainmember(p).m_pNext==NULL);
	ASSERT(!Contains(p));
#endif

	// Semantics...
	TSem::OnAdd(p, this);

	if (m_pFirst)
		{
		chainmember(chainmember(m_pFirst).m_pPrev).m_pNext=p;
		chainmember(p).m_pPrev=chainmember(m_pFirst).m_pPrev;
		chainmember(p).m_pNext=NULL;
		chainmember(m_pFirst).m_pPrev=p;
		}
	else
		{
		m_pFirst=p;
		chainmember(p).m_pNext=NULL;
		chainmember(p).m_pPrev=p;
		}

	m_iSize++;
}



template_linkedlist
void CLinkedList_template::Insert(T* p, T* pBefore)
{
#ifdef _DEBUG
	ASSERT(chainmember(p).m_pPrev==NULL);
	ASSERT(chainmember(p).m_pNext==NULL);
#endif

	// Insert at end?
	if (!pBefore)
		{
		Add(p);
		return;
		}

	ASSERT(Contains(pBefore));

	// Insert at start
	if (pBefore==m_pFirst)
		{
		Prepend(p);
		return;
		}

	TSem::OnAdd(p, this);

	// Work out if insert position is before the current iterate position
	bool bBeforeIteratePos=IsBeforeIteratePos(pBefore);

	// Insert in middle
	chainmember(p).m_pNext=pBefore;
	chainmember(p).m_pPrev=chainmember(pBefore).m_pPrev;
	chainmember(chainmember(p).m_pNext).m_pPrev=p;
	chainmember(chainmember(p).m_pPrev).m_pNext=p;

	// Update size
	m_iSize++;

	// Update iterate position
	if (bBeforeIteratePos)
	{
		m_pIterElem=GetPrevious(m_pIterElem);
	}
}


// Work out if an element is before the current iterate position
template_linkedlist
bool CLinkedList_template::IsBeforeIteratePos(T* pElem) const
{
	if (!m_pIterElem)
		return false;

	ASSERT(m_iSize!=0);
	ASSERT(Contains(pElem));

	if (m_pIterElem==pElem)
		return true;

	// Need to scan either before current iterate position or after, choose the smaller one
	if (m_iIterPos<m_iSize/2)
	{
		// Scan first half
		for (T* p=m_pIterElem; p!=NULL; p=GetPrevious(p))
		{
			if (p==pElem)
				return true;
		}
		return false;
	}
	else
	{
		// Scan second half
		for (T* p=m_pIterElem; p!=NULL; p=chainmember(p).m_pNext)
		{
			if (p==pElem)
				return false;
		}
		return true;
	}
}

template_linkedlist
T* CLinkedList_template::Detach(T* p)
{
	RemoveOrDetach(p, true);
	return p;
}

template_linkedlist
void CLinkedList_template::Remove(T* p)
{
	RemoveOrDetach(p, false);
}

template_linkedlist
void CLinkedList_template::RemoveOrDetach(T* p, bool bDetach)
{
	ASSERT(p!=NULL);
	ASSERT(Contains(p));

	// Update iterate position
	if (IsBeforeIteratePos(p))
	{
		if (m_iIterPos==m_iSize-2)
		{
			// New iterate position will be last, clear iterate position
			m_pIterElem=NULL;
		}
		else
		{
			// Move iterate position forward...
			m_pIterElem=GetNext(m_pIterElem);
		}
	}

	// Deleting current item?
	if (p==m_pCurrent)
	{
		if (m_bLastIterForward)
			m_pCurrent=m_pCurrent==m_pFirst ? NULL : chainmember(m_pCurrent).m_pPrev;
		else
			m_pCurrent=chainmember(m_pCurrent).m_pNext;
	}

	if (m_pFirst==p)
		{
		// Removing first element

		// Only remaining item?
		if (chainmember(m_pFirst).m_pPrev==m_pFirst)
			{
			// Empty list
			m_pFirst=NULL;
			}
		else
			{
			ASSERT(chainmember(m_pFirst).m_pNext);

			// Maintain the last item pointer...
			chainmember(chainmember(m_pFirst).m_pNext).m_pPrev=chainmember(m_pFirst).m_pPrev;
			m_pFirst=chainmember(m_pFirst).m_pNext;
			}
		}
	else if (chainmember(m_pFirst).m_pPrev==p)
		{
		// Clear previous item's next pointer
		chainmember(chainmember(p).m_pPrev).m_pNext=NULL;

		// Store the new last element
		chainmember(m_pFirst).m_pPrev=chainmember(p).m_pPrev;
		}
	else
		{
		// Removing internal element
		ASSERT(chainmember(p).m_pPrev!=NULL);
		ASSERT(chainmember(p).m_pNext!=NULL);
		chainmember(chainmember(p).m_pPrev).m_pNext=chainmember(p).m_pNext;
		chainmember(chainmember(p).m_pNext).m_pPrev=chainmember(p).m_pPrev;
		}


#ifdef _DEBUG
	chainmember(p).m_pNext=NULL;
	chainmember(p).m_pPrev=NULL;
#endif

	if (bDetach)
		TSem::OnDetach(p, this);
	else
		TSem::OnRemove(p, this);

	m_iSize--;
}

template_linkedlist
void CLinkedList_template::RemoveAll()
{
	while (m_pFirst)
	{
		Remove(m_pFirst);
	}
	m_pIterElem=NULL;
}

template_linkedlist
bool CLinkedList_template::Contains(T* p) const
{
	T* pIter=m_pFirst;
	while (pIter)
		{
		if (pIter==p)
			return true;
		pIter=chainmember(pIter).m_pNext;
		}
	return false;
}

template_linkedlist
bool CLinkedList_template::IsEmpty() const
{
	return m_iSize==0;
}

template_linkedlist
int CLinkedList_template::GetSize() const
{
	return m_iSize;
}

template_linkedlist
T* CLinkedList_template::GetFirst() const
{
	return m_pFirst;
}

template_linkedlist
T* CLinkedList_template::GetLast() const
{
	return m_pFirst ? chainmember(m_pFirst).m_pPrev : NULL;
}

template_linkedlist
T* CLinkedList_template::GetNext(T* p) const
{
	ASSERT(Contains(p));
	return chainmember(p).m_pNext;
}

template_linkedlist
T* CLinkedList_template::GetPrevious(T* p) const
{
	ASSERT(Contains(p));
	if (p==m_pFirst)
		return NULL;
	else
		return chainmember(p).m_pPrev;
}


template_linkedlist
bool CLinkedList_template::IsEOF() const
{
	return m_pFirst==NULL || (m_pCurrent==NULL && m_bLastIterForward);
}


template_linkedlist
bool CLinkedList_template::IsBOF() const
{
	return m_pFirst==NULL || (m_pCurrent==NULL && !m_bLastIterForward);
}


template_linkedlist
void CLinkedList_template::MoveFirst()
{
	m_pCurrent=m_pFirst;
	m_bLastIterForward=true;
}


template_linkedlist
void CLinkedList_template::MoveLast()
{
	m_pCurrent=GetLast();
	m_bLastIterForward=false;
}


template_linkedlist
void CLinkedList_template::MoveNext()
{
	if (m_pCurrent)
		m_pCurrent=chainmember(m_pCurrent).m_pNext;
	else
		if (m_bLastIterForward)
			m_pCurrent=m_pFirst;
	m_bLastIterForward=true;
}

template_linkedlist
void CLinkedList_template::MovePrevious()
{
	if (m_pCurrent)
	{
		if (m_pCurrent==m_pFirst)
			m_pCurrent=NULL;
		else
			m_pCurrent=chainmember(m_pCurrent).m_pPrev;
	}
	else
		if (!m_bLastIterForward)
			m_pCurrent=GetLast();
	m_bLastIterForward=false;
}

template_linkedlist
T* CLinkedList_template::Current() const
{
	ASSERT(m_pCurrent!=NULL);
	return m_pCurrent;
}


template_linkedlist
T* CLinkedList_template::GetAt(int iPos) const
{
	ASSERT(iPos>=0 && iPos<GetSize());

	// First
	if (iPos==0)
	{
		m_pIterElem=NULL;
		m_iIterPos=-1;
		return GetFirst();
	}

	// Second?
	if (iPos==1)
	{
		m_iIterPos=1;
		return m_pIterElem=GetNext(GetFirst());
	}

	// Last?
	if (iPos==m_iSize-1)
	{
		m_pIterElem=NULL;
		m_iIterPos=-1;
		return GetLast();
	}

	// Second last?
	if (iPos==m_iSize-2)
	{
		m_iIterPos=iPos;
		return m_pIterElem=GetPrevious(GetLast());
	}

	// Check for next/prev/current from current iterate position
	if (m_pIterElem)
	{
		if (iPos==m_iIterPos)
		{
			return m_pIterElem;
		}
		if (iPos==m_iIterPos+1)
		{
			m_iIterPos=iPos;
			return m_pIterElem=GetNext(m_pIterElem);
		}
		if (iPos==m_iIterPos-1)
		{
			m_iIterPos=iPos;
			return m_pIterElem=GetPrevious(m_pIterElem);
		}
	}

	int iDistanceFromIterPos=abs(m_iIterPos-iPos);
	int iDistanceFromStart=iPos;
	int iDistanceFromEnd=(m_iSize-1)-iPos;

	if (m_pIterElem && iDistanceFromIterPos<iDistanceFromEnd && iDistanceFromIterPos<iDistanceFromEnd)
	{
		while (m_iIterPos<iPos)
		{
			m_pIterElem=chainmember(m_pIterElem).m_pNext;
			m_iIterPos++;
		}
		while (m_iIterPos>iPos)
		{
			m_pIterElem=chainmember(m_pIterElem).m_pPrev;
			m_iIterPos--;
		}
	}
	else
	{
		if (iDistanceFromStart<iDistanceFromEnd)
		{
			m_pIterElem=m_pFirst;
			for (int i=0; i<iPos; i++)
				m_pIterElem=chainmember(m_pIterElem).m_pNext;
		}
		else
		{
			m_pIterElem=GetLast();
			for (int i=0; i<iDistanceFromEnd; i++)
			{
				m_pIterElem=chainmember(m_pIterElem).m_pPrev;
			}
		}
	}

	// Done
	m_iIterPos=iPos;
	return m_pIterElem;
}

template_linkedlist
T* CLinkedList_template::operator[](int iPos) const
{
	return GetAt(iPos);
}

// Stack operators
template_linkedlist
void CLinkedList_template::Push(T* val)
{
	Add(val);
}

template_linkedlist
bool CLinkedList_template::Pop(T*& val)
{
	if (IsEmpty())
	{
		val=NULL;
		return false;
	}
	else
	{
		val=Detach(GetLast());
		return true;
	}

}

template_linkedlist
T* CLinkedList_template::Pop()
{
	T* p;
	Pop(p);
	return p;
}

template_linkedlist
bool CLinkedList_template::Top(T*& val) const
{
	if (IsEmpty())
	{
		val=NULL;
		return false;
	}
	else
	{
		val=GetLast();
		return true;
	}
}

template_linkedlist
T* CLinkedList_template::Top() const
{
	return GetLast();
}

template_linkedlist
void CLinkedList_template::Enqueue(T* val)
{
	Add(val);
}

template_linkedlist
bool CLinkedList_template::Dequeue(T*& val)
{
	if (IsEmpty())
	{
		val=NULL;
		return false;
	}
	else
	{
		val=Detach(GetFirst());
		return true;
	}
}

template_linkedlist
T* CLinkedList_template::Dequeue()
{
	T* p;
	Dequeue(p);
	return p;
}

template_linkedlist
T* CLinkedList_template::Peek() const
{
	return GetFirst();
}

template_linkedlist
bool CLinkedList_template::Peek(T*& val) const
{
	val=GetFirst();
	return !IsEmpty();
}

/////////////////////////////////////////////////////////////////////////////
// CPlex implementation

// Constructor
template <class T>
CPlex<T>::CPlex(int iBlockSize)
{
	if (iBlockSize==-1)
	{
		iBlockSize=(256-sizeof(BLOCK))/sizeof(T);
	}

	if (iBlockSize<4)
		iBlockSize=4;

	m_iBlockSize=iBlockSize;

	m_pHead=NULL;
	m_pFreeList=NULL;
	m_iCount=0;
}

// Destructor
template <class T>
CPlex<T>::~CPlex()
{
	FreeAll();
}

// Allocate a new item
template <class T>
T* CPlex<T>::Alloc()
{
	// If no free list, create a new block
	if (!m_pFreeList)
		{
		// Allocate a new block
		BLOCK* pNewBlock=(BLOCK*)malloc(sizeof(BLOCK) + m_iBlockSize * sizeof(T));

		// Add to list of blocks
		pNewBlock->m_pNext=m_pHead;
		m_pHead=pNewBlock;

		// Setup free list chain
		m_pFreeList=(FREEITEM*)&pNewBlock->m_bData[0];
		FREEITEM* p=m_pFreeList;
		for (int i=0; i<m_iBlockSize-1; i++)
			{
			p->m_pNext=reinterpret_cast<FREEITEM*>(reinterpret_cast<char*>(p)+sizeof(T));
			p=p->m_pNext;
			}

		// NULL terminate the list
		p->m_pNext=NULL;
		}

	// Remove top item from the free list
	T* p=(T*)m_pFreeList;
	m_pFreeList=m_pFreeList->m_pNext;

	// Update count
	m_iCount++;

	new ((void*)p) T;

	// Return pointer
	return (T*)p;
}

template <class T>
void CPlex<T>::Free(T* p)
{
	ASSERT(m_iCount>0);

	Destructor(p);

	if (m_iCount==1)
		{
		// When freeing last item, free everything!
		FreeAll();
		}
	else
		{
		// Add to list of free items
		FREEITEM* pNewFreeItem=(FREEITEM*)p;
		pNewFreeItem->m_pNext=m_pFreeList;
		m_pFreeList=pNewFreeItem;

		// Update count
		m_iCount--;
		}
}

template <class T>
void CPlex<T>::FreeAll()
{
	// Release all blocks
	BLOCK* pBlock=m_pHead;
	while (pBlock)
		{
		// Save next
		BLOCK* pNext=pBlock->m_pNext;

		// Free it
		free(pBlock);

		// Move on
		pBlock=pNext;
		}

	// Reset state
	m_pHead=NULL;
	m_pFreeList=NULL;
	m_iCount=0;
}

template <class T>
int CPlex<T>::GetCount() const
{
	return m_iCount;
}

template <class T>
void CPlex<T>::SetBlockSize(int iNewBlockSize)
{
	ASSERT(m_pHead==NULL && "SetBlockSize only support when plex is empty");
	m_iBlockSize=iNewBlockSize;
}


/////////////////////////////////////////////////////////////////////////////
// CMap implementation

// Constructor
template <class TKey, class TValue, class TKeySem, class TValueSem, class TKeyArg>
CMap<TKey, TValue, TKeySem, TValueSem, TKeyArg>::CMap() :
	m_pRoot(&m_Leaf),
	m_iSize(0)
{
	m_Leaf.m_pParent = NULL;
	m_Leaf.m_pLeft= &m_Leaf;
	m_Leaf.m_pRight = &m_Leaf;
	m_Leaf.m_bRed = false;
	m_iIterPos=-1;
	m_pIterNode=NULL;
}

// Destructor
template <class TKey, class TValue, class TKeySem, class TValueSem, class TKeyArg>
CMap<TKey, TValue, TKeySem, TValueSem, TKeyArg>::~CMap()
{
	FreeNode(m_pRoot);
}


template <class TKey, class TValue, class TKeySem, class TValueSem, class TKeyArg>
inline int CMap<TKey, TValue, TKeySem, TValueSem, TKeyArg>::GetSize() const
{
	return m_iSize;
}


template <class TKey, class TValue, class TKeySem, class TValueSem, class TKeyArg>
inline bool CMap<TKey, TValue, TKeySem, TValueSem, TKeyArg>::IsEmpty() const
{
	return m_iSize==0;
}

template <class TKey, class TValue, class TKeySem, class TValueSem, class TKeyArg>
typename CMap<TKey, TValue, TKeySem, TValueSem, TKeyArg>::CKeyPair CMap<TKey, TValue, TKeySem, TValueSem, TKeyArg>::operator[](int iIndex) const
{
	ASSERT(iIndex>=0 && iIndex<m_iSize);
#ifdef _DEBUG_CHECKS
	CheckAll();
#endif

	if (iIndex==0)
	{
		m_iIterPos=0;
		m_pIterNode=m_pFirst;
		return CKeyPair(m_pIterNode->m_KeyPair.m_Key, m_pIterNode->m_KeyPair.m_Value);
	}

	if (iIndex==m_iSize-1)
	{
		m_iIterPos=m_iSize-1;
		m_pIterNode=m_pLast;
		return CKeyPair(m_pIterNode->m_KeyPair.m_Key, m_pIterNode->m_KeyPair.m_Value);
	}

	if (iIndex==m_iIterPos)
	{
		return CKeyPair(m_pIterNode->m_KeyPair.m_Key, m_pIterNode->m_KeyPair.m_Value);
	}

	if (iIndex==m_iIterPos+1)
	{
		m_iIterPos=iIndex;
		m_pIterNode=m_pIterNode->m_pNext;
		return CKeyPair(m_pIterNode->m_KeyPair.m_Key, m_pIterNode->m_KeyPair.m_Value);
	}

	if (iIndex==m_iIterPos-1)
	{
		m_iIterPos=iIndex;
		m_pIterNode=m_pIterNode->m_pPrev;
		return CKeyPair(m_pIterNode->m_KeyPair.m_Key, m_pIterNode->m_KeyPair.m_Value);
	}

	int iDistanceFromIterPos=abs(m_iIterPos-iIndex);
	int iDistanceFromStart=iIndex;
	int iDistanceFromEnd=(m_iSize-1)-iIndex;

	if (m_iIterPos>=0 && iDistanceFromIterPos<iDistanceFromEnd && iDistanceFromIterPos<iDistanceFromEnd)
	{
		while (m_iIterPos<iIndex)
		{
			m_pIterNode=m_pIterNode->m_pNext;
			m_iIterPos++;
		}
		while (m_iIterPos>iIndex)
		{
			m_pIterNode=m_pIterNode->m_pPrev;
			m_iIterPos--;
		}
	}
	else
	{
		if (iDistanceFromStart<iDistanceFromEnd)
		{
			m_pIterNode=m_pFirst;
			for (int i=0; i<iIndex; i++)
				m_pIterNode=m_pIterNode->m_pNext;
		}
		else
		{
			m_pIterNode=m_pLast;
			for (int i=0; i<iDistanceFromEnd; i++)
			{
				m_pIterNode=m_pIterNode->m_pPrev;
			}
		}
	}

	m_iIterPos=iIndex;

#ifdef _DEBUG_CHECKS
	CheckAll();
#endif
	return CKeyPair(m_pIterNode->m_KeyPair.m_Key, m_pIterNode->m_KeyPair.m_Value);
}


template <class TKey, class TValue, class TKeySem, class TValueSem, class TKeyArg>
void CMap<TKey, TValue, TKeySem, TValueSem, TKeyArg>::Add(const TKey& Key, const TValue& Value)
{
	CNode* pNode = m_pRoot;
	CNode* pParent = NULL;

	int iCompare=0;
	while (pNode != &m_Leaf)
	{
		pParent = pNode;
		iCompare = TKeySem::Compare(Key, pNode->m_KeyPair.m_Key);

		if (iCompare < 0)
			pNode = pNode->m_pLeft;
		else if (iCompare > 0)
			pNode = pNode->m_pRight;
		else
		{
			// Found a duplicate, replace it. We replace the key too, since
			// equivalence is not always exact (e.g. case insensitive strings)
			TKeySem::OnRemove(pNode->m_KeyPair.m_Key, this);
			TValueSem::OnRemove(pNode->m_KeyPair.m_Value, this);
			pNode->m_KeyPair.m_Value = TValueSem::OnAdd(Value, this);
			pNode->m_KeyPair.m_Key = TKeySem::OnAdd(Key, this);
			#ifdef _DEBUG_CHECKS
			CheckAll();
			#endif
			return;
		}
	}

	CNode* pNew = m_NodePlex.Alloc();
	pNew->m_pParent = pParent;
	pNew->m_pLeft = &m_Leaf;
	pNew->m_pRight = &m_Leaf;
	pNew->m_bRed = true;
	pNew->m_KeyPair.m_Value = TValueSem::OnAdd(Value, this);
	pNew->m_KeyPair.m_Key = TKeySem::OnAdd(Key, this);

	if (pParent)
	{
		if (iCompare<0)
		{
			pParent->m_pLeft = pNew;
			pNew->m_pNext=pParent;
			pNew->m_pPrev=pParent->m_pPrev;
		}
		else if (iCompare>0)
		{
			pParent->m_pRight = pNew;
			pNew->m_pPrev=pParent;
			pNew->m_pNext=pParent->m_pNext;
		}
		else
		{
			ASSERT(false);
		}
	}
	else
	{
		m_pRoot = pNew;
		pNew->m_pPrev=NULL;
		pNew->m_pNext=NULL;
	}

	// Fix up traverse links
	if (pNew->m_pPrev)
		pNew->m_pPrev->m_pNext=pNew;
	else
		m_pFirst=pNew;

	if (pNew->m_pNext)
		pNew->m_pNext->m_pPrev=pNew;
	else
		m_pLast=pNew;

	if (m_pIterNode)
	{
		int iCompare=TKeySem::Compare(Key, m_pIterNode->m_KeyPair.m_Key);

		ASSERT(iCompare!=0);

		// If the new key is before the current iterate position, update the iterate position
		if (iCompare<0)
		{
			m_pIterNode=m_pIterNode->m_pPrev;
		}
	}

	// Now rebalance the tree.

	pNode = pNew;

	while (pNode != m_pRoot && pNode->m_pParent->m_bRed)
	{
		pParent = pNode->m_pParent;
		CNode* pGrandParent = pParent->m_pParent;

		if (pParent == pGrandParent->m_pLeft)
		{
			CNode* pUncle = pGrandParent->m_pRight;

			if (pUncle->m_bRed)
			{
				pParent->m_bRed = false;
				pUncle->m_bRed = false;
				pGrandParent->m_bRed = true;
				pNode = pGrandParent;
			}
			else
			{
				if (pNode == pParent->m_pRight)
				{
					pNode = pParent;
					RotateLeft(pNode);
				}

				pNode->m_pParent->m_bRed = false;
				pNode->m_pParent->m_pParent->m_bRed = true;
				RotateRight(pNode->m_pParent->m_pParent);
			}
		}
		else
		{
			CNode* pUncle = pGrandParent->m_pLeft;

			if (pUncle->m_bRed)
			{
				pParent->m_bRed = false;
				pUncle->m_bRed = false;
				pGrandParent->m_bRed = true;
				pNode = pGrandParent;
			}
			else
			{
				if (pNode == pParent->m_pLeft)
				{
					pNode = pParent;
					RotateRight(pNode);
				}

				pNode->m_pParent->m_bRed = false;
				pNode->m_pParent->m_pParent->m_bRed = true;
				RotateLeft(pNode->m_pParent->m_pParent);
			}
		}
	}

	m_pRoot->m_bRed = false;
	m_iSize++;

	#ifdef _DEBUG_CHECKS
	CheckAll();
	#endif
}

template <class TKey, class TValue, class TKeySem, class TValueSem, class TKeyArg>
void CMap<TKey, TValue, TKeySem, TValueSem, TKeyArg>::Remove(const TKeyArg& Key)
{
	RemoveOrDetach(Key, NULL);
}

template <class TKey, class TValue, class TKeySem, class TValueSem, class TKeyArg>
TValue CMap<TKey, TValue, TKeySem, TValueSem, TKeyArg>::Detach(const TKeyArg& Key)
{
	TValue val;
	RemoveOrDetach(Key, &val);
	return val;
}

template <class TKey, class TValue, class TKeySem, class TValueSem, class TKeyArg>
const TValue& CMap<TKey, TValue, TKeySem, TValueSem, TKeyArg>::Get(const TKeyArg& Key, const TValue& Default) const
{
	CNode* pNode=FindNode(Key);
	if (!pNode)
		return Default;

	return pNode->m_KeyPair.m_Value;
}

template <class TKey, class TValue, class TKeySem, class TValueSem, class TKeyArg>
bool CMap<TKey, TValue, TKeySem, TValueSem, TKeyArg>::Find(const TKeyArg& Key, TValue& Value) const
{
	CNode* pNode=FindNode(Key);
	if (!pNode)
		return false;
	Value=pNode->m_KeyPair.m_Value;
	return true;
}

template <class TKey, class TValue, class TKeySem, class TValueSem, class TKeyArg>
bool CMap<TKey, TValue, TKeySem, TValueSem, TKeyArg>::HasKey(const TKeyArg& Key) const
{
	return FindNode(Key)!=NULL;
}




#ifdef _DEBUG
template <class TKey, class TValue, class TKeySem, class TValueSem, class TKeyArg>
void CMap<TKey, TValue, TKeySem, TValueSem, TKeyArg>::CheckAll()
{
	CheckTree();
	CheckChain();
}

template <class TKey, class TValue, class TKeySem, class TValueSem, class TKeyArg>
void CMap<TKey, TValue, TKeySem, TValueSem, TKeyArg>::CheckChain()
{
	if (m_pFirst)
	{
		ASSERT(m_pFirst->m_pPrev==NULL);
		ASSERT(m_pLast!=NULL);
		ASSERT(m_pLast->m_pNext==NULL);

		int i=0;
		CNode* pNode=m_pFirst;
		while (pNode)
		{
			if (pNode->m_pPrev)
			{
				ASSERT(pNode->m_pPrev->m_pNext==pNode);
			}
			else
			{
				ASSERT(pNode==m_pFirst);
			}

			if (pNode->m_pNext)
			{
				// Check order
				int iCompare=TKeySem::Compare(pNode->m_KeyPair.m_Key, pNode->m_pNext->m_KeyPair.m_Key);
				ASSERT(iCompare<0);

				ASSERT(pNode->m_pNext->m_pPrev==pNode);
			}
			else
			{
				ASSERT(pNode==m_pLast);
			}

			if (i==m_iIterPos)
			{
				ASSERT(m_pIterNode==pNode);
			}

			pNode=pNode->m_pNext;
			i++;
		}

		if (m_iIterPos>=0)
		{
			ASSERT(m_pIterNode!=NULL);
		}


		ASSERT(i==m_iSize);
	}

}

template <class TKey, class TValue, class TKeySem, class TValueSem, class TKeyArg>
bool CMap<TKey, TValue, TKeySem, TValueSem, TKeyArg>::CheckTree(CNode* pNode)
{
	int lh = 1, rh = 1;

	if (!pNode)
		pNode = m_pRoot;

	if (pNode->m_pLeft != &m_Leaf)
		lh = CheckTree(pNode->m_pLeft);

	if (pNode->m_pRight != &m_Leaf)
		rh = CheckTree(pNode->m_pRight);

	ASSERT(lh == rh);

	return !!(lh + !pNode->m_bRed);
}
#endif

template <class TKey, class TValue, class TKeySem, class TValueSem, class TKeyArg>
void CMap<TKey, TValue, TKeySem, TValueSem, TKeyArg>::FreeNode(CNode* pNode)
{
	if (pNode && pNode!=&m_Leaf)
	{
		FreeNode(pNode->m_pLeft);
		FreeNode(pNode->m_pRight);
		TKeySem::OnRemove(pNode->m_KeyPair.m_Key, this);
		TValueSem::OnRemove(pNode->m_KeyPair.m_Value, this);
		m_NodePlex.Free(pNode);
	}
}

template <class TKey, class TValue, class TKeySem, class TValueSem, class TKeyArg>
typename CMap<TKey, TValue, TKeySem, TValueSem, TKeyArg>::CNode* CMap<TKey, TValue, TKeySem, TValueSem, TKeyArg>::nextNode(CNode* pNode)
{
	if (pNode->m_pRight != &m_Leaf)
	{
		pNode = pNode->m_pRight;

		while (pNode->m_pLeft != &m_Leaf)
			pNode = pNode->m_pLeft;

		return pNode;
	}

	CNode* pParent = pNode->m_pParent;

	while (pParent != &m_Leaf && pNode == &m_Leaf)
	{
		pNode = pParent;
		pParent = pParent->m_pParent;
	}

	return pParent;
}


template <class TKey, class TValue, class TKeySem, class TValueSem, class TKeyArg>
void CMap<TKey, TValue, TKeySem, TValueSem, TKeyArg>::RotateLeft(CNode* x)
{
	CNode* parent = m_Leaf.m_pParent;
	CNode* y = x->m_pRight;

	// Turn y's left subtree into x's right subtree

	x->m_pRight = y->m_pLeft;
	x->m_pRight->m_pParent = x;

	// Link x's parent to y

	y->m_pParent = x->m_pParent;

	if (x != m_pRoot)
	{
		if (x->m_pParent->m_pLeft == x)
			x->m_pParent->m_pLeft = y;
		else
			x->m_pParent->m_pRight = y;
	}
	else
		m_pRoot = y;

	// Put x on y's left

	y->m_pLeft = x;
	x->m_pParent = y;
	m_Leaf.m_pParent = parent;
}


template <class TKey, class TValue, class TKeySem, class TValueSem, class TKeyArg>
void CMap<TKey, TValue, TKeySem, TValueSem, TKeyArg>::RotateRight(CNode* y)
{
	CNode* parent = m_Leaf.m_pParent;
	CNode* x = y->m_pLeft;

	// Turn x's right subtree into y's left subtree

	y->m_pLeft = x->m_pRight;
	y->m_pLeft->m_pParent = y;

	// Link y's parent to x

	x->m_pParent = y->m_pParent;

	if (y != m_pRoot)
	{
		if (y->m_pParent->m_pLeft == y)
			y->m_pParent->m_pLeft = x;
		else
			y->m_pParent->m_pRight = x;
	}
	else
		m_pRoot = x;

	// Put y on x's right

	x->m_pRight = y;
	y->m_pParent = x;
	m_Leaf.m_pParent = parent;
}

template <class TKey, class TValue, class TKeySem, class TValueSem, class TKeyArg>
void CMap<TKey, TValue, TKeySem, TValueSem, TKeyArg>::RemoveOrDetach(const TKeyArg& Key, TValue* pvalDetached)
{

#ifdef _DEBUG_CHECKS
	CheckAll();
#endif

	CNode* z = m_pRoot;

	while (z != &m_Leaf)
	{
		int iCompare = TKeySem::Compare(Key, z->m_KeyPair.m_Key);

		if (iCompare < 0)
			z = z->m_pLeft;
		else if (iCompare > 0)
			z = z->m_pRight;
		else
			break;
	}

	if (z == &m_Leaf)
		return;

	CNode* y = (z->m_pLeft == &m_Leaf || z->m_pRight == &m_Leaf) ?	z : nextNode(z);

	CNode* x = (y->m_pLeft != &m_Leaf) ? y->m_pLeft : y->m_pRight;

	// Ensure that x->m_pParent is correct.
	// This is needed in case x == &m_Leaf

	x->m_pParent = y->m_pParent;

	if (y != m_pRoot)
	{
		if (y->m_pParent->m_pLeft == y)
			y->m_pParent->m_pLeft = x;
		else
			y->m_pParent->m_pRight = x;
	}
	else
		m_pRoot = x;


	if (m_pIterNode)
	{
		int iCompare=TKeySem::Compare(Key, m_pIterNode->m_KeyPair.m_Key);
		if (iCompare<=0)
		{
			m_pIterNode=m_pIterNode->m_pNext;
		}
	}

	TKeySem::OnRemove(z->m_KeyPair.m_Key, this);
	if (!pvalDetached)
	{
		TValueSem::OnRemove(z->m_KeyPair.m_Value, this);
	}
	else
	{
		TValueSem::OnDetach(z->m_KeyPair.m_Value, this);
		*pvalDetached=z->m_KeyPair.m_Value;
	}


	if (y != z)
	{
		// deleting value in z, but keeping z node and moving value from y node
		z->m_KeyPair.m_Key = y->m_KeyPair.m_Key;
		z->m_KeyPair.m_Value = y->m_KeyPair.m_Value;
		z->m_pNext = y->m_pNext;
		if (z->m_pNext)
			z->m_pNext->m_pPrev=z;
		else
			m_pLast=z;

		if (m_pIterNode==y)
			m_pIterNode=z;
	}
	else
	{
		// Update linked list
		if (z->m_pPrev)
			z->m_pPrev->m_pNext=z->m_pNext;
		else
			m_pFirst=z->m_pNext;

		if (z->m_pNext)
			z->m_pNext->m_pPrev=z->m_pPrev;
		else
			m_pLast=z->m_pPrev;

		if (m_pIterNode==z)
		{
			m_pIterNode=m_pIterNode->m_pNext;
			if (!m_pIterNode)
			{
				m_iIterPos=-1;
			}
		}
	}

	// Rebalance the tree (see page 274 of Introduction to Algorithms)

	if (!y->m_bRed)
	{
		CNode* pNode = x;

		while (pNode != m_pRoot && !pNode->m_bRed)
		{
			if (pNode == pNode->m_pParent->m_pLeft)
			{
				CNode* pSibling = pNode->m_pParent->m_pRight;

				if (pSibling->m_bRed)
				{
					// Case 1: Sibling is m_bRed
					pSibling->m_bRed = false;
					pNode->m_pParent->m_bRed = true;
					RotateLeft(pNode->m_pParent);
					pSibling = pNode->m_pParent->m_pRight;
				}
				if (!pSibling->m_pLeft->m_bRed && !pSibling->m_pRight->m_bRed)
				{
					// Case 2: Sibling and its children are all black
					pSibling->m_bRed = true;
					pNode = pNode->m_pParent;
					continue;
				}
				else if (!pSibling->m_pRight->m_bRed)
				{
					// Case 3: Sibling and its right child are both black
					pSibling->m_pLeft->m_bRed = false;
					pSibling->m_bRed = true;
					RotateRight(pSibling);
					pSibling = pNode->m_pParent->m_pRight;
				}

				// Case 4: Sibling and its left child are both black
				pSibling->m_bRed = pNode->m_pParent->m_bRed;
				pNode->m_pParent->m_bRed = false;
				pSibling->m_pRight->m_bRed = false;
				RotateLeft(pNode->m_pParent);
				pNode = m_pRoot;
			}
			else
			{
				CNode* pSibling = pNode->m_pParent->m_pLeft;

				if (pSibling->m_bRed)
				{
					// Case 5: Sibling is m_bRed
					pSibling->m_bRed = false;
					pNode->m_pParent->m_bRed = true;
					RotateRight(pNode->m_pParent);
					pSibling = pNode->m_pParent->m_pLeft;
				}
				if (!pSibling->m_pLeft->m_bRed && !pSibling->m_pRight->m_bRed)
				{
					// Case 6: Sibling and its children are all black
					pSibling->m_bRed = true;
					pNode = pNode->m_pParent;
					continue;
				}
				else if (!pSibling->m_pLeft->m_bRed)
				{
					// Case 7: Sibling and its left child are both black
					pSibling->m_pRight->m_bRed = false;
					pSibling->m_bRed = true;
					RotateLeft(pSibling);
					pSibling = pNode->m_pParent->m_pLeft;
				}

				// Case 8: Sibling and its right child are both black
				pSibling->m_bRed = pNode->m_pParent->m_bRed;
				pNode->m_pParent->m_bRed = false;
				pSibling->m_pLeft->m_bRed = false;
				RotateRight(pNode->m_pParent);
				pNode = m_pRoot;
			}
		}

		pNode->m_bRed = false;
	}

	m_NodePlex.Free(y);
	m_iSize--;

	#ifdef _DEBUG_CHECKS
	CheckAll();
	#endif
}

template <class TKey, class TValue, class TKeySem, class TValueSem, class TKeyArg>
void CMap<TKey, TValue, TKeySem, TValueSem, TKeyArg>::RemoveAll()
{
	FreeNode(m_pRoot);
	m_pRoot = &m_Leaf;
	m_pFirst = NULL;
	m_pLast = NULL;
	m_iSize = 0;
	m_iIterPos=-1;
	m_pIterNode=NULL;

	#ifdef _DEBUG_CHECKS
	CheckAll();
	#endif
}

template <class TKey, class TValue, class TKeySem, class TValueSem, class TKeyArg>
typename CMap<TKey, TValue, TKeySem, TValueSem, TKeyArg>::CNode* CMap<TKey, TValue, TKeySem, TValueSem, TKeyArg>::FindNode(const TKeyArg& Key) const
{
	CNode* pNode = m_pRoot;

	while (pNode != &m_Leaf)
	{
		int iCompare = TKeySem::Compare(Key, pNode->m_KeyPair.m_Key);

		if (iCompare < 0)
			pNode = pNode->m_pLeft;
		else if (iCompare > 0)
			pNode = pNode->m_pRight;
		else
			return pNode;
	}

	return NULL;
}


/////////////////////////////////////////////////////////////////////////////
// Hashing routines

/*
* SuperFastHash.cpp
* Copyright 2004-2007 by Paul Hsieh
* http://www.azillionmonkeys.com/qed/hash.html

  This code is covered by Paul Hsieh derivative licence
    http://www.azillionmonkeys.com/qed/license-derivative.html
*/


#undef get16bits
#if (defined(__GNUC__) && defined(__i386__)) || defined(__WATCOMC__) \
	|| defined(_MSC_VER) || defined (__BORLANDC__) || defined (__TURBOC__)
#define get16bits(d) (*((const unsigned short *) (d)))
#endif

#if !defined (get16bits)
#define get16bits(d) ((((unsigned long)(((const uint8_t *)(d))[1])) << 8)\
	+(unsigned long)(((const uint8_t *)(d))[0]) )
#endif

inline unsigned long SuperFastHash (const char * data, int len) {
	unsigned long hash = len, tmp;
	int rem;

	if (len <= 0 || data == NULL) return 0;

	rem = len & 3;
	len >>= 2;

	/* Main loop */
	for (;len > 0; len--) {
		hash  += get16bits (data);
		tmp    = (get16bits (data+2) << 11) ^ hash;
		hash   = (hash << 16) ^ tmp;
		data  += 2*sizeof (unsigned short);
		hash  += hash >> 11;
	}

	/* Handle end cases */
	switch (rem) {
	case 3: hash += get16bits (data);
		hash ^= hash << 16;
		hash ^= data[sizeof (unsigned short)] << 18;
		hash += hash >> 11;
		break;
	case 2: hash += get16bits (data);
		hash ^= hash << 11;
		hash += hash >> 17;
		break;
	case 1: hash += *data;
		hash ^= hash << 10;
		hash += hash >> 1;
	}

	/* Force "avalanching" of final 127 bits */
	hash ^= hash << 3;
	hash += hash >> 5;
	hash ^= hash << 4;
	hash += hash >> 17;
	hash ^= hash << 25;
	hash += hash >> 6;

	return hash;
}


/////////////////////////////////////////////////////////////////////////////
// CHashMap

// Minimum hash table size - must be a power of two
#define MIN_TABLE_SIZE	64
#define REALLOC_PERCENT	70


// Constructor
template <class TKey, class TValue, class TKeySem, class TValueSem, class TKeyArg, class THash>
CHashMap<TKey,TValue,TKeySem,TValueSem,TKeyArg,THash>::CHashMap(int iInitialSize) :
	m_iInitialSize(iInitialSize)
{
}

// Destructor
template <class TKey, class TValue, class TKeySem, class TValueSem, class TKeyArg, class THash>
CHashMap<TKey,TValue,TKeySem,TValueSem,TKeyArg,THash>::~CHashMap()
{
	RemoveAll();
}


template <class TKey, class TValue, class TKeySem, class TValueSem, class TKeyArg, class THash>
inline int CHashMap<TKey,TValue,TKeySem,TValueSem,TKeyArg,THash>::GetSize() const
{
	return m_List.GetSize();
}


template <class TKey, class TValue, class TKeySem, class TValueSem, class TKeyArg, class THash>
inline bool CHashMap<TKey,TValue,TKeySem,TValueSem,TKeyArg,THash>::IsEmpty() const
{
	return m_List.IsEmpty();
}

template <class TKey, class TValue, class TKeySem, class TValueSem, class TKeyArg, class THash>
typename CHashMap<TKey,TValue,TKeySem,TValueSem,TKeyArg,THash>::CKeyPair CHashMap<TKey,TValue,TKeySem,TValueSem,TKeyArg,THash>::operator[](int iIndex) const
{
	ASSERT(iIndex>=0 && iIndex<m_List.GetSize());

	// Get node from list
	CNode* pNode=m_List.GetAt(iIndex);

	// Create key pair
	return CKeyPair(pNode->m_KeyPair.m_Key, pNode->m_KeyPair.m_Value);
}

template <class TKey, class TValue, class TKeySem, class TValueSem, class TKeyArg, class THash>
void CHashMap<TKey,TValue,TKeySem,TValueSem,TKeyArg,THash>::InitHashTable(int iSize)
{
	// Get the first power of two thats as big as the desired size
	int iTableSize = MIN_TABLE_SIZE;
	while(iTableSize < iSize)
		iTableSize <<= 1;

	// Allocate the table
	m_Table.SetSize(iTableSize, NULL);

	// Pre-calculate constants that are based on the table size
	m_nHashMask = m_Table.GetSize()-1;
	m_iThreshold = m_Table.GetSize() * REALLOC_PERCENT / 100;
}


template <class TKey, class TValue, class TKeySem, class TValueSem, class TKeyArg, class THash>
void CHashMap<TKey,TValue,TKeySem,TValueSem,TKeyArg,THash>::Rehash(int iNewSize)
{
	// Resize the table
	InitHashTable(iNewSize);

	// Clear it
	memset(m_Table.GetBuffer(), 0, sizeof(CNode*) * iNewSize);

	// Insert each node into the new table
	CNode* pNode=m_List.GetFirst();
	while (pNode)
	{
		// Hash the key
		unsigned int nHash = THash::Hash(pNode->m_KeyPair.m_Key) & m_nHashMask;

		// Add to table
		pNode->m_pHashNext=m_Table[nHash];
		m_Table.ReplaceAt(nHash, pNode);

		// Get next node
		pNode=pNode->m_Chain.m_pNext;
	}
}


template <class TKey, class TValue, class TKeySem, class TValueSem, class TKeyArg, class THash>
void CHashMap<TKey,TValue,TKeySem,TValueSem,TKeyArg,THash>::Add(const TKey& Key, const TValue& Value)
{
	// Make sure table created
	if (!m_Table.GetSize())
		InitHashTable(m_iInitialSize);

	// Hash the key
	unsigned int nHash = THash::Hash(Key) & m_nHashMask;

	// Check for duplicate and just replace value if found
	CNode* pNode=m_Table[nHash];
	while (pNode)
	{
		if (TKeySem::Compare(Key, pNode->m_KeyPair.m_Key)==0)
		{
			// Replace value
			TKeySem::OnRemove(pNode->m_KeyPair.m_Key, this);
			TValueSem::OnRemove(pNode->m_KeyPair.m_Value, this);
			pNode->m_KeyPair.m_Value = TValueSem::OnAdd(Value, this);
			pNode->m_KeyPair.m_Key = TKeySem::OnAdd(Key, this);
			return;
		}

		// Next
		pNode=pNode->m_pHashNext;
	}


	// Create a new node
	CNode* pNew = m_NodePlex.Alloc();
	pNew->m_KeyPair.m_Value = TValueSem::OnAdd(Value, this);
	pNew->m_KeyPair.m_Key = TKeySem::OnAdd(Key, this);

	// Setup collision chain
	pNew->m_pHashNext=m_Table[nHash];
	m_Table.ReplaceAt(nHash, pNew);

	/// Add to navigation link
	m_List.Add(pNew);

	// Time to rehash?
	if(m_List.GetSize() > m_iThreshold)
		Rehash(m_Table.GetSize() * 2);

	// Done!
}

template <class TKey, class TValue, class TKeySem, class TValueSem, class TKeyArg, class THash>
void CHashMap<TKey,TValue,TKeySem,TValueSem,TKeyArg,THash>::Remove(const TKeyArg& Key)
{
	RemoveOrDetach(Key, NULL);
}

template <class TKey, class TValue, class TKeySem, class TValueSem, class TKeyArg, class THash>
TValue CHashMap<TKey,TValue,TKeySem,TValueSem,TKeyArg,THash>::Detach(const TKeyArg& Key)
{
	TValue val;
	RemoveOrDetach(Key, &val);
	return val;
}

template <class TKey, class TValue, class TKeySem, class TValueSem, class TKeyArg, class THash>
const TValue& CHashMap<TKey,TValue,TKeySem,TValueSem,TKeyArg,THash>::Get(const TKeyArg& Key, const TValue& Default) const
{
	CNode* pNode=FindNode(Key);
	if (!pNode)
		return Default;

	return pNode->m_KeyPair.m_Value;
}

template <class TKey, class TValue, class TKeySem, class TValueSem, class TKeyArg, class THash>
bool CHashMap<TKey,TValue,TKeySem,TValueSem,TKeyArg,THash>::Find(const TKeyArg& Key, TValue& Value) const
{
	CNode* pNode=FindNode(Key);
	if (!pNode)
		return false;
	Value=pNode->m_KeyPair.m_Value;
	return true;
}

template <class TKey, class TValue, class TKeySem, class TValueSem, class TKeyArg, class THash>
bool CHashMap<TKey,TValue,TKeySem,TValueSem,TKeyArg,THash>::HasKey(const TKeyArg& Key) const
{
	return FindNode(Key)!=NULL;
}

template <class TKey, class TValue, class TKeySem, class TValueSem, class TKeyArg, class THash>
typename CHashMap<TKey, TValue, TKeySem, TValueSem, TKeyArg, THash>::CNode* CHashMap<TKey, TValue, TKeySem, TValueSem, TKeyArg, THash>::FindNode(const TKeyArg& Key) const
{
	if (IsEmpty())
		return NULL;

	// Hash the key
	unsigned int nHash = THash::Hash(Key) & m_nHashMask;

	// Check for duplicate and just replace value if found
	CNode* pNode=m_Table[nHash];
	while (pNode)
	{
		if (TKeySem::Compare(Key, pNode->m_KeyPair.m_Key)==0)
			return pNode;

		pNode=pNode->m_pHashNext;
	}

	return NULL;
}


template <class TKey, class TValue, class TKeySem, class TValueSem, class TKeyArg, class THash>
void CHashMap<TKey,TValue,TKeySem,TValueSem,TKeyArg,THash>::RemoveOrDetach(const TKeyArg& Key, TValue* pvalDetached)
{
	if (IsEmpty())
		return;

	// Hash the key
	unsigned int nHash = THash::Hash(Key) & m_nHashMask;

	// Check for duplicate and just replace value if found
	CNode* pNode=m_Table[nHash];
	CNode* pPrev=NULL;
	while (pNode)
	{
		// Match?
		if (TKeySem::Compare(Key, pNode->m_KeyPair.m_Key)==0)
		{
			// Do release semantics
			TKeySem::OnRemove(pNode->m_KeyPair.m_Key, this);
			if (!pvalDetached)
			{
				TValueSem::OnRemove(pNode->m_KeyPair.m_Value, this);
			}
			else
			{
				TValueSem::OnDetach(pNode->m_KeyPair.m_Value, this);
				*pvalDetached=pNode->m_KeyPair.m_Value;
			}

			// Remove from hash list
			if (pPrev)
				pPrev->m_pHashNext=pNode->m_pHashNext;
			else
				m_Table.ReplaceAt(nHash, pNode->m_pHashNext);

			// Remove from list
			m_List.Remove(pNode);
			m_NodePlex.Free(pNode);

			return;
		}

		pPrev=pNode;
		pNode=pNode->m_pHashNext;
	}

	// Not found

}

template <class TKey, class TValue, class TKeySem, class TValueSem, class TKeyArg, class THash>
void CHashMap<TKey,TValue,TKeySem,TValueSem,TKeyArg,THash>::RemoveAll()
{
	// Call release semantics on all keys and values
	CNode* pNode;
	while ((pNode=m_List.GetFirst()))
	{
		TKeySem::OnRemove(pNode->m_KeyPair.m_Key, this);
		TValueSem::OnRemove(pNode->m_KeyPair.m_Value, this);
		m_List.Remove(pNode);
		m_NodePlex.Free(pNode);
	}

	// Clear the list
	m_Table.RemoveAll();
	m_nHashMask=0;
	m_iThreshold=0;
}

/////////////////////////////////////////////////////////////////////////////
// CIndex

template <class TKey, class TValue, class TKeySem, class TValueSem, class TKeyArg>
CIndex<TKey,TValue,TKeySem,TValueSem, TKeyArg>::CIndex()
{
}

template <class TKey, class TValue, class TKeySem, class TValueSem, class TKeyArg>
CIndex<TKey,TValue,TKeySem,TValueSem, TKeyArg>::~CIndex()
{
	RemoveAll();
}

template <class TKey, class TValue, class TKeySem, class TValueSem, class TKeyArg>
int CIndex<TKey,TValue,TKeySem,TValueSem, TKeyArg>::GetSize() const
{
	return m_Entries.GetSize();
}

template <class TKey, class TValue, class TKeySem, class TValueSem, class TKeyArg>
bool CIndex<TKey,TValue,TKeySem,TValueSem, TKeyArg>::IsEmpty() const
{
	return GetSize()==0;
}

template <class TKey, class TValue, class TKeySem, class TValueSem, class TKeyArg>
typename CIndex<TKey,TValue,TKeySem,TValueSem, TKeyArg>::CKeyPair CIndex<TKey,TValue,TKeySem,TValueSem, TKeyArg>::operator[](int iIndex) const
{
	ASSERT(iIndex>=0 && iIndex<GetSize());
	return CKeyPair(m_Entries[iIndex]);
}

template <class TKey, class TValue, class TKeySem, class TValueSem, class TKeyArg>
void CIndex<TKey,TValue,TKeySem,TValueSem, TKeyArg>::Add(const TKey& Key, const TValue& Value)
{
	int iPos;
#ifdef _MSC_VER
	if (m_Entries.QuickSearchKey<TKeyArg>(Key, &CEntry::CompareKey, iPos))
#else
	if (m_Entries.QuickSearchKey(Key, &CEntry::CompareKey, iPos))
#endif
	{
		m_Entries.ReplaceAt(iPos, CEntry(TKeySem::OnAdd(Key, this), TValueSem::OnAdd(Value, this)));
	}
	else
	{
		m_Entries.InsertAt(iPos, CEntry(TKeySem::OnAdd(Key, this), TValueSem::OnAdd(Value, this)));
	}
}

template <class TKey, class TValue, class TKeySem, class TValueSem, class TKeyArg>
void CIndex<TKey,TValue,TKeySem,TValueSem, TKeyArg>::Remove(const TKeyArg& Key)
{
	int iPos;
#ifdef _MSC_VER
	if (m_Entries.QuickSearchKey<TKeyArg>(Key, &CEntry::CompareKey, iPos))
#else
	if (m_Entries.QuickSearchKey(Key, &CEntry::CompareKey, iPos))
#endif
	{
		TKeySem::OnRemove(m_Entries[iPos].m_Key,this);
		TValueSem::OnRemove(m_Entries[iPos].m_Value,this);
		m_Entries.RemoveAt(iPos);
	}

}

template <class TKey, class TValue, class TKeySem, class TValueSem, class TKeyArg>
void CIndex<TKey,TValue,TKeySem,TValueSem, TKeyArg>::RemoveAll()
{
	for (int i=0; i<m_Entries.GetSize(); i++)
	{
		TKeySem::OnRemove(m_Entries[i].m_Key,this);
		TValueSem::OnRemove(m_Entries[i].m_Value,this);
	}
	m_Entries.RemoveAll();
}

template <class TKey, class TValue, class TKeySem, class TValueSem, class TKeyArg>
TValue CIndex<TKey,TValue,TKeySem,TValueSem, TKeyArg>::Detach(const TKeyArg& Key)
{
	int iPos;
#ifdef _MSC_VER
	if (m_Entries.QuickSearchKey<TKeyArg>(Key, &CEntry::CompareKey, iPos))
#else
	if (m_Entries.QuickSearchKey(Key, &CEntry::CompareKey, iPos))
#endif
	{
		TValue v=m_Entries[iPos].m_Value;
		TKeySem::OnRemove(m_Entries[iPos].m_Key,this);
		TValueSem::OnDetach(m_Entries[iPos].m_Value,this);
		m_Entries.RemoveAt(iPos);
		return v;
	}
	return TValue();
}

template <class TKey, class TValue, class TKeySem, class TValueSem, class TKeyArg>
const TValue& CIndex<TKey,TValue,TKeySem,TValueSem, TKeyArg>::Get(const TKeyArg& Key, const TValue& Default) const
{
	int iPos;
#ifdef _MSC_VER
	if (m_Entries.QuickSearchKey<TKeyArg>(Key, &CEntry::CompareKey, iPos))
#else
	if (m_Entries.QuickSearchKey(Key, &CEntry::CompareKey, iPos))
#endif
	{
		return m_Entries[iPos].m_Value;
	}
	else
	{
		return Default;
	}
}

template <class TKey, class TValue, class TKeySem, class TValueSem, class TKeyArg>
bool CIndex<TKey,TValue,TKeySem,TValueSem, TKeyArg>::Find(const TKeyArg& Key, TValue& Value) const
{
	int iPos;
#ifdef _MSC_VER
	if (m_Entries.QuickSearchKey<TKeyArg>(Key, &CEntry::CompareKey, iPos))
#else
	if (m_Entries.QuickSearchKey(Key, &CEntry::CompareKey, iPos))
#endif
	{
		Value=m_Entries[iPos].m_Value;
		return true;
	}
	else
	{
		return false;
	}
}

template <class TKey, class TValue, class TKeySem, class TValueSem, class TKeyArg>
bool CIndex<TKey,TValue,TKeySem,TValueSem, TKeyArg>::HasKey(const TKeyArg& Key) const
{
	int iPos;
#ifdef _MSC_VER
	return m_Entries.QuickSearchKey<TKeyArg>(Key, &CEntry::CompareKey, iPos);
#else
	return m_Entries.QuickSearchKey(Key, &CEntry::CompareKey, iPos);
#endif
}







/////////////////////////////////////////////////////////////////////////////
// Implementation of CRingBuffer

// Constructor
template <class T, class TSem>
CRingBuffer<T,TSem>::CRingBuffer(int iCapacity)
{
	m_iSize=0;
	m_iCapacity=iCapacity;
	m_pMem=(T*)malloc(sizeof(T)*m_iCapacity);
	m_pWritePos=m_pMem;
	m_pReadPos=m_pMem;
	m_bOverflow=false;
}

// Destructor
template <class T, class TSem>
CRingBuffer<T,TSem>::~CRingBuffer()
{
	RemoveAll();
	free(m_pMem);
}

template <class T, class TSem>
void CRingBuffer<T,TSem>::Reset(int iNewCapacity)
{
	RemoveAll();
	if (iNewCapacity && iNewCapacity!=m_iCapacity)
	{
		free(m_pMem);

		m_iSize=0;
		m_iCapacity=iNewCapacity;
		m_pMem=(T*)malloc(sizeof(T)*m_iCapacity);
		m_pWritePos=m_pMem;
		m_pReadPos=m_pMem;
		m_bOverflow=false;
	}
}

template <class T, class TSem>
bool CRingBuffer<T,TSem>::IsEmpty() const
{
	return m_iSize==0;
}

template <class T, class TSem>
bool CRingBuffer<T,TSem>::IsFull() const
{
	return m_iSize==m_iCapacity;
}

template <class T, class TSem>
bool CRingBuffer<T,TSem>::IsOverflow() const
{
	return m_bOverflow;
}

template <class T, class TSem>
bool CRingBuffer<T,TSem>::Enqueue(const T& t)
{
	// Check if full
	if (IsFull())
		{
		m_bOverflow=true;
		return false;
		}

	T* pNextWritePos=AdvancePtr(m_pWritePos);
	m_iSize++;

	Constructor(m_pWritePos, TSem::OnAdd(t, this));

	// Store next write pos
	m_pWritePos=pNextWritePos;

	return true;
}

template <class T, class TSem>
T CRingBuffer<T,TSem>::Dequeue()
{
	ASSERT(!IsEmpty());

	// Remember where we are
	T* pSave=m_pReadPos;

	// Advance read position
	m_pReadPos=AdvancePtr(m_pReadPos);

	m_iSize--;

	// Return data
	T temp=*pSave;

	// Detach
	TSem::OnDetach(*pSave, this);

	// and destroy
	Destructor(pSave);

	// Done
	return temp;
}

template <class T, class TSem>
bool CRingBuffer<T,TSem>::Dequeue(T& t)
{
	if (IsEmpty())
		return false;

	t=Dequeue();
	return true;
}

template <class T, class TSem>
T CRingBuffer<T,TSem>::Peek()
{
	ASSERT(!IsEmpty());

	// Remember where we are
	return *m_pReadPos;
}

template <class T, class TSem>
bool CRingBuffer<T,TSem>::Peek(T& t)
{
	if (IsEmpty())
		return false;

	t=Peek();
	return true;
}

template <class T, class TSem>
bool CRingBuffer<T,TSem>::Unenqueue(T& t)
{
	if (IsEmpty())
		return false;

	t=Unenqueue();
	return true;
}

template <class T, class TSem>
T CRingBuffer<T,TSem>::Unenqueue()
{
	ASSERT(!IsEmpty());
	m_pWritePos=RewindPtr(m_pWritePos);


	// Return data
	T temp=*m_pWritePos;

	// Detach
	TSem::OnDetach(*m_pWritePos, this);

	// and destroy
	Destructor(m_pWritePos);

	// Done
	m_iSize--;
	return temp;
}

template <class T, class TSem>
bool CRingBuffer<T,TSem>::PeekLast(T& t)
{
	if (IsEmpty())
		return false;

	t=PeekLast();
	return true;
}

template <class T, class TSem>
T CRingBuffer<T,TSem>::PeekLast()
{
	ASSERT(!IsEmpty());
	return GetAt(m_iSize-1);
}

template <class T, class TSem>
void CRingBuffer<T,TSem>::RemoveAll()
{
	while (!IsEmpty())
	{
		Dequeue();
	}

	m_bOverflow=false;
}

template <class T, class TSem>
int CRingBuffer<T,TSem>::GetCapacity() const
{
	return m_iCapacity;
}

template <class T, class TSem>
int CRingBuffer<T,TSem>::GetSize() const
{
	return m_iSize;
}

template <class T, class TSem>
T CRingBuffer<T,TSem>::GetAt(int iPos) const
{
	ASSERT(iPos>=0 && iPos<GetSize());

	// Next position
	T* p = m_pReadPos + iPos;

	// Past end?
	if (p>=m_pMem+m_iCapacity)
		{
		// Back to start
		p-=m_iCapacity;
		}

	return *p;
}

template <class T, class TSem>
T CRingBuffer<T,TSem>::operator [] (int iPos) const
{
	return GetAt(iPos);
}


// Operations
template <class T, class TSem>
T* CRingBuffer<T,TSem>::AdvancePtr(T* p) const
{
	// Next position
	p++;

	// At end?
	if (p==m_pMem+m_iCapacity)
		{
		// Back to start
		p=m_pMem;
		}

	return p;
}

template <class T, class TSem>
T* CRingBuffer<T,TSem>::RewindPtr(T* p) const
{
	// At start?
	if (p==m_pMem)
		{
		p=m_pMem+m_iCapacity-1;
		}
	else
		{
		p--;
		}

	return p;
}


/////////////////////////////////////////////////////////////////////////////
// CPool


// Constructor
template <class T>
CPool<T>::CPool(int iMinSize, int iMaxSize)
{
	m_iMinSize=iMinSize;
	for (int i=0; i<iMinSize; i++)
	{
		Free(Alloc());
	}
}

// Destructor
template <class T>
CPool<T>::~CPool()
{
	while (m_Pool.GetSize())
	{
		delete m_Pool.Pop();
	}
}

// Allocate an item
template <class T>
T* CPool<T>::Alloc()
{
	if (!m_Pool.IsEmpty())
		return m_Pool.Pop();
	else
		return new T();
}

// Free an item
template <class T>
void CPool<T>::Free(T* p)
{
	if (m_iMaxSize>m_iMinSize && m_Pool.GetSize()>=m_iMaxSize)
	{
		delete p;
	}
	else
	{
		m_Pool.Push(p);
	}
}

// Shrink pool to m_iMinSize
template <class T>
void CPool<T>::FreeExtra(int iNewMinSize)
{
	if (iNewMinSize>=0)
		m_iMinSize=iNewMinSize;

	while (m_Pool.GetSize()>m_iMinSize)
	{
		delete m_Pool.Pop();
	}
}


/////////////////////////////////////////////////////////////////////////////
// CDynType

#ifndef _SIMPLELIB_NO_DYNAMIC

// WHAT THE HELL IS THIS???
// It's a cross platform version of __declspec(selectany) to store
// the global pointer to the first type entry
class CDynType;
template <int iDummy=0>
struct CDynTypeFirstHolder
{
	static CDynType* m_pFirst;
};
template <int iDummy> CDynType* CDynTypeFirstHolder<iDummy>::m_pFirst=NULL;


inline CDynType::CDynType(int iID, void* (*pfnCreate)(), const wchar_t* pszName) :
	m_iID(iID),
	m_pfnCreate(pfnCreate),
	m_strName(pszName)
{
#ifdef _DEBUG
	// Check for duplicate type id's
	if (iID!=0)
	{
		ASSERT(GetTypeFromID(iID)==NULL && "Duplicate CDynamicCreateable type id");
	}
#endif

	m_pNext=CDynTypeFirstHolder<>::m_pFirst;
	CDynTypeFirstHolder<>::m_pFirst=this;
}

inline CDynType* CDynType::GetTypeFromID(int iID)
{
	CDynType* p=CDynTypeFirstHolder<>::m_pFirst;
	while (p)
	{
		if (p->m_iID==iID)
			return p;
		p=p->m_pNext;
	}
	return NULL;
}

inline CDynType* CDynType::GetTypeFromName(const wchar_t* pszName)
{
	CDynType* p=CDynTypeFirstHolder<>::m_pFirst;
	while (p)
	{
		if (IsEqualString(p->m_strName, pszName))
			return p;
		p=p->m_pNext;
	}
	return NULL;
}


inline void* CDynType::CreateInstance() const
{
	ASSERT(m_pfnCreate!=NULL);
	return m_pfnCreate();
}

inline int CDynType::GetID() const
{
	return m_iID;
}

inline const wchar_t* CDynType::GetName() const
{
	return m_strName;
}



/////////////////////////////////////////////////////////////////////////////
// CDynamicBase

template <class TSelf, class TBase>
void* CDynamicBase<TSelf,TBase>::QueryCast(CDynType* ptype)
{
	if (ptype==GetType())
	{
		return static_cast<TSelf*>(this);
	}

	void* p=TBase::QueryCast(ptype);
	return p;
}

template <class TSelf, class TBase>
CDynType* CDynamicBase<TSelf,TBase>::QueryType()
{
	return GetType();
}

template <class TSelf, class TBase>
CDynType* CDynamicBase<TSelf,TBase>::GetType()
{
	return &TSelf::dyntype;
}

template <class TSelf, class TBase>
const wchar_t* CDynamicBase<TSelf,TBase>::GetTypeName()
{
	return NULL;
}


template <class TSelf, class TBase>
CDynType CDynamic<TSelf,TBase>::dyntype(0,NULL,TSelf::GetTypeName());


/////////////////////////////////////////////////////////////////////////////
// CDynamicCreatable

template <class TSelf, class TBase, int iID>
int CDynamicCreatable<TSelf,TBase,iID>::GenerateTypeID()
{
	return 0;
}

template <class TSelf, class TBase, int iID>
void* CDynamicCreatable<TSelf,TBase,iID>::CreateInstance()
{
	return new TSelf();
}

template <class TSelf, class TBase, int iID>
CDynType CDynamicCreatable<TSelf,TBase,iID>::dyntype(iID?iID:TSelf::GenerateTypeID(),TSelf::CreateInstance,TSelf::GetTypeName());


#endif		// _SIMPLELIB_NO_DYNAMIC

}	// namespace Simple


