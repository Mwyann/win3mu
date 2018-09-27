//////////////////////////////////////////////////////////////////////////
//
// SimpleLib Version 1.0
// Copyright (C) 1998-2007 Topten Software.  All Rights Reserved
//
// For documentation, updates, disclaimers etc... please see:
//		http://www.toptensoftware.com/devextra
//
// Special thanks to Nick Maher of GrofSoft for original map implementation
//		http://www.grofsoft.com
//
// This code has been released for use "as is".  Any redistribution or
// modification however is strictly prohibited.   See the readme.txt file
// for complete terms and conditions.
//
//////////////////////////////////////////////////////////////////////////

//////////////////////////////////////////////////////////////////////////
// SimpleLib.h - declaration of SimpleVector

#ifndef __SIMPLELIB_H
#define __SIMPLELIB_H

#ifndef ASSERT
#include <assert.h>
#define ASSERT assert
#endif

#include <stdarg.h>
#include <stdlib.h>
#include <string.h>
#include <search.h>
#include <stdio.h>
#include <wchar.h>
#include <ctype.h>
#include <wctype.h>

#if defined(_WIN32)
#include <malloc.h>
#endif

#ifdef _MSC_VER
#define SIMPLEAPI __stdcall
#else
#define SIMPLEAPI
#endif


#ifdef _MSC_VER
typedef unsigned int uint32_t;
typedef unsigned char uint8_t;
typedef __int64 int64_t;
typedef unsigned __int64 uint64_t;
#else
#include <stdint.h>
typedef unsigned long long int uint64_t;
#endif


#ifndef SIMPLELIB_NOMINMAX

#ifndef max
#define max(a,b) (((a) > (b)) ? (a) : (b))
#endif

#ifndef min
#define min(a,b) (((a) < (b)) ? (a) : (b))
#endif

#endif

#ifndef _countof
#define _countof(x) (sizeof(x)/sizeof(x[0]))
#endif


#ifdef __GNUG__
#define MAX_PATH 4096
#define _cdecl
#define __cdecl
#define _stricmp strcasecmp
#define _wcsicmp Simple::lazy_wcsicmp
#define _wcsnicmp Simple::lazy_wcsnicmp
#define _strnicmp Simple::lazy_strnicmp
inline char* strupr(char* psz)
{
	char* p=psz;
	while (p[0])
	{
		*p=toupper(*p);
		p++;
	}
	return psz;
}
inline char* strlwr(char* psz)
{
	char* p=psz;
	while (p[0])
	{
		*p=tolower(*p);
		p++;
	}
	return psz;
}
inline wchar_t* wcsupr(wchar_t* psz)
{
	wchar_t* p=psz;
	while (p[0])
	{
		*p=towupper(*p);
		p++;
	}
	return psz;
}
inline wchar_t* wcslwr(wchar_t* psz)
{
	wchar_t* p=psz;
	while (p[0])
	{
		*p=towlower(*p);
		p++;
	}
	return psz;
}
#endif



#if defined(_MSC_VER) && (_MSC_VER < 1300)
#define _SIMPLELIB_NO_LINKEDLIST_MULTICHAIN
#define _SIMPLELIB_NO_DYNAMIC
#endif


/////////////////////////////////////////////////////////////////////////////
// Misc useful macros

// Return the offset of a member in a class/struct
#ifndef offsetof
#define offsetof(s,m)   (size_t)&reinterpret_cast<const volatile char&>((((s *)0)->m))
#endif

// Return the outer class from the address of a member variable
#ifndef outerclassptr
#define outerclassptr(outerClassName, memberName, ptrToMember) \
	reinterpret_cast<outerClassName*>(reinterpret_cast<char*>(ptrToMember) - offsetof(outerClassName, memberName))
#endif


/////////////////////////////////////////////////////////////////////////////
// Placed construction/destruction

#ifdef _MSC_VER
#ifndef __PLACEMENT_NEW_INLINE
#define __PLACEMENT_NEW_INLINE
inline void *__CRTDECL operator new(size_t, void *_Where)
        {return (_Where); }
#if     _MSC_VER >= 1200
inline void __CRTDECL operator delete(void *, void *)
        {return; }
#endif
#endif

#else
inline void* operator new(size_t, void* pMem)
{
	return pMem;
}
inline void operator delete(void* pMem, void*)
{
}
#endif

template<class T, class T2> inline
void Constructor(T* ptr, const T2& src)
{
	new ((void*)ptr) T(src);
}

template <class T> inline
void Destructor(T *ptr)
{
	ptr->~T();
}


namespace Simple
{

/////////////////////////////////////////////////////////////////////////////
// Character template - used to get char <-> wchar_t opposite type in
//						templatized manner

template <class T>
class SChar
{
};

template <>
class SChar<char>
{
public:
	typedef wchar_t TAlt;

	static void ToUpper(char* psz)
	{
		#if defined(_MSC_VER) && (_MSC_VER>=1400)
		_strupr_s(psz, strlen(psz)+1);
		#else
		strupr(psz);
		#endif
	}
	static void ToLower(char* psz)
	{
		#if defined(_MSC_VER) && (_MSC_VER>=1400)
		_strlwr_s(psz, strlen(psz)+1);
		#else
		strlwr(psz);
		#endif
	}

	static int Length(const char* a)
	{
		return (int)strlen(a);
	}

	static int Compare(const char* a, const char* b, int len)
	{
		return strncmp(a, b, len);
	}

	static int CompareI(const char* a, const char* b, int len)
	{
		return _strnicmp(a, b, len);
	}

	static const char* EmptyString() { return ""; }
};

template <>
class SChar<wchar_t>
{
public:
	typedef char TAlt;

	static void ToUpper(wchar_t* psz)
	{
		#if defined(_MSC_VER) && (_MSC_VER>=1400)
		_wcsupr_s(psz, wcslen(psz)+1);
		#else
		wcsupr(psz);
		#endif
	}
	static void ToLower(wchar_t* psz)
	{
		#if defined(_MSC_VER) && (_MSC_VER>=1400)
		_wcslwr_s(psz, wcslen(psz)+1);
		#else
		wcslwr(psz);
		#endif
	}

	static int Length(const wchar_t* a)
	{
		return (int)wcslen(a);
	}

	static int Compare(const wchar_t* a, const wchar_t* b, int len)
	{
		return wcsncmp(a, b, len);
	}

	static int CompareI(const wchar_t* a, const wchar_t* b, int len)
	{
		return _wcsnicmp(a, b, len);
	}

