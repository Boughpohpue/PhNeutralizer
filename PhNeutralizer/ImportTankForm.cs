using System;
using System.Windows.Forms;

namespace PhNeutralizer
{
    public partial class ImportTankForm : Form
    {
        private TankHandler _tankHandler;
        private string _importFilePath;
        public bool ImportResult = false;

        public ImportTankForm(TankHandler tankHandler)
        {
            InitializeComponent();

            _tankHandler = tankHandler;

            openFileDialog1.Filter = "Tank config file|*.tnk";
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                _importFilePath = openFileDialog1.FileName;
                filePathTextBox.Text = _importFilePath;
            }
        }

        private void importButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_importFilePath))
            {
                try
                {
                    _tankHandler.ImportTank(_importFilePath);
                    ImportResult = true;
                    Close();
                }
                catch (FormatException fex)
                {
                    MessageBox.Show(fex.Message, "Incorrect file format!", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
