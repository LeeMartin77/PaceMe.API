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
    public class TrainingPlanController
    {
        private const string Route = "user/{userId}/trainingplan";
        private readonly IRequestAuthenticator _requestAuthenticator;
        private readonly ITrainingPlanRepository _trainingPlanRepository;

        public TrainingPlanController(
            IRequestAuthenticator requestAuthenticator,
            ITrainingPlanRepository trainingPlanRepository
            )
        {
            _trainingPlanRepository = trainingPlanRepository;
            _requestAuthenticator = requestAuthenticator;
        }

        [FunctionName("TrainingPlanController_GetTrainingPlans")]
        public async Task<IActionResult> GetTrainingPlans(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = Route)] HttpRequest req,
            Guid userId)
        {
            if(! await _requestAuthenticator.AuthenticateRequest(userId, req)){
                return new UnauthorizedResult();
            }

            var trainingPlans = await _trainingPlanRepository.GetForParentId(userId);
            
            return new JsonResult(trainingPlans);

        }

        
        [FunctionName("TrainingPlanController_GetTrainingPlan")]
        public async Task<IActionResult> GetTrainingPlan(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = Route + "/{trainingPlanId}")] HttpRequest req,
            Guid userId,
            Guid trainingPlanId,
            ILogger log)
        {
            if(! await _requestAuthenticator.AuthenticateRequest(userId, req)){
                return new UnauthorizedResult();
            }

            var trainingPlan = await _trainingPlanRepository.GetById(trainingPlanId);
            
            if(trainingPlan == null || trainingPlan.UserId != userId){
                return new NotFoundResult();
            }

            return new JsonResult(trainingPlan);
        }

        [FunctionName("TrainingPlanController_CreateTrainingPlan")]
        public async Task<IActionResult> CreateTrainingPlan(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = Route)] HttpRequest req,
            Guid userId,
            ILogger log)
        {
            if(! await _requestAuthenticator.AuthenticateRequest(userId, req)){
                return new UnauthorizedResult();
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            TrainingPlanRecord trainingPlan = JsonConvert.DeserializeObject<TrainingPlanRecord>(requestBody);

            TrainingPlanRecord createRecord = new TrainingPlanRecord {
                UserId = userId,
                TrainingPlanId = Guid.NewGuid(),
                Name = trainingPlan.Name,
                Active = trainingPlan.Active
            };

            await _trainingPlanRepository.Create(createRecord);
            
            return new JsonResult(createRecord.TrainingPlanId);

        }

        [FunctionName("TrainingPlanController_UpdateTrainingPlan")]
        public async Task<IActionResult> UpdateTrainingPlan(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = Route + "/{trainingPlanId}")] HttpRequest req,
            Guid userId,
            Guid trainingPlanId,
            ILogger log)
        {
            if(! await _requestAuthenticator.AuthenticateRequest(userId, req)){
                return new UnauthorizedResult();
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            TrainingPlanRecord trainingPlan = JsonConvert.DeserializeObject<TrainingPlanRecord>(requestBody);

            var existing = await _trainingPlanRepository.GetById(trainingPlanId);

            if(existing == null || existing.UserId != userId){
                return new BadRequestResult();
            }

            TrainingPlanRecord updateRecord = new TrainingPlanRecord {
                UserId = userId,
                TrainingPlanId = trainingPlanId,
                Name = trainingPlan.Name,
                Active = trainingPlan.Active
            };

            await _trainingPlanRepository.Update(updateRecord);
            
            return new OkResult();

        }

        
        [FunctionName("TrainingPlanController_DeleteTrainingPlan")]
        public async Task<IActionResult> DeleteTrainingPlan(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = Route + "/{trainingPlanId}")] HttpRequest req,
            Guid userId,
            Guid trainingPlanId,
            ILogger log)
        {
            if(! await _requestAuthenticator.AuthenticateRequest(userId, req)){
                return new UnauthorizedResult();
            }

            var existing = await _trainingPlanRepository.GetById(trainingPlanId);

            if(existing == null || existing.UserId != userId){
                return new BadRequestResult();
            }

            await _trainingPlanRepository.Delete(existing);
            
            return new OkResult();

        }
    }
}
