using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PhNeutralizer
{
    public partial class Form1 : Form
    {        
        private TankHandler _tankHandler;
        private BindingSource _bindingSource;

        
        int rotatorPanelHeight = 20;
        int rotatorPanelMaxWidth = 492;
        int mainTankLevelFullHeight = 377;

        int originalInputStreamLocationX = 0;
        int originalAcidStreamLocationX = 0;
        int originalBaseStreamLocationX = 0;
        int originalOutputStreamLocationX = 0;
        int originalInputOutputBlockerWidth = 0;
        int originalAcidBaseBlockerWidth = 0;
        Image switchButtonOnImage;
        Image switchButtonOffImage;
        bool switchButtonIsOn = false;

        public Form1()
        {
            InitializeComponent();

            var factory = new TankFactory();            
            _tankHandler = new TankHandler(factory.Create());

            outputPipeAutoOpenMainTankLevelTrackBar.Maximum = _tankHandler.MainTankVolume * 10;

            originalInputStreamLocationX = inputStreamPanel.Left;
            originalAcidStreamLocationX = acidStreamPanel.Left;
            originalBaseStreamLocationX = baseStreamPanel.Left;
            originalOutputStreamLocationX = outputPipePanelStream.Left;
            originalAcidBaseBlockerWidth = acidTankPipeBlockPanelLeft.Width;

            rotatorSpeedLabel.Text = rotatorSpeedController.Value.ToString();            

            switch (_tankHandler.OutputPipeValve)
            {
                case OutputPipeValvePosition.AlwaysOpen:
                    outputPipeValveOpenRadioButton.Checked = true;
                    break;
                case OutputPipeValvePosition.AlwaysClosed:
                    outputPipeValveClosedRadioButton.Checked = true;
                    break;
                case OutputPipeValvePosition.Auto:
                    outputPipeValveAutoRadioButton.Checked = true;
                    break;
            }

            _bindingSource = new BindingSource();
            _bindingSource.DataSource = typeof(TankHandler);

            _tankHandler.AcidTankMaxFlowRateExceeded += AcidTankMaxFlowRateExceeded_EventHandler;
            _tankHandler.BaseTankMaxFlowRateExceeded += BaseTankMaxFlowRateExceeded_EventHandler;

            systemTimer.Interval = 100;
            systemTimer.Enabled = false;

            rotatorsTimer.Interval = 30;
            rotatorsTimer.Enabled = false;

            inputFlowTimer.Interval = 1500;
            inputFlowTimer.Enabled = false;

            inputPhTimer.Interval = 2250;
            inputPhTimer.Enabled = false;
        }

        bool testSwitchIsOn = false;

        private void Form1_Load(object sender, EventArgs e)
        {
            //switchButtonOnImage = Image.FromFile(@"c:\Users\kulikluk\Documents\switch_on_3.gif");
            //switchButtonOffImage = Image.FromFile(@"c:\Users\kulikluk\Documents\switch_off_3.gif");

            //mainTankFlowSwitchButton.BackgroundImage = switchButtonOffImage;
            //mainTankFlowSwitchButton.BackgroundImageLayout = ImageLayout.Stretch;
            //mainTankFlowSwitchButton.ImageAlign = ContentAlignment.MiddleRight;
            //mainTankFlowSwitchButton.TextAlign = ContentAlignment.MiddleLeft;
            //mainTankFlowSwitchButton.FlatStyle = FlatStyle.Flat;
            //mainTankFlowSwitchButton.Text = "";

            // main tank bindings
            mainTankVolumeTextBox.DataBindings.Add(nameof(TextBox.Text), _bindingSource, "MainTankVolume_TextBoxMap", false, DataSourceUpdateMode.OnPropertyChanged);
            mainTankCurrentLevelTextBox.DataBindings.Add(nameof(TextBox.Text), _bindingSource, "MainTankLevel_TextBoxMap", false, DataSourceUpdateMode.OnPropertyChanged);
            mainTankPhTextBox.DataBindings.Add(nameof(TextBox.Text), _bindingSource, "MainTankPh_TextBoxMap", false, DataSourceUpdateMode.OnPropertyChanged);
            mainTankPhTrackBar.DataBindings.Add(nameof(TrackBar.Value), _bindingSource, "MainTankPh_ProgressBarMap", false, DataSourceUpdateMode.OnPropertyChanged);
            mainTankLevelPanel.DataBindings.Add(nameof(Height), _bindingSource, "MainTankLevel_PanelHeightMap", false, DataSourceUpdateMode.OnPropertyChanged);
            mainTankLevelPanel.DataBindings.Add(nameof(Top), _bindingSource, "MainTankLevel_PanelTopMap", false, DataSourceUpdateMode.OnPropertyChanged);
            mainTankLevelPanel.DataBindings.Add(nameof(BackColor), _bindingSource, "MainTankLevel_PanelColorMap", false, DataSourceUpdateMode.OnPropertyChanged);
            mainTankVolumeTextBox.DataBindings.Add(nameof(Enabled), _bindingSource, "IsTankAdjustable", false, DataSourceUpdateMode.OnPropertyChanged);
            mainTankCurrentLevelTextBox.DataBindings.Add(nameof(Enabled), _bindingSource, "IsTankAdjustable", false, DataSourceUpdateMode.OnPropertyChanged);
            mainTankPhTextBox.DataBindings.Add(nameof(Enabled), _bindingSource, "IsTankAdjustable", false, DataSourceUpdateMode.OnPropertyChanged);
            mainTankPhTrackBar.DataBindings.Add(nameof(Enabled), _bindingSource, "IsTankAdjustable", false, DataSourceUpdateMode.OnPropertyChanged);

            // input pipe bindings
            inputPipePhTextBox.DataBindings.Add(nameof(TextBox.Text), _bindingSource, "InputPh_TextBoxMap", false, DataSourceUpdateMode.OnPropertyChanged);
            inputPipePhTrackBar.DataBindings.Add(nameof(TrackBar.Value), _bindingSource, "InputPh_ProgressBarMap", false, DataSourceUpdateMode.OnPropertyChanged);            
            inputPipeFlowRateTextBox.DataBindings.Add(nameof(TextBox.Text), _bindingSource, "InputFlowRate_TextBoxMap", false, DataSourceUpdateMode.OnPropertyChanged);
            inputPipeMaxFlowRateTextBox.DataBindings.Add(nameof(TextBox.Text), _bindingSource, "InputMaxFlowRate_TextBoxMap", false, DataSourceUpdateMode.OnPropertyChanged);
            inputPipeMaxFlowRateTextBox.DataBindings.Add(nameof(Enabled), _bindingSource, "IsTankAdjustable", false, DataSourceUpdateMode.OnPropertyChanged);
            inputPipeFlowRateTrackBar.DataBindings.Add(nameof(TrackBar.Value), _bindingSource, "InputFlowRate_ProgressBarMap", false, DataSourceUpdateMode.OnPropertyChanged);
            inputPipeFlowRateTrackBar.DataBindings.Add(nameof(TrackBar.Maximum), _bindingSource, "InputMaxFlowRate_ProgressBarMap", false, DataSourceUpdateMode.OnPropertyChanged);
            inputPipePanel.DataBindings.Add(nameof(BackColor), _bindingSource, "InputPipe_PanelColorMap", false, DataSourceUpdateMode.OnPropertyChanged);
            inputStreamPanel.DataBindings.Add(nameof(BackColor), _bindingSource, "InputPipe_PanelColorMap", false, DataSourceUpdateMode.OnPropertyChanged);

            // output pipe bindings            
            outputPipeMaxFlowRateTextBox.DataBindings.Add(nameof(TextBox.Text), _bindingSource, "OutputPipeMaxFlowRate_TextBoxMap", false, DataSourceUpdateMode.OnPropertyChanged);
            outputPipeMaxFlowRateTextBox.DataBindings.Add(nameof(Enabled), _bindingSource, "IsTankAdjustable", false, DataSourceUpdateMode.OnPropertyChanged);
            outputPipeFlowRateTextBox.DataBindings.Add(nameof(TextBox.Text), _bindingSource, "OutputPipeFlowRate_TextBoxMap", false, DataSourceUpdateMode.OnPropertyChanged);
            outputPipeFlowRateTrackBar.DataBindings.Add(nameof(TrackBar.Value), _bindingSource, "OutputPipeFlowRate_ProgressBarMap", false, DataSourceUpdateMode.OnPropertyChanged);
            outputPipeFlowRateTrackBar.DataBindings.Add(nameof(TrackBar.Maximum), _bindingSource, "OutputPipeFlowRateMaximum_ProgressBarMap", false, DataSourceUpdateMode.OnPropertyChanged);
            outputPipeAutoOpenMainTankLevelTrackBar.DataBindings.Add(nameof(TrackBar.Value), _bindingSource, "OutputPipeOpenMainTankLevel_ProgressBarMap", false, DataSourceUpdateMode.OnPropertyChanged);
            outputPipeAutoOpenMainTankLevelTrackBar.DataBindings.Add(nameof(TrackBar.Maximum), _bindingSource, "OutputPipeOpenMainTankLevelMaximum_ProgressBarMap", false, DataSourceUpdateMode.OnPropertyChanged);
            outputPipeAutoOpenMainTankLevelTrackBar.DataBindings.Add(nameof(TrackBar.Enabled), _bindingSource, "IsTankAdjustable", false, DataSourceUpdateMode.OnPropertyChanged);
            outputPipeAutoOpenMainTankLevelTextBox.DataBindings.Add(nameof(TextBox.Text), _bindingSource, "OutputPipeOpenMainTankLevel_TextBoxMap", false, DataSourceUpdateMode.OnPropertyChanged);
            outputPipeAutoOpenMainTankLevelTextBox.DataBindings.Add(nameof(TextBox.Enabled), _bindingSource, "IsTankAdjustable", false, DataSourceUpdateMode.OnPropertyChanged);
            outputPipeAutoOpenMainTankLevelPercentTextBox.DataBindings.Add(nameof(TextBox.Text), _bindingSource, "OutputPipeOpenMainTankLevelPercent_TextBoxMap", false, DataSourceUpdateMode.OnPropertyChanged);
            outputPipeAutoOpenMainTankLevelPercentTextBox.DataBindings.Add(nameof(TextBox.Enabled), _bindingSource, "IsTankAdjustable", false, DataSourceUpdateMode.OnPropertyChanged);
            outputPipePanelStream.DataBindings.Add(nameof(BackColor), _bindingSource, "OutputPipe_PanelColorMap", false, DataSourceUpdateMode.OnPropertyChanged);

            // acid tank bindings
            acidTankVolumeTextBox.DataBindings.Add(nameof(TextBox.Text), _bindingSource, "AcidTankVolume_TextBoxMap", false, DataSourceUpdateMode.OnPropertyChanged);
            acidTankCurrentLevelTextBox.DataBindings.Add(nameof(TextBox.Text), _bindingSource, "AcidTankLevel_TextBoxMap", false, DataSourceUpdateMode.OnPropertyChanged);
            acidTankPhTextBox.DataBindings.Add(nameof(TextBox.Text), _bindingSource, "AcidTankPh_TextBoxMap", false, DataSourceUpdateMode.OnPropertyChanged);
            acidTankPhTrackBar.DataBindings.Add(nameof(TrackBar.Value), _bindingSource, "AcidTankPh_ProgressBarMap", false, DataSourceUpdateMode.OnPropertyChanged);            
            acidTankFlowRateTextBox.DataBindings.Add(nameof(TextBox.Text), _bindingSource, "AcidTankFlowRate_TextBoxMap", false, DataSourceUpdateMode.OnPropertyChanged);
            acidTankFlowRateTrackBar.DataBindings.Add(nameof(TrackBar.Value), _bindingSource, "AcidTankFlowRate_ProgressBarMap", false, DataSourceUpdateMode.OnPropertyChanged);
            acidTankFlowRateTrackBar.DataBindings.Add(nameof(TrackBar.Maximum), _bindingSource, "AcidTankMaxFlowRate_ProgressBarMap", false, DataSourceUpdateMode.OnPropertyChanged);
            acidTankMaxFlowRateTextBox.DataBindings.Add(nameof(TextBox.Text), _bindingSource, "AcidTankMaxFlowRate_TextBoxMap", false, DataSourceUpdateMode.OnPropertyChanged);
            acidTankMaxFlowRateTextBox.DataBindings.Add(nameof(Enabled), _bindingSource, "IsTankAdjustable", false, DataSourceUpdateMode.OnPropertyChanged);
            acidTankLevelPanel.DataBindings.Add(nameof(Height), _bindingSource, "AcidTankLevel_PanelHeightMap", false, DataSourceUpdateMode.OnPropertyChanged);
            acidTankLevelPanel.DataBindings.Add(nameof(Top), _bindingSource, "AcidTankLevel_PanelTopMap", false, DataSourceUpdateMode.OnPropertyChanged);
            acidTankLevelPanel.DataBindings.Add(nameof(BackColor), _bindingSource, "AcidTankLevel_PanelColorMap", false, DataSourceUpdateMode.OnPropertyChanged);
            acidTankVolumeTextBox.DataBindings.Add(nameof(Enabled), _bindingSource, "IsTankAdjustable", false, DataSourceUpdateMode.OnPropertyChanged);
            acidTankCurrentLevelTextBox.DataBindings.Add(nameof(Enabled), _bindingSource, "IsTankAdjustable", false, DataSourceUpdateMode.OnPropertyChanged);
            acidTankPhTextBox.DataBindings.Add(nameof(Enabled), _bindingSource, "IsTankAdjustable", false, DataSourceUpdateMode.OnPropertyChanged);
            acidTankPhTrackBar.DataBindings.Add(nameof(Enabled), _bindingSource, "IsTankAdjustable", false, DataSourceUpdateMode.OnPropertyChanged);
            acidStreamPanel.DataBindings.Add(nameof(BackColor), _bindingSource, "AcidTankFlowRate_PanelColorMap", false, DataSourceUpdateMode.OnPropertyChanged);
            acidTankAutoRefill.DataBindings.Add(nameof(CheckBox.Checked), _bindingSource, "AcidTankAutoRefill", false, DataSourceUpdateMode.OnPropertyChanged);

            // base tank bindings
            baseTankVolumeTextBox.DataBindings.Add(nameof(TextBox.Text), _bindingSource, "BaseTankVolume_TextBoxMap", false, DataSourceUpdateMode.OnPropertyChanged);
            baseTankCurrentLevelTextBox.DataBindings.Add(nameof(TextBox.Text), _bindingSource, "BaseTankLevel_TextBoxMap", false, DataSourceUpdateMode.OnPropertyChanged);
            baseTankPhTextBox.DataBindings.Add(nameof(TextBox.Text), _bindingSource, "BaseTankPh_TextBoxMap", false, DataSourceUpdateMode.OnPropertyChanged);
            baseTankPhTrackBar.DataBindings.Add(nameof(TrackBar.Value), _bindingSource, "BaseTankPh_ProgressBarMap", false, DataSourceUpdateMode.OnPropertyChanged);
            baseTankFlowRateTextBox.DataBindings.Add(nameof(TextBox.Text), _bindingSource, "BaseTankFlowRate_TextBoxMap", false, DataSourceUpdateMode.OnPropertyChanged);
            baseTankFlowRateTrackBar.DataBindings.Add(nameof(TrackBar.Value), _bindingSource, "BaseTankFlowRate_ProgressBarMap", false, DataSourceUpdateMode.OnPropertyChanged);
            baseTankFlowRateTrackBar.DataBindings.Add(nameof(TrackBar.Maximum), _bindingSource, "BaseTankMaxFlowRate_ProgressBarMap", false, DataSourceUpdateMode.OnPropertyChanged);
            baseTankMaxFlowRateTextBox.DataBindings.Add(nameof(TextBox.Text), _bindingSource, "BaseTankMaxFlowRate_TextBoxMap", false, DataSourceUpdateMode.OnPropertyChanged);
            baseTankMaxFlowRateTextBox.DataBindings.Add(nameof(Enabled), _bindingSource, "IsTankAdjustable", false, DataSourceUpdateMode.OnPropertyChanged);
            baseTankLevelPanel.DataBindings.Add(nameof(Height), _bindingSource, "BaseTankLevel_PanelHeightMap", false, DataSourceUpdateMode.OnPropertyChanged);
            baseTankLevelPanel.DataBindings.Add(nameof(Top), _bindingSource, "BaseTankLevel_PanelTopMap", false, DataSourceUpdateMode.OnPropertyChanged);
            baseTankLevelPanel.DataBindings.Add(nameof(BackColor), _bindingSource, "BaseTankLevel_PanelColorMap", false, DataSourceUpdateMode.OnPropertyChanged);
            baseTankVolumeTextBox.DataBindings.Add(nameof(Enabled), _bindingSource, "IsTankAdjustable", false, DataSourceUpdateMode.OnPropertyChanged);
            baseTankCurrentLevelTextBox.DataBindings.Add(nameof(Enabled), _bindingSource, "IsTankAdjustable", false, DataSourceUpdateMode.OnPropertyChanged);
            baseTankPhTextBox.DataBindings.Add(nameof(Enabled), _bindingSource, "IsTankAdjustable", false, DataSourceUpdateMode.OnPropertyChanged);
            baseTankPhTrackBar.DataBindings.Add(nameof(Enabled), _bindingSource, "IsTankAdjustable", false, DataSourceUpdateMode.OnPropertyChanged);
            baseStreamPanel.DataBindings.Add(nameof(BackColor), _bindingSource, "BaseTankFlowRate_PanelColorMap", false, DataSourceUpdateMode.OnPropertyChanged);
            baseTankAutoRefill.DataBindings.Add(nameof(CheckBox.Checked), _bindingSource, "BaseTankAutoRefill", false, DataSourceUpdateMode.OnPropertyChanged);

            // other bindings
            mainTankFlowSwitchButton.DataBindings.Add(nameof(Text), _bindingSource, "TankIsWorking_ButtonTextMap", false, DataSourceUpdateMode.OnPropertyChanged);
            mainTankFlowSwitchButton.DataBindings.Add(nameof(Enabled), _bindingSource, "CanProcessFlow", false, DataSourceUpdateMode.OnPropertyChanged);
            mainTankFullInLabel.DataBindings.Add(nameof(Text), _bindingSource, "TimeLeftToFullMainTank", false, DataSourceUpdateMode.OnPropertyChanged);
            acidTankEmptyInLabel.DataBindings.Add(nameof(Text), _bindingSource, "TimeLeftToEmptyAcidTank", false, DataSourceUpdateMode.OnPropertyChanged);
            baseTankEmptyInLabel.DataBindings.Add(nameof(Text), _bindingSource, "TimeLeftToEmptyBaseTank", true, DataSourceUpdateMode.OnPropertyChanged);
            totalLiquidInLabel.DataBindings.Add(nameof(Text), _bindingSource, "TotalLiquidLitresIn_LabelMap", true, DataSourceUpdateMode.OnPropertyChanged);
            totalLiquidOutLabel.DataBindings.Add(nameof(Text), _bindingSource, "TotalLiquidLitresOut_LabelMap", true, DataSourceUpdateMode.OnPropertyChanged);
            totalAcidInLabel.DataBindings.Add(nameof(Text), _bindingSource, "TotalAcidLitresOut_LabelMap", true, DataSourceUpdateMode.OnPropertyChanged);
            totalBaseInLabel.DataBindings.Add(nameof(Text), _bindingSource, "TotalBaseLitresOut_LabelMap", true, DataSourceUpdateMode.OnPropertyChanged);
            liquidDeltaLabel.DataBindings.Add(nameof(Text), _bindingSource, "LiquidDelta_LabelMap", true, DataSourceUpdateMode.OnPropertyChanged);

            flowTimeLabel.DataBindings.Add(nameof(Label.ForeColor), _bindingSource, "ControlPanel_ValueLabelColorMap", true, DataSourceUpdateMode.OnPropertyChanged);
            mainTankFullInLabel.DataBindings.Add(nameof(Label.ForeColor), _bindingSource, "ControlPanel_ValueLabelColorMap", true, DataSourceUpdateMode.OnPropertyChanged);
            acidTankEmptyInLabel.DataBindings.Add(nameof(Label.ForeColor), _bindingSource, "ControlPanel_ValueLabelColorMap", true, DataSourceUpdateMode.OnPropertyChanged);
            baseTankEmptyInLabel.DataBindings.Add(nameof(Label.ForeColor), _bindingSource, "ControlPanel_ValueLabelColorMap", true, DataSourceUpdateMode.OnPropertyChanged);
            totalLiquidInLabel.DataBindings.Add(nameof(Label.ForeColor), _bindingSource, "ControlPanel_ValueLabelColorMap", true, DataSourceUpdateMode.OnPropertyChanged);
            totalLiquidOutLabel.DataBindings.Add(nameof(Label.ForeColor), _bindingSource, "ControlPanel_ValueLabelColorMap", true, DataSourceUpdateMode.OnPropertyChanged);
            totalAcidInLabel.DataBindings.Add(nameof(Label.ForeColor), _bindingSource, "ControlPanel_ValueLabelColorMap", true, DataSourceUpdateMode.OnPropertyChanged);
            totalBaseInLabel.DataBindings.Add(nameof(Label.ForeColor), _bindingSource, "ControlPanel_ValueLabelColorMap", true, DataSourceUpdateMode.OnPropertyChanged);
            rotatorSpeedLabel.DataBindings.Add(nameof(Label.ForeColor), _bindingSource, "ControlPanel_ValueLabelColorMap", true, DataSourceUpdateMode.OnPropertyChanged);
            liquidDeltaLabel.DataBindings.Add(nameof(Label.ForeColor), _bindingSource, "ControlPanel_ValueLabelColorMap", true, DataSourceUpdateMode.OnPropertyChanged);

            wtLabel.DataBindings.Add(nameof(Label.ForeColor), _bindingSource, "ControlPanel_TitleLabelColorMap", true, DataSourceUpdateMode.OnPropertyChanged);
            tfiLabel.DataBindings.Add(nameof(Label.ForeColor), _bindingSource, "ControlPanel_TitleLabelColorMap", true, DataSourceUpdateMode.OnPropertyChanged);
            aeiLabel.DataBindings.Add(nameof(Label.ForeColor), _bindingSource, "ControlPanel_TitleLabelColorMap", true, DataSourceUpdateMode.OnPropertyChanged);
            beiLabel.DataBindings.Add(nameof(Label.ForeColor), _bindingSource, "ControlPanel_TitleLabelColorMap", true, DataSourceUpdateMode.OnPropertyChanged);
            liLabel.DataBindings.Add(nameof(Label.ForeColor), _bindingSource, "ControlPanel_TitleLabelColorMap", true, DataSourceUpdateMode.OnPropertyChanged);
            loLabel.DataBindings.Add(nameof(Label.ForeColor), _bindingSource, "ControlPanel_TitleLabelColorMap", true, DataSourceUpdateMode.OnPropertyChanged);
            auLabel.DataBindings.Add(nameof(Label.ForeColor), _bindingSource, "ControlPanel_TitleLabelColorMap", true, DataSourceUpdateMode.OnPropertyChanged);
            buLabel.DataBindings.Add(nameof(Label.ForeColor), _bindingSource, "ControlPanel_TitleLabelColorMap", true, DataSourceUpdateMode.OnPropertyChanged);
            rsLabel.DataBindings.Add(nameof(Label.ForeColor), _bindingSource, "ControlPanel_TitleLabelColorMap", true, DataSourceUpdateMode.OnPropertyChanged);
            ldLabel.DataBindings.Add(nameof(Label.ForeColor), _bindingSource, "ControlPanel_TitleLabelColorMap", true, DataSourceUpdateMode.OnPropertyChanged);

            autoFlowCheckBox.DataBindings.Add(nameof(CheckBox.Checked), _bindingSource, "IsAutoFlow", false, DataSourceUpdateMode.OnPropertyChanged);

            _bindingSource.Add(_tankHandler);

            AdjustValves();

            AdjustRotatorsColor(Math.Abs(currentRotatorStep));


            ControlPanelForm cpf = new ControlPanelForm();
            cpf.Show();
        }

        private void AdjustValves()
        {
            AdjustAcidPipeValve();
            AdjustBasePipeValve();
            AdjustInputPipeValve();
            AdjustOutputPipeValve();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (buttonAction == 1)
            {
                mainTankFlowSwitchButton.BackgroundImage = switchButtonOffImage;
            }
            else if (buttonAction == 0)
            {
                mainTankFlowSwitchButton.BackgroundImage = switchButtonOnImage;
            }

            _tankHandler.IsWorking = !_tankHandler.IsWorking;       
            rotatorsTimer.Enabled = _tankHandler.IsWorking;
            inputFlowTimer.Enabled = _tankHandler.IsWorking;
            inputPhTimer.Enabled = _tankHandler.IsWorking;
            systemTimer.Enabled = _tankHandler.IsWorking;

            AdjustValves();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        int RunningTime = 0;
        private void systemTimer_Tick(object sender, EventArgs e)
        {
            if (!_tankHandler.CanProcessFlow)
            {
                _tankHandler.IsWorking = false;
                rotatorsTimer.Enabled = _tankHandler.IsWorking;
                inputFlowTimer.Enabled = _tankHandler.IsWorking;
                inputPhTimer.Enabled = _tankHandler.IsWorking;
                systemTimer.Enabled = _tankHandler.IsWorking;

                AdjustValves();

                if (_tankHandler.MainTankIsFull)
                {
                    MessageBox.Show("Main tank is full! Input has been closed!");
                }
                if (_tankHandler.AcidTankIsEmpty)
                {
                    MessageBox.Show("Acid tank is empty! Input has been closed!");
                }
                if (_tankHandler.BaseTankIsEmpty)
                {
                    MessageBox.Show("Base tank is empty! Input has been closed!");
                }

                return;
            }

            try
            {
                _tankHandler.UpdateTankLevels((double)systemTimer.Interval / 1000);

                RunningTime += systemTimer.Interval;

                flowTimeLabel.Text = double.IsInfinity(RunningTime) ? $"{RunningTime}" : $"{TimeSpan.FromMilliseconds(RunningTime).ToString(@"dd\.hh\:mm\:ss")}";

                AdjustValves();
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }

        }

        private void inputPipePhTrackBar_Scroll(object sender, EventArgs e)
        {
            return;
            AdjustValves();
        }

        private void inputPipeFlowRateTrackBar_Scroll(object sender, EventArgs e)
        {
            return;
            AdjustValves();
        }
        private void outputPipeFlowRateTrackBar_Scroll(object sender, EventArgs e)
        {
            return;
            AdjustValves();
            if (outputPipeFlowRateTrackBar.Value == 0)
            {
                outputPipeValveClosedRadioButton.Checked = true;
            }
            else
            {
                outputPipeValveOpenRadioButton.Checked = true;
            }
        }





        private void AdjustValve(Panel valve1, Panel valve2, Panel stream, TrackBar bar, TextBox frTextbox, int valveMaxWidth, double flow, bool adjustHeight = true)
        {
            if (!_tankHandler.IsWorking)
            {
                if (valve1.Width != valve2.Width)
                {
                    valve2.Width = valve1.Width;
                }

                if (valve1.Width == valveMaxWidth)
                {
                    stream.Width = (valveMaxWidth * 2) - valve1.Width * 2;
                    stream.Left = valve1.Left + valve1.Width;
                }

                while (valve1.Width < valveMaxWidth)
                {
                    if (valve1.Width != valve2.Width)
                    {
                        valve2.Width = valve1.Width;
                    }

                    valve1.Width++;
                    valve2.Left--;
                    valve2.Width++;

                    stream.Width = (valveMaxWidth * 2) - valve1.Width * 2;
                    stream.Left = valve1.Left + valve1.Width;
                }

                return;
            }

            var newWidth = GetValveWidthByTrackbarValue(bar, valveMaxWidth);
            if (valve1.Width != newWidth && valve2.Width != valve1.Width)
            {
                valve2.Width++;
                valve2.Left--;
            }
            while (valve1.Width != newWidth)
            {
                var delta = valve1.Width < newWidth ? 1 : -1;

                valve1.Width += delta;
                valve2.Left -= delta;
                valve2.Width = valve1.Width;

                stream.Width = (valveMaxWidth * 2) - valve1.Width * 2;
                stream.Left = valve1.Left + valve1.Width;
            }

            if (flow > 0)
            {
                if (stream.Width == 0)
                {
                    stream.Width = 1;
                    valve2.Width--;
                    valve2.Left++;
                }
            }
            else
            {
                if (stream.Width != 0)
                {
                    stream.Width = 0;
                    valve2.Width++;
                    valve2.Left--;
                }
            }
                

            if (adjustHeight && stream.Width > 0)
            {
                stream.Height = GetDistanceToLiquidLevel() - stream.Top;
            }
        }

        private int GetValveWidthByTrackbarValue(TrackBar bar, int valveMaxWidth)
        {
            if (bar.Value == 0)
            {
                return valveMaxWidth;
            }
            else if (bar.Value == bar.Maximum)
            {
                return 0;
            }

            var valuePercent = bar.Value * 100 / bar.Maximum;
            return valveMaxWidth - (valveMaxWidth * valuePercent / 100);
        }
        private int GetDistanceToLiquidLevel()
        {
            return mainTankContainerPanel.Top + mainTankLevelPanel.Top;
        }
        private void AdjustInputPipeValve()
        {
            AdjustValve(inputPipeBlockPanelLeft, inputPipeBlockPanelRight, inputStreamPanel, inputPipeFlowRateTrackBar, inputPipeFlowRateTextBox, 28, _tankHandler.InputFlowRate);
        }

        private void AdjustOutputPipeValve()
        {
            AdjustValve(outputPipeBlockPanelLeft, outputPipeBlockPanelRight, outputPipePanelStream, outputPipeFlowRateTrackBar, outputPipeFlowRateTextBox, 28, _tankHandler.OutputFlowRate, false);
        }

        private void AdjustAcidPipeValve()
        {
            AdjustValve(acidTankPipeBlockPanelLeft, acidTankPipeBlockPanelRight, acidStreamPanel, acidTankFlowRateTrackBar, acidTankFlowRateTextBox, 25, _tankHandler.AcidTankFlowRate);
        }

        private void AdjustBasePipeValve()
        {
            AdjustValve(baseTankPipeBlockPanelLeft, baseTankPipeBlockPanelRight, baseStreamPanel, baseTankFlowRateTrackBar, baseTankFlowRateTextBox, 25, _tankHandler.BaseTankFlowRate);
        }

        private void AcidTankMaxFlowRateExceeded_EventHandler(object sender, EventArgs e)
        {
            //MessageBox.Show("Maximum flow rate of acid tank is too small to perform neutralization! Input pipe flow rate has been auto adjsuted.");
        }
        private void BaseTankMaxFlowRateExceeded_EventHandler(object sender, EventArgs e)
        {
            //MessageBox.Show("Maximum flow rate of base tank is too small to perform neutralization! Input pipe flow rate has been auto adjsuted.");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var f = new ExportTankForm(_tankHandler);
            f.ShowDialog();

            if (f.ExportResult)
            {
                MessageBox.Show("Tank configuration has been successfully exported to file.", "Export success!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            var f = new ImportTankForm(_tankHandler);
            f.ShowDialog();

            if (f.ImportResult)
            {
                MessageBox.Show("Tank configuration has been successfully imported from file.", "Import success!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {

        }

        private void baseTankLevelPanel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void outputPipeValveOpenRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            _tankHandler.OutputPipeValve = OutputPipeValvePosition.AlwaysOpen;
            outputPipeFlowRateTrackBar.Value = outputPipeFlowRateTrackBar.Maximum;
            outputPipeFlowRateTrackBar.Enabled = true;
            AdjustValves();
        }

        private void outputPipeValveClosedRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            _tankHandler.OutputPipeValve = OutputPipeValvePosition.AlwaysClosed;
            outputPipeFlowRateTrackBar.Value = outputPipeFlowRateTrackBar.Minimum;
            outputPipeFlowRateTrackBar.Enabled = true;
            AdjustValves();
        }

        private void outputPipeValveAutoRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            _tankHandler.OutputPipeValve = OutputPipeValvePosition.Auto;
            outputPipeFlowRateTrackBar.Enabled = false;
            AdjustValves();
        }

        private void inputPipePhTrackBar_Enter(object sender, EventArgs e)
        {
            groupBox1.Focus();
        }


        int maxRotatorStep = 60;
        int currentMaxRotatorStep = 30;
        private void rotatorSpeedController_Scroll(object sender, EventArgs e)
        {            
            rotatorSpeedLabel.Text = rotatorSpeedController.Value.ToString();

            if (rotatorSpeedController.Value == 0)
            {
                rotatorsTimer.Enabled = false;                
            }
            else
            {
                if (!rotatorsTimer.Enabled && systemTimer.Enabled)
                {
                    rotatorsTimer.Enabled = true;
                }

                currentMaxRotatorStep = maxRotatorStep / rotatorSpeedController.Value;
            }
        }

        int currentRotatorStep = -30;        
        bool leftRotatorWingInFront = true;
        private void rotatorsTimer_Tick(object sender, EventArgs e)
        {
            if (leftRotatorWingInFront)
            {
                if (++currentRotatorStep > currentMaxRotatorStep)
                {
                    currentRotatorStep = currentMaxRotatorStep - 1;
                    leftRotatorWingInFront = false;
                }
            }
            else
            {
                if (--currentRotatorStep < -currentMaxRotatorStep)
                {
                    currentRotatorStep = -currentMaxRotatorStep + 1;
                    leftRotatorWingInFront = true;
                }
            }

            var currentAbs = Math.Abs(currentRotatorStep);

            // adjust rotators color
            AdjustRotatorsColor(currentAbs);

            // adjust rotators bounds
            AdjustRotatorsBounds(currentAbs);
        }

        private void AdjustRotatorsColor(int currentAbs)
        {
            var maxRgb = 255;
            var avgRgb = maxRgb / 2;
            var rotator1RgbFactor = currentAbs * avgRgb / currentMaxRotatorStep;
            var rotator2RgbFactor = rotator1RgbFactor;
            var currentLiquidColor = mainTankLevelPanel.BackColor;

            if (!leftRotatorWingInFront)
            {
                rotator1RgbFactor = Math.Abs(maxRgb - rotator1RgbFactor);
            }
            else
            {
                rotator2RgbFactor = Math.Abs(maxRgb - rotator2RgbFactor);
            }

            var r1 = (rotator1RgbFactor + currentLiquidColor.R) / 2;
            var g1 = (rotator1RgbFactor + currentLiquidColor.G) / 2;
            var b1 = (rotator1RgbFactor + currentLiquidColor.B) / 2;
            var r2 = (rotator2RgbFactor + currentLiquidColor.R) / 2;
            var g2 = (rotator2RgbFactor + currentLiquidColor.G) / 2;
            var b2 = (rotator2RgbFactor + currentLiquidColor.B) / 2;

            r1 = r1 <= maxRgb ? r1 : maxRgb;
            g1 = g1 <= maxRgb ? g1 : maxRgb;
            b1 = b1 <= maxRgb ? b1 : maxRgb;
            r2 = r2 <= maxRgb ? r2 : maxRgb;
            g2 = g2 <= maxRgb ? g2 : maxRgb;
            b2 = b2 <= maxRgb ? b2 : maxRgb;

            rotatorPanel.BackColor = System.Drawing.Color.FromArgb(r1, g1, b1);
            rotatorPanel2.BackColor = System.Drawing.Color.FromArgb(r2, g2, b2);
        }

        private void AdjustRotatorsBounds(int currentAbs)
        {
            var len = currentAbs * rotatorPanelMaxWidth / currentMaxRotatorStep;
            var yAxisCenter = rotatorAxisPanel.Location.X + rotatorAxisPanel.Width / 2;

            if (currentRotatorStep > 0)
            {
                rotatorPanel.SetBounds(yAxisCenter, rotatorPanel.Location.Y, len, rotatorPanelHeight);
                rotatorPanel2.SetBounds(yAxisCenter - len, rotatorPanel2.Location.Y, len, rotatorPanelHeight);
            }
            else if (currentRotatorStep < 0)
            {
                rotatorPanel.SetBounds(yAxisCenter - len, rotatorPanel.Location.Y, len, rotatorPanelHeight);
                rotatorPanel2.SetBounds(yAxisCenter, rotatorPanel2.Location.Y, len, rotatorPanelHeight);
            }
            else
            {
                rotatorPanel.SetBounds(yAxisCenter, rotatorPanel.Location.Y, 0, rotatorPanelHeight);
                rotatorPanel2.SetBounds(yAxisCenter, rotatorPanel2.Location.Y, 0, rotatorPanelHeight);
            }
        }

        private void inputFlowTimer_Tick(object sender, EventArgs e)
        {
            if (_tankHandler.CanProcessFlow)
            {
                _tankHandler.AutoChangeInputFlowRate();
            }
        }

        private void inputPhTimer_Tick(object sender, EventArgs e)
        {
            if (_tankHandler.CanProcessFlow)
            {
                _tankHandler.AutoChangeInputPh();
            }
        }

        private void groupBox6_Enter(object sender, EventArgs e)
        {

        }


        int buttonAction = 0;
        private void mainTankFlowSwitchButton_MouseDown(object sender, MouseEventArgs e)
        {
            return;
            if (e.Y <= ((Button)sender).Height / 2)
            {
                buttonAction = 1;
            }
            else
            {
                buttonAction = 0;
            }
        }
    }
}
