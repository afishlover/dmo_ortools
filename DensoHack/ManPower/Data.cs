using System.Data;
using System.Text.Json;
using ExcelDataReader;

namespace ManPower;

public class Data
{
    public int NumOfLines { get; set; } = 5;
    public int NumOfStages { get; set; } = 3;
    public int NumOfWorkers { get; set; } = 15;
    public int NumOfEquipments { get; set; }
    public int NumOfFunctions { get; set; }
    public int NumOfDays { get; set; }
    public int NumOfShifts { get; set; }

    public int[][] LineStages { get; set; } // Determine if a line has this stage
    public int[][] WorkerStageAllowance { get; set; } // Determine if a worker can do this stage
    public int[][] WorkerShifts { get; set; } // Determine if a worker can work at this shift
    public int[][] WorkerPreassigns { get; set; } // Determine if a worker is forced to do this stage

    public int[][] StageFunctions { get; set; } // Determine what functions a stage need

    private readonly DataTable _input = ExcelDataContext.GetInstance().Sheets["InputData"];
    private readonly DataTable _lineStage = ExcelDataContext.GetInstance().Sheets["LineStage"];
    private readonly DataTable _workerStageAllowance = ExcelDataContext.GetInstance().Sheets["WorkerStageAllowance"];
    private readonly DataTable _workerShift = ExcelDataContext.GetInstance().Sheets["WorkerShift"];
    private readonly DataTable _workerPreassigned = ExcelDataContext.GetInstance().Sheets["WorkerPreassigned"];
    
    public void Populate()
    {
        #region Numeric
        NumOfDays = Convert.ToInt32(_input.Rows[0][1]);
        NumOfShifts = Convert.ToInt32(_input.Rows[1][1]);
        NumOfLines = Convert.ToInt32(_input.Rows[2][1]);
        NumOfStages = Convert.ToInt32(_input.Rows[3][1]);
        NumOfWorkers = Convert.ToInt32(_input.Rows[4][1]);
        NumOfEquipments = Convert.ToInt32(_input.Rows[5][1]);
        NumOfFunctions = Convert.ToInt32(_input.Rows[6][1]);
        #endregion

        LineStages = new int[NumOfLines][];
        for (int ln = 0; ln < NumOfLines; ln++)
        {
            var tmp = new int[NumOfStages];
            for (int st = 0; st < NumOfStages; st++)
            {
                tmp[st] = Convert.ToInt32(_lineStage.Rows[ln + 1][st + 1]);
            }

            LineStages[ln] = tmp;
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

        WorkerShifts = new int[NumOfWorkers][];
        for (int w = 0; w < NumOfWorkers; w++)
        {
            var tmp = new int[NumOfShifts];
            for (int sh = 0; sh < NumOfShifts; sh++)
            {
                tmp[sh] = Convert.ToInt32(_workerShift.Rows[w + 1][sh + 1]);
            }

            WorkerShifts[w] = tmp;
        }

        WorkerPreassigns = new int[NumOfStages][];
        for (int st = 0; st < NumOfStages; st++)
        {
            var tmp = new int[NumOfWorkers];
            for (int w = 0; w < NumOfWorkers; w++)
            {
                tmp[w] = Convert.ToInt32(_workerPreassigned.Rows[st + 1][w + 1]);
            }

            WorkerPreassigns[st] = tmp;
        }
    }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
    }
}