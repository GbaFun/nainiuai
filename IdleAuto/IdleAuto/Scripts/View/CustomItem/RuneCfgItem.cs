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
    public partial class RuneCfgItem : UserControl
    {
        private RuneCompandData Data;
        private Action<int, int> _onValueChanged;

        public RuneCfgItem()
        {
            InitializeComponent();
        }

        public void SetData(RuneCompandData cfg, Action<int, int> OnValueChanged)
        {
            Data = cfg;
            _onValueChanged = OnValueChanged;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (Data != null)
            {
                RuneName.Text = Data.ID.ToString();
                CountInput.Value = Data.CompandNum;
            }
        }
        protected override void OnHandleDestroyed(EventArgs e)
        {
            base.OnHandleDestroyed(e);
            Data = null;
            _onValueChanged = null;
        }

        private void CountInput_ValueChanged(object sender, EventArgs e)
        {
            _onValueChanged?.Invoke(Data.ID, (int)CountInput.Value);
        }


    }
}
