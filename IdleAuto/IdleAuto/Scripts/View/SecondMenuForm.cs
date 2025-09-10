using IdleAuto.Configs.CfgExtension;
using IdleAuto.Db;
using IdleAuto.Scripts.Controller;
using IdleAuto.Scripts.Model;
using IdleAuto.Scripts.Utils;
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
            LoadData();
        }
        /// <summary>
        /// N乔丹数量
        /// </summary>
        public TextBox TxtJordan => txtJordan;
        /// <summary>
        /// 改造珠宝id
        /// </summary>
        public TextBox TxtJewelryId => txtJewelryId;

        /// <summary>
        /// 发送装备条件
        /// </summary>
        public TextBox TxtSendEqCon => txtSendEqCon;

        /// <summary>
        /// 发送装备数量
        /// </summary>
        public TextBox TxtSendEqNum => txtSendEqNum;

        /// <summary>
        /// 发送对象
        /// </summary>
        public TextBox TxtRoleToSend => txtRoleToSend;


        private Dictionary<string, ArtifactBaseConfig> ArtifactData => ArtifactBaseCfg.Instance.Data.ToDictionary(p => p.Key.ToString(), p => p.Value);

        private Dictionary<string, Equipment> EquipEmData => EmEquipCfg.Instance.Data.ToDictionary(p => p.Key.ToString(), p => p.Value);
        public string GetSelectedMethod()
        {
            var key = this.comArtifact.SelectedItem.ToString();
            return ArtifactData[key].Method;
        }

        public Equipment GetSelectedEquipmentConfig()
        {
            var key = this.comJewelryType.SelectedItem.ToString();
            return EquipEmData[key];
        }

        public void LoadData()
        {
            var dic = ArtifactData;
            comArtifact.DataSource = dic.Select(p => p.Key).ToList();
            comJewelryType.DataSource = EquipEmData.Select(p => p.Key).ToList();
        }




        private void btnCollectDelAll_Click(object sender, EventArgs e)
        {
            Task.Run(async () =>
            {
                try
                {

                    FreeDb.Sqlite.Delete<EquipModel>().Where(p => 1 == 1).ExecuteAffrows();
                    await FlowController.GroupWork(3, 1, RepairManager.Instance.UpdateEquips, RepairManager.ActiveAcc);



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

        private void btnRollMinshen_Click(object sender, EventArgs e)
        {
            FlowController.ReformMinshen();
        }

        private void btnBoss_Click(object sender, EventArgs e)
        {
            FlowController.ThrowJordan();
        }

        private void btnArtifact_Click(object sender, EventArgs e)
        {
            string methodName = MenuInstance.SecondForm.GetSelectedMethod();
            ReflectUtil.Invoke(typeof(FlowController), methodName);
        }

        private void btnSwitchJustice_Click(object sender, EventArgs e)
        {
            FlowController.SwitchFrostAndJustice();
        }

        private void btnRollJewelry_Click(object sender, EventArgs e)
        {
            FlowController.RollJewelry();
        }

        private void btnSendEq_Click(object sender, EventArgs e)
        {
            FlowController.SendEquip();
        }

        private void btnReformJustice_Click(object sender, EventArgs e)
        {
            FlowController.ReformBaseJustice();
        }

        private void btnLunhuiBase_Click(object sender, EventArgs e)
        {
            var list = FreeDb.Sqlite.Select<LunhuiBase>().Where(p => p.RoleID == 0).ToList();
            var toList = list.ToObject<List<EquipModel>>();
            FlowController.SendEquip(toList);
        }
    }
}
