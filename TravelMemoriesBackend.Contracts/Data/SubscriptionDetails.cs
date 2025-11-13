using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelMemoriesBackend.Contracts.Data
{
    public class SubscriptionDetails
    {
        public int Id { get; set; }

        public string UserEmail { get; set; }

        public SubscriptionType SubscriptionType { get; set; } = SubscriptionType.Free;

        public DateTime? PlanStartDate { get; set; }

        public DateTime? PlanEndDate { get; set; }

        // by default every user is on free plan
        public PlanType PlanType { get; set; } = PlanType.Free;

        public float StorageUsedInGB { get; set; } = 0.0f;

        public float StorageCapacityInGB { get; set; } = 5.0f; // default free plan storage capacity

        public UserInfo User { get; set; }
    }

    public enum PlanType
    {
        Monthly,
        Yearly,
        Free
    }

    public enum SubscriptionType
    {
        Free,
        Basic,
        Pro
    }
}
