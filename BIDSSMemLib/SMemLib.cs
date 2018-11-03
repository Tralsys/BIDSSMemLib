using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.MemoryMappedFiles;
namespace TR.BIDSSMemLib
{
  public class SMemLib
  {
    public SMemLib()
    {

    }
    MemoryMappedFile MMF = MemoryMappedFile.CreateOrOpen("BIDSSharedMemory", 0);
  }
}
