using Google.OrTools.Sat;
using Server.Models;

namespace Server.Utils
{
    public class SAT
    {
        private CpModel _cpModel;
        private CpSolver _cpSolver;
        private Dictionary<(int, int, int), BoolVar> assignWorker;
        private DataModel _dataModel;

        public SAT(DataModel dataModel)
        {
            _cpModel = new CpModel();
            _cpSolver = new CpSolver();
            assignWorker = new Dictionary<(int, int, int), BoolVar>();
            _dataModel = dataModel;
        }

        public void Setup()
        {

        }


    }
}
