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
    public interface IActivitySegmentRepository : IBaseRepository<ActivitySegmentRecord>
    {
        Task<IEnumerable<ActivitySegmentRecord>> GetForParentId(Guid trainingPlanActivityId);
        Task<ActivitySegmentRecord> GetById(Guid trainingPlanActivitySegmentId);
    }

    public class ActivitySegmentRepository : BaseRepository<ActivitySegmentRecord>, IActivitySegmentRepository
    {
        public ActivitySegmentRepository(IConfiguration configuration): base("TrainingPlanActivitySegment", configuration){
        }

        public override DynamicTableEntity GenerateEntityFromRecord(ActivitySegmentRecord record)
        {
            return new DynamicTableEntity(record.ActivityId.ToString(), record.ActivitySegmentId.ToString()){
                Properties = TableEntity.Flatten(record, new OperationContext())
            };
        }

    }
}