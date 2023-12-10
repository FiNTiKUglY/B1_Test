using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Spire.Xls;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace DesktopApplication
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            button1.Click += button1_Click;
            openFileDialog1.Filter = "Excel files(*.xls)|*.xls|All files(*.*)|*.*";
        }

        private async Task ReadFile(string filename)
        {
            using (ApplicationDBContext db = new ApplicationDBContext())
            {
                await db.Database.ExecuteSqlRawAsync($"DELETE FROM circulation");
                await db.Database.ExecuteSqlRawAsync($"DELETE FROM input_balance");
                await db.Database.ExecuteSqlRawAsync($"DELETE FROM output_balance");
                var wb = new Workbook();
                wb.LoadFromFile(filename);
                var sheet = wb.Worksheets[0];
                var locatedRange = sheet.AllocatedRange;
                for (int i = 9; i <= locatedRange.Rows.Length; i++)
                {
                    double temp;
                    if (Double.TryParse(locatedRange[i, 1].Value, out temp) == true && locatedRange[i, 1].Value.Length == 4)
                    {
                        await db.Database.ExecuteSqlRawAsync($"INSERT INTO input_balance (\"account\", \"active\", \"passive\", \"file_name\") " +
                           $"VALUES ('{locatedRange[i, 1].Value.Replace(',', '.')}', '{locatedRange[i, 2].Value.Replace(',', '.')}', '{locatedRange[i, 3].Value.Replace(',', '.')}', '{filename}')");

                        await db.Database.ExecuteSqlRawAsync($"INSERT INTO circulation (\"account\", \"debit\", \"credit\", \"file_name\") " +
                           $"VALUES ('{locatedRange[i, 1].Value.Replace(',', '.')}', '{locatedRange[i, 4].Value.Replace(',', '.')}', '{locatedRange[i, 5].Value.Replace(',', '.')}', '{filename}')");

                        await db.Database.ExecuteSqlRawAsync($"INSERT INTO output_balance (\"account\", \"active\", \"passive\", \"file_name\") " +
                           $"VALUES ('{locatedRange[i, 1].Value.Replace(',', '.')}', '{locatedRange[i, 6].Value.Replace(',', '.')}', '{locatedRange[i, 7].Value.Replace(',', '.')}', '{filename}')");
                    }
                }
            }
        }
        private void DrawTable()
        {
            dataGridView1.Columns.Add("Id", "Id");
            dataGridView1.Columns.Add("Active input", "Active input");
            dataGridView1.Columns.Add("Passive input", "Passive input");
            dataGridView1.Columns.Add("Debit circulation", "Debit circulation");
            dataGridView1.Columns.Add("Credit circulation", "Credit circulation");
            dataGridView1.Columns.Add("Active output", "Active output");
            dataGridView1.Columns.Add("Passive output", "Passive output");
            dataGridView1.RowHeadersVisible = false;

            using (ApplicationDBContext db = new ApplicationDBContext())
            {
                var valuesInput = db.Database.SqlQueryRaw<InputBalance>("SELECT * FROM input_balance").ToList();
                var valuesCirculation = db.Database.SqlQueryRaw<Circulation>("SELECT * FROM circulation").ToList();
                var valuesOutput = db.Database.SqlQueryRaw<InputBalance>("SELECT * FROM output_balance").ToList();

                for (var i = 0; i < valuesInput.Count; i++)
                {
                    if (i > 0 && valuesInput[i].Account.ToString()[1] != valuesInput[i - 1].Account.ToString()[1])
                    {
                        var aggIndex = valuesInput[i - 1].Account.ToString().Remove(2, 2);
                        dataGridView1.Rows.Add(aggIndex, valuesInput.Where(v => v.Account.ToString().StartsWith(aggIndex)).Sum(v => v.Active),
                                                 valuesInput.Where(v => v.Account.ToString().StartsWith(aggIndex)).Sum(v => v.Passive),
                                                 valuesCirculation.Where(v => v.Account.ToString().StartsWith(aggIndex)).Sum(v => v.Debit),
                                                 valuesCirculation.Where(v => v.Account.ToString().StartsWith(aggIndex)).Sum(v => v.Credit),
                                                 valuesOutput.Where(v => v.Account.ToString().StartsWith(aggIndex)).Sum(v => v.Active),
                                                 valuesOutput.Where(v => v.Account.ToString().StartsWith(aggIndex)).Sum(v => v.Passive));
                    }
                    if (i > 0 && valuesInput[i].Account.ToString()[0] != valuesInput[i - 1].Account.ToString()[0])
                    {
                        var aggIndex = valuesInput[i - 1].Account.ToString().Remove(1, 3);
                        dataGridView1.Rows.Add("ÏÎ ÊËÀÑÑÓ", valuesInput.Where(v => v.Account.ToString().StartsWith(aggIndex)).Sum(v => v.Active),
                                                 valuesInput.Where(v => v.Account.ToString().StartsWith(aggIndex)).Sum(v => v.Passive),
                                                 valuesCirculation.Where(v => v.Account.ToString().StartsWith(aggIndex)).Sum(v => v.Debit),
                                                 valuesCirculation.Where(v => v.Account.ToString().StartsWith(aggIndex)).Sum(v => v.Credit),
                                                 valuesOutput.Where(v => v.Account.ToString().StartsWith(aggIndex)).Sum(v => v.Active),
                                                 valuesOutput.Where(v => v.Account.ToString().StartsWith(aggIndex)).Sum(v => v.Passive));
                    }
                    dataGridView1.Rows.Add(valuesInput[i].Account, valuesInput[i].Active, valuesInput[i].Passive,
                        valuesCirculation[i].Debit, valuesCirculation[i].Credit, valuesOutput[i].Active, valuesOutput[i].Passive);
                }
                var index = valuesInput[^1].Account.ToString().Remove(2, 2);
                dataGridView1.Rows.Add(index, valuesInput.Where(v => v.Account.ToString().StartsWith(index)).Sum(v => v.Active),
                                         valuesInput.Where(v => v.Account.ToString().StartsWith(index)).Sum(v => v.Passive),
                                         valuesCirculation.Where(v => v.Account.ToString().StartsWith(index)).Sum(v => v.Debit),
                                         valuesCirculation.Where(v => v.Account.ToString().StartsWith(index)).Sum(v => v.Credit),
                                         valuesOutput.Where(v => v.Account.ToString().StartsWith(index)).Sum(v => v.Active),
                                         valuesOutput.Where(v => v.Account.ToString().StartsWith(index)).Sum(v => v.Passive));
                dataGridView1.Rows.Add("ÏÎ ÊËÀÑÑÓ", valuesInput.Where(v => v.Account.ToString().StartsWith(index[0])).Sum(v => v.Active),
                                                 valuesInput.Where(v => v.Account.ToString().StartsWith(index[0])).Sum(v => v.Passive),
                                                 valuesCirculation.Where(v => v.Account.ToString().StartsWith(index[0])).Sum(v => v.Debit),
                                                 valuesCirculation.Where(v => v.Account.ToString().StartsWith(index[0])).Sum(v => v.Credit),
                                                 valuesOutput.Where(v => v.Account.ToString().StartsWith(index[0])).Sum(v => v.Active),
                                                 valuesOutput.Where(v => v.Account.ToString().StartsWith(index[0])).Sum(v => v.Passive));
                dataGridView1.Rows.Add("ÁÀËÀÍÑ", valuesInput.Sum(v => v.Active),
                                                 valuesInput.Sum(v => v.Passive),
                                                 valuesCirculation.Sum(v => v.Debit),
                                                 valuesCirculation.Sum(v => v.Credit),
                                                 valuesOutput.Sum(v => v.Active),
                                                 valuesOutput.Sum(v => v.Passive));
            }
        }

        async void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
                return;
            string filename = openFileDialog1.FileName;
            await ReadFile(filename);
            DrawTable();
        }
    }
}
