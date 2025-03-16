using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MDIPaint
{
    public partial class CanvasSizeForm: Form
    {
        public int CanvasWidth { get; private set; }
        public int CanvasHeight { get; private set; }

        public CanvasSizeForm()
        {
            InitializeComponent();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            CanvasWidth = int.Parse(textBoxWidth.Text);
            CanvasHeight = int.Parse(textBoxHeight.Text);
        }
    }
}
