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

namespace PaceMe.FunctionApp.Controller
{
    public class TrainingPlanActivitySegmentController
    {
        private const string Route = "user/{userId}/trainingplan/{trainingPlanId}/activity/{trainingPlanActivityId}/segment";
        private readonly IRequestAuthenticator _requestAuthenticator;
        private readonly ITrainingPlanRepository _trainingPlanRepository;
        private readonly ITrainingPlanActivityRepository _TrainingPlanActivityRepository;
        private readonly ITrainingPlanActivitySegmentRepository _TrainingPlanActivitySegmentRepository;

        public TrainingPlanActivitySegmentController(
            IRequestAuthenticator requestAuthenticator,
            ITrainingPlanRepository trainingPlanRepository,
            ITrainingPlanActivityRepository TrainingPlanActivityRepository,
            ITrainingPlanActivitySegmentRepository TrainingPlanActivitySegmentRepository
            )
        {
            _requestAuthenticator = requestAuthenticator;
            _trainingPlanRepository = trainingPlanRepository;
            _TrainingPlanActivityRepository = TrainingPlanActivityRepository;
            _TrainingPlanActivitySegmentRepository = TrainingPlanActivitySegmentRepository;
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
            var segments = await _TrainingPlanActivitySegmentRepository.GetForParentId(trainingPlanActivityId);
            
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
            var segment = await _TrainingPlanActivitySegmentRepository.GetById(trainingPlanActivitySegmentId);
            
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
            TrainingPlanActivitySegmentRecord segment = JsonConvert.DeserializeObject<TrainingPlanActivitySegmentRecord>(requestBody);

            TrainingPlanActivitySegmentRecord createRecord = new TrainingPlanActivitySegmentRecord {
                TrainingPlanActivitySegmentId = Guid.NewGuid(),
                TrainingPlanActivityId = trainingPlanActivityId,
                Order = segment.Order,
                DurationSeconds = segment.DurationSeconds,
                Notes = segment.Notes,
                SegmentGroup = segment.SegmentGroup
            };

            await _TrainingPlanActivitySegmentRepository.Create(createRecord);
            
            var segments = await _TrainingPlanActivitySegmentRepository.GetForParentId(trainingPlanActivityId);

            var orderedSegments = OrderAndReindexSegments(segments);

            await Task.WhenAll(orderedSegments.Select(x => _TrainingPlanActivitySegmentRepository.Update(x)));
            
            return new JsonResult(createRecord.TrainingPlanActivitySegmentId);

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
            var segment = await _TrainingPlanActivitySegmentRepository.GetById(trainingPlanActivitySegmentId);

            if(InvalidRequest(userId, trainingPlan, activity, segment)){
                return new NotFoundResult();
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            TrainingPlanActivitySegmentRecord requestSegment = JsonConvert.DeserializeObject<TrainingPlanActivitySegmentRecord>(requestBody);

            TrainingPlanActivitySegmentRecord updateRecord = new TrainingPlanActivitySegmentRecord {
                TrainingPlanActivitySegmentId = trainingPlanActivitySegmentId,
                TrainingPlanActivityId = trainingPlanActivityId,
                Order = segment.Order,
                DurationSeconds = segment.DurationSeconds,
                Notes = segment.Notes,
                SegmentGroup = segment.SegmentGroup
            };

            await _TrainingPlanActivitySegmentRepository.Update(updateRecord);
            
            var segments = await _TrainingPlanActivitySegmentRepository.GetForParentId(trainingPlanActivityId);

            var orderedSegments = OrderAndReindexSegments(segments);

            await Task.WhenAll(orderedSegments.Select(x => _TrainingPlanActivitySegmentRepository.Update(x)));
            
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
            var segment = await _TrainingPlanActivitySegmentRepository.GetById(trainingPlanActivitySegmentId);

            if(InvalidRequest(userId, trainingPlan, activity, segment)){
                return new BadRequestResult();
            }

            await _TrainingPlanActivitySegmentRepository.Delete(segment);

            var segments = await _TrainingPlanActivitySegmentRepository.GetForParentId(trainingPlanActivityId);

            var orderedSegments = OrderAndReindexSegments(segments);

            await Task.WhenAll(orderedSegments.Select(x => _TrainingPlanActivitySegmentRepository.Update(x)));
            
            return new OkResult();

        }

        private static bool InvalidRequest(Guid userId, TrainingPlanRecord trainingPlan, TrainingPlanActivityRecord existingActivity, TrainingPlanActivitySegmentRecord existingSegment){
            return InvalidRequest(userId, trainingPlan, existingActivity) || existingSegment == null || existingSegment.TrainingPlanActivityId != existingActivity.TrainingPlanActivityId;
        }

        private static bool InvalidRequest(Guid userId, TrainingPlanRecord trainingPlan, TrainingPlanActivityRecord existingActivity){
            return InvalidRequest(userId, trainingPlan) || existingActivity == null || existingActivity.TrainingPlanId != trainingPlan.TrainingPlanId;
        }

        private static bool InvalidRequest(Guid userId, TrainingPlanRecord trainingPlan){
            return trainingPlan == null || trainingPlan.UserId != userId;
        }

        private static IEnumerable<TrainingPlanActivitySegmentRecord> OrderAndReindexSegments(IEnumerable<TrainingPlanActivitySegmentRecord> records)
        {
            var orderedSegments = new List<TrainingPlanActivitySegmentRecord>();
            int i = 1;
            foreach(var segment in records.OrderBy(x => x.Order)){
                orderedSegments.Add(
                    new TrainingPlanActivitySegmentRecord {
                        TrainingPlanActivityId = segment.TrainingPlanActivityId,
                        TrainingPlanActivitySegmentId = segment.TrainingPlanActivitySegmentId,
                        Order = i,
                        DurationSeconds = segment.DurationSeconds,
                        Notes = segment.Notes,
                        SegmentGroup = segment.SegmentGroup
                    }
                );
                i++;
            }
            return orderedSegments;
        }
    }
}
