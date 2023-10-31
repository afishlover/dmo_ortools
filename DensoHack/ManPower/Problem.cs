using Google.OrTools.Sat;

namespace ManPower;

public class Problem
{
    private CpModel _cpModel;
    private CpSolver _cpSolver;
    private Data _data;

    private Dictionary<(int, int), BoolVar> assignWorker; // assign a specific worker to a line
    private Dictionary<(int, int), BoolVar> assignEquipment;

    public Problem(Data data)
    {
        _cpModel = new CpModel();
        _cpSolver = new CpSolver();
        assignWorker = new Dictionary<(int, int), BoolVar>();
        assignEquipment = new Dictionary<(int, int), BoolVar>();
        _data = data;
    }

    public void Setup()
    {
        _cpSolver.StringParameters +=
            $"num_workers:{Environment.ProcessorCount};"
            + $"enumerate_all_solutions:{false};"
            + $"log_search_progress:{true};"
            + $"cp_model_presolve:{true};"
            + $"max_time_in_seconds:{200};"
            + $"subsolvers:\"no_lp\";"
            + $"linearization_level:0;";
    }

    public void VariableDefinition()
    {
        for (int i = 0; i < _data.NumOfLines; i++)
        {
            for (int j = 0; j < _data.NumOfWorkers; j++)
            {
                assignWorker[(i, j)] = _cpModel.NewBoolVar($"A[({i}, {j})]");
            }
        }
    }

    public void ConstraintDefinition()
    {
        for (int i = 0; i < _data.NumOfLines; i++)
        {
            for (int j = 0; j < _data.NumOfShifts; j++)
            {
                var literals = new List<LinearExpr>();

                for (int l = 0; l < _data.NumOfStages; l++)
                {
                    if (_data.LineStages[i][j] < 0) continue;
                    for (int k = 0; k < _data.NumOfWorkers; k++)
                    {
                        literals.Add(assignWorker[(i, k)] * _data.WorkerStageAllowance[j][k]);
                    }
                    _cpModel.Add(LinearExpr.Sum(literals) == _data.LineStages[i][l]);
                }
            }
        }
    }

    public void Solve()
    {
        _cpSolver.Solve(_cpModel);
    }
}