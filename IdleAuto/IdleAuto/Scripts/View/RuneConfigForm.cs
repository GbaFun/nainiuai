using CefSharp.DevTools.FedCm;
using IdleAuto.Db;
using IdleAuto.Scripts.Wrap;
using System;
using System.Collections;
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
    public partial class RuneConfigForm : Form
    {
        private List<RuneCompandData> Runes;

        public RuneConfigForm()
        {
            InitializeComponent();
            InitializeRuneCfgItems();
        }

        private void InitializeRuneCfgItems()
        {
            var runeDb = FreeDb.Sqlite.Select<RuneCompandData>();
            Runes = runeDb.ToList();
            if (Runes == null || Runes.Count == 0)
            {
                Runes = RuneCompandCfg.Instance.RuneCompandData;
                FreeDb.Sqlite.Insert<RuneCompandData>(Runes).ExecuteAffrows();
            }

            Runes.Sort((a, b) =>
            {
                return a.ID.CompareTo(b.ID);
            });
            for (int i = 0; i < Runes.Count; i++)
            {
                var item = new RuneCfgItem();
                this.ListPanel.Controls.Add(item);
                item.SetData(Runes[i], OnRuneItemValueChanged);
            }
        }

        private void OnRuneItemValueChanged(int id, int value)
        {
            Runes.Find((rune) =>
            {
                return rune.ID == id;
            }).CompandNum = value;
        }

        private void BtnCancle_Click(object sender, EventArgs e)
        {
            ClearItems();
            this.Close();
        }

        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            StartUpgradeRune();
        }

        private async Task StartUpgradeRune()
        {
            var runs = FreeDb.Sqlite.Update<RuneCompandData>().SetSource(Runes).ExecuteAffrows();
            ClearItems();
            await Task.Delay(500);
            this.Close();

         
            BroWindow window = await TabManager.Instance.TriggerAddBroToTap(AccountController.Instance.User);
            RuneController controller = new RuneController(window);
            await controller.AutoUpgradeRune(window, AccountController.Instance.User);

            var result = MessageBox.Show("合符文成功，是否需要关闭当前页面", "提示", MessageBoxButtons.OKCancel);
            if (result == DialogResult.OK)
            {
                window.Close();
            }
        }

        private void ClearItems()
        {
            for (int i = 0; i < ListPanel.Controls.Count; i++)
            {
                ListPanel.Controls[i].Dispose();
            }
            this.ListPanel.Controls.Clear();
        }
    }
}
