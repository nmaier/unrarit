DIRS = \
  Alone\~ \
  Alone7z\~ \
  Fm\~ \
  Format7z\~ \
  Format7zF\~ \
  Format7zR\~ \
  Format7zExtract\~ \
  Format7zExtractR\~

!IF "$(CPU)" == "x86"
DIRS = $(DIRS) SFXCon\~ SFXSetup\~ SFXWin\~
!ENDIF
  
all: $(DIRS)

$(DIRS):
!include "../SubBuild.mak"
