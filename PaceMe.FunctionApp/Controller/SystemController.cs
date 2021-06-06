
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
namespace PaceMe.FunctionApp.Controller
{
    public class SystemController
    {
        [FunctionName("SystemController_WarupMethod")]
        public IActionResult WarmupApplication(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "system/warmup")] HttpRequest req
            )
        {
            // This endpoint exists to hit as soon as possible with a request,
            // to avoid waiting for the function app to warm up.
            return new OkResult();
        }
    }
}