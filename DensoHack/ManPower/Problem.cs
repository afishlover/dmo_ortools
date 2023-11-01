using Google.OrTools.Sat;

namespace ManPower;

public class Problem
{
    private CpModel _cpModel;
    private CpSolver _cpSolver;
    private Data _data;

    private Dictionary<(int, int, int), BoolVar>
        _assignWorker = new Dictionary<(int, int, int), BoolVar>(); // assign a specific worker to a line

    public Problem(Data data)
    {
        _cpModel = new CpModel();
        _cpSolver = new CpSolver();
        _data = data;
        Setup();
    }

    public void Setup()
    {
        _cpSolver.StringParameters =
            $"num_workers:{Environment.ProcessorCount} "
            + $"enumerate_all_solutions:{false} "
            + $"log_search_progress:{true} "
            + $"cp_model_presolve:{true} "
            + $"max_time_in_seconds:200 "
            + $"subsolvers:\"no_lp\" "
            + $"linearization_level:0";
    }

    public void VariableDefinition()
    {
        // 1. Determine in a stage/shift/ which worker do
        for (int sh = 0; sh < _data.NumOfShifts; sh++)
        {
            for (int st = 0; st < _data.NumOfStages; st++)
            {
                for (int w = 0; w < _data.NumOfWorkers; w++)
                {
                    if (_data.WorkerStageAllowance[st][w] > 0 && _data.WorkerShifts[w][sh] > 0)
                    {
                        _assignWorker[(sh, st, w)] = _cpModel.NewBoolVar($"A[{sh}|{st}|{w}]");
                    }
                }
            }
        }
    }

    public void ConstraintDefinition()
    {
        // 1. Each stage need exactly one worker for every shift
        for (int sh = 0; sh < _data.NumOfShifts; sh++)
        {
            for (int st = 0; st < _data.NumOfStages; st++)
            {
                var literals = new List<ILiteral>();

                for (int w = 0; w < _data.NumOfWorkers; w++)
                {
                    if (_data.WorkerStageAllowance[st][w] > 0 && _data.WorkerShifts[w][sh] > 0)
                    {
                        literals.Add(_assignWorker[(sh, st, w)]);
                    }
                }

                _cpModel.AddExactlyOne(literals);
            }
        }

        // 2. Worker can only work at one stage every shift
        for (int sh = 0; sh < _data.NumOfShifts; sh++)
        {
            for (int st = 0; st < _data.NumOfStages; st++)
            {
                var literals = new List<ILiteral>();
                for (int w = 0; w < _data.NumOfWorkers; w++)
                {
                    if (_data.WorkerStageAllowance[st][w] > 0 && _data.WorkerShifts[w][sh] > 0)
                    {
                        literals.Add(_assignWorker[(sh, st, w)]);
                    }
                }

                _cpModel.AddExactlyOne(literals);
            }
        }

        // 3. Worker can only work at most one shift every day (every 3 shift)
        for (int st = 0; st < _data.NumOfStages; st++)
        {
            int sh = 0;
            while (sh < _data.NumOfShifts)
            {
                if (sh + 2 < _data.NumOfShifts)
                {
                    for (int temp = sh; temp <= sh + 2; temp++)
                    {
                        var literals = new List<ILiteral>();

                        for (int w = 0; w < _data.NumOfWorkers; w++)
                        {
                            if (_data.WorkerStageAllowance[st][w] > 0)
                            {
                                if (_data.WorkerShifts[w][sh] > 0)
                                {
                                    literals.Add(_assignWorker[(sh, st, w)]);
                                }
                            }
                        }

                        _cpModel.AddExactlyOne(literals);
                    }
                }
                else if (sh + 1 < _data.NumOfShifts)
                {
                    for (int temp = sh; temp <= sh + 1; temp++)
                    {
                        var literals = new List<ILiteral>();

                        for (int w = 0; w < _data.NumOfWorkers; w++)
                        {
                            if (_data.WorkerStageAllowance[st][w] > 0)
                            {
                                if (_data.WorkerShifts[w][sh] > 0)
                                {
                                    literals.Add(_assignWorker[(sh, st, w)]);
                                }
                            }
                        }

                        _cpModel.AddExactlyOne(literals);
                    }
                }
                else
                {
                    for (int temp = sh; temp <= sh + 2; temp++)
                    {
                        var literals = new List<ILiteral>();

                        for (int w = 0; w < _data.NumOfWorkers; w++)
                        {
                            if (_data.WorkerStageAllowance[st][w] > 0)
                            {
                                if (_data.WorkerShifts[w][sh] > 0)
                                {
                                    literals.Add(_assignWorker[(sh, st, w)]);
                                }
                            }
                        }

                        _cpModel.AddExactlyOne(literals);
                    }
                }

                sh += 3;
            }
        }
        
        // 4. Worker pre-assign
        for (int sh = 0; sh < _data.NumOfShifts; sh++)
        {
            for (int st = 0; st < _data.NumOfStages; st++)
            {
                for (int w = 0; w < _data.NumOfWorkers; w++)
                {
                    if (_data.WorkerStageAllowance[st][w] > 0 && _data.WorkerShifts[w][sh] > 0)
                    {
                        if (_data.WorkerPreassigns[st][w] == 1)
                        {
                            _cpModel.Add(_assignWorker[(sh, st, w)] == 1);
                        }

                        if (_data.WorkerPreassigns[st][w] == -1)
                        {
                            _cpModel.AddHint(_assignWorker[(sh, st, w)], 0);
                        }

                        if (_data.WorkerPreassigns[st][w] == -2)
                        {
                            _cpModel.Add(_assignWorker[(sh, st, w)] == 0);
                        }
                    }
                }
            }
        }
    }

    public void ObjectiveDefinition()
    {
        // 1. For every worker works on the same line and the same shift, their productivity levels should be close to each others
    }
    
    public void Solve()
    {
        _cpSolver.Solve(_cpModel);
    }
}