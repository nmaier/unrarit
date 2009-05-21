I build the dlls using the Intel Compiler Collection 11.
This requires to have libirc.lib linked. Remove that reference when not using icc.

The source is mainly the same as the original unrar dll. (The original pre-compiled DLL should be usable with UnRarIt as well).
However, the custom unicode string functions (str*w) were replaced by the default crt ones (wcs*)

I statically link the DLLs with a custom built C/C++ Runtime (based on the MSVC), which is an "optimized" built and furthermore includes jemalloc.
Kudos to the mozilla folks for making this possible.
Unfortunately Microsoft does not allow to distribute source and/or object versions of their CRT, just the binaries.

If you don't want to mess with the dirty details yourself consider using the DLLs distributed along with UnRarIt.