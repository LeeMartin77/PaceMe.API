using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using PaceMe.Model.Record;

namespace PaceMe.Storage.Repository 
{
    public interface ITrainingPlanActivitySegmentRepository : IBaseRepository<TrainingPlanActivitySegmentRecord>
    {
        Task<IEnumerable<TrainingPlanActivitySegmentRecord>> GetForParentId(Guid trainingPlanActivityId);
        Task<TrainingPlanActivitySegmentRecord> GetById(Guid trainingPlanActivitySegmentId);
    }

    public class TrainingPlanActivitySegmentRepository : BaseRepository<TrainingPlanActivitySegmentRecord>, ITrainingPlanActivitySegmentRepository
    {
        public TrainingPlanActivitySegmentRepository(IConfiguration configuration): base("TrainingPlanActivitySegment", configuration){
        }

        public override DynamicTableEntity GenerateEntityFromRecord(TrainingPlanActivitySegmentRecord record)
        {
            return new DynamicTableEntity(record.TrainingPlanActivityId.ToString(), record.TrainingPlanActivitySegmentId.ToString()){
                Properties = TableEntity.Flatten(record, new OperationContext())
            };
        }

    }
}