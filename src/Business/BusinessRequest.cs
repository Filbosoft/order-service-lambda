using System;

namespace Business
{
    public class BusinessRequest
    {
        public string RequestingUserId { get; set; }
        public DateTime RequestedAt { get; set; }
    }
}