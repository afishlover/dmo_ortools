namespace Server.Models
{
    public class WorkerModel
    {
        public string WorkerId { get; set; } = Guid.NewGuid().ToString();
        public string WorkerName { get; set; } = string.Empty;
        public int WorkerAge { get; set; } = 0;
        public int WorkerSalary { get; set; } = 0;
        public List<StageModel> WorkerStages { get; set; } = new List<StageModel>();
    }
}
