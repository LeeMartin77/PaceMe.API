using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace PaceMe.Model.Record
{
    public record TrainingPlanActivityRecord
    {
        public Guid TrainingPlanId { get; init; }
        public Guid TrainingPlanActivityId { get; init; }
        public string Name { get; init; }
        public bool Completed { get; init; }
        public DateTime DateTime { get; init; }
    }
}
