﻿// Copyright 2013 The Chromium Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

using System.Collections.Generic;
using System.Diagnostics;
using VsChromium.Core.Files;
using VsChromium.Core.Utility;

namespace VsChromium.Server.FileSystemNames {
  public class FileSystemNameComparer : IComparer<FileSystemName>, IEqualityComparer<FileSystemName> {
    public static readonly FileSystemNameComparer Instance = new FileSystemNameComparer();

    public int Compare(FileSystemName x, FileSystemName y) {
      if (ReferenceEquals(x, y))
        return 0;
      if (x == null)
        return -1;
      if (y == null)
        return 1;

      var xdist = GetDistanceToAbsolutePath(x);
      var ydist = GetDistanceToAbsolutePath(y);
      if (xdist == ydist) {
        return CompareSameDistance(x, y);
      }

      var xSameLevel = (xdist > ydist ? GetAncestor(x, xdist - ydist) : x);
      var ySameLevel = (ydist > xdist ? GetAncestor(y, ydist - xdist) : y);
      int result = CompareSameDistance(xSameLevel, ySameLevel);
      if (result != 0) {
        return result;
      }

      if (xdist > ydist) return 1;
      return -1;
    }

    public int CompareSameDistance(FileSystemName x, FileSystemName y) {
      Debug.Assert(x != null);
      Debug.Assert(y != null);

      var xabs = x as AbsoluteDirectoryName;
      var yabs = y as AbsoluteDirectoryName;
      if (xabs != null || yabs != null) {
        Debug.Assert(xabs != null && yabs != null);
        return xabs.FullPath.CompareTo(yabs.FullPath);
      }

      var result = Compare(x.Parent, y.Parent);
      if (result == 0)
        result = SystemPathComparer.Instance.StringComparer.Compare(x.Name, y.Name);
      return result;
    }

    private FileSystemName GetAncestor(FileSystemName x, int count) {
      Debug.Assert(x != null);
      for (var i = 0; i < count; i++) {
        x = x.Parent;
      }
      Debug.Assert(x != null);
      return x;
    }

    private int GetDistanceToAbsolutePath(FileSystemName x) {
      Debug.Assert(x != null);
      for (int result = 0;; result++) {
        if (x is AbsoluteDirectoryName)
          return result;
        x = x.Parent;
        Debug.Assert(x != null);
      }
    }
    public bool Equals(FileSystemName x, FileSystemName y) {
      if (x == null || y == null)
        return ReferenceEquals(x, y);
      var xabs = x as AbsoluteDirectoryName;
      var yabs = y as AbsoluteDirectoryName;
      if (xabs != null || yabs != null) {
        if (xabs == null) return false;
        if (yabs == null) return false;
        return xabs.FullPath.Equals(yabs.FullPath);
      }

      var result = Equals(x.Parent, y.Parent);
      if (result)
        result = SystemPathComparer.Instance.StringComparer.Equals(x.Name, y.Name);
      return result;
    }

    public int GetHashCode(FileSystemName x) {
      var abs = x as AbsoluteDirectoryName;
      if (abs != null) {
        return abs.FullPath.GetHashCode();
      }

      return HashCode.Combine(GetHashCode(x.Parent), SystemPathComparer.Instance.StringComparer.GetHashCode(x.Name));
    }
  }
}
