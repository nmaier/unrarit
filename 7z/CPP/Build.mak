CC = icl

LIBS = $(LIBS) oleaut32.lib ole32.lib

!IFDEF CPU
!IFNDEF NO_BUFFEROVERFLOWU
LIBS = $(LIBS) bufferoverflowU.lib
!ENDIF
!ENDIF

!IFNDEF ARCH
ARCH = dbg
!ENDIF

!IFNDEF O
!IFDEF CPU
O=$(CPU)-$(ARCH)
!ELSE
O=O
!ENDIF
!ENDIF

!IF "$(CPU)" != "IA64"
!IF "$(CPU)" != "AMD64"
MY_ML = ml
EXTRA_CFLAGS = -Oy
CRT = Intel
!ELSE
MY_ML = ml64
EXTRA_CFLAGS = 
CRT = AMD64
!ENDIF
!ENDIF


!IFDEF UNDER_CE
RFLAGS = $(RFLAGS) -dUNDER_CE
!IFDEF MY_CONSOLE
LFLAGS = $(LFLAGS) /ENTRY:mainACRTStartup
!ENDIF
!ELSE
LFLAGS = $(LFLAGS) -OPT:NOWIN98
CFLAGS = $(CFLAGS) -Gr
LIBS = $(LIBS) user32.lib advapi32.lib shell32.lib
!ENDIF


COMPL_ASM = $(MY_ML) -c -Fo$O/ $**

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
CFLAGS_O1 = $(CFLAGS) -Ox -Oi -Ot -GT -GF -Og -Qipo -Ob2 -GS -arch:SSE3 -QxSSE3 -QaxHost
CFLAGS_O2 = $(CFLAGS) -Ox -Oi -Ot -GT -GF -Qipo -Ob2 -Og -QxSSE3 -QaxHost
!ELSE
CFLAGS_O1 = $(CFLAGS) -Ox -Oi -Ot -GT -GF -Og -Qipo -Ob2 -GS
CFLAGS_O2 = $(CFLAGS) -Ox -Oi -Ot -GT -GF -Qipo -Ob2 -Og 
!IF "$(CPU)" != "IA64" && "$(CPU)" != "AMD64"
CFLAGS_O1 = $(CFLAGS_O1) -arch:IA32
CFLAGS_O2 = $(CFLAGS_O2) -arch:IA32
!ENDIF
!ENDIF



LFLAGS = $(LFLAGS) -nologo -OPT:NOWIN98 -OPT:REF -OPT:ICF

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
