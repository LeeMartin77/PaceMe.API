using System;
using System.Collections.Generic;

namespace PaceMe.Model.Record
{
    public record TrainingPlanActivitySegmentRecord
    {
        public Guid TrainingPlanActivityId { get; init; }
        public Guid TrainingPlanActivitySegmentId { get; init; }
        public string Name { get; init; }
        public int Order { get; init; }
        public int Repetitions { get; init; }
    }
}
