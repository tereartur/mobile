using System;
using System.Threading.Tasks;
using Toggl.Phoebe.Data.Json;
using Toggl.Phoebe.Data.Models;
using Toggl.Phoebe.Net;
using Toggl.Phoebe.Logging;
using XPlatUtils;

namespace Toggl.Phoebe
{
    public static class OBMExperimentManager
    {
#if __ANDROID__
        public const int ExperimentNumber = 93;
#else
        public const int ExperimentNumber = 116;
#endif

        //public const string StartButtonActionKey = "startButton";
        //public const string ClickActionValue = "click";
    }
}