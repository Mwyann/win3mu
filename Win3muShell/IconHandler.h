// IconHandler.h : Declaration of the CIconHandler

#pragma once
#include "resource.h"       // main symbols
#include "Win3muShell_i.h"

#include "NeFile.h"

using namespace ATL;


// CIconHandler
class ATL_NO_VTABLE CIconHandler :
	public CComObjectRootEx<CComSingleThreadModel>,
	public CComCoClass<CIconHandler, &CLSID_IconHandler>,
	public IIconHandler,
	public IPersistFile,
	public IExtractIcon
{
public:
	CIconHandler();

DECLARE_REGISTRY_RESOURCEID(IDR_ICONHANDLER)


BEGIN_COM_MAP(CIconHandler)
	COM_INTERFACE_ENTRY(IIconHandler)
	COM_INTERFACE_ENTRY(IPersist)
	COM_INTERFACE_ENTRY(IPersistFile)
	COM_INTERFACE_ENTRY(IExtractIcon)
END_COM_MAP()



	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct()
	{
		return S_OK;
	}

	void FinalRelease()
	{
	}

	CUniString m_strFileName;
	DWORD m_dwMode;

public:
// IPersist
	STDMETHODIMP GetClassID(CLSID* pClassID);

// IPersistFile
	STDMETHODIMP IsDirty();
	STDMETHODIMP Load(LPCOLESTR pszFileName, DWORD dwMode);
	STDMETHODIMP Save(LPCOLESTR pszFileName, BOOL fRemember);
	STDMETHODIMP SaveCompleted(LPCOLESTR pszFileName);
	STDMETHODIMP GetCurFile(LPOLESTR *ppszFileName);

// IExtractIcon
	STDMETHODIMP GetIconLocation(UINT uFlags, PWSTR pszIconFile, UINT cchMax, int* piIndex, UINT* pwFlags);
	STDMETHODIMP Extract(PCWSTR pszFile, UINT nIconIndex, HICON* phiconLarge, HICON* phiconSmall, UINT nIconSize);

};

OBJECT_ENTRY_AUTO(__uuidof(IconHandler), CIconHandler)
