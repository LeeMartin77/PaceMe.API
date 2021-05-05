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
    public interface ITrainingPlanActivityRepository : IBaseRepository<TrainingPlanActivityRecord>
    {
        Task<IEnumerable<TrainingPlanActivityRecord>> GetForParentId(Guid trainingPlanId);
        Task<TrainingPlanActivityRecord> GetById(Guid trainingPlanActivityId);
    }
    

    public class TrainingPlanActivityRepository : BaseRepository<TrainingPlanActivityRecord>, ITrainingPlanActivityRepository
    {
        public TrainingPlanActivityRepository(IConfiguration configuration): base("TrainingPlanActivity", configuration){
        }

        public override DynamicTableEntity GenerateEntityFromRecord(TrainingPlanActivityRecord record)
        {
            return new DynamicTableEntity(record.TrainingPlanId.ToString(), record.TrainingPlanActivityId.ToString()){
                Properties = TableEntity.Flatten(record, new OperationContext())
            };
        }
    }
}