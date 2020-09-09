using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public struct CommsPackage
{
    public string Command;
    public byte[] Data;
}

