using System;
using System.Collections.Generic;

namespace PaceMe.Model.Record
{
    public record TrainingPlanActivitySegmentRecord
    {
        public Guid TrainingPlanActivityId { get; init; }
        public Guid TrainingPlanActivitySegmentId { get; init; }
        public int Order { get; init; }
        public int DurationSeconds { get; init; }
        public string Notes { get; init; }
        public Guid? SegmentGroup { get; init; }
    }
}
