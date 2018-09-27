

/* this ALWAYS GENERATED file contains the definitions for the interfaces */


 /* File created by MIDL compiler version 8.00.0603 */
/* at Wed Sep 28 14:21:49 2016
 */
/* Compiler settings for Win3muShell.idl:
    Oicf, W1, Zp8, env=Win64 (32b run), target_arch=AMD64 8.00.0603 
    protocol : dce , ms_ext, c_ext, robust
    error checks: allocation ref bounds_check enum stub_data 
    VC __declspec() decoration level: 
         __declspec(uuid()), __declspec(selectany), __declspec(novtable)
         DECLSPEC_UUID(), MIDL_INTERFACE()
*/
/* @@MIDL_FILE_HEADING(  ) */

#pragma warning( disable: 4049 )  /* more than 64k source lines */


/* verify that the <rpcndr.h> version is high enough to compile this file*/
#ifndef __REQUIRED_RPCNDR_H_VERSION__
#define __REQUIRED_RPCNDR_H_VERSION__ 475
#endif

#include "rpc.h"
#include "rpcndr.h"

#ifndef __RPCNDR_H_VERSION__
#error this stub requires an updated version of <rpcndr.h>
#endif // __RPCNDR_H_VERSION__

#ifndef COM_NO_WINDOWS_H
#include "windows.h"
#include "ole2.h"
#endif /*COM_NO_WINDOWS_H*/

#ifndef __Win3muShell_i_h__
#define __Win3muShell_i_h__

#if defined(_MSC_VER) && (_MSC_VER >= 1020)
#pragma once
#endif

/* Forward Declarations */ 

#ifndef __IIconHandler_FWD_DEFINED__
#define __IIconHandler_FWD_DEFINED__
typedef interface IIconHandler IIconHandler;

#endif 	/* __IIconHandler_FWD_DEFINED__ */


#ifndef __IconHandler_FWD_DEFINED__
#define __IconHandler_FWD_DEFINED__

#ifdef __cplusplus
typedef class IconHandler IconHandler;
#else
typedef struct IconHandler IconHandler;
#endif /* __cplusplus */

#endif 	/* __IconHandler_FWD_DEFINED__ */


/* header files for imported files */
#include "oaidl.h"
#include "ocidl.h"

#ifdef __cplusplus
extern "C"{
#endif 


#ifndef __IIconHandler_INTERFACE_DEFINED__
#define __IIconHandler_INTERFACE_DEFINED__

/* interface IIconHandler */
/* [unique][uuid][object] */ 


EXTERN_C const IID IID_IIconHandler;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("342404A0-B3F5-4E53-894C-33EF03778F21")
    IIconHandler : public IUnknown
    {
    public:
    };
    
    
#else 	/* C style interface */

    typedef struct IIconHandlerVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            IIconHandler * This,
            /* [in] */ REFIID riid,
            /* [annotation][iid_is][out] */ 
            _COM_Outptr_  void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            IIconHandler * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            IIconHandler * This);
        
        END_INTERFACE
    } IIconHandlerVtbl;

    interface IIconHandler
    {
        CONST_VTBL struct IIconHandlerVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define IIconHandler_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define IIconHandler_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define IIconHandler_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#endif /* COBJMACROS */


#endif 	/* C style interface */




#endif 	/* __IIconHandler_INTERFACE_DEFINED__ */



#ifndef __Win3muShellLib_LIBRARY_DEFINED__
#define __Win3muShellLib_LIBRARY_DEFINED__

/* library Win3muShellLib */
/* [version][uuid] */ 


EXTERN_C const IID LIBID_Win3muShellLib;

EXTERN_C const CLSID CLSID_IconHandler;

#ifdef __cplusplus

class DECLSPEC_UUID("4D0AA5D6-1C23-4975-8257-91E316544445")
IconHandler;
#endif
#endif /* __Win3muShellLib_LIBRARY_DEFINED__ */

/* Additional Prototypes for ALL interfaces */

/* end of Additional Prototypes */

#ifdef __cplusplus
}
#endif

#endif