	static const wchar_t* EmptyString() { return L""; }
};


#ifdef __GNUC__
template <>
class SChar<char16_t>
{
public:
	typedef char TAlt;
};
#endif

/////////////////////////////////////////////////////////////////////////////
// String Class

/*

Simple string class stores a string.  Normally NULL terminated but not
necessarily so.

Implements "copy on write" for effecient copy of CString to CString
	(eg: function return values etc...)

Also, implemented with internal data prefixed to string memory so string class can be
	passed as a string pointer for sprintf type functions. (ie: the whole class is same
	size as a pointer)  (See nested CHeader struct + Get/SetHeader functions)

eg:

	CString<char>	str("Hello World");
	str+=" - from Topten Software!"

Also, Format for easy sprintf type formatting:

	CString<char> strName("Topten Software");
	CString<char> strGreeting=Format("Hello World from %s", strName);

*/

class CAnyString;

template <class T>
class CString
{
	typedef typename SChar<T>::TAlt TAlt;
public:
// Construction
	CString();
	CString(const CString<T>& Other);
	CString(const T* psz, int iLen=-1);
	CString(const CAnyString& Other);
	~CString();

// Types
	typedef CString<T> _CString;

// Operators
	CString<T>& operator=(const CString<T>& Other);
	CString<T>& operator=(const T* psz);
	operator const T* () const;
	const T* sz() const;
	const T& operator[] (int iPos);

// Operations
	void FreeExtra();
	T* GetBuffer(int iBufSize=-1);			// Ensures at least iBufSize, grows to iBufSize
	T* GrowBuffer(int iNewSize);			// Smart grow for appending, doubles buffer size when too small
	void Empty();
	bool IsEmpty() const;
	bool Assign(const CString<T>& Other);
	bool Assign(const T* psz, int iLen=-1);
	bool Assign(const TAlt* psz, int iLen=-1);
	int GetLength() const;
	bool Replace(int iPos, int iOldLen, const T* psz, int iNewLen=-1);
	bool Append(const T* psz, int iLen=-1);
	bool Append(const T ch);
	bool Insert(int iPos, const T* psz, int iLen=-1);
	bool Delete(int iPos, int iLen=-1);
	CString<T>& operator+=(const T* psz);
	CString<T>& operator+=(T ch);
	CString<T> ToUpper();
	CString<T> ToLower();
	int Compare(const T* psz);
	CString<T> Left(int iCount);
	CString<T> Right(int iCount);
	CString<T> SubStr(int iFrom, int iCount=-1);
	CString<T> Mid(int iFrom, int iCount=-1);
	int Find(const T* psz, int startOffset = 0);
	int FindI(const T* psz, int startOffset = 0);
	CString<T> Replace(const T* find, const T* replace, int maxReplacements = -1, int startOffset = 0);
	CString<T> ReplaceI(const T* find, const T* replace, int maxReplacements = -1, int startOffset = 0);
	bool StartsWith(const T* find);
	bool StartsWithI(const T* find);
	bool EndsWith(const T* find);
	bool EndsWithI(const T* find);

#ifdef __wtypes_h__
	/*
	CString(const CComBSTR& bstr)
	{
		Assign(bstr.m_str);
	}
	bool Assign(const CComBSTR& bstr)
	{
		Assign(bstr.m_str);
	}
	*/
	BSTR SysAllocString() const
	{
		if (sizeof(T)==sizeof(OLECHAR))
		{
			return m_psz ? ::SysAllocString(m_psz) : NULL;
		}
		else
		{
			return t2t<wchar_t,T>(m_psz).SysAllocString();
		}
	}
	HRESULT CopyTo(BSTR* pVal) const
	{
		*pVal=SysAllocString();
		return S_OK;
	}
#endif

// Misc static helpers
	static int len(const T* psz);
	static void copy(T* pszDest, const T* pszSrc, int iLen);

protected:
	struct CHeader
	{
		int m_iRef;
		int	m_iMemSize;
		int m_iLength;
		T	m_sz[1];
	};

	CHeader* GetHeader() const { return m_psz ? outerclassptr(CHeader, m_sz, m_psz) : NULL; }
	void SetHeader(CHeader* pHeader) { m_psz=pHeader ? pHeader->m_sz : NULL; }

