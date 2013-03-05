using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;

namespace UnRarIt.Interop
{
  public static class FileIcon
  {
    private readonly static IFileIcon impl = FindImplementation();


    private static IFileIcon FindImplementation()
    {
      foreach (Type t in Assembly.GetExecutingAssembly().GetTypes()) {
        if (new List<Type>(t.GetInterfaces()).Contains(typeof(IFileIcon))) {
          try {
            var ctor = t.GetConstructor(new Type[] { });
            return (IFileIcon)ctor.Invoke(new Object[] { });
          }
          catch (Exception ex) {
            Console.Error.WriteLine(ex);
          }
        }
      }
      return null;
    }


    public static Image GetIcon(string path, FileIconSize size)
    {
      if (impl != null) {
        try {
          return impl.GetIcon(path, size);
        }
        catch (Exception) {
        }
      }
      return null;
    }
  }
}
