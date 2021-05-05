using System;
using System.Collections.Generic;

namespace PaceMe.Model.Record
{
    public record TrainingPlanRecord
    {
        public Guid UserId { get; init; }
        public Guid TrainingPlanId { get; init; }
        public string Name { get; init; }
        public bool Active { get; init; }
    }
}
