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
    private readonly Dictionary<(int, int), BoolVar> _assignEquipment; // assign a specific equipment to a line

    // Objectives
    private List<LinearExpr> _totalDiversity;
    private List<LinearExpr> _totalGaps;
    private List<LinearExpr> _totalSalary;
    private List<LinearExpr> _totalChance;
    private List<LinearExpr> _totalProductivity;

    #endregion

    public Problem(Data data)
    {
        _assignWorker = new();
        _assignEquipment = new();
        _totalDiversity = new List<LinearExpr>();
        _totalGaps = new List<LinearExpr>();
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

        for (int ln = 0; ln < _data.NumOfLines; ln++)
        {
            for (int eq = 0; eq < _data.NumOfEquipments; eq++)
            {
                _assignEquipment[(ln, eq)] = _cpModel.NewBoolVar($"E[{ln}|{eq}]");
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

        // 5. Equipment can only stay at one line
        for (int ln = 0; ln < _data.NumOfLines - 1; ln++)
        {
            for (int oln = ln + 1; oln < _data.NumOfLines; oln++)
            {
                for (int eq = 0; eq < _data.NumOfEquipments; eq++)
                {
                    _cpModel.AddAtMostOne(new[] { _assignEquipment[(ln, eq)], _assignEquipment[(oln, eq)] });
                }
            }
        }

        // 6. Total equipment assigned to a line must fulfill its requirements
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
                        literals.Add(_assignEquipment[(ln, eq)]);
                        coefficients.Add(_data.EquipmentFunction[eq][fu]);
                    }

                    if (_data.LineFunctionRequirement[ln][fu] > 0)
                    {
                        _cpModel.Add(
                            LinearExpr.WeightedSum(literals, coefficients) >= _data.LineFunctionRequirement[ln][fu]);
                    }
                }
            }
        }

        // 7. Productivity of every line must greater equal than the minimum requirement for every day (each 3 shift)
        // Consider treat it as an objective -> minimize the gaps between the must and the wanted
    }

    public void ObjectiveDefinition()
    {
        // 1. Minimize the productivity gaps between members in same line/shift
        if (_data.Activate[0] > 0)
        {
            for (int sh = 0; sh < _data.NumOfShifts; sh++)
            {
                for (int ln = 0; ln < _data.NumOfLines; ln++)
                {
                    for (int st = 0; st < _data.NumOfStages; st++)
                    {
                        if (_data.LineStage[ln][st] > 0)
                        {
                            for (int w = 0; w < _data.NumOfWorkers - 1; w++)
                            {
                                if (_data.WorkerStageAllowance[st][w] > 0 && _data.WorkerShift[w][sh] > 0)
                                {
                                    for (int ow = w + 1; ow < _data.NumOfWorkers; ow++)
                                    {
                                        if (_data.WorkerStageAllowance[st][ow] > 0 && _data.WorkerShift[ow][sh] > 0)
                                        {
                                            var tmp = _cpModel.NewIntVar(-100, 100, $"tmp[{sh}|{ln}|{st}|{w}|{ow}]");
                                            var abTmp = _cpModel.NewIntVar(0, 100, $"abTmp[{sh}|{ln}|{st}|{w}|{ow}]");
                                            _cpModel.Add(tmp == _data.WorkerStageProductivityScore[st][w] -
                                                _data.WorkerStageProductivityScore[st][ow]).OnlyEnforceIf(
                                                new ILiteral[]
                                                    { _assignWorker[(sh, st, w)], _assignWorker[(sh, st, ow)] });
                                            _cpModel.AddAbsEquality(abTmp, tmp);
                                            _totalGaps.Add(tmp);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        // 2. Minimize the chance of team shuffle
        if (_data.Activate[1] > 0)
        {
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
        }

        // 3. Maximize the productivity of a line
        if (_data.Activate[2] > 0)
        {
            for (int ln = 0; ln < _data.NumOfLines; ln++)
            {
                var dailyProductivity = new List<LinearExpr>();
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
            }
        }

        // 4. Minimize the money used to hire worker
        if (_data.Activate[3] > 0)
        {
        }

        // 5. Maximize the chance of being assigned for new worker
        if (_data.Activate[4] > 0)
        {
        }

        // Casting LinearExpr
        var totalDiversity = _cpModel.NewIntVar(0, int.MaxValue, "TotalDiversity");
        _cpModel.Add(LinearExpr.Sum(_totalDiversity) == totalDiversity);

        var totalGaps = _cpModel.NewIntVar(0, int.MaxValue, "TotalGaps");
        _cpModel.Add(LinearExpr.Sum(_totalGaps) == totalGaps);

        // Weighted Sum
        _cpModel.Minimize(totalGaps);
    }

    public void GetJSONResult()
    {
        var result = new ResultModel
        {
            NumOfShifts = _data.NumOfShifts,
            NumOfLines = _data.NumOfLines,
            NumOfStages = _data.NumOfStages,
            NumOfWorkers = _data.NumOfWorkers,
            NumOfEquipments = _data.NumOfEquipments,
            NumOfFunctions = _data.NumOfFunctions
        };
    }

    public void Solve()
    {
        var status = _cpSolver.Solve(_cpModel);
        if (status is CpSolverStatus.Feasible or CpSolverStatus.Optimal)
        {
            // for (int sh = 0; sh < _data.NumOfShifts; sh++)
            // {
            //     for (int st = 0; st < _data.NumOfStages; st++)
            //     {
            //         for (int w = 0; w < _data.NumOfWorkers; w++)
            //         {
            //             if (_data.WorkerStageAllowance[st][w] > 0 && _data.WorkerShift[w][sh] > 0 &&
            //                 _cpSolver.Value(_assignWorker[(sh, st, w)]) == 1L)
            //             {
            //                 Console.WriteLine($"Worker {w} is assigned to stage {st} at shift {sh}");
            //             }
            //         }
            //     }
            // }


            for (int ln = 0; ln < _data.NumOfLines; ln++)
            {
                for (int eq = 0; eq < _data.NumOfEquipments; eq++)
                {
                    if (_cpSolver.Value(_assignEquipment[(ln, eq)]) == 1L)
                    {
                        Console.WriteLine($"Equipment {eq} is assigned to line {ln}");
                    }
                }
            }
        }
    }
}