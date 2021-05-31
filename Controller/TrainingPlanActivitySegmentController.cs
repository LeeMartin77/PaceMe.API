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
using System.Collections.Generic;
using PaceMe.Storage.Service;
using PaceMe.Model.DTO;

namespace PaceMe.FunctionApp.Controller
{
    public class TrainingPlanActivitySegmentController
    {
        private const string Route = "user/{userId}/trainingplan/{trainingPlanId}/activity/{trainingPlanActivityId}/segment";
        private readonly IRequestAuthenticator _requestAuthenticator;
        private readonly ITrainingPlanRepository _trainingPlanRepository;
        private readonly ITrainingPlanActivityRepository _TrainingPlanActivityRepository;
        private readonly IActivitySegmentDTOService _ActivitySegmentDTOService;

        public TrainingPlanActivitySegmentController(
            IRequestAuthenticator requestAuthenticator,
            ITrainingPlanRepository trainingPlanRepository,
            ITrainingPlanActivityRepository TrainingPlanActivityRepository,
            IActivitySegmentDTOService ActivitySegmentDTOService
            )
        {
            _requestAuthenticator = requestAuthenticator;
            _trainingPlanRepository = trainingPlanRepository;
            _TrainingPlanActivityRepository = TrainingPlanActivityRepository;
            _ActivitySegmentDTOService = ActivitySegmentDTOService;
        }

        [FunctionName("TrainingPlanActivitySegmentController_GetTrainingPlanActivitySegments")]
        public async Task<IActionResult> GetTrainingPlanActivitySegments(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = Route)] HttpRequest req,
            Guid userId,
            Guid trainingPlanId,
            Guid trainingPlanActivityId,
            ILogger log)
        {
            if(! await _requestAuthenticator.AuthenticateRequest(userId, req)){
                return new UnauthorizedResult();
            }

            var trainingPlan = await _trainingPlanRepository.GetById(trainingPlanId);
            var activity = await _TrainingPlanActivityRepository.GetById(trainingPlanActivityId);
            var segments = await _ActivitySegmentDTOService.GetForParentId(trainingPlanActivityId);
            
            if(InvalidRequest(userId, trainingPlan, activity)){
                return new NotFoundResult();
            }

            return new JsonResult(segments);

        }

        
        [FunctionName("TrainingPlanActivitySegmentController_GetTrainingPlanActivitySegment")]
        public async Task<IActionResult> GetTrainingPlanActivitySegment(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = Route + "/{trainingPlanActivitySegmentId}")] HttpRequest req,
            Guid userId,
            Guid trainingPlanId,
            Guid trainingPlanActivityId,
            Guid trainingPlanActivitySegmentId,
            ILogger log)
        {
            if(! await _requestAuthenticator.AuthenticateRequest(userId, req)){
                return new UnauthorizedResult();
            }

            var trainingPlan = await _trainingPlanRepository.GetById(trainingPlanId);
            var activity = await _TrainingPlanActivityRepository.GetById(trainingPlanActivityId);
            var segment = await _ActivitySegmentDTOService.Get(trainingPlanActivitySegmentId);
            
            if(InvalidRequest(userId, trainingPlan, activity, segment)){
                return new NotFoundResult();
            }

            return new JsonResult(segment);
        }

        [FunctionName("TrainingPlanActivitySegmentController_CreateTrainingPlanActivitySegment")]
        public async Task<IActionResult> CreateTrainingPlanSegment(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = Route)] HttpRequest req,
            Guid userId,
            Guid trainingPlanId,
            Guid trainingPlanActivityId,
            ILogger log)
        {
            if(! await _requestAuthenticator.AuthenticateRequest(userId, req)){
                return new UnauthorizedResult();
            }

            var trainingPlan = await _trainingPlanRepository.GetById(trainingPlanId);
            var activity = await _TrainingPlanActivityRepository.GetById(trainingPlanActivityId);

            if(InvalidRequest(userId, trainingPlan)){
                return new NotFoundResult();
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            ActivitySegmentDTO segment = JsonConvert.DeserializeObject<ActivitySegmentDTO>(requestBody);

            await _ActivitySegmentDTOService.Create(segment);
            
            return new JsonResult(segment.ActivitySegmentId);

        }

        [FunctionName("TrainingPlanActivitySegmentController_UpdateTrainingPlanActivitySegment")]
        public async Task<IActionResult> UpdateTrainingPlanSegment(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = Route + "/{trainingPlanActivitySegmentId}")] HttpRequest req,
            Guid userId,
            Guid trainingPlanId,
            Guid trainingPlanActivityId,
            Guid trainingPlanActivitySegmentId,
            ILogger log)
        {
            if(! await _requestAuthenticator.AuthenticateRequest(userId, req)){
                return new UnauthorizedResult();
            }

            var trainingPlan = await _trainingPlanRepository.GetById(trainingPlanId);
            var activity = await _TrainingPlanActivityRepository.GetById(trainingPlanActivityId);
            var segment = await _ActivitySegmentDTOService.Get(trainingPlanActivitySegmentId);

            if(InvalidRequest(userId, trainingPlan, activity, segment)){
                return new NotFoundResult();
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            ActivitySegmentDTO updateSegment = JsonConvert.DeserializeObject<ActivitySegmentDTO>(requestBody);

            await _ActivitySegmentDTOService.Update(updateSegment);
            
            return new OkResult();

        }

        
        [FunctionName("TrainingPlanActivitySegmentController_DeleteTrainingPlanActivitySegment")]
        public async Task<IActionResult> DeleteTrainingPlanSegment(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = Route + "/{trainingPlanActivitySegmentId}")] HttpRequest req,
            Guid userId,
            Guid trainingPlanId,
            Guid trainingPlanActivityId,
            Guid trainingPlanActivitySegmentId,
            ILogger log)
        {
            if(! await _requestAuthenticator.AuthenticateRequest(userId, req)){
                return new UnauthorizedResult();
            }

            var trainingPlan = await _trainingPlanRepository.GetById(trainingPlanId);
            var activity = await _TrainingPlanActivityRepository.GetById(trainingPlanActivityId);
            var segment = await _ActivitySegmentDTOService.Get(trainingPlanActivitySegmentId);

            if(InvalidRequest(userId, trainingPlan, activity, segment)){
                return new BadRequestResult();
            }

            await _ActivitySegmentDTOService.Delete(segment);
            
            return new OkResult();

        }

        private static bool InvalidRequest(Guid userId, TrainingPlanRecord trainingPlan, TrainingPlanActivityRecord existingActivity, ActivitySegmentDTO existingSegment){
            return InvalidRequest(userId, trainingPlan, existingActivity) || existingSegment == null || existingSegment.ActivityId != existingActivity.TrainingPlanActivityId;
        }

        private static bool InvalidRequest(Guid userId, TrainingPlanRecord trainingPlan, TrainingPlanActivityRecord existingActivity, ActivitySegmentRecord existingSegment){
            return InvalidRequest(userId, trainingPlan, existingActivity) || existingSegment == null || existingSegment.ActivityId != existingActivity.TrainingPlanActivityId;
        }

        private static bool InvalidRequest(Guid userId, TrainingPlanRecord trainingPlan, TrainingPlanActivityRecord existingActivity){
            return InvalidRequest(userId, trainingPlan) || existingActivity == null || existingActivity.TrainingPlanId != trainingPlan.TrainingPlanId;
        }

        private static bool InvalidRequest(Guid userId, TrainingPlanRecord trainingPlan){
            return trainingPlan == null || trainingPlan.UserId != userId;
        }
    }
}