	T*	m_psz;
};

typedef CString<char>		CAnsiString;
typedef CString<wchar_t>	CUniString;

class CAnyString
{
public:
	CAnyString(const char* psz, int iLen=-1);
	CAnyString(const wchar_t* psz, int iLen=-1);
	CAnyString(const CAnsiString& str);
	CAnyString(const CUniString& str);
	CAnyString(const CAnyString& other);
	operator const char*() const;
	operator const wchar_t*() const;
	template <class T>
	CString<T> As() const;

protected:
	const wchar_t* m_pszW;
	const char* m_pszA;
	mutable CUniString m_strW;
	mutable CAnsiString m_strA;
};

template<>
inline CString<char> CAnyString::As<char>() const
{
	if (m_strW!=NULL && m_strA==NULL)
	{
		m_strA.Assign(m_strW);
	}
	return m_strA;
}

template<>
inline CString<wchar_t> CAnyString::As<wchar_t>() const
{
	if (m_strA!=NULL && m_strW==NULL)
	{
		m_strW.Assign(m_strA);
	}
	return m_strW;
}



// Lazy man's version of wcsicmp for compilers that don't support it
inline int lazy_wcsicmp(const wchar_t* psz1, const wchar_t* psz2)
{
	while (*psz1 || *psz2)
	{
		int icmp=int(towupper(*psz1++))-int(towupper(*psz2++));
		if (icmp!=0)
			return icmp;
	}

	return 0;
}

inline int lazy_wcsnicmp(const wchar_t* psz1, const wchar_t* psz2, size_t len)
{
	while ((*psz1 || *psz2) && len)
	{
		int icmp=int(towupper(*psz1++))-int(towupper(*psz2++));
		if (icmp!=0)
			return icmp;

		len--;
	}

	return 0;
}

inline int lazy_strnicmp(const char* psz1, const char* psz2, size_t len)
{
	while ((*psz1 || *psz2) && len)
	{
		int icmp=int(towupper(*psz1++))-int(towupper(*psz2++));
		if (icmp!=0)
			return icmp;

		len--;
	}

	return 0;
}

// For case insensitive FindKey on CVector<CUniString> - vec.FindKey(L"XYZ", FindKeyI)
inline int FindKeyI(const CUniString& str1, const wchar_t* psz2)
{
	return _wcsicmp(str1, psz2);
};



// Format function.  Specialization for char and wchar_t implemented
template <class T>
CString<T> Format(const T* format, ...);

template <class T>
CString<T> Format(const T* pszFormat, va_list args);

template <class T>
CString<T> Mid(const T* psz, int iStart, int iLength=-1);

template <class T>
CString<T> SubStr(const T* psz, int iStart, int iLength=-1);

template <class T>
CString<T> Left(const T* psz, int iLength);

template <class T>
CString<T> Right(const T* psz, int iLength);

template <class T>
CString<T> Repeat(const T* psz, int iCount);


bool IsEmptyString(const wchar_t* psz);
bool IsEmptyString(const char* psz);

bool IsEqualString(const wchar_t* p1, const wchar_t* p2);
bool IsEqualString(const char* p1, const char* p2);
bool IsEqualStringI(const wchar_t* p1, const wchar_t* p2);
bool IsEqualStringI(const char* p1, const char* p2);




/////////////////////////////////////////////////////////////////////////////
// String conversion

#if defined(_MSC_VER) && (_MSC_VER>=1400)
#pragma warning(disable:4996)
#endif

#ifdef _MSC_VER

inline CAnsiString w_2_utf8(const wchar_t* psz, int iLen=-1)
{
	if (psz==NULL)
		return NULL;
		
	if (iLen<0)
		iLen = (int)wcslen(psz);
		
	CAnsiString str;
	int destLen = (iLen+1)*3;
	WideCharToMultiByte(CP_UTF8, 0, psz, iLen+1, str.GetBuffer(destLen), destLen, NULL, NULL);
	return str;
}

inline CUniString utf8_2_w(const char* psz, int iLen=-1)
{
	if (psz==NULL)
		return NULL;
		
	int destLen = iLen < 0 ? ((int)strlen(psz) + 1) : iLen;
	CUniString str;
	MultiByteToWideChar(CP_UTF8, 0, psz, iLen, str.GetBuffer(destLen), destLen);
	return str;
}

#endif

#ifdef __GNUC__

#include <iconv.h>

inline CAnsiString w_2_utf8(const wchar_t* psz, int iLen=-1)
{
    if (!psz)
        return NULL;
    
    if (iLen<0)
        iLen = (int)wcslen(psz);
    
    size_t sizeIn = iLen * sizeof(wchar_t);
    size_t sizeOut = iLen * 4;
    
    CAnsiString str;
    char* bufOut= str.GetBuffer((int)sizeOut);
    
#if __WCHAR_MAX__ > 0x10000
    iconv_t conv = iconv_open("UTF-8", "UTF-32LE");
#else
    iconv_t conv = iconv_open("UTF-8", "UTF-16LE");
#endif
    iconv(conv, (char**)&psz, &sizeIn, &bufOut, &sizeOut);
    iconv_close(conv);
    
    *bufOut='\0';
    
    return str;
}

inline CUniString utf8_2_w(const char* psz, int iLen=-1)
{
	if (psz==NULL)
		return NULL;
		
	size_t sizeIn = iLen < 0 ? strlen(psz) + 1 : iLen;
	size_t sizeOut = sizeIn * sizeof(wchar_t);
	
	CUniString str;
	wchar_t* bufOut= str.GetBuffer((int)sizeIn);
    
#if __WCHAR_MAX__ > 0x10000
    iconv_t conv = iconv_open("UTF-32LE", "UTF-8");
#else
    iconv_t conv = iconv_open("UTF-16LE", "UTF-8");
#endif
    iconv(conv, (char**)&psz, &sizeIn, (char**)&bufOut, &sizeOut);
    iconv_close(conv);
    
    *bufOut='\0';
    
    return str;
}

#if __WCHAR_MAX__ > 0x10000

inline int c16len(const char16_t* p)
{
    const char16_t* pStart = p;
    while (p[0])
        p++;
    return (int)(p-pStart);
    
}

inline CUniString c16_2_w(const char16_t* psz, int iLen=-1)
{
    if (!psz)
        return NULL;
    
    if (iLen<0)
    {
        
        iLen = c16len(psz);
    }
    
    size_t sizeIn = iLen * sizeof(char16_t);
    size_t sizeOut = iLen * sizeof(wchar_t);
    
    CUniString str;
    wchar_t* bufOut= str.GetBuffer((int)sizeOut);
    
    iconv_t conv = iconv_open("UTF-32LE", "UTF-16LE");
    iconv(conv, (char**)&psz, &sizeIn, (char**)&bufOut, &sizeOut);
    iconv_close(conv);
    
    *bufOut='\0';
    
    return str;
}

inline CString<char16_t> w_2_c16(const wchar_t* psz, int iLen=-1)
{
    if (!psz)
        return NULL;
    
    if (iLen<0)
    {
        iLen = (int)wcslen(psz);
    }
    
    size_t sizeIn = iLen * sizeof(wchar_t);
    size_t sizeOut = iLen * sizeof(char16_t) * 2;
    
    CString<char16_t> str;
    char16_t* bufOut= str.GetBuffer((int)sizeOut);
    
    iconv_t conv = iconv_open("UTF-16LE", "UTF-32LE");
    iconv(conv, (char**)&psz, &sizeIn, (char**)&bufOut, &sizeOut);
    iconv_close(conv);
    
    *bufOut='\0';
    
    return str;
}

#else

#define c16_2_w(x) (x)
#define w_2_c16(x) (x)

#endif

#endif


#ifdef _MSC_VER
#define c16_2_w(x) ((const wchar_t*)(x))
#define w_2_c16(x) ((const char16_t*)(x))
#endif

inline CUniString a2w(const char* psz, int iLen=-1)
{
	if (!psz)
		return 0;

	CUniString str;

	if (iLen<0)
	{
		iLen=int(strlen(psz));
		mbstowcs(str.GetBuffer(int(iLen*2+1)), psz, iLen+1);
	}
	else
	{
		char* pszTemp=(char*)alloca(iLen+1);
		memcpy(pszTemp, psz, iLen);
		pszTemp[iLen]='\0';
		mbstowcs(str.GetBuffer(int(iLen*2+1)), pszTemp, iLen+1);
	}

	return str;
}

inline CAnsiString w2a(const wchar_t* psz, int iLen=-1)
{
	if (!psz)
		return 0;

	CAnsiString str;

	if (iLen<0)
	{
		iLen=int(wcslen(psz));
		wcstombs(str.GetBuffer(int(iLen*2+1)), psz, iLen+1);
	}
	else
	{
		wchar_t* pszTemp=(wchar_t*)alloca((iLen+1)*2);
		memcpy(pszTemp, psz, iLen*2);
		pszTemp[iLen]=L'\0';
		wcstombs(str.GetBuffer(int(iLen*2+1)), pszTemp, iLen+1);
	}

	return str;
}


// Templatatized type conversion

template <class TDest, class TSrc>
CString<TDest> t2t(const TSrc* psz, int iLen=-1);


template <> inline CString<char> t2t<char, char>(const char* psz, int iLen)
	{ return CString<char>(psz,iLen); }
template <> inline CString<char> t2t<char, wchar_t>(const wchar_t* psz, int iLen)
	{ return w2a(psz,iLen); }
template <> inline CString<wchar_t> t2t<wchar_t, wchar_t>(const wchar_t* psz, int iLen)
	{ return CString<wchar_t>(psz,iLen); }
template <> inline CString<wchar_t> t2t<wchar_t, char>(const char* psz, int iLen)
	{ return a2w(psz,iLen); }

template <class TSrc>
CUniString t2w(const TSrc* psz) { return t2t<wchar_t, TSrc>(psz); }

template <class TSrc>
CAnsiString t2a(const TSrc* psz) { return t2t<char, TSrc>(psz); }


#if defined(_MSC_VER) && (_MSC_VER>=1400)
#pragma warning(default:4996)
#endif


}	// Close namespace while defining global scope Compare functions


template <class T>
int _cdecl Compare(const T& a, const T& b)
{
	return a > b ? 1 : a < b ? -1 : 0;
}

inline int Compare(const char* psz1, const char* psz2)			{ return strcmp(psz1, psz2); }
inline int Compare(const wchar_t* psz1, const wchar_t* psz2)	{ return wcscmp(psz1, psz2); }
inline int __cdecl CompareI(const char* psz1, const char* psz2)			{ return _stricmp(psz1, psz2); }
inline int __cdecl CompareI(const wchar_t* psz1, const wchar_t* psz2)	{ return _wcsicmp(psz1, psz2); }

template <class T>
int Compare(Simple::CString<T> const& str1, Simple::CString<T> const& str2)
{
	return Compare(static_cast<const T*>(str1), static_cast<const T*>(str2));
}

template <class T>
int __cdecl CompareI(Simple::CString<T> const& str1, Simple::CString<T> const& str2)
{
	return CompareI(static_cast<const T*>(str1), static_cast<const T*>(str2));
}


