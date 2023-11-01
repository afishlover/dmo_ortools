using Google.OrTools.Sat;

namespace ManPower;

public class Problem
{
    #region Ortools

    private CpModel _cpModel;
    private CpSolver _cpSolver;
    private Data _data;

    // Unknowns
    private readonly Dictionary<(int, int, int), BoolVar> _assignWorker; // assign a specific worker to a stage
    private readonly Dictionary<(int, int, int), BoolVar> _assignEquipment; // assign a specific equipment to a line

    // Objectives
    private List<LinearExpr> _totalDiversity;

    #endregion

    public Problem(Data data)
    {
        _assignWorker = new();
        _assignEquipment = new();
        _totalDiversity = new List<LinearExpr>();
        _cpSolver = new CpSolver();
        _cpModel = new CpModel();
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
                    if (_data.WorkerStageAllowance[st][w] > 0 && _data.WorkerShift[w][sh] > 0)
                    {
                        _assignWorker[(sh, st, w)] = _cpModel.NewBoolVar($"A[{sh}|{st}|{w}]");
                    }
                }
            }
        }

        // 2. Determine in a line/shift/ which equipment stay
        for (int sh = 0; sh < _data.NumOfShifts; sh++)
        {
            for (int ln = 0; ln < _data.NumOfLines; ln++)
            {
                for (int eq = 0; eq < _data.NumOfEquipments; eq++)
                {
                    _assignEquipment[(sh, ln, eq)] = _cpModel.NewBoolVar($"E[{sh}|{ln}|{eq}]");
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
                    if (_data.WorkerStageAllowance[st][w] > 0 && _data.WorkerShift[w][sh] > 0)
                    {
                        literals.Add(_assignWorker[(sh, st, w)]);
                    }
                }

                _cpModel.AddExactlyOne(literals);
            }
        }

        // 2. Worker can only work at one stage every shift
        for (int w = 0; w < _data.NumOfWorkers; w++)
        {
            for (int sh = 0; sh < _data.NumOfShifts; sh++)
            {
                for (int st = 0; st < _data.NumOfStages - 1; st++)
                {
                    for (int ost = st + 1; ost < _data.NumOfStages; ost++)
                    {
                        if (_data.WorkerStageAllowance[st][w] > 0 && _data.WorkerStageAllowance[ost][w] > 0 &&
                            _data.WorkerShift[w][sh] > 0)
                        {
                            _cpModel.AddAtMostOne(new[] { _assignWorker[(sh, st, w)], _assignWorker[(sh, ost, w)] });
                        }
                    }
                }
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
                                if (_data.WorkerShift[w][sh] > 0)
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
                                if (_data.WorkerShift[w][sh] > 0)
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
                                if (_data.WorkerShift[w][sh] > 0)
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
                    if (_data.WorkerStageAllowance[st][w] > 0 && _data.WorkerShift[w][sh] > 0)
                    {
                        if (_data.WorkerPreassign[st][w] == 1)
                        {
                            // _cpModel.Add(_assignWorker[(sh, st, w)] == 1);
                        }

                        if (_data.WorkerPreassign[st][w] == 1)
                        {
                            // _cpModel.AddHint(_assignWorker[(sh, st, w)], 1);
                        }

                        if (_data.WorkerPreassign[st][w] == -1)
                        {
                            // _cpModel.AddHint(_assignWorker[(sh, st, w)], 0);
                        }

                        if (_data.WorkerPreassign[st][w] == -2)
                        {
                            // _cpModel.Add(_assignWorker[(sh, st, w)] == 0);
                        }
                    }
                }
            }
        }

        // 5. Each line need exactly a number of team lead

        // 6. Equipment can only stay at most one shift in every line
        for (int eq = 0; eq < _data.NumOfEquipments; eq++)
        {
            for (int sh = 0; sh < _data.NumOfShifts; sh++)
            {
                for (int ln = 0; ln < _data.NumOfLines - 1; ln++)
                {
                    for (int oln = ln + 1; oln < _data.NumOfLines; oln++)
                    {
                        _cpModel.AddAtMostOne(new[]
                            { _assignEquipment[(sh, ln, eq)], _assignEquipment[(sh, oln, eq)] });
                    }
                }
            }
        }

        // 7. Total equipment assigned to a line must fulfill its requirements
        for (int sh = 0; sh < _data.NumOfShifts; sh++)
        {
            for (int ln = 0; ln < _data.NumOfLines; ln++)
            {
                for (int fu = 0; fu < _data.NumOfFunctions; fu++)
                {
                    var literals = new List<BoolVar>();
                    var coefficients = new List<int>();
                    for (int eq = 0; eq < _data.NumOfEquipments; eq++)
                    {
                        literals.Add(_assignEquipment[(sh, ln, eq)]);
                        coefficients.Add(_data.EquipmentFunction[eq][fu]);
                    }

                    _cpModel.Add(
                        LinearExpr.WeightedSum(literals, coefficients) >= _data.LineFunctionRequirement[ln][fu]);
                }
            }
        }

        // 8. Productivity of every line must greater equal than the minimum requirement
    }

    public void ObjectiveDefinition()
    {
        // 1. Minimize the productivity gaps between members in same line/shift

        // 2. Minimize the chance of team shuffle
        for (int w = 0; w < _data.NumOfWorkers; w++)
        {
            var literals = new List<ILiteral>();
            for (int sh = 0; sh < _data.NumOfShifts; sh++)
            {
                for (int st = 0; st < _data.NumOfStages; st++)
                {
                    if (_data.WorkerStageAllowance[st][w] > 0 && _data.WorkerShift[w][sh] > 0)
                    {
                        literals.Add(_assignWorker[(sh, st, w)]);
                    }
                }
            }

            _totalDiversity.Add(LinearExpr.Sum(literals));
        }

        // 3. Maximize the productivity of a line

        // Casting LinearExpr
        var totalDiversity = _cpModel.NewIntVar(0, int.MaxValue, "TotalDiversity");
        _cpModel.Add(LinearExpr.Sum(_totalDiversity) == totalDiversity);

        // Weighted Sum
        _cpModel.Minimize(totalDiversity);
    }

    public void GetJSONResult()
    {
    }

    public void Solve()
    {
        var status = _cpSolver.Solve(_cpModel);
        if (status is CpSolverStatus.Feasible or CpSolverStatus.Optimal)
        {
            for (int sh = 0; sh < _data.NumOfShifts; sh++)
            {
                for (int st = 0; st < _data.NumOfStages; st++)
                {
                    for (int w = 0; w < _data.NumOfWorkers; w++)
                    {
                        if (_data.WorkerStageAllowance[st][w] > 0 && _data.WorkerShift[w][sh] > 0 &&
                            _cpSolver.Value(_assignWorker[(sh, st, w)]) == 1L)
                        {
                            Console.WriteLine($"Worker {w} is assigned to stage {st} at shift {sh}");
                        }
                    }
                }
            }
        }
    }
}