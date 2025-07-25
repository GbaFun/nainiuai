using IdleAuto.Db;
using IdleAuto.Scripts.Controller;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IdleAuto.Scripts.View
{
    public partial class SecondMenuForm : Form
    {
        public SecondMenuForm()
        {
            InitializeComponent();
        }

     


        private void btnCollectDelAll_Click(object sender, EventArgs e)
        {
            Task.Run(async () =>
            {
                try
                {

                    FreeDb.Sqlite.Delete<EquipModel>().Where(p => 1 == 1).ExecuteAffrows();
                    await FlowController.GroupWork(3, 1, RepairManager.Instance.UpdateEquips);



                }
                catch (Exception ex)
                {
                    throw ex;
                }
            });
        }

        private void btnCollectNotDelAll_Click(object sender, EventArgs e)
        {
            Task.Run(async () =>
            {
                try
                {

                    FreeDb.Sqlite.Delete<EquipModel>().Where(p => p.RoleID == 0).ExecuteAffrows();
                    await FlowController.GroupWork(3, 1, RepairManager.Instance.UpdateEquips);

                }
                catch (Exception ex)
                {
                    throw ex;
                }
            });
        }
    }
}
