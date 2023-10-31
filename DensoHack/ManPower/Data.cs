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

    public int[][] StageFunctions { get; set; } // Determine what functions a stage need

    private readonly DataTable _input = ExcelDataContext.GetInstance().Sheets["InputData"];
    private readonly DataTable _lineStage = ExcelDataContext.GetInstance().Sheets["LineStage"];
    private readonly DataTable _workerStageAllowance = ExcelDataContext.GetInstance().Sheets["WorkerStageAllowance"];
    
    public void Populate()
    {
        NumOfDays = Convert.ToInt32(_input.Rows[0][1]);
        NumOfShifts = Convert.ToInt32(_input.Rows[1][1]);
        NumOfLines = Convert.ToInt32(_input.Rows[2][1]);
        NumOfStages = Convert.ToInt32(_input.Rows[3][1]);
        NumOfWorkers = Convert.ToInt32(_input.Rows[4][1]);
        NumOfEquipments = Convert.ToInt32(_input.Rows[5][1]);
        NumOfFunctions = Convert.ToInt32(_input.Rows[6][1]);

        LineStages = new int[NumOfLines][];
        for (int i = 0; i < NumOfLines; i++)
        {
            var tmp = new int[NumOfStages];
            for (int j = 0; j < NumOfStages; j++)
            {
                tmp[j] = Convert.ToInt32(_lineStage.Rows[i + 1][j + 1]);
            }

            LineStages[i] = tmp;
        }

        WorkerStageAllowance = new int[NumOfStages][];
        for (int i = 0; i < NumOfStages; i++)
        {
            var tmp = new int[NumOfWorkers];
            for (int j = 0; j < NumOfWorkers; j++)
            {
                tmp[j] = Convert.ToInt32(_workerStageAllowance.Rows[i + 1][j + 1]);
            }

            WorkerStageAllowance[i] = tmp;
        }
    }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
    }
}