using System.Data;
using System.Text.Json;
using ExcelDataReader;

namespace ManPower;

public sealed class Data
{
    public int[] Activate = new[] { 0, 0, 0, 0, 0 };
    public int[] Weights = new[] { 10, -20, 10, 10, 5 };
    public int[] Levels = { -2, -1, 0, 1, 2 };
    public int[] Scores = { 0, 1, 2, 3, 4, 5 };
    public int NumOfLines { get; set; }
    public int NumOfStages { get; set; }
    public int NumOfWorkers { get; set; }
    public int NumOfShifts { get; set; }

    public int[][] LineShift { get; set; } // Determine if a line is active in this shift
    public int[][] LineStage { get; set; } // Determine if a line has this stage
    public int[][] StageShift { get; set; } // Determine if a stage is active in this shift
    public int[] LineMinProductivity { get; set; }  // Determine a line required minimum productivity
    public int[][] WorkerStageAllowance { get; set; } // Determine if a worker can do this stage
    public int[][] WorkerStageExperience { get; set; } // Determine worker stage experience
    public int[][] WorkerShift { get; set; } // Determine if a worker can work at this shift
    public int[][] WorkerPreassign { get; set; } // Determine if a worker is forced to do this stage
    public int[] WorkerAge { get; set; } // Determine worker age
    public int[] WorkerHealth { get; set; } // Determine worker health
    public int[] WorkerSalary { get; set; } // Determine worker shift salary

    public int[][]
        WorkerStageProductivityScore { get; set; } // Determine worker productivity with regard to every stage


    private readonly DataTable _input = ExcelDataContext.GetInstance().Sheets["InputData"]!;
    private readonly DataTable _lineStage = ExcelDataContext.GetInstance().Sheets["LineStage"]!;
    private readonly DataTable _lineShift = ExcelDataContext.GetInstance().Sheets["LineShift"]!;
    private readonly DataTable _lineMinProductivity = ExcelDataContext.GetInstance().Sheets["LineMinProductivity"]!;
    private readonly DataTable _stageEligibleScore = ExcelDataContext.GetInstance().Sheets["StageEligibleScore"]!;
    private readonly DataTable _workerStageScore = ExcelDataContext.GetInstance().Sheets["WorkerStageScore"]!;
    private readonly DataTable _workerStageExperience = ExcelDataContext.GetInstance().Sheets["WorkerStageExperience"]!;
    private readonly DataTable _workerShift = ExcelDataContext.GetInstance().Sheets["WorkerShift"]!;
    private readonly DataTable _workerProfile = ExcelDataContext.GetInstance().Sheets["WorkerProfile"]!;
    private readonly DataTable _workerPreassigned = ExcelDataContext.GetInstance().Sheets["WorkerPreassigned"]!;


    public void Populate()
    {
        NumOfShifts = Convert.ToInt32(_input.Rows[0][1]);
        NumOfLines = Convert.ToInt32(_input.Rows[1][1]);
        NumOfStages = Convert.ToInt32(_input.Rows[2][1]);
        NumOfWorkers = Convert.ToInt32(_input.Rows[3][1]);

        LineStage = new int[NumOfLines][];
        for (int ln = 0; ln < NumOfLines; ln++)
        {
            var tmp = new int[NumOfStages];
            for (int st = 0; st < NumOfStages; st++)
            {
                tmp[st] = Convert.ToInt32(_lineStage.Rows[ln + 1][st + 1]);
            }

            LineStage[ln] = tmp;
        }
        
        WorkerStageAllowance = new int[NumOfStages][];
        for (int st = 0; st < NumOfStages; st++)
        {
            var tmp = new int[NumOfWorkers];
            for (int w = 0; w < NumOfWorkers; w++)
            {
                if (Convert.ToInt32(_workerStageScore.Rows[st + 1][w + 1]) >=
                    Convert.ToInt32(_stageEligibleScore.Rows[st + 1][1]))
                {
                    tmp[w] = 1;
                }
                else
                {
                    tmp[w] = 0;
                }
            }

            WorkerStageAllowance[st] = tmp;
        }

        WorkerShift = new int[NumOfWorkers][];
        for (int w = 0; w < NumOfWorkers; w++)
        {
            var tmp = new int[NumOfShifts];
            for (int sh = 0; sh < NumOfShifts; sh++)
            {
                tmp[sh] = Convert.ToInt32(_workerShift.Rows[w + 1][sh + 1]);
            }

            WorkerShift[w] = tmp;
        }

        WorkerPreassign = new int[NumOfStages][];
        for (int st = 0; st < NumOfStages; st++)
        {
            var tmp = new int[NumOfWorkers];
            for (int w = 0; w < NumOfWorkers; w++)
            {
                tmp[w] = Convert.ToInt32(_workerPreassigned.Rows[st + 1][w + 1]);
            }

            WorkerPreassign[st] = tmp;
        }
        
        WorkerStageExperience = new int[NumOfStages][];
        for (int st = 0; st < NumOfStages; st++)
        {
            var tmp = new int[NumOfWorkers];
            for (int w = 0; w < NumOfWorkers; w++)
            {
                tmp[w] = Convert.ToInt32(_workerStageExperience.Rows[st + 1][w + 1]);
            }

            WorkerStageExperience[st] = tmp;
        }

        WorkerSalary = new int[NumOfWorkers];
        WorkerHealth = new int[NumOfWorkers];
        WorkerAge = new int[NumOfWorkers];
        for (int w = 0; w < NumOfWorkers; w++)
        {
            WorkerSalary[w] = Convert.ToInt32(_workerProfile.Rows[w + 1][2]);
            WorkerHealth[w] = Convert.ToInt32(_workerProfile.Rows[w + 1][3]);
            WorkerAge[w] = Convert.ToInt32(_workerProfile.Rows[w + 1][1]);
        }

        LineShift = new int[NumOfLines][];
        for (int ln = 0; ln < NumOfLines; ln++)
        {
            var tmp = new int[NumOfShifts];
            for (int sh = 0; sh < NumOfShifts; sh++)
            {
                tmp[sh] = Convert.ToInt32(Convert.ToInt32(_lineShift.Rows[ln + 1][sh + 1]));
            }

            LineShift[ln] = tmp;
        }

        StageShift = new int[NumOfShifts][];
        for (int ln = 0; ln < NumOfLines; ln++)
        {
            for (int sh = 0; sh < NumOfShifts; sh++)
            {
                var tmp = new int[NumOfStages];
                if (LineShift[ln][sh] < 0) continue;
                for (int st = 0; st < NumOfStages; st++)
                {
                    if (LineStage[ln][st] < 0) continue;
                    tmp[st] = 1;
                }

                StageShift[sh] = tmp;
            }
        }

        LineMinProductivity = new int[NumOfLines];
        for (int ln = 0; ln < NumOfLines; ln++)
        {
            // LineMinProductivity[ln] = Convert.ToInt32(_lineMinProductivity.Rows[ln + 1][1]);
        }

        CalculateProductivity();
    }

    private void CalculateProductivity()
    {
        WorkerStageProductivityScore = new int[NumOfStages][];
        for (int st = 0; st < NumOfStages; st++)
        {
            var tmp = new int[NumOfWorkers];
            for (int w = 0; w < NumOfWorkers; w++)
            {
                tmp[w] = (int)(Math.Round(
                    0.6 * WorkerStageExperience[st][w] + 0.2 / WorkerAge[w] + 0.2 * WorkerHealth[w], 3) * 1000);
            }

            WorkerStageProductivityScore[st] = tmp;
        }
    }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
    }
}