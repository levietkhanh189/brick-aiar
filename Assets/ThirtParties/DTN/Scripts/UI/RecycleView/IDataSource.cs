using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Cooldhands.UI
{
    public interface IDataSource
    {
        object GetData(int index);
        int GetCount();
    }
}