namespace Simple
{

/////////////////////////////////////////////////////////////////////////////
// Semantics, used by container classes to control semantic behaviour of collection

/*

Semantic classes are used to control what happens when an object is added to or
removed from a collection class and how to compare items for sorting etc...

Eg:

a vector of pointers

	CVector<CMyObject>	vec;

a vector of pointers that will be deleted when removed from collection or
collection is destroyed

	CVector<CMyObject, SOwnedPtr>		vec;


*/



// Simple value semantics
class SValue
{
public:
	template <class T, class TOwner>
	static const T& OnAdd(const T& val, TOwner* pOwner)
		{ return val; }

	template <class T, class TOwner>
	static void OnRemove(T& val, TOwner* pOwner)
		{ }

	template <class T, class TOwner>
	static void OnDetach(T& val, TOwner* pOwner)
		{ }

	template <class T1, class T2>
	static int __cdecl Compare(const T1& a, const T2& b)
		{ return ::Compare(a,b); }

};

// Owned ptr semantics
class SOwnedPtr
{
public:
	template <class T, class TOwner>
	static const T& OnAdd(const T& val, TOwner* pOwner)
		{ return val; }

	template <class T, class TOwner>
	static void OnRemove(T& val, TOwner* pOwner)
		{ delete val; }

	template <class T, class TOwner>
	static void OnDetach(T& val, TOwner* pOwner)
		{ }

	template <class T>
	static int __cdecl Compare(const T& a, const T& b)
		{ return ::Compare(a,b); }

};

// RefCounted ptr semantics (COM interface pointers objects)
class SRefCounted
{
public:
	template <class T, class TOwner>
	static const T& OnAdd(const T& val, TOwner* pOwner)
		{ val->AddRef(); return val; }

	template <class T, class TOwner>
	static void OnRemove(T& val, TOwner* pOwner)
		{ val->Release(); }

	template <class T, class TOwner>
	static void OnDetach(T& val, TOwner* pOwner)
		{ }

	template <class T>
	static int Compare(const T& a, const T& b)
		{ return ::Compare(a,b); }

};

// Explicit case sensitive string semantic (same as SValue but more meaningful)
class SCaseSensitive : public SValue
{
public:
	// Nothing to see here!
};

// Case insensitive strings
class SCaseInsensitive : public SValue
{
public:
	template <class T>
	static int Compare(const T& a, const T& b)
		{ return ::CompareI(a,b); }
};




/////////////////////////////////////////////////////////////////////////////
// Simple Vector Class

/*

Vector class implements a simple vector with semantics support.

Stack operations are simulated with Push, Pop (2 flavours) and Top
Queue operations are simulated with Enqueue, Dequeue (2 flavours) and Peek

eg:

	// simple as it gets - a vector of integer values
	CVector<int>			vecInts;

	// a vector of class objects
	CVector<CMyObject>	vecObjects;

	// a vector of pointers to class objects
	CVector<CMyObject*>	vecPtrObjects;

	// a vector of pointers to class objects where the objects will be deleted
	// on removal from the collection.   Use DetachAt to remove without delete.
	CVector<CMyObject*, SOwnedPtr> vecPtrObjects;



*/

// CVector
template <class T, class TSem=SValue, class TArg=T>
class CVector
{
public:
 // Construction
	CVector();
	virtual ~CVector();

// Types
	typedef TSem SSemantics;
	typedef T CValue;
	typedef TArg CArg;
	typedef CVector<T,TSem,TArg>	_CVector;

// Operations
	void GrowTo(int iRequiredSize);
	void SetSize(int iRequiredSize, const TArg& val);
	void FreeExtra();
	void InsertAt(int iPosition, const T& val);
	void ReplaceAt(int iPosition, const T& val);
	void Swap(int iPosA, int iPosB);
	void Move(int iFrom, int iTo);
	int Add(const T& val);
	int Remove(const TArg& val);
	void RemoveAt(int iPosition);
	void RemoveAt(int iPosition, int iCount);
	T DetachAt(int iPosition);
	void Detach(const TArg& val);
	void DetachAll();
	void RemoveAll();
	T& GetAt(int iPosition) const;
	T& operator[](int iPosition) const;
	T* GetBuffer() const;
	int GetSize() const;
	bool IsEmpty() const;

	template <class TSem2, class TArg2>
	void Add(CVector<T, TSem2, TArg2>& vec);

	template <class TSem2, class TArg2>
	void InsertAt(int iPosition, CVector<T, TSem2, TArg2>& vec);

	void Swap(CVector<T, TSem, TArg>& other);



// Search and sort
	int Find(const TArg& val, int iStartAfter=-1) const;
	void QuickSort();
	void QuickSort(int (_cdecl *pfnCompare)(T const& a, T const& b));
#if defined(_MSC_VER) && (_MSC_VER>=1400)
	void QuickSort(int (__cdecl *pfnCompare)(void* ctx, T const& a, T const& b), void* ctx);
#endif
	bool QuickSearch(const TArg& key, int& iPosition) const;
	bool QuickSearch(const TArg& key, int (__cdecl *pfnCompare)(T const& a, TArg const& b), int& iPosition) const;
	bool QuickSearch(const TArg& key, void* ctx, int (__cdecl *pfnCompare)(void* ctx, T const& a, TArg const& b), int& iPosition) const;

// FindKey and QuickSearchKey allow search by a key that is a different type than the vector elements
//			eg: useful for searching on member variable of a the vector element type.
	template <class TKey>
	int FindKey(TKey key, int (__cdecl *pfnCompare)(T const& a, TKey b), int iStartAfter=-1) const;
	template <class TKey>
	bool QuickSearchKey(TKey key, int (__cdecl *pfnCompare)(T const& a, TKey b), int& iPosition) const;
	template <class TKey>
	int FindKey(TKey key, void* ctx, int (__cdecl *pfnCompare)(void* ctx, T const& a, TKey b), int iStartAfter=-1) const;
	template <class TKey>
	bool QuickSearchKey(TKey key, void* ctx, int (__cdecl *pfnCompare)(void* ctx, T const& a, TKey b), int& iPosition) const;



// Stack operators
	void Push(const TArg& val);
	bool Pop(T& val);
	T Pop();
	bool Top(T& val) const;
	T& Top() const;

// Queue operators
	void Enqueue(const TArg& val);
	bool Dequeue(T& val);
	T Dequeue();
	T& Peek() const;
	bool Peek(T& val) const;

protected:
	int		m_iSize;
	int		m_iMemSize;
	T*		m_pData;

	void InsertAtInternal(int iPosition, const T* pVal, int iCount);

private:
// Unsupported
	CVector(const CVector& Other);
	CVector& operator=(const CVector& Other);
};



/////////////////////////////////////////////////////////////////////////////
// CUniStringVector

class CUniStringVector : public CVector<CUniString>
{
public:
// Constructor
	CUniStringVector() {};
	virtual ~CUniStringVector() {};

	static int __cdecl FindFunc(const CUniString& a, const wchar_t* psz)
	{
		return wcscmp(a, psz);
	}

	static int __cdecl FindFuncI(const CUniString& a, const wchar_t* psz)
	{
		return _wcsicmp(a, psz);
	}

	int Find(const wchar_t* psz, int iStartAfter=-1) const
	{
		return FindKey(psz, FindFunc, iStartAfter);
	}

