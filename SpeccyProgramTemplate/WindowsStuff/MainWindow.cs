using SpeccyEngine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpeccyProgramTemplate
{
    public partial class MainWindow : Form
    {
        Speccy eng;
        public MainWindow()
        {
            InitializeComponent();

            eng = new Speccy(32, 24, 4, this);

            eng.NextScene += (ee, o) =>
            {
                eng.SetScene(null);
            };

            eng.SetScene(new SpeccyProgram());

            eng.Start();
        }
    }
}
