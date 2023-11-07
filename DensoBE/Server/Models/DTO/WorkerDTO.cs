namespace Server.Models.DTO
{
    public class WorkerDTO
    {
        public string WorkerName { get; set; } = string.Empty;
        public int WorkerSalary { get; set; } = 0;
        public int WorkerAge { get; set; } = 0;
        public List<StageModel> WorkerStages { get; set; } = new List<StageModel>();
    }
}
