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
    public partial class RuneConfigForm : Form
    {
        public RuneConfigForm()
        {
            InitializeComponent();
            InitializeRuneCfgItems();
        }

        private void InitializeRuneCfgItems()
        {
            var runes = RuneCompandCfg.Instance.RuneCompandData;
            runes.Sort((a, b) =>
            {
                return a.ID.CompareTo(b.ID);
            });
            for (int i = 0; i < runes.Count; i++)
            {
                var item = new RuneCfgItem();
                this.Controls.Add(item);
                item.SetData(runes[i], OnRuneItemValueChanged);
            }
        }

        private void OnRuneItemValueChanged(int id, int value)
        {
            RuneCompandCfg.Instance.RuneCompandData.Find((rune) =>
            {
                return rune.ID == id;
            }).CompandNum = value;
        }

        private void BtnCancle_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < ListPanel.Controls.Count; i++)
            {
                ListPanel.Controls[i].Dispose();
            }
            this.ListPanel.Controls.Clear();
            this.Close();
        }

        private void BtnConfirm_Click(object sender, EventArgs e)
        {

        }
    }
}
