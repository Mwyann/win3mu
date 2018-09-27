// IconHandler.cpp : Implem`entation of CIconHandler

#include "stdafx.h"
#include "IconHandler.h"
#include "NeFile.h"

// CIconHandler

CIconHandler::CIconHandler()
{
	m_dwMode = 0;
}


// IPersist

STDMETHODIMP CIconHandler::GetClassID(CLSID* pClassID)
{
	*pClassID = CLSID_IconHandler;
	return S_OK;
}

// IPersistFile

STDMETHODIMP CIconHandler::IsDirty()
{
	return E_NOTIMPL;
}

STDMETHODIMP CIconHandler::Load(LPCOLESTR pszFileName, DWORD dwMode)
{
	m_strFileName = pszFileName;
	m_dwMode = dwMode;
	return S_OK;
}

STDMETHODIMP CIconHandler::Save(LPCOLESTR pszFileName, BOOL fRemember)
{
	return E_NOTIMPL;
}

STDMETHODIMP CIconHandler::SaveCompleted(LPCOLESTR pszFileName)
{
	return E_NOTIMPL;
}

STDMETHODIMP CIconHandler::GetCurFile(LPOLESTR *ppszFileName)
{
	return E_NOTIMPL;
}


// IExtractIcon

STDMETHODIMP CIconHandler::GetIconLocation(UINT uFlags, PWSTR pszIconFile, UINT cchMax, int* piIndex, UINT* pwFlags)
{
	*pwFlags = GIL_NOTFILENAME;
	lstrcpynW(pszIconFile, m_strFileName.sz(), cchMax);
	*piIndex = 0;
	return S_OK;
}

STDMETHODIMP CIconHandler::Extract(PCWSTR pszFile, UINT nIconIndex, HICON* phiconLarge, HICON* phiconSmall, UINT nIconSize)
{
	// Open file
	CNeFile file;
	if (!file.Open(pszFile))
		return E_FAIL;

	// Extract icon
	if (!file.ExtractIcon(nIconSize, phiconLarge, phiconSmall))
		return E_FAIL;

	return S_OK;
}


