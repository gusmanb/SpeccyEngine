using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SpeccyEngine;
using System.Drawing.Text;

namespace CampoMinado
{
    public partial class Pantalla : Form
    {
        Speccy eng;

        public Pantalla()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            eng = new Speccy(32, 24, 4, this);

            eng.NextScene += (ee, o) =>
            {
                EndLevelInfo info = ee as EndLevelInfo;

                var newScene = new FieldScene(info.NextScreen, info.Score);
                eng.SetScene(newScene);
            };

            eng.SetScene(new IntroScene()); ///FieldScene(0));

            eng.Start();
        }
    }
}
