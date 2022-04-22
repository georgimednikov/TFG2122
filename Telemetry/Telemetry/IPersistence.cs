using System;
using System.Collections.Generic;
using System.Text;

namespace Telemetry
{
    interface IPersistence
    {
        void Send(TrackerEvent tEvent);
        void Flush();
    }
}
