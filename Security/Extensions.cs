﻿/*
AdamMil.Security is a .NET library providing OpenPGP-based security.
http://www.adammil.net/
Copyright (C) 2008-2011 Adam Milazzo

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.
You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
*/

using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace AdamMil.Security
{

/// <summary>A class containing various helpful extensions for using <see cref="SecureString"/> objects.</summary>
public static class SecureStringExtensions
{
  /// <summary>Performs the given action on the characters within a <see cref="SecureString"/>.</summary>
  public static void Process(this SecureString secureString, Action<char[]> processor)
  {
    if(secureString == null || processor == null) throw new ArgumentNullException();

    // treat the string as securely as we can by ensuring that it doesn't stick around in memory longer than necessary
    IntPtr bstr  = IntPtr.Zero;
    char[] chars = new char[secureString.Length];
    try
    {
      bstr = Marshal.SecureStringToBSTR(secureString);
      Marshal.Copy(bstr, chars, 0, chars.Length);
      processor(chars);
    }
    finally
    {
      if(bstr != IntPtr.Zero) Marshal.ZeroFreeBSTR(bstr);
      SecurityUtility.ZeroBuffer(chars);
    }
  }

  /// <summary>Performs the given action on the UTF-8 encoded bytes of the <see cref="SecureString"/>.</summary>
  public static void Process(this SecureString secureString, Action<byte[]> processor)
  {
    Process(secureString, Encoding.UTF8, processor);
  }

  /// <summary>Performs the given action on the encoded bytes of the <see cref="SecureString"/>.</summary>
  public static void Process(this SecureString secureString, Encoding encoding, Action<byte[]> processor)
  {
    if(encoding == null || processor == null) throw new ArgumentNullException();

    Process(secureString, delegate(char[] chars)
    {
      byte[] bytes = null;
      try
      {
        bytes = encoding.GetBytes(chars);
        processor(bytes);
      }
      finally { SecurityUtility.ZeroBuffer(bytes); }
    });
  }
}

} // namespace AdamMil.Security