	int FindInsensitive(const wchar_t* psz, int iStartAfter=-1)
	{
		return FindKey(psz, FindFuncI, iStartAfter);
	}

	void Sort()
	{
		QuickSort(Compare);
	}

	void SortInsensitive()
	{
		QuickSort(CompareI);
	}
};


/////////////////////////////////////////////////////////////////////////////
// CSortedVector

template <class T, class TSem=SValue, class TArg=T>
class CSortedVector
{
public:
// Construction
			CSortedVector();
	virtual ~CSortedVector();

// Types
	typedef TSem SSemantics;
	typedef T CValue;
	typedef CSortedVector<T,TSem,TArg>	_CSortedVector;

// Operations
	int Add(const TArg& val);
	int GetSize() const;
	int Remove(const TArg& val);
	void RemoveAt(int iPosition);
	T DetachAt(int iPosition);
	void RemoveAll();
	const T& GetAt(int iPosition) const;
	const T& operator[](int iPosition) const;
	const T* GetBuffer() const;
	bool IsEmpty() const;
	bool QuickSearch(const TArg& key, int& iPosition) const;
	int Find(const TArg& key, int iStartAfter) const;
	int Find(const TArg& key) const;
	void Resort(int (_cdecl *pfnCompare)(const T& a, const T& b), bool bAllowDuplicates=true);
#if defined(_MSC_VER) && (_MSC_VER>=1400)
	void Resort(void* ctx, int (__cdecl *pfnCompare)(void* ctx, const T& a, const T& b), bool bAllowDuplicates=true);
#endif
	template <class TKey>
	bool FindKey(TKey key, int (__cdecl *pfnCompare)(T const& a, TKey b), int& iPos) const
	{
		return m_vec.QuickSearchKey(key, pfnCompare, iPos);
	}
	template <class TKey>
	bool FindKey(TKey key, void* ctx, int (__cdecl *pfnCompare)(void* ctx, T const& a, TKey b), int& iPos) const
	{
		return m_vec.QuickSearchKey(key, ctx, pfnCompare, iPos);
	}

private:
	CVector<T,TSem,TArg>		m_vec;
	bool						m_bAllowDuplicates;
	int (__cdecl *m_pfnCompare)(const T& a, const T& b);
#if defined(_MSC_VER) && (_MSC_VER>=1400)
	int (__cdecl *m_pfnCompareEx)(void* ctx, const T& a, const T& b);
	void*						m_ctx;
#endif
	CSortedVector(const CSortedVector& Other);
	CSortedVector& operator=(const CSortedVector& Other);
};


/////////////////////////////////////////////////////////////////////////////
// CGrid

template <class T, class TSem=SValue, class TArg=T>
class CGrid
{
public:
	CGrid(int iWidth=0, int iHeight=0);

	void SetSize(int iWidth, int iHeight, const TArg& val=T());
	void InsertColumn(int iPosition, const TArg& val=T());
	void RemoveColumn(int iPosition);
	void InsertRow(int iPosition, const TArg& val=T());
	void RemoveRow(int iPosition);
	void RemoveAll();
	int GetWidth();
	int GetHeight();

	class CColumn : public CVector<T, TSem>
	{
	public:
		CColumn(int iHeight, const TArg& val)
		{
			SetSize(iHeight, val);
		}
	};

	CColumn& operator[](int x);

protected:
	CVector<CColumn*, SOwnedPtr>	m_Columns;
	int m_iHeight;
};


/////////////////////////////////////////////////////////////////////////////
// CLinkedList

/*

Simple intrusive linked list.

To Use:  Add an element CChain<T> m_Chain to items to be added to list:

eg:

// Item like this:
class CItem
{
	CChain<CItem>	m_Chain;
}

// List like this:
CLinkedList<CItem>	List;
CLinkedList<CItem, SOwnedPtr>	List;		// For auto delete


// To add support for placing items in multiple lists:
class CItem
{
	CChain<CItem>	m_Chain1;
	CChain<CItem>	m_Chain2;
}

// Lists declared like this...
CLinkedList<CItem, SValue, &CItem::m_Chain1>		List1;
CLinkedList<CItem, SVAlue, &CItem::m_Chain2>		List2;

// Iterating a list forward
for (List.MoveFirst(); !List.IsEOF(); List.MoveNext())
{
	List.Current()->blah();
}

// Iterating a list backward
for (List.MoveLast(); !List.IsBOF(); List.MovePrevious())
{
	List.Current()->blah();
}

*/



template <class T>
struct CChain
{
	CChain()
	{
		m_pNext=NULL;
		m_pPrev=NULL;
	}
	T*	m_pNext;
	T*	m_pPrev;
};


#ifndef _SIMPLELIB_NO_LINKEDLIST_MULTICHAIN
template <class T, class TSem=SValue, CChain<T> T::* pMember=&T::m_Chain>
#else
template <class T, class TSem=SValue>
#endif
class CLinkedList
{
public:
// Consruction
			CLinkedList();
	virtual ~CLinkedList();

// Types
#ifndef _SIMPLELIB_NO_LINKEDLIST_MULTICHAIN
	typedef CLinkedList<T, TSem, pMember> _CLinkedList;
#else
	typedef CLinkedList<T, TSem> _CLinkedList;
#endif

// Operations
	void Prepend(T* p);
	void Add(T* p);
	void Insert(T* p, T* pBefore=NULL);
	void Remove(T* p);
	T* Detach(T* p);
	void RemoveAll();
	bool Contains(T* p) const;
	bool IsEmpty() const;
	int GetSize() const;
	T* GetFirst() const;
	T* GetLast() const;
	T* GetNext(T* p) const;
	T* GetPrevious(T* p) const;

	void Add(_CLinkedList& OtherList);

// Iteration
	bool IsEOF() const;
	bool IsBOF() const;
	void MoveFirst();
	void MoveLast();
	void MoveNext();
	void MovePrevious();
	T* Current() const;

// Pseudo random access
	T* GetAt(int iPos) const;
	T* operator[](int iPos) const;

// Stack operators
	void Push(T* val);
	bool Pop(T*& val);
	T* Pop();
	bool Top(T*& val) const;
	T* Top() const;

// Queue operators
	void Enqueue(T* val);
	bool Dequeue(T*& val);
	T* Dequeue();
	T* Peek() const;
	bool Peek(T*& val) const;

// Implementation
protected:
	T*				m_pFirst;
	T*				m_pCurrent;
	bool			m_bLastIterForward;
	int				m_iSize;
	mutable int		m_iIterPos;
	mutable T*		m_pIterElem;


	bool IsBeforeIteratePos(T* p) const;
	void RemoveOrDetach(T* p, bool bDetach);

private:
// Unsupported
	CLinkedList(const CLinkedList& Other);
	CLinkedList& operator=(const CLinkedList& Other);
};


/////////////////////////////////////////////////////////////////////////////
// CPlex - segmented memory allocator used for allocating nodes in
//				CMap and CHashMap

template <class T>
class CPlex
{
public:
// Construction
	CPlex(int iBlockSize=-1);
	~CPlex();

// Operations
	T* Alloc();
	void Free(T* p);
	void FreeAll();
	int GetCount() const;
	void SetBlockSize(int iNewBlockSize);


protected:


#ifdef _MSC_VER
	#pragma warning(disable: 4200)
#endif
	struct BLOCK
	{
		BLOCK*	m_pNext;
		char	m_bData[0];
	};
#ifdef _MSC_VER
	#pragma warning(default: 4200)
#endif

