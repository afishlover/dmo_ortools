using Newtonsoft.Json;
using Server.Models;

namespace Server.Utils
{
    public class FakeContext
    {
        private List<WorkerModel> Workers = new List<WorkerModel>();
        private List<LineModel> Lines = new List<LineModel>();
        public FakeContext()
        {
            var workerStr = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), @"Localdb/worker.json"));
            Workers = JsonConvert.DeserializeObject<List<WorkerModel>>(workerStr);

            var lineStr = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), @"Localdb/line.json"));
            Lines = JsonConvert.DeserializeObject<List<LineModel>>(lineStr);
        }

        #region Worker
        public List<WorkerModel> GetAllWorkers()
        {
            return Workers;
        }

        public List<WorkerModel> GetWorkerById(string workerId)
        {
            return Workers.FindAll(w => w.WorkerId.Equals(workerId.ToLower()));
        }

        public bool CreateWorker(WorkerModel worker)
        {
            try
            {
                Workers.Add(worker);
                var workerStr = JsonConvert.SerializeObject(Workers, Formatting.Indented);
                File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), @"Localdb/worker.json"), workerStr);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool DeleteWorker(string workerId)
        {
            try
            {
                Workers.Remove(Workers.Find(w => w.WorkerId.Equals(workerId)));
                var workerStr = JsonConvert.SerializeObject(Workers, Formatting.Indented);
                File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), @"Localdb/worker.json"), workerStr);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region Lines
        #endregion
    }
}
