using System;
using System.Collections.Generic;
using System.Text;

namespace Telemetry
{
    interface ISerializer
    {
        string Serialize(TrackerEvent tEvent);
    }
}
