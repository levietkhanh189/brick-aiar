using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Cooldhands.UI
{
    public interface IRecycleItemChanged
    {
        void OnRecycle(ListItemContent item, Action complete);
    }
}
