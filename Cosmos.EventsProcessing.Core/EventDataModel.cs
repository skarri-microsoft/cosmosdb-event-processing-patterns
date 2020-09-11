using System;
using System.Dynamic;

namespace Cosmos.EventsProcessing.Core
{
    public class EventDataModel
    {
#pragma warning disable IDE1006 // Naming Styles
        public string id { get; set; }

        public string topic { get; set; }

        public string message { get; set; }

        public DateTime createdTime { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    }
}
