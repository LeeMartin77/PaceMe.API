using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using PaceMe.Model.Record;

namespace PaceMe.Storage.Repository
{
    public interface ITrainingPlanRepository : IBaseRepository<TrainingPlanRecord>
    {
        Task<IEnumerable<TrainingPlanRecord>> GetForParentId(Guid userId);
        Task<TrainingPlanRecord> GetById(Guid trainingPlanId);
    }

    public class TrainingPlanRepository : BaseRepository<TrainingPlanRecord>, ITrainingPlanRepository
    {
        public TrainingPlanRepository(IConfiguration configuration): base("TrainingPlan", configuration){
        }

        public override DynamicTableEntity GenerateEntityFromRecord(TrainingPlanRecord record)
        {
            return new DynamicTableEntity(record.UserId.ToString(), record.TrainingPlanId.ToString()){
                Properties = TableEntity.Flatten(record, new OperationContext())
            };
        }
    }
}