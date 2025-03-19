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
    public partial class CloseDialogForm: Form
    {
        public CloseDialogForm(string filename)
        {
            InitializeComponent();
            Text = filename;
        }
    }
}