	struct FREEITEM
	{
		FREEITEM*	m_pNext;
	};

	BLOCK*		m_pHead;
	FREEITEM*	m_pFreeList;
	int			m_iCount;
	int			m_iBlockSize;
};


/////////////////////////////////////////////////////////////////////////////
// CMap

/*

Map class implements a map with semantics support.  Implemented as a red-black tree
with linked-list between values for fast iteration.

Supports:
	* Pseudo random access - operator[](int iIndex)
	* Insert/Delete during iteration
	* Semanatics

eg:

	// map of ints to int
	CMap<int, int>			mapInts;

	// map of ints to object
	CMap<int, CMyObject>	mapObjects;

	// map of ints to object ptrs
	CMap<CMyObject*>	mapPtrObjects;

	// a map of ints to pointers where pointers deleted on removal
	CMap<int, CMyObject*, SValue, SOwnedPtr> mapPtrObjects;

	// a map of strings to integers, with case insensitivity on the keys
	CMap< CString<char>, int, SCaseInsensitive>	map;
	map.Add("Apples", 10);
	map.Add("Pears", 20);
	map.Add("Bananas", 30);

	// Iterating the above map
	for (int i=0; i<map.GetSize(); i++)
	{
		printf("%s - %i\n", map[i].Key, map[i].Value);
	}
*/

template <class TKey, class TValue, class TKeySem=SValue, class TValueSem=SValue, class TKeyArg=TKey >
class CMap
{

public:
// Constructor
			CMap();
	virtual ~CMap();

// Types
	typedef CMap<TKey, TValue, TKeySem, TValueSem, TKeyArg> _CMap;


// Type used as return value from operator[]
	class CKeyPair
	{
	public:
		CKeyPair(const TKey& Key, TValue& Value) :
			Key(Key),
			Value(Value)
		{
		}
		CKeyPair(const CKeyPair& Other) :
			Key(Other.Key),
			Value(Other.Value)
		{
		}

		const TKey&	Key;
		TValue&	Value;

#ifdef _MSC_VER
	private:
		// "Occassionally" MSVC compiler gives warning about inability
		// to generate assignment operator that it never even uses.
		// This seems to fix warning and will give link error if actually needed
		CKeyPair& operator=(const CKeyPair& Other);
#endif
	};

// Operations
	int GetSize() const;
	bool IsEmpty() const;
	CKeyPair operator[](int iIndex) const;
	void Add(const TKey& Key, const TValue& Value);
	void Remove(const TKeyArg& Key);
	void RemoveAll();
	TValue Detach(const TKeyArg& Key);
	const TValue& Get(const TKeyArg& Key, const TValue& Default=TValue()) const;
	bool Find(const TKeyArg& Key, TValue& Value) const;
	bool HasKey(const TKeyArg& Key) const;

	#ifdef _DEBUG
	void CheckAll();
	#endif

// Implementation
protected:
// CNode
	struct CKeyPairInternal
	{
		TKey	m_Key;
		TValue	m_Value;
	};
	struct CNode
	{
		CKeyPairInternal	m_KeyPair;
		CNode*	m_pParent;
		CNode*	m_pLeft;
		CNode*	m_pRight;
		CNode*	m_pPrev;
		CNode*	m_pNext;
		bool	m_bRed;
	};

// Operations
#ifdef _DEBUG
	void CheckChain();
	bool CheckTree(CNode* pNode=NULL);
#endif
	void FreeNode(CNode* pNode);
	CNode* nextNode(CNode* pNode);
	void RotateLeft(CNode* x);
	void RotateRight(CNode* y);
	void RemoveOrDetach(const TKeyArg& Key, TValue* pvalDetached);
	CNode* FindNode(const TKeyArg& Key) const;

// Attributes
	CPlex<CNode>	m_NodePlex;
	CNode*			m_pRoot;
	CNode*			m_pFirst;
	CNode*			m_pLast;
	CNode			m_Leaf;
	mutable int		m_iIterPos;
	mutable CNode*	m_pIterNode;
	int				m_iSize;

private:
// Unsupported
	CMap(const CMap& Other);
	CMap& operator=(const CMap& Other);
};




/////////////////////////////////////////////////////////////////////////////
// CIndex

template <class TKey, class TValue, class TKeySem=SValue, class TValueSem=SValue, class TKeyArg=TKey >
class CIndex
{

public:
// Constructor
			CIndex();
	virtual ~CIndex();

// Types
	typedef CIndex<TKey, TValue, TKeySem, TValueSem, TKeyArg> _CIndex;

	class CEntry
	{
	public:
		CEntry(const TKey& key, const TValue& value) :
			m_Key(key),
			m_Value(value)
		{
		}
		~CEntry()
		{
		}
		TKey		m_Key;
		TValue		m_Value;

		static int Compare(const CEntry& a, const CEntry& b)
		{
			return TKeySem::Compare(a.m_Key, b.m_Key);
		}
		static int CompareKey(CEntry const& a, TKeyArg b)
		{
			return TKeySem::Compare(a.m_Key, b);
		}
	};

// Type used as return value from operator[]
	class CKeyPair
	{
	public:
		CKeyPair(CEntry& e) :
			Key(e.m_Key),
			Value(e.m_Value)
		{
		}
		CKeyPair(const CKeyPair& Other) :
			Key(Other.Key),
			Value(Other.Value)
		{
		}

		const TKey&	Key;
		TValue&	Value;

#ifdef _MSC_VER
	private:
		// "Occassionally" MSVC compiler gives warning about inability
		// to generate assignment operator that it never even uses.
		// This seems to fix warning and will give link error if actually needed
		CKeyPair& operator=(const CKeyPair& Other);
#endif
	};

	// Operations
	int GetSize() const;
	bool IsEmpty() const;
	CKeyPair operator[](int iIndex) const;
	void Add(const TKey& Key, const TValue& Value);
	void Remove(const TKeyArg& Key);
	void RemoveAll();
	TValue Detach(const TKeyArg& Key);
	const TValue& Get(const TKeyArg& Key, const TValue& Default=TValue()) const;
	bool Find(const TKeyArg& Key, TValue& Value) const;
	bool HasKey(const TKeyArg& Key) const;

protected:
	CVector<CEntry, SValue>			m_Entries;

private:
// Unsupported
	CIndex(const CIndex& Other);
	CIndex& operator=(const CIndex& Other);
};



/////////////////////////////////////////////////////////////////////////////
// Hashing functions and semantics

unsigned long SuperFastHash (const char * data, int len);

template <class T>
class SHash
{
public:
	static unsigned long Hash(const T& Key)
	{
		return SuperFastHash((const char*)&Key, sizeof(Key));
	}
};

template <>
class SHash<const char*>
{
public:
	static unsigned long Hash(const char* Key)
	{
		return SuperFastHash(Key, CAnsiString::len(Key));
	}
};

template <>
class SHash<const wchar_t*>
{
public:
	static unsigned long Hash(const wchar_t* Key)
	{
		return SuperFastHash((const char*)Key, CUniString::len(Key)*sizeof(wchar_t));
	}
};

template <>
class SHash<CString<wchar_t> >
{
public:
	static unsigned long Hash(const CString<wchar_t>& Key)
	{
		return SuperFastHash((const char*)static_cast<const wchar_t*>(Key), Key.GetLength()*sizeof(wchar_t));
	}
};

template <>
class SHash<CString<char> >
{
public:
	static unsigned long Hash(const CString<char>& Key)
	{
		return SuperFastHash(static_cast<const char*>(Key), Key.GetLength()*sizeof(char));
	}
};



/////////////////////////////////////////////////////////////////////////////
// CHashMap

template <class TKey, class TValue, class TKeySem=SValue, class TValueSem=SValue, class TKeyArg=TKey, class THash=SHash<TKeyArg> >
class CHashMap
{

public:
	// Constructor
			CHashMap(int iInitialSize=64);
	virtual ~CHashMap();

