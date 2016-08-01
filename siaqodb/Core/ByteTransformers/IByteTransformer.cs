using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Sqo.Core
{
    interface IByteTransformer
    {
        byte[] GetBytes(object obj,LightningDB.LightningTransaction transaction);
        object GetObject(byte[] bytes, LightningDB.LightningTransaction transaction);

    }
}
