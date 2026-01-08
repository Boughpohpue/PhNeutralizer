using System;
using System.Windows.Forms;

namespace PhNeutralizer
{
    public partial class ExportTankForm : Form
    {
        private TankHandler _tankHandler;
        private string _exportFilePath;
        public bool ExportResult = false;

        public ExportTankForm(TankHandler tankHandler)
        {
            InitializeComponent();

            _tankHandler = tankHandler;

            saveFileDialog1.Filter = "Tank config file|*.tnk";
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                _exportFilePath = saveFileDialog1.FileName;
                filePathTextBox.Text = _exportFilePath;
            }   
        }

        private void exportButtonClick(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_exportFilePath))
            {
                try
                {
                    _tankHandler.ExportTank(saveFileDialog1.FileName);
                    ExportResult = true;
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Provided file path is not correct!", "Incorrect file path!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Provided file path is not correct!", "Incorrect file path!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
