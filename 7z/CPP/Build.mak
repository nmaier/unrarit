CC = icl

LIBS = $(LIBS) oleaut32.lib ole32.lib

!IFDEF CPU
!IFNDEF NO_BUFFEROVERFLOWU
#LIBS = $(LIBS) bufferoverflowU.lib
!ENDIF
!ENDIF


!IFNDEF O
!IFDEF CPU
O=$(CPU)-$(ARCH)
!ELSE
O=O
!ENDIF
!ENDIF

EXTRA_CFLAGS = -DDO_NOT_DECLARE_COMSTUBS=1 -GS

!IF "$(CPU)" == "AMD64"
MY_ML = ml64 -Dx64
CRT = AMD64
!ELSEIF "$(CPU)" == "ARM"
MY_ML = armasm
!ELSE
MY_ML = ml
EXTRA_CFLAGS = $(EXTRA_CFLAGS) -Oy
CRT = Intel
!ENDIF


!IFDEF UNDER_CE
RFLAGS = $(RFLAGS) -dUNDER_CE
!IFDEF MY_CONSOLE
LFLAGS = $(LFLAGS) /ENTRY:mainACRTStartup
!ENDIF
!ELSE
!IFNDEF NEW_COMPILER
LFLAGS = $(LFLAGS) -OPT:NOWIN98
!ENDIF
CFLAGS = $(CFLAGS) -Gr
LIBS = $(LIBS) user32.lib advapi32.lib shell32.lib
!ENDIF

!IF "$(CPU)" == "ARM"
COMPL_ASM = $(MY_ML) $** $O/$(*B).obj
!ELSE
COMPL_ASM = $(MY_ML) -c -Fo$O/ $**
!ENDIF

CFLAGS = $(CFLAGS) $(EXTRA_CFLAGS) -nologo -c -Fo$O/ -WX -EHsc -Gr -Gy -GR-

CFLAGS = $(CFLAGS) -MT

!IFDEF NEW_COMPILER
CFLAGS = $(CFLAGS) -W4 -GS- -Zc:forScope
!ELSE
CFLAGS = $(CFLAGS) -W3
!ENDIF

!IF "$(ARCH)" == "dbg"
CFLAGS_O1 = $(CFLAGS) -Od
CFLAGS_O2 = $(CFLAGS) -Od
!ELSEIF "$(ARCH)" == "sse3"
CFLAGS_O1 = $(CFLAGS) -Og -Ox -Oi -Ot -GT -GF -Og -Qipo -Ob2 -GS -arch:SSE3 -QxSSE3 -QaxSSE4.2 -Quse-intel-optimized-headers
CFLAGS_O2 = $(CFLAGS) -Og -Ox -Oi -Ot -GT -GF -Qipo -Ob2 -Og -QxSSE3 -QaxSSE4.2 -Quse-intel-optimized-headers
!ELSE
CFLAGS_O1 = $(CFLAGS) -Ox -Oi -Ot -GT -GF -Og -Qipo -Ob2 -GS
CFLAGS_O2 = $(CFLAGS) -Ox -Oi -Ot -GT -GF -Qipo -Ob2 -Og 
!IF "$(CPU)" != "IA64" && "$(CPU)" != "AMD64"
CFLAGS_O1 = $(CFLAGS_O1) -arch:IA32
CFLAGS_O2 = $(CFLAGS_O2) -arch:IA32
!ENDIF
!ENDIF

LFLAGS = $(LFLAGS) -nologo -OPT:REF -OPT:ICF

!IFDEF DEF_FILE
LFLAGS = $(LFLAGS) -DLL -DEF:$(DEF_FILE)
!ENDIF

PROGDIR = E:\MSVC\UnRarIt\7z\bin\$O\$(PROJ)
PROGPATH = $(PROGDIR)\$(PROG)

COMPL_O1   = $(CC) $(CFLAGS_O1) $**
COMPL_O2   = $(CC) $(CFLAGS_O2) $**
COMPL_PCH  = $(CC) $(CFLAGS_O1) -Yc"StdAfx.h" -Fp$O/a.pch $**
COMPL      = $(CC) $(CFLAGS_O1) -Yu"StdAfx.h" -Fp$O/a.pch $**

all: $(PROGPATH)
    echo $(PROGPATH)

clean:
	-del /Q $(PROGPATH) $O\*.exe $O\*.dll $O\*.obj $O\*.lib $O\*.exp $O\*.res $O\*.pch

$O:
	if not exist "$O" mkdir "$O"

$(PROGDIR):
	if not exist "$(PROGDIR)" mkdir "$(PROGDIR)"

$(PROGPATH): $O  $(PROGDIR) $(OBJS) $(DEF_FILE) 
	xilink $(LFLAGS) -out:$(PROGPATH) $(OBJS) $(LIBS) /DEBUG /PDB:$(PROGPATH).pdb /MAP:$(PROGPATH).map /MAPINFO:EXPORTS ../../../../crt/$(CRT)/libcmt.lib ../../../../crt/$(CRT)/libcpmt.lib /NODEFAULTLIB:libc.lib /NODEFAULTLIB:libcmt.lib /NODEFAULTLIB:libcpmt.lib
$O\resource.res: $(*B).rc
	rc $(RFLAGS) -fo$@ $**
$O\StdAfx.obj: $(*B).cpp
	$(COMPL_PCH)
