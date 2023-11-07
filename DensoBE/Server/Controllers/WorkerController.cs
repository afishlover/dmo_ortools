using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Server.Models;
using Server.Models.DTO;
using Server.Utils;

namespace Server.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class WorkerController : ControllerBase
    {
        private FakeContext _fakeContext;

        public WorkerController()
        {
            _fakeContext = new FakeContext();
        }

        [HttpGet]
        public IActionResult GetAllWorkers()
        {
            try
            {
                return new ContentResult
                {
                    Content = JsonConvert.SerializeObject(_fakeContext.GetAllWorkers()),
                    ContentType = "application/json",
                    StatusCode = 200
                };
            }
            catch (Exception)
            {
                return new ContentResult
                {
                    Content = null,
                    StatusCode = 500
                };
            }
        }

        [HttpPost("CreateWorker")]
        public IActionResult CreateWorker(WorkerDTO workerDTO)
        {
            try
            {
                var workerModel = new WorkerModel
                {
                    WorkerName = workerDTO.WorkerName,
                    WorkerSalary = workerDTO.WorkerSalary,
                    WorkerAge = workerDTO.WorkerAge,
                    WorkerStages = workerDTO.WorkerStages
                };

                _fakeContext.CreateWorker(workerModel);

                return new ContentResult
                {
                    Content = JsonConvert.SerializeObject($"Create new worker with id {workerModel.WorkerId} successfully."),
                    ContentType = "application/json",
                    StatusCode = 200
                };
            }
            catch (Exception)
            {
                return new ContentResult
                {
                    Content = "Create new worker failed.",
                    StatusCode = 400
                };
            };
        }

        [HttpDelete("DeleteWorker")]
        public IActionResult DeleteWorker(string workerId)
        {
            try
            {
                _fakeContext.DeleteWorker(workerId);

                return new ContentResult
                {
                    Content = JsonConvert.SerializeObject($"Deleted worker with id {workerId}."),
                    ContentType = "application/json",
                    StatusCode = 200
                };
            }
            catch (Exception)
            {
                return new ContentResult
                {
                    Content = JsonConvert.SerializeObject($"No worker with id {workerId} found."),
                    StatusCode = 404
                };
            };
        }
    }
}
