!IFDEF CPU
!IFNDEF NO_BUFFEROVERFLOWU
LIBS = $(LIBS) bufferoverflowU.lib
!ENDIF
!ENDIF


!IFNDEF O
!IFDEF CPU
O=$(CPU)
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

COMPL_ASM = $(MY_ML) -c -Fo$O/ $**

CFLAGS = $(CFLAGS) $(EXTRA_CFLAGS) -nologo -c -Fo$O/ -WX -EHsc -Gr -Gy -GR-

CFLAGS = $(CFLAGS) -MT

!IFDEF NEW_COMPILER
CFLAGS = $(CFLAGS) -W4 -GS- -Zc:forScope
!ELSE
CFLAGS = $(CFLAGS) -W3
!ENDIF

CFLAGS_O1 = $(CFLAGS) -Ox -Oi -Ot -GT -GF -GS
CFLAGS_O2 = $(CFLAGS) -Ox -Oi -Ot -GT -GF 

LFLAGS = $(LFLAGS) -nologo -OPT:NOWIN98 -OPT:REF -OPT:ICF

!IFDEF DEF_FILE
LFLAGS = $(LFLAGS) -DLL -DEF:$(DEF_FILE)
!ENDIF

PROGDIR = E:\MSVC\UnRarIt\7z\bin\$O\$(PROJ)
PROGPATH = $(PROGDIR)\$(PROG)

COMPL_O1   = $(CPP) $(CFLAGS_O1) $**
COMPL_O2   = $(CPP) $(CFLAGS_O2) $**
COMPL_PCH  = $(CPP) $(CFLAGS_O1) -Yc"StdAfx.h" -Fp$O/a.pch $**
COMPL      = $(CPP) $(CFLAGS_O1) -Yu"StdAfx.h" -Fp$O/a.pch $**

all: $(PROGPATH)

clean:
	-del /Q $(PROGPATH) $O\*.exe $O\*.dll $O\*.obj $O\*.lib $O\*.exp $O\*.res $O\*.pch

$O:
	if not exist "$O" mkdir "$O"

$(PROGDIR):
	if not exist "$(PROGDIR)" mkdir "$(PROGDIR)"

$(PROGPATH): $O  $(PROGDIR) $(OBJS) $(DEF_FILE) 
	link $(LFLAGS) -out:$(PROGPATH) $(OBJS) $(LIBS) /DEBUG /PDB:$(PROGDIR)\$(*B).pdb /MAP:$(PROGDIR)\$(*B).map /MAPINFO:EXPORTS ../../../../crt/$(CRT)/libcmt.lib ../../../../crt/$(CRT)/libcpmt.lib /NODEFAULTLIB:libc.lib /NODEFAULTLIB:libcmt.lib /NODEFAULTLIB:libcpmt.lib
$O\resource.res: $(*B).rc
	rc -fo$@ $**
$O\StdAfx.obj: $(*B).cpp
	$(COMPL_PCH)
