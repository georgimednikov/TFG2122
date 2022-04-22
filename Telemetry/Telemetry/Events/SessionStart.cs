using System;
using System.Collections.Generic;
using System.Text;

namespace Telemetry.Events
{
    public class SessionStart : TrackerEvent
    {
        public SessionStart() : base(EventType.SessionStart) {}
        public override string ToJSON() { return $"[\n{base.ToJSON()}"; }
    }
}