	// Types
	typedef CHashMap<TKey, TValue, TKeySem, TValueSem, TKeyArg, THash> _CHashMap;


	// Type used as return value from operator[]
	class CKeyPair
	{
	public:
		CKeyPair(const TKey& Key, TValue& Value) :
			Key(Key),
			Value(Value)
		{
		}
		CKeyPair(const CKeyPair& Other) :
			Key(Other.Key),
			Value(Other.Value)
		{
		}

		const TKey&	Key;
		TValue&		Value;

#ifdef _MSC_VER
	private:
		// "Occassionally" MSVC compiler gives warning about inability
		// to generate assignment operator that it never even uses.
		// This seems to fix warning and will give link error if actually needed
		CKeyPair& operator=(const CKeyPair& Other);
#endif
	};

	// Operations
	int GetSize() const;
	bool IsEmpty() const;
	CKeyPair operator[](int iIndex) const;
	void Add(const TKey& Key, const TValue& Value);
	void Remove(const TKeyArg& Key);
	void RemoveAll();
	TValue Detach(const TKeyArg& Key);
	const TValue& Get(const TKeyArg& Key, const TValue& Default=TValue()) const;
	bool Find(const TKeyArg& Key, TValue& Value) const;
	bool HasKey(const TKeyArg& Key) const;

	// Implementation
protected:
	struct CKeyPairInternal
	{
		TKey			m_Key;
		TValue			m_Value;
	};
	struct CNode
	{
		CKeyPairInternal	m_KeyPair;
		CNode*			m_pHashNext;
		CChain<CNode>	m_Chain;
	};

	CPlex<CNode>		m_NodePlex;		// Node allocator
	CVector<CNode*>		m_Table;		// Hash table
	CLinkedList<CNode>	m_List;			// Navigation list
	unsigned int		m_nHashMask;	// Mask of hash value -> hash table
	int					m_iThreshold;	// Size at which to rehash
	int					m_iInitialSize;	// Initial size when table first created

	// Operations
	void InitHashTable(int iSize);
	void Rehash(int iNewSize);
	void RemoveOrDetach(const TKeyArg& Key, TValue* pvalDetached);
	CNode* FindNode(const TKeyArg& Key) const;

	// Attributes
private:
	// Unsupported
	CHashMap(const CHashMap& Other);
	CHashMap& operator=(const CHashMap& Other);
};



/////////////////////////////////////////////////////////////////////////////
// CRingBuffer

/*

Implements a simple ring buffer that is safe for single reader-single writer
multithreaded access.

*/


// CRingBuffer Class
template <class T, class TSem=SValue>
class CRingBuffer
{
public:
// Construction
			CRingBuffer(int iCapacity);
	virtual ~CRingBuffer();

// Types
	typedef CRingBuffer<T,TSem> _CRingBuffer;

// Operations
	void Reset(int iNewCapacity=0);
	bool IsEmpty() const;
	bool IsFull() const;
	bool IsOverflow() const;
	bool Enqueue(const T& t);
	bool Dequeue(T& t);
	T Dequeue();
	bool Peek(T& t);
	T Peek();
	bool Unenqueue(T& t);
	T Unenqueue();
	bool PeekLast(T& t);
	T PeekLast();
	void RemoveAll();
	int GetCapacity() const;
	int GetSize() const;
	T GetAt(int iPos) const;
	T operator [] (int iPos) const;


// Implementation
protected:
// Attributes
	T*	m_pMem;
	int	m_iCapacity;
	T*		m_pWritePos;
	T*		m_pReadPos;
	int		m_iSize;
	bool	m_bOverflow;

// Operations
	T* AdvancePtr(T* p)	const;
	T* RewindPtr(T* p)	const;

private:
// Unsupported
	CRingBuffer(const CRingBuffer& Other);
	CRingBuffer& operator=(const CRingBuffer& Other);
};



/////////////////////////////////////////////////////////////////////////////
// CPool

template <class T>
class CPool
{
public:
// Construction
			CPool(int iMinSize=0, int iMaxSize=-1);
	virtual ~CPool();

// Operations
	T* Alloc();
	void Free(T* p);
	void FreeExtra(int iNewMinSize=-1);

protected:
	CVector<T*>	m_Pool;
	int			m_iMinSize;
	int			m_iMaxSize;
};



/////////////////////////////////////////////////////////////////////////////
// CSingleton

/*

CSingleton provides standard framework for a singleton object.  Derive class
from CSingleton and use ClassName::GetInstance to get instance ptr.

*/

template <class T>
class CSingleton
{
public:
	CSingleton()
	{
		m_pInstance=static_cast<T*>(this);
	}
	virtual ~CSingleton()
	{
		ASSERT(m_pInstance==static_cast<T*>(this));
		m_pInstance=NULL;
	}

	static T* GetInstance() { return m_pInstance; };

protected:
	static T* m_pInstance;
};

template <class T> T* CSingleton<T>::m_pInstance=NULL;



/////////////////////////////////////////////////////////////////////////////
// CAutoRestore - automatically restore tha value of a variable on leaving scope

template <class T>
class CAutoRestore
{
public:
	CAutoRestore(T& Var, T NewValue) :
		m_Var(Var)
	{
		m_OldValue=Var;
		m_Var=NewValue;
	};

	~CAutoRestore()
	{
		m_Var=m_OldValue;
	}

	T&	m_Var;
	T	m_OldValue;
};


template <class T, class TSem=SOwnedPtr>
class CAutoPtr
{
public:
	CAutoPtr(T* ptr=NULL)
	{
		p=ptr;
		if (p)
			TSem::OnAdd(p,this);
	};
	~CAutoPtr()
	{
		Release();
	};
	T* Detach()
	{
		if (p)
			TSem::OnDetach(p, this);
		T* ptr=p;
		p=NULL;
		return ptr;
	};
	T** operator&()
	{
		ASSERT(p==NULL);
		return &p;
	}
	T* operator->() const
	{
		ASSERT(p!=NULL);
		return p;
	}
	bool operator!() const
	{
		return (p == NULL);
	}
	T* operator=(T* pNew)
	{
		if (p==pNew)
			return p;
		Release();
		p=pNew;
		if (p)
		{
			TSem::OnAdd(p,this);
		}
		return p;
	}
	void Release()
	{
		if (p)
		{
			TSem::OnRemove(p, this);
			p=NULL;
		}
	}

	operator T*()
	{
		return p;
	}


	T& operator*()
	{
		return *p;
	}

