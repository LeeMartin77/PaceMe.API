using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PaceMe.Storage;
using PaceMe.Model.Record;
using PaceMe.Storage.Utilities;
using System.Net.Http;
using System.Net;
using System.Text;
using PaceMe.Storage.Repository;
using System.Security.Claims;
using PaceMe.FunctionApp.Authentication;

namespace PaceMe.FunctionApp.Controller
{
    public class TrainingPlanActivityController
    {
        private const string Route = "user/{userId}/trainingplan/{trainingPlanId}/activity";
        private readonly IRequestAuthenticator _requestAuthenticator;
        private readonly ITrainingPlanRepository _trainingPlanRepository;
        private readonly ITrainingPlanActivityRepository _trainingPlanActivityRepository;

        public TrainingPlanActivityController(
            IRequestAuthenticator requestAuthenticator,
            ITrainingPlanRepository trainingPlanRepository,
            ITrainingPlanActivityRepository trainingPlanActivityRepository
            )
        {
            _requestAuthenticator = requestAuthenticator;
            _trainingPlanRepository = trainingPlanRepository;
            _trainingPlanActivityRepository = trainingPlanActivityRepository;
        }

        [FunctionName("TrainingPlanActivityController_GetTrainingPlanActivities")]
        public async Task<IActionResult> GetTrainingPlanActivities(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = Route)] HttpRequest req,
            Guid userId,
            Guid trainingPlanId,
            ILogger log)
        {
            if(! await _requestAuthenticator.AuthenticateRequest(userId, req)){
                return new UnauthorizedResult();
            }

            var trainingPlan = await _trainingPlanRepository.GetById(trainingPlanId);
            var activities = await _trainingPlanActivityRepository.GetForParentId(trainingPlanId);
            
            if(trainingPlan == null || trainingPlan.UserId != userId){
                return new NotFoundResult();
            }

            return new JsonResult(activities);

        }

        
        [FunctionName("TrainingPlanActivityController_GetTrainingPlanActivity")]
        public async Task<IActionResult> GetTrainingPlanActivity(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = Route + "/{trainingPlanActivityId}")] HttpRequest req,
            Guid userId,
            Guid trainingPlanId,
            Guid trainingPlanActivityId,
            ILogger log)
        {
            if(! await _requestAuthenticator.AuthenticateRequest(userId, req)){
                return new UnauthorizedResult();
            }

            var trainingPlan = await _trainingPlanRepository.GetById(trainingPlanId);
            var activity = await _trainingPlanActivityRepository.GetById(trainingPlanActivityId);
            
            if(InvalidRequest(userId, trainingPlan, activity)){
                return new NotFoundResult();
            }

            return new JsonResult(activity);
        }

        [FunctionName("TrainingPlanActivityController_CreateTrainingPlanActivity")]
        public async Task<IActionResult> CreateTrainingPlan(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = Route)] HttpRequest req,
            Guid userId,
            Guid trainingPlanId,
            ILogger log)
        {
            if(! await _requestAuthenticator.AuthenticateRequest(userId, req)){
                return new UnauthorizedResult();
            }

            var trainingPlan = await _trainingPlanRepository.GetById(trainingPlanId);

            if(InvalidRequest(userId, trainingPlan)){
                return new NotFoundResult();
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            TrainingPlanActivityRecord activity = JsonConvert.DeserializeObject<TrainingPlanActivityRecord>(requestBody);

            TrainingPlanActivityRecord createRecord = new TrainingPlanActivityRecord {
                TrainingPlanActivityId = Guid.NewGuid(),
                TrainingPlanId = trainingPlanId,
                Name = activity.Name,
                Completed = activity.Completed,
                DateTime = activity.DateTime
            };

            await _trainingPlanActivityRepository.Create(createRecord);
            
            return new JsonResult(createRecord.TrainingPlanActivityId);

        }

        [FunctionName("TrainingPlanActivityController_UpdateTrainingPlanActivity")]
        public async Task<IActionResult> UpdateTrainingPlan(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = Route + "/{trainingPlanActivityId}")] HttpRequest req,
            Guid userId,
            Guid trainingPlanId,
            Guid trainingPlanActivityId,
            ILogger log)
        {
            if(! await _requestAuthenticator.AuthenticateRequest(userId, req)){
                return new UnauthorizedResult();
            }

            var trainingPlan = await _trainingPlanRepository.GetById(trainingPlanId);
            var existing = await _trainingPlanActivityRepository.GetById(trainingPlanActivityId);

            if(InvalidRequest(userId, trainingPlan, existing)){
                return new NotFoundResult();
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            TrainingPlanActivityRecord activity = JsonConvert.DeserializeObject<TrainingPlanActivityRecord>(requestBody);

            TrainingPlanActivityRecord updateRecord = new TrainingPlanActivityRecord {
                TrainingPlanActivityId = trainingPlanActivityId,
                TrainingPlanId = trainingPlanId,
                Name = activity.Name,
                Completed = activity.Completed,
                DateTime = activity.DateTime
            };

            await _trainingPlanActivityRepository.Update(updateRecord);
            
            return new OkResult();

        }

        
        [FunctionName("TrainingPlanActivityController_DeleteTrainingPlanActivity")]
        public async Task<IActionResult> DeleteTrainingPlan(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = Route + "/{trainingPlanActivityId}")] HttpRequest req,
            Guid userId,
            Guid trainingPlanId,
            Guid trainingPlanActivityId,
            ILogger log)
        {
            if(! await _requestAuthenticator.AuthenticateRequest(userId, req)){
                return new UnauthorizedResult();
            }

            var trainingPlan = await _trainingPlanRepository.GetById(trainingPlanId);
            var existing = await _trainingPlanActivityRepository.GetById(trainingPlanActivityId);

            if(InvalidRequest(userId, trainingPlan, existing)){
                return new BadRequestResult();
            }

            await _trainingPlanActivityRepository.Delete(existing);
            
            return new OkResult();

        }

        private static bool InvalidRequest(Guid userId, TrainingPlanRecord trainingPlan, TrainingPlanActivityRecord existingActivity){
            return InvalidRequest(userId, trainingPlan) || existingActivity == null || existingActivity.TrainingPlanId != trainingPlan.TrainingPlanId;
        }

        
        private static bool InvalidRequest(Guid userId, TrainingPlanRecord trainingPlan){
            return trainingPlan == null || trainingPlan.UserId != userId;
        }
    }
}
