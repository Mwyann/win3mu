// dllmain.h : Declaration of module class.

class CWin3muShellModule : public ATL::CAtlDllModuleT< CWin3muShellModule >
{
public :
	DECLARE_LIBID(LIBID_Win3muShellLib)
	DECLARE_REGISTRY_APPID_RESOURCEID(IDR_WIN3MUSHELL, "{9A70705D-1120-4868-9E34-FBF1F416E597}")
};

extern class CWin3muShellModule _AtlModule;
