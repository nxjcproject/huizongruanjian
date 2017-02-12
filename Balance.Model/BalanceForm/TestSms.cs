using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BalanceForm
{
    public partial class TestSms : Form
    {
        private Balance.Service.SmsSend.Realtime_SmsSend SmsSend;                              //发送短信
        public TestSms()
        {
            InitializeComponent();
            SmsSend = new Balance.Service.SmsSend.Realtime_SmsSend();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Balance.Service.SmsSend.Function_SmsSender m_SmsSend = new Balance.Service.SmsSend.Function_SmsSender();
            //m_SmsSend.SendSms();
            GetDailyTzBalance();
        }
        public DataTable GetDailyTzBalance()
        {
            string organizationId = "zc_nxjc_qtx_tys";
                Balance.Infrastructure.BasicDate.SingleBasicData singleBasicData = Balance.Infrastructure.BasicDate.SingleBasicData.Creat();
                singleBasicData.Init(organizationId, textBox1.Text );
                //Balance.Infrastructure.BasicDate.SingleTimeService singleTimeService = Balance.Infrastructure.BasicDate.SingleTimeService.Creat();
                //每天都重新初始化
                DataTable result = singleBasicData.TzBalance.Clone();
                DataRow tzRow = result.NewRow();
                tzRow["BalanceId"] = singleBasicData.KeyId;
                tzRow["BalanceName"] = "自动平衡";
                tzRow["OrganizationID"] = singleBasicData.OrganizationId;
                tzRow["DataTableName"] = "";
                tzRow["StaticsCycle"] = "day";
                tzRow["TimeStamp"] = singleBasicData.Date;
                tzRow["BalanceStatus"] = 1;
                //tzRow=ShiftAndWorkingTeamService.AddRelationshipToTzbalance(singleBasicData.Date,singleBasicData.OrganizationId,ConnectionStringFactory.NXJCConnectionString,tzRow);
                Balance.Model.Schedule.WorkScheduleRule.AddRelationshipToTzbalance(tzRow);//排班
                result.Rows.Add(tzRow);
                MessageBox.Show(tzRow["FirstWorkingTeam"].ToString() + "||" + tzRow["SecondWorkingTeam"].ToString() + "||" + tzRow["ThirdWorkingTeam"].ToString());
                return result;
           
        }
    }
}