	T* p;
};


/////////////////////////////////////////////////////////////////////////////
// CDynamicBase and CDynamicCreateable

/*

CDynamicBase and CDynamicCreatable  provide a lightweight mechanism for runtime type info.

CDynamicBase - defines a dynamic object - one that can be queried for its type
CDynamicCreatable - defines a dynamically creatable object - one that can be instantiated
					from its type info.
CDynType - class representing the type info for a class.

Example:

// CFruit is base for type below it has no base class, so no second parameter to CDynamicBase
class CFruit : public CDynamicBase<CFruit>
{
	virtual void x()=0;
};

// CApple derives from CFruit
class CApple : public CDynamicBase<CApple, CFruit>
{
};

// CRedApple derives from CApple and is dynamically createable from id 1
class CRedApple : public CDynamicCreatable<CRedApple, CApple, 1>
{
	virtual void x() {};
};

// CRedApple derives from CApple and is dynamically createable from id 2
class CGreenApple : public CDynamicCreatable<CGreenApple, CApple, 2>
{
	virtual void x() {};
};

// CBanana derives from CFruit and is dynamically createable from id 3
class CBanana : public CDynamicCreatable<CBanana, CFruit, 3>
{
	virtual void x() {};
};

// To query if an object is of a particular type
CFruit* pFruit;
pFruit->QueryAs<CBanana>();		// returns NULL if pFruit is not of type CBanana
pFruit->As<CBanana>();			// Asserts if pFruit is not of type CBanana

// To create an instance from a type
CDynType* pType;							// eg: pType=CBanana::GetType()
pFruit=(CFruit*)pType->CreateInstance();	// This will create an instance of whatever pType is the type info for

// To create a type from type id parameter
CDynType* pType=CDynType::GetType(3);		// Get type info for CBanana from its ID
pFruit=(CFruit*)pType->CreateInstance();	// This should create an instance of CBanana

*/

#ifndef _SIMPLELIB_NO_DYNAMIC

// Store information about a CDynamicBase type
class CDynType
{
public:
// Construction
	CDynType(int iID, void* (*pfnCreate)(), const wchar_t* pszName);

// Given a type ID, return the CDynType for it
	static CDynType* GetTypeFromID(int iID);
	static CDynType* GetTypeFromName(const wchar_t* pszName);

// Operations
	void* CreateInstance() const;
	int GetID() const;
	const wchar_t* GetName() const;

// Implementation
protected:
// Attributes
	int	m_iID;
	void* (*m_pfnCreate)();
	CUniString	m_strName;
	CDynType* m_pNext;

// Operations
};

// Base class for CDynamicBase classes
class none
{
public:
	virtual void* QueryCast(CDynType* ptype) { return NULL; }
	virtual CDynType* QueryType() { return NULL; };
	static CDynType* GetType() { return NULL; };
};


// CDynamicBase
template <class TOwner, class TBase=none>
class CDynamicBase : public TBase
{
public:
	template <class T>
	T* As()
	{
		if (!this) return NULL;
		T* p=QueryAs<T>();
		ASSERT(p!=NULL);
		return p;
	}
	template <class T>
	T* QueryAs()
	{
		if (!this) return NULL;
		return (T*)QueryCast(&T::dyntype);
	}
	static CDynType* GetType();
	static CDynType* GetBaseType() { return TBase::GetType(); };
	static const wchar_t* GetTypeName();


	virtual void* QueryCast(CDynType* ptype);
	virtual CDynType* QueryType();

};

template <class TOwner, class TBase=none>
class CDynamic : public CDynamicBase<TOwner, TBase>
{
public:
	static CDynType dyntype;
};

// CDynamicBase
template <class TOwner, class TBase=none, int iID=0>
class CDynamicCreatable : public CDynamicBase<TOwner, TBase>
{
public:
	static void* CreateInstance();
	static int GenerateTypeID();
	static CDynType dyntype;
};

#endif // _SIMPLELIB_NO_DYNAMIC


};		// namespace Simple




template <class T>
void Swap(T& a, T& b)
{
	T t(a);
	a=b;
	b=t;

}

template <class T>
void Normalize(T& a, T& b)
{
	if (a>b)
		Swap(a,b);
}


// Implementation of templates is here!
#include "SimpleLib.cpp"



/////////////////////////////////////////////////////////////////////////////
// Visual Studio 2005/2008 autoexp.dat definitions

// Copy paste the following into <vsdir>\Common7\Packages\Debugger\autoexp.dat
//	in the [Visualizer] section.
// Currently doesn't work for CHashMap or CIndex, due to problems interating
//  intrusive lists where the next/prev pointers are in an embedded struct.

/*

;------------------------------------------------------------------------------
; SimpleLib
;------------------------------------------------------------------------------

Simple::CString<char>{
		preview			(#if($e.m_psz==0) ("<null>") #else ([$e.m_psz,s]))
		stringview		([$e.m_psz,sb])
}

Simple::CString<wchar_t>{
		preview			(#if($e.m_psz==0) ("<null>") #else ([$e.m_psz,su]))
		stringview		([$e.m_psz,sub])
}
Simple::CVector<*>|Simple::CSortedVector<*>|Simple::CUniStringVector{
	children
	(
		#array
		(
			expr :		($e.m_pData)[$i],
			size :		$e.m_iSize
		)
	)
	preview
	(
		#if($e.m_iSize==0) ("<empty>") #else (
		#(
			"[", $e.m_iSize , "](",
			#array
			(
				expr :	($e.m_pData)[$i],
				size :	$e.m_iSize
			),
			")"
		)
		)
	)
}

Simple::CMap<*>::CKeyPairInternal{
		preview			(#("[",$e.m_Key,"] = ", $e.m_Value))
}
Simple::CMap<*>{
	children
	(
		#tree
		(
			head : $e.m_pRoot,
			skip : &$e.m_Leaf,
			size : $e.m_iSize,
			left : m_pLeft,
			right : m_pRight
		) : $e.m_KeyPair
	)
	preview
	(
		#(
			"[", $e.m_iSize, "](",
			#tree
			(
				head : $e.m_pRoot,
				skip : &$e.m_Leaf,
				size : $e.m_iSize,
				left : m_pLeft,
				right : m_pRight
			) : $e.m_KeyPair,
			")"
		)
	)
}


Simple::CIndex<*>::CEntry{
		preview			(#("[",$e.m_Key,"] = ", $e.m_Value))
}
Simple::CIndex<*>{
	children
	(
		#array
		(
			expr :		($e.m_Entries.m_pData)[$i],
			size :		$e.m_Entries.m_iSize
		)
	)
	preview
	(
		#(
			"[", $e.m_Entries.m_iSize, "](",
			#array
			(
				expr :		($e.m_Entries.m_pData)[$i],
				size :		$e.m_Entries.m_iSize
			),
			")"
		)
	)
}

Simple::CRingBuffer<*>{
	preview
	(
		#if($e.m_iSize==0) ("<empty>") #else (
		#(
			"[", $e.m_iSize , "](",
			#array
			(
				expr :		($e.m_pMem)[(($e.m_pReadPos - $e.m_pMem) + $i) % $e.m_iCapacity],
				size :		$e.m_iSize
			),
			")"
		)
		)
	)
	children
	(
		#array
		(
			expr :		($e.m_pMem)[(($e.m_pReadPos - $e.m_pMem) + $i) % $e.m_iCapacity],
			size :		$e.m_iSize
		)
	)
}

*/


#endif	// __SIMPLELIB_H

