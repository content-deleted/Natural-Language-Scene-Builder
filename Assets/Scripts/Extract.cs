using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEngine;

public static class Extract
{
    public delegate void ProgressDelegate(string sMessage);
    public static void DecompressToDirectory(string sCompressedFile, string sDir, ProgressDelegate progress)
    {
      using (FileStream inFile = new FileStream(sCompressedFile, FileMode.Open, FileAccess.Read, FileShare.None))
      using (GZipStream zipStream = new GZipStream(inFile, CompressionMode.Decompress, true))
        while (DecompressFile(sDir, zipStream, progress));
    }
    public static bool DecompressFile(string sDir, GZipStream zipStream, ProgressDelegate progress)
    {
      //Decompress file name
      byte[] bytes = new byte[sizeof(int)];
      int Readed = zipStream.Read(bytes, 0, sizeof(int));
      if (Readed < sizeof(int))
        return false;

      int iNameLen = BitConverter.ToInt32(bytes, 0);
      bytes = new byte[sizeof(char)];
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < iNameLen; i++)
      {
        zipStream.Read(bytes, 0, sizeof(char));
        char c = BitConverter.ToChar(bytes, 0);
        sb.Append(c);
      }
      string sFileName = sb.ToString();
      if (progress != null)
        progress(sFileName);

      //Decompress file content
      bytes = new byte[sizeof(int)];
      zipStream.Read(bytes, 0, sizeof(int));
      int iFileLen = BitConverter.ToInt32(bytes, 0);

      bytes = new byte[iFileLen];
      zipStream.Read(bytes, 0, bytes.Length);

      string sFilePath = Path.Combine(sDir, sFileName);
      string sFinalDir = Path.GetDirectoryName(sFilePath);
      if (!Directory.Exists(sFinalDir))
        Directory.CreateDirectory(sFinalDir);

      using (FileStream outFile = new FileStream(sFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
        outFile.Write(bytes, 0, iFileLen);

      return true;
    }
}
