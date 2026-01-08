using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PhNeutralizer
{
    public partial class ControlPanelForm : Form
    {
        public ControlPanelForm()
        {
            InitializeComponent();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                this.Capture = false;
                Message msg = Message.Create(this.Handle, 0XA1, new IntPtr(2), IntPtr.Zero);
                this.WndProc(ref msg);
            }
        }

        private void ControlPanelForm_Load(object sender, EventArgs e)
        {

        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {
            var colors = MappingService.GetPhColors();
            panel3.Width = colors.Count * 20;

            using (var g = e.Graphics)
            {
                var rectWidth = 20;
                var rectHeight = 50;
                var sepWidth = 5;

                var lastX = 0;
                var lastY = 0;
                var brushBlack = new SolidBrush(Color.Black);
                for (var x = 0; x < colors.Count; x++)
                {

                    using (Brush brush = new SolidBrush(colors[x]))    // specify color here and brush type here
                    {
                        g.FillRectangle(brush, lastX, lastY, rectWidth, rectHeight);
                        lastX += rectWidth;
                    }

                    //using (Brush brush = new SolidBrush(Color.Black))    // specify color here and brush type here
                    //{
                    //    g.FillRectangle(brush, lastX, lastY, sepWidth, rectHeight);
                    //    lastX += sepWidth;
                    //}
                }
            }
        }
    }
}
