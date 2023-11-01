using System.Data;
using System.Text.Json;
using ExcelDataReader;

namespace ManPower;

public sealed class Data
{
    public int[] Weights = new[] { 10, -20, 10, 10, 5 };
    public int NumOfLines { get; set; }
    public int NumOfStages { get; set; }
    public int NumOfWorkers { get; set; }
    public int NumOfEquipments { get; set; }
    public int NumOfFunctions { get; set; }
    public int NumOfShifts { get; set; }

    public int[][] LineShift { get; set; } // Determine if a line work in this shift
    public int[][] LineStage { get; set; } // Determine if a line has this stage
    public int[][] WorkerStageAllowance { get; set; } // Determine if a worker can do this stage
    public int[][] WorkerLineAllowance { get; set; } // Determine if a worker can do this line
    public int[][] WorkerShift { get; set; } // Determine if a worker can work at this shift
    public int[][] WorkerPreassign { get; set; } // Determine if a worker is forced to do this stage
    public int[][] WorkerStageExperience { get; set; } // Determine worker stage experience
    public int[] WorkerAge { get; set; } // Determine worker age
    public int[] WorkerHealth { get; set; } // Determine worker health
    public int[] WorkerSalary { get; set; } // Determine worker shift salary
    public int[] EquipmentProductivityScore { get; set; } // Determine productivity score of an equipment

    public int[][]
        WorkerStageProductivityScore { get; set; } // Determine worker productivity with regard to every stage

    public int[][] LineFunctionRequirement { get; set; } // Determine what functions a stage need
    public int[][] EquipmentFunction { get; set; } // Determine what functions an equipment has

    #region DataTable

    private readonly DataTable _input = ExcelDataContext.GetInstance().Sheets["InputData"]!;
    private readonly DataTable _lineStage = ExcelDataContext.GetInstance().Sheets["LineStage"]!;
    private readonly DataTable _workerStageAllowance = ExcelDataContext.GetInstance().Sheets["WorkerStageAllowance"]!;
    private readonly DataTable _workerProfile = ExcelDataContext.GetInstance().Sheets["WorkerProfile"]!;
    private readonly DataTable _workerStageExperience = ExcelDataContext.GetInstance().Sheets["WorkerStageExperience"]!;
    private readonly DataTable _workerShift = ExcelDataContext.GetInstance().Sheets["WorkerShift"]!;
    private readonly DataTable _workerPreassigned = ExcelDataContext.GetInstance().Sheets["WorkerPreassigned"]!;
    private readonly DataTable _equipmentFunction = ExcelDataContext.GetInstance().Sheets["EquipmentFunction"]!;

    private readonly DataTable _equipmentProductivityScore =
        ExcelDataContext.GetInstance().Sheets["EquipmentProductivityScore"]!;

    private readonly DataTable _lineFunctionRequirement =
        ExcelDataContext.GetInstance().Sheets["LineFunctionRequirement"]!;

    #endregion

    public void Populate()
    {
        NumOfShifts = Convert.ToInt32(_input.Rows[0][1]);
        NumOfLines = Convert.ToInt32(_input.Rows[1][1]);
        NumOfStages = Convert.ToInt32(_input.Rows[2][1]);
        NumOfWorkers = Convert.ToInt32(_input.Rows[3][1]);
        NumOfEquipments = Convert.ToInt32(_input.Rows[4][1]);
        NumOfFunctions = Convert.ToInt32(_input.Rows[5][1]);

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
                tmp[w] = Convert.ToInt32(_workerStageAllowance.Rows[st + 1][w + 1]);
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

        EquipmentFunction = new int[NumOfEquipments][];
        for (int eq = 0; eq < NumOfEquipments; eq++)
        {
            var tmp = new int[NumOfFunctions];
            for (int fu = 0; fu < NumOfFunctions; fu++)
            {
                tmp[fu] = Convert.ToInt32(_equipmentFunction.Rows[eq + 1][fu + 1]);
            }

            EquipmentFunction[eq] = tmp;
        }

        LineFunctionRequirement = new int[NumOfLines][];
        for (int ln = 0; ln < NumOfLines; ln++)
        {
            var tmp = new int[NumOfFunctions];
            for (int fu = 0; fu < NumOfFunctions; fu++)
            {
                tmp[fu] = Convert.ToInt32(_lineFunctionRequirement.Rows[ln + 1][fu + 1]);
            }

            LineFunctionRequirement[ln] = tmp;
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

        EquipmentProductivityScore = new int[NumOfEquipments];
        for (int eq = 0; eq < NumOfEquipments; eq++)
        {
            EquipmentProductivityScore[eq] = Convert.ToInt32(_equipmentProductivityScore.Rows[eq + 1][1]);
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

        CalculateProductivity();
        GetWorkerLineAllowance();
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

    private void GetWorkerLineAllowance()
    {
        WorkerLineAllowance = new int[NumOfLines][];
        for (int ln = 0; ln < NumOfLines; ln++)
        {
            var tmp = new int[NumOfWorkers];
            for (int w = 0; w < NumOfWorkers; w++)
            {
                var flag = true;
                for (int st = 0; st < NumOfStages; st++)
                {
                    if (LineStage[ln][st] > 0)
                    {
                        if (WorkerStageAllowance[st][w] < 0)
                        {
                            flag = false;
                        }
                    }
                }

                if (flag)
                {
                    tmp[w] = 1;
                }
            }

            WorkerLineAllowance[ln] = tmp;
        }
    }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
    }
}