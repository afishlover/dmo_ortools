using System.Data;
using ExcelDataReader;

namespace ManPower;

public class ExcelDataContext
{
    // creating an object of ExcelDataContext
    private static ExcelDataContext _instance = new ExcelDataContext();
    // no instantiated available
    private ExcelDataContext()
    {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        FileStream stream = File.Open(@"C:\Users\Trung\Documents\GitHub\dmo_ortools\DensoHack\ManPower\InputSample.xlsx", FileMode.Open, FileAccess.Read);
        IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);

        DataSet result = excelReader.AsDataSet(new ExcelDataSetConfiguration()
        {
            ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
            {
            }
        });

        this.Sheets = result .Tables;
    }

    // accessing to ExcelDataContext singleton
    public static ExcelDataContext GetInstance()
    {
        return _instance;
    }

    // the dataset of Excel
    public DataTableCollection Sheets { get; private set; }
}