/* ************************************************************** *\
EmmyGui - A simple mencoder GUI for xvid/mp3 encoding
Copyright (C) 2008  Nils Maier

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
\* *************************************************************** */

using System;
using System.Drawing;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.Drawing.Imaging;

namespace UnRarIt.Interop
{
    public enum FileIconSize
    {
        Small = 0x1,
        Large = 0x0,
        ExtraLarge = 0x2
    };
    public class FileIcon
    {
        private static IFileIcon Impl;
        static FileIcon()
        {

            foreach (Type t in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (new List<Type>(t.GetInterfaces()).Contains(typeof(IFileIcon)))
                {
                    try
                    {
                        ConstructorInfo ctor = t.GetConstructor(new Type[] { });
                        Impl = (IFileIcon)ctor.Invoke(new Object[] { });
                        return;
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine(ex);
                    }
                }
            }            
        }

        private FileIcon()
        {
        }

        public static Image GetIcon(string aPath, FileIconSize aSize)
        {
            if (Impl != null)
            {
                try
                {
                    return Impl.GetIcon(aPath, aSize);
                }
                catch (Exception)
                {
                    // fall through
                }
            }
            return null;
        }
    }
}