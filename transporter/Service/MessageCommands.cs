﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transporter.Service
{
    public enum MessageCommands
    {
        OpenDataListener,
        DataListenerCreated,
        IsFree,
        OK,
        Cancel
    }
}