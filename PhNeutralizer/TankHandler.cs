using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;

namespace PhNeutralizer
{
    public class TankHandler : INotifyPropertyChanged
    {
        private MainTank _tank;
        private bool _isWorking;
        private bool _isAutoFlow = true;

        private int _mainTankLevelPanelHeight = 377;
        private int _neutralizerTanklevelPanelHeight = 119;

        public event EventHandler<EventArgs> AcidTankMaxFlowRateExceeded;
        public event EventHandler<EventArgs> BaseTankMaxFlowRateExceeded;

        public double TotalLiquidLitresIn = 0.0;
        public double TotalLiquidLitresOut = 0.0;
        public double TotalAcidLitresOut = 0.0;
        public double TotalBaseLitresOut = 0.0;

        private Random _random = new Random();


        public event PropertyChangedEventHandler PropertyChanged;

        public TankHandler(MainTank tank)
        {
            _tank = tank;            
            AdjustNeutralizers();
        }

        private void OnAcidTankMaxFlowRateExceeded(EventArgs e)
        {
            if (AcidTankMaxFlowRateExceeded != null)
                AcidTankMaxFlowRateExceeded(this, e);
        }

        private void OnBaseTankMaxFlowRateExceeded(EventArgs e)
        {
            if (BaseTankMaxFlowRateExceeded != null)
                BaseTankMaxFlowRateExceeded(this, e);
        }


        public bool IsWorking
        {
            get { return _isWorking; }
            set 
            { 
                _isWorking = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("TankIsWorking_ButtonTextMap"));
                }
            }
        }
        public bool IsTankAdjustable
        {
            get { return !IsWorking; }
        }

        public bool IsAutoFlow
        {
            get
            {
                return _isAutoFlow;
            }
            set
            {
                _isAutoFlow = value;

            }
        }

        public bool CanProcessFlow
        {
            get
            {
                return !MainTankIsFull && !(AcidTankIsEmpty && _tank.InputPipe.Ph > _tank.Ph) && !(BaseTankIsEmpty && _tank.InputPipe.Ph < _tank.Ph);
            }
        }

        public int MainTankVolume
        {
            get
            {
                return (int)_tank.Volume;
            }
        }

        public bool AcidTankAutoRefill
        {
            get
            {
                return _tank.AcidTank.AutoRefill;
            }
            set
            {
                _tank.AcidTank.AutoRefill = value;
            }
        }
        public bool BaseTankAutoRefill
        {
            get
            {
                return _tank.BaseTank.AutoRefill;
            }
            set
            {
                _tank.BaseTank.AutoRefill = value;
            }
        }

        public double InputFlowRate
        {
            get
            {
                return _tank.InputPipe.FlowRate;
            }
        }
        public double AcidTankFlowRate
        {
            get
            {
                return _tank.AcidTank.FlowRate;
            }
        }
        public double BaseTankFlowRate
        {
            get
            {
                return _tank.BaseTank.FlowRate;
            }
        }
        public double OutputFlowRate
        {
            get
            {
                return _tank.OutputPipe.FlowRate;
            }
        }

        public void AutoChangeInputFlowRate()
        {
            if (!canAutoUpdateFlow || !IsAutoFlow)
            {
                return;
            }

            // random change
            var change = (double)_random.Next(1, 10) / 10;
            // multiply by random +/-
            change *= _random.Next(0, 100) % 2 == 0 ? 1 : -1;

            // while change would exceed volume, decrease
            while (_tank.InputPipe.FlowRate + change > _tank.InputPipe.MaxFlowRate)
            {
                change = -(Math.Abs(change) + 0.1);
            }
            // if change would decrease 0, increase
            while (_tank.InputPipe.FlowRate + change < 0)
            {
                change = Math.Abs(change) + 0.1;
            }

            // set new flowrate
            _tank.InputPipe.FlowRate += change;

            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("InputFlowRate_ProgressBarMap"));
            }
        }
        public void AutoChangeInputPh()
        {
            if (!IsAutoFlow)
            {
                return;
            }

            var currentPh = _tank.InputPipe.Ph;

            // random change
            var change = (double)_random.Next(1, 10) / 10;
            change *= _random.Next(0, 100) % 2 == 0 ? 1 : -1;

            while (currentPh + change < 3)
            {
                change = (Math.Abs(change) + 0.1);
            }
            while (currentPh + change > 13)
            {
                change = -(Math.Abs(change) + 0.1);
            }

            _tank.InputPipe.Ph += change;

            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("InputPh_ProgressBarMap"));
            }
        }

        public void AdjustNeutralizers()
        {
            if (_tank.InputPipe.Ph == _tank.Ph)
            {
                _tank.AcidTank.FlowRate = 0.0;
                _tank.BaseTank.FlowRate = 0.0;
            }
            else if (_tank.InputPipe.Ph < _tank.Ph)
            {
                var newFlowRate = GetNeutralizerTankFlowRate(_tank.BaseTank);

                if (newFlowRate > _tank.BaseTank.MaxFlowRate)
                {
                    OnBaseTankMaxFlowRateExceeded(new EventArgs());
                    while (newFlowRate > _tank.BaseTank.MaxFlowRate)
                    {
                        _tank.InputPipe.FlowRate -= 0.1;
                        newFlowRate = GetNeutralizerTankFlowRate(_tank.BaseTank);
                    }
                }

                _tank.BaseTank.FlowRate = newFlowRate;
                _tank.AcidTank.FlowRate = 0.0;
            }
            else
            {
                var newFlowRate = GetNeutralizerTankFlowRate(_tank.AcidTank);

                if (newFlowRate > _tank.AcidTank.MaxFlowRate)
                {
                    OnAcidTankMaxFlowRateExceeded(new EventArgs());
                    while (newFlowRate > _tank.AcidTank.MaxFlowRate)
                    {
                        _tank.InputPipe.FlowRate -= 0.1;
                        newFlowRate = GetNeutralizerTankFlowRate(_tank.AcidTank);
                    }
                }

                _tank.AcidTank.FlowRate = newFlowRate;
                _tank.BaseTank.FlowRate = 0.0;
            }

            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("BaseTankFlowRate_ProgressBarMap"));
            }
        }

        private double GetNeutralizerTankFlowRate(NeutralizerTank tank)
        {
            return _tank.InputPipe.FlowRate * (_tank.InputPipe.Ph - _tank.Ph) / (_tank.Ph - tank.Ph);
        }

        public double GetCurrentOutputFlowRate()
        {
            //if (_tank.OutputPipe.ValvePosition == OutputPipeValvePosition.AlwaysOpen || (_tank.OutputPipe.ValvePosition == OutputPipeValvePosition.Auto && _tank.OutputPipeAutoOpenTankLevel <= _tank.CurrentLevel))
            //{
            if (_tank.OutputPipe.ValvePosition != OutputPipeValvePosition.AlwaysClosed)
            {
                return _tank.OutputPipe.FlowRate;
            }
            //}

            return 0.0;
        }

        bool canAutoUpdateFlow = true;
        public void UpdateTankLevels(double multiplier)
        {
            var liquidIn = _tank.InputPipe.FlowRate * multiplier;
            var acidIn = _tank.AcidTank.FlowRate * multiplier;
            var baseIn = _tank.BaseTank.FlowRate * multiplier;
            var liquidOut = GetCurrentOutputFlowRate() * multiplier;

            var delta = liquidIn + acidIn + baseIn - liquidOut;


            TotalLiquidLitresIn += liquidIn;
            TotalAcidLitresOut += acidIn;
            TotalBaseLitresOut += baseIn;            
            TotalLiquidLitresOut += liquidOut;

            _tank.AcidTank.CurrentLevel -= acidIn;
            _tank.BaseTank.CurrentLevel -= baseIn;
            _tank.CurrentLevel += delta;

            if (_tank.AcidTank.AutoRefill && _tank.AcidTank.CurrentLevel <= _tank.AcidTank.AutoRefillLevel)
            {
                _tank.AcidTank.CurrentLevel = _tank.AcidTank.Volume;
            }
            if (_tank.BaseTank.AutoRefill && _tank.BaseTank.CurrentLevel <= _tank.BaseTank.AutoRefillLevel)
            {
                _tank.BaseTank.CurrentLevel = _tank.BaseTank.Volume;
            }


            if (_tank.CurrentLevel < 0)
            {
                _tank.CurrentLevel = 0;
            }
            if (_tank.CurrentLevel > _tank.Volume)
            {
                _tank.CurrentLevel = _tank.Volume;
            }
            if (_tank.AcidTank.CurrentLevel < 0)
            {
                _tank.AcidTank.CurrentLevel = 0;
            }
            if (_tank.BaseTank.CurrentLevel < 0)
            {
                _tank.BaseTank.CurrentLevel = 0;
            }

            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("MainTankLevel_TextBoxMap"));
                PropertyChanged(this, new PropertyChangedEventArgs("AcidTankFlowRate_TextBoxMap"));
                PropertyChanged(this, new PropertyChangedEventArgs("BaseTankFlowRate_TextBoxMap"));
            }


            // environment homeostasis
            var nextCyclesToTest = 1000;
            if (IsAutoFlow)
            {
                if (_tank.CurrentLevel + (delta * nextCyclesToTest) > _tank.Volume)
                {
                    canAutoUpdateFlow = false;

                    var overflowHandled = false;



                    // increase output flow
                    if (_tank.OutputPipe.ValvePosition != OutputPipeValvePosition.AlwaysClosed)
                    {
                        var outputFlowDelta = 0.1;
                        while (_tank.OutputPipe.FlowRate + outputFlowDelta <= _tank.OutputPipe.MaxFlowRate)
                        {
                            var testLiquidOut = (_tank.OutputPipe.FlowRate + outputFlowDelta) * multiplier;
                            delta = liquidIn + acidIn + baseIn - testLiquidOut;

                            if (_tank.CurrentLevel + (delta * nextCyclesToTest) < _tank.Volume)
                            {
                                _tank.OutputPipe.FlowRate += outputFlowDelta;
                                overflowHandled = true;
                                break;
                            }

                            outputFlowDelta += 0.1;
                        }
                    }
                    // decrease input flow
                    if (!overflowHandled)
                    {
                        var inputFlowDelta = 0.1;
                        while (_tank.InputPipe.FlowRate - inputFlowDelta > 0)
                        {
                            var testLiquidIn = (_tank.InputPipe.FlowRate - inputFlowDelta) * multiplier;
                            delta = testLiquidIn + acidIn + baseIn - (GetCurrentOutputFlowRate() * multiplier);

                            if (_tank.CurrentLevel + (delta * nextCyclesToTest) < _tank.Volume)
                            {
                                _tank.InputPipe.FlowRate -= inputFlowDelta;
                                break;
                            }

                            inputFlowDelta += 0.1;
                        }
                    }
                }
                else
                {
                    canAutoUpdateFlow = true;
                    if (_tank.OutputPipe.ValvePosition != OutputPipeValvePosition.AlwaysClosed)
                    {
                        var flowDelta = 0.0;
                        var testLiquidOut = _tank.OutputPipe.FlowRate;
                        var testLiquidIn = _tank.InputPipe.FlowRate;
                        var neutralizersFlow = _tank.AcidTank.FlowRate + _tank.BaseTank.FlowRate;
                        delta = testLiquidIn + neutralizersFlow - testLiquidOut;
                        while (Math.Abs(delta) > 1)
                        {
                            flowDelta += 0.1;
                            var deltaMultiplierIn = delta < 0 ? 1 : -1;
                            var deltaMultiplierOut = delta > 0 ? 1 : -1;

                            testLiquidIn = _tank.InputPipe.FlowRate + (flowDelta * deltaMultiplierIn);
                            if (testLiquidIn > _tank.InputPipe.MaxFlowRate)
                                testLiquidIn = _tank.InputPipe.MaxFlowRate;
                            else if (testLiquidIn < 0)
                            {
                                testLiquidIn = 0;
                            }

                            testLiquidOut = _tank.OutputPipe.FlowRate + (flowDelta * deltaMultiplierOut);
                            if (testLiquidOut > _tank.OutputPipe.MaxFlowRate)
                                testLiquidOut = _tank.OutputPipe.MaxFlowRate;
                            else if (testLiquidOut < 0)
                            {
                                testLiquidOut = 0;
                            }

                            delta = testLiquidIn + neutralizersFlow - testLiquidOut;
                        }

                        _tank.InputPipe.FlowRate = testLiquidIn;
                        _tank.OutputPipe.FlowRate = testLiquidOut;
                    }
                }
            }
        }

        public void ExportTank(string tankFilePath)
        {
            var jsonedTank = JsonConvert.SerializeObject(_tank, Formatting.Indented);
            File.WriteAllText(tankFilePath, jsonedTank);
        }
        public void ImportTank(string tankFilePath)
        {
            try
            {
                var jsonedTank = File.ReadAllText(tankFilePath);
                _tank = JsonConvert.DeserializeObject<MainTank>(jsonedTank);
                AdjustNeutralizers();
            }
            catch
            {
                throw new FormatException("Provided config file has incorrect format!");
            }
        }


        public bool MainTankIsFull => _tank.CurrentLevel >= _tank.Volume;
        public bool AcidTankIsEmpty => _tank.AcidTank.CurrentLevel <= 0;
        public bool BaseTankIsEmpty => _tank.BaseTank.CurrentLevel <= 0;
        public bool OutputPipeIsOpen => _tank.OutputPipe.ValvePosition == OutputPipeValvePosition.AlwaysOpen || 
            (_tank.OutputPipe.ValvePosition == OutputPipeValvePosition.Auto && _tank.OutputPipeAutoOpenTankLevel < _tank.CurrentLevel);

        

        public string TankIsWorking_ButtonTextMap
        {
            get
            {
                return IsWorking ? "SYSTEM STOP" : "SYSTEM START";
            }
        }

        public string MainTankVolume_TextBoxMap
        {
            get
            {
                return MappingService.MapIntToTextBox((int)_tank.Volume);
            }
            set
            {
                _tank.Volume = MappingService.MapTextBoxToDouble(value);
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("TankIsWorking_ButtonTextMap"));
                }
            }
        }

        public string MainTankLevel_TextBoxMap
        {
            get
            {
                return MappingService.MapIntToTextBox((int)_tank.CurrentLevel);
            }
            set
            {
                _tank.CurrentLevel = MappingService.MapTextBoxToDouble(value);
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("TankIsWorking_ButtonTextMap"));
                }
            }
        }

        public int MainTankLevel_PanelHeightMap
        {
            get
            {
                return (int)(_mainTankLevelPanelHeight * (_tank.CurrentLevel / _tank.Volume));
            }

        }

        public int MainTankLevel_PanelTopMap
        {
            get
            {
                return _mainTankLevelPanelHeight - MainTankLevel_PanelHeightMap + 3;
            }
        }

        public int AcidTankLevel_PanelHeightMap
        {
            get
            {
                return (int)(_neutralizerTanklevelPanelHeight * (_tank.AcidTank.CurrentLevel / _tank.AcidTank.Volume));
            }
        }

        public int AcidTankLevel_PanelTopMap
        {
            get
            {
                return _neutralizerTanklevelPanelHeight - AcidTankLevel_PanelHeightMap + 3;
            }
        }

        public int BaseTankLevel_PanelHeightMap
        {
            get
            {
                return (int)(_neutralizerTanklevelPanelHeight * (_tank.BaseTank.CurrentLevel / _tank.BaseTank.Volume));
            }
        }

        public int BaseTankLevel_PanelTopMap
        {
            get
            {
                return _neutralizerTanklevelPanelHeight - BaseTankLevel_PanelHeightMap + 3;
            }
        }


        public int MainTankPh_ProgressBarMap
        {
            get
            {
                return MappingService.MapDoubleToProgressBar(_tank.Ph);
            }
            set
            {
                _tank.Ph = MappingService.MapProgressBarToDouble(value);
                AdjustNeutralizers();
            }
        }
        public string MainTankPh_TextBoxMap
        {
            get
            {
                return MappingService.MapDoubleToTextBox1Dc(_tank.Ph);
            }
            set
            {
                _tank.Ph = MappingService.MapTextBoxToDouble(value);
                AdjustNeutralizers();
            }
        }

        public int InputPh_ProgressBarMap
        {
            get
            {
                return MappingService.MapDoubleToProgressBar(_tank.InputPipe.Ph);
            }
            set
            {
                _tank.InputPipe.Ph = MappingService.MapProgressBarToDouble(value);
                AdjustNeutralizers();
            }
        }
        public string InputPh_TextBoxMap
        {
            get
            {
                return MappingService.MapDoubleToTextBox1Dc(_tank.InputPipe.Ph);
            }
            set
            {
                _tank.InputPipe.Ph = MappingService.MapTextBoxToDouble(value);
                AdjustNeutralizers();
            }
        }

        public int InputFlowRate_ProgressBarMap
        {
            get
            {
                return MappingService.MapDoubleToProgressBar(_tank.InputPipe.FlowRate);
            }
            set
            {
                _tank.InputPipe.FlowRate = MappingService.MapProgressBarToDouble(value);
                AdjustNeutralizers();
            }
        }
        public string InputFlowRate_TextBoxMap
        {
            get
            {
                return MappingService.MapDoubleToTextBox3Dc(_tank.InputPipe.FlowRate);
            }
            set
            {
                _tank.InputPipe.FlowRate = MappingService.MapTextBoxToDouble(value);
                AdjustNeutralizers();
            }
        }

        public string InputMaxFlowRate_TextBoxMap
        {
            get
            {
                return MappingService.MapIntToTextBox((int)_tank.InputPipe.MaxFlowRate);
            }
            set
            {
                _tank.InputPipe.MaxFlowRate = MappingService.MapTextBoxToInt(value);
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("TankIsWorking_ButtonTextMap"));
                }
            }
        }

        public int InputMaxFlowRate_ProgressBarMap
        {
            get
            {
                return MappingService.MapDoubleToProgressBar(_tank.InputPipe.MaxFlowRate);
            }
            set
            {
                _tank.InputPipe.MaxFlowRate = MappingService.MapProgressBarToDouble(value);
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("TankIsWorking_ButtonTextMap"));
                }
            }
        }

        public int OutputPipeOpenMainTankLevel_ProgressBarMap
        {
            get
            {
                return MappingService.MapDoubleToProgressBar(_tank.OutputPipeAutoOpenTankLevel);
            }
            set
            {
                _tank.OutputPipeAutoOpenTankLevel = MappingService.MapProgressBarToDouble(value);
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("TankIsWorking_ButtonTextMap"));
                }
            }
        }

        public int OutputPipeOpenMainTankLevelMaximum_ProgressBarMap
        {
            get
            {
                return MappingService.MapDoubleToProgressBar(_tank.Volume);
            }
        }

        public string OutputPipeOpenMainTankLevel_TextBoxMap
        {
            get
            {

                return MappingService.MapIntToTextBox((int)_tank.OutputPipeAutoOpenTankLevel);
            }
            set
            {
                _tank.OutputPipeAutoOpenTankLevel = MappingService.MapTextBoxToDouble(value);
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("TankIsWorking_ButtonTextMap"));
                }
            }
        }
        public string OutputPipeOpenMainTankLevelPercent_TextBoxMap
        {
            get
            {
                var percent = (int)(_tank.OutputPipeAutoOpenTankLevel * 100 / _tank.Volume);
                return MappingService.MapIntToTextBox(percent);
            }
            set
            {
                var val = (int)(double.Parse(value) * _tank.OutputPipeAutoOpenTankLevel / 100);
                _tank.BaseTank.FlowRate = MappingService.MapTextBoxToInt(val.ToString());
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("TankIsWorking_ButtonTextMap"));
                }
            }
        }

        public OutputPipeValvePosition OutputPipeValve
        {
            get { return _tank.OutputPipe.ValvePosition; }
            set { _tank.OutputPipe.ValvePosition = value; }
        }

        public bool OutputPipeValveOpen_RadioButtonCheckedMap
        {
            get
            {
                return _tank.OutputPipe.ValvePosition == OutputPipeValvePosition.AlwaysOpen;
            }
            set
            {
                _tank.OutputPipe.ValvePosition = OutputPipeValvePosition.AlwaysOpen;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("TankIsWorking_ButtonTextMap"));
                }
            }
        }
        public bool OutputPipeValveClosed_RadioButtonCheckedMap
        {
            get
            {
                return _tank.OutputPipe.ValvePosition == OutputPipeValvePosition.AlwaysClosed;
            }
            set
            {
                _tank.OutputPipe.ValvePosition = OutputPipeValvePosition.AlwaysClosed;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("TankIsWorking_ButtonTextMap"));
                }
            }
        }
        public bool OutputPipeValveAuto_RadioButtonCheckedMap
        {
            get
            {
                return _tank.OutputPipe.ValvePosition == OutputPipeValvePosition.Auto;
            }
            set
            {
                _tank.OutputPipe.ValvePosition = OutputPipeValvePosition.Auto;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("TankIsWorking_ButtonTextMap"));
                }
            }
        }

        public int OutputPipeFlowRate_ProgressBarMap
        {
            get
            {
                return MappingService.MapDoubleToProgressBar(_tank.OutputPipe.FlowRate);
            }
            set
            {
                _tank.OutputPipe.FlowRate = MappingService.MapProgressBarToDouble(value);
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("TankIsWorking_ButtonTextMap"));
                }
            }
        }

        public int OutputPipeFlowRateMaximum_ProgressBarMap
        {
            get
            {
                return MappingService.MapDoubleToProgressBar(_tank.OutputPipe.MaxFlowRate);
            }

        }

        public string OutputPipeFlowRate_TextBoxMap
        {
            get
            {
                return MappingService.MapDoubleToTextBox3Dc(_tank.OutputPipe.FlowRate);
            }
            set
            {
                _tank.OutputPipe.FlowRate = MappingService.MapTextBoxToDouble(value);
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("TankIsWorking_ButtonTextMap"));
                }
            }
        }
        public string OutputPipeMaxFlowRate_TextBoxMap
        {
            get
            {
                return MappingService.MapIntToTextBox((int)_tank.OutputPipe.MaxFlowRate);
            }
            set
            {
                _tank.OutputPipe.MaxFlowRate = MappingService.MapTextBoxToInt(value);
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("TankIsWorking_ButtonTextMap"));
                }
            }
        }

        public int AcidTankPh_ProgressBarMap
        {
            get
            {
                return MappingService.MapDoubleToProgressBar(_tank.AcidTank.Ph);
            }
            set
            {
                _tank.AcidTank.Ph = MappingService.MapProgressBarToDouble(value);
                AdjustNeutralizers();
            }
        }
        public string AcidTankPh_TextBoxMap
        {
            get
            {
                return MappingService.MapDoubleToTextBox1Dc(_tank.AcidTank.Ph);
            }
            set
            {
                _tank.AcidTank.Ph = MappingService.MapTextBoxToDouble(value);
                AdjustNeutralizers();
            }
        }

        public int AcidTankFlowRate_ProgressBarMap
        {
            get
            {
                return MappingService.MapDoubleToProgressBar(_tank.AcidTank.FlowRate);
            }
            set
            {
                _tank.AcidTank.FlowRate = MappingService.MapProgressBarToDouble(value);
            }
        }
        public string AcidTankFlowRate_TextBoxMap
        {
            get
            {
                return MappingService.MapDoubleToTextBox3Dc(_tank.AcidTank.FlowRate);
            }
            set
            {
                _tank.AcidTank.FlowRate = MappingService.MapTextBoxToDouble(value);
            }
        }
        public string AcidTankMaxFlowRate_TextBoxMap
        {
            get
            {
                return MappingService.MapIntToTextBox((int)_tank.AcidTank.MaxFlowRate);
            }
            set
            {
                _tank.AcidTank.MaxFlowRate = MappingService.MapTextBoxToInt(value);
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("TankIsWorking_ButtonTextMap"));
                }
            }
        }
        public int AcidTankMaxFlowRate_ProgressBarMap
        {
            get
            {
                return MappingService.MapDoubleToProgressBar(_tank.AcidTank.MaxFlowRate);
            }
            set
            {
                _tank.AcidTank.MaxFlowRate = MappingService.MapProgressBarToDouble(value);
            }
        }

        public int BaseTankPh_ProgressBarMap
        {
            get
            {
                return MappingService.MapDoubleToProgressBar(_tank.BaseTank.Ph);
            }
            set
            {
                _tank.BaseTank.Ph = MappingService.MapProgressBarToDouble(value);
                AdjustNeutralizers();
            }
        }
        public string BaseTankPh_TextBoxMap
        {
            get
            {
                return MappingService.MapDoubleToTextBox1Dc(_tank.BaseTank.Ph);
            }
            set
            {
                _tank.BaseTank.Ph = MappingService.MapTextBoxToDouble(value);
                AdjustNeutralizers();
            }
        }

        public int BaseTankFlowRate_ProgressBarMap
        {
            get
            {
                return MappingService.MapDoubleToProgressBar(_tank.BaseTank.FlowRate);
            }
            set
            {
                _tank.BaseTank.FlowRate = MappingService.MapProgressBarToDouble(value);
            }
        }

        public string BaseTankFlowRate_TextBoxMap
        {
            get
            {
                return MappingService.MapDoubleToTextBox3Dc(_tank.BaseTank.FlowRate);
            }
            set
            {
                _tank.BaseTank.FlowRate = MappingService.MapTextBoxToDouble(value);
            }
        }
        public string BaseTankMaxFlowRate_TextBoxMap
        {
            get
            {
                return MappingService.MapIntToTextBox((int)_tank.BaseTank.MaxFlowRate);
            }
            set
            {
                _tank.BaseTank.MaxFlowRate = MappingService.MapTextBoxToInt(value);
            }
        }
        public int BaseTankMaxFlowRate_ProgressBarMap
        {
            get
            {
                return MappingService.MapDoubleToProgressBar(_tank.BaseTank.MaxFlowRate);
            }
            set
            {
                _tank.BaseTank.MaxFlowRate = MappingService.MapProgressBarToDouble(value);
            }
        }

        public string AcidTankVolume_TextBoxMap
        {
            get
            {
                return MappingService.MapIntToTextBox((int)_tank.AcidTank.Volume);
            }
            set
            {
                _tank.AcidTank.Volume = MappingService.MapTextBoxToDouble(value);
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("TankIsWorking_ButtonTextMap"));
                }
            }
        }

        public string AcidTankLevel_TextBoxMap
        {
            get
            {
                return MappingService.MapIntToTextBox((int)_tank.AcidTank.CurrentLevel);
            }
            set
            {
                _tank.AcidTank.CurrentLevel = MappingService.MapTextBoxToDouble(value);
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("TankIsWorking_ButtonTextMap"));
                }
            }
        }

        public string BaseTankVolume_TextBoxMap
        {
            get
            {
                return MappingService.MapIntToTextBox((int)_tank.BaseTank.Volume);
            }
            set
            {
                _tank.BaseTank.Volume = MappingService.MapTextBoxToDouble(value);
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("TankIsWorking_ButtonTextMap"));
                }
            }
        }

        public string BaseTankLevel_TextBoxMap
        {
            get
            {
                return MappingService.MapIntToTextBox((int)_tank.BaseTank.CurrentLevel);
            }
            set
            {
                _tank.BaseTank.CurrentLevel = MappingService.MapTextBoxToDouble(value);
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("TankIsWorking_ButtonTextMap"));
                }
            }
        }

        public Color BaseTankFlowRate_PanelColorMap
        {
            get
            {
                return _tank.BaseTank.CurrentLevel > 0 ? MappingService.MapPhToColor2(_tank.BaseTank.Ph) : Color.Silver;
            }
        }

        public Color AcidTankFlowRate_PanelColorMap
        {
            get
            {
                return _tank.AcidTank.CurrentLevel > 0 ? MappingService.MapPhToColor2(_tank.AcidTank.Ph) : Color.Silver;
            }
        }


        public Color BaseTankLevel_PanelColorMap
        {
            get
            {
                return MappingService.MapPhToColor2(_tank.BaseTank.Ph);
            }
        }

        public Color AcidTankLevel_PanelColorMap
        {
            get
            {
                return MappingService.MapPhToColor2(_tank.AcidTank.Ph);
            }
        }

        public Color MainTankLevel_PanelColorMap
        {
            get
            {
                return MappingService.MapPhToColor2(_tank.Ph);
            }
        }

        public Color InputPipe_PanelColorMap
        {
            get
            {
                return MappingService.MapPhToColor2(_tank.InputPipe.Ph);
            }
        }
        public Color OutputPipe_PanelColorMap
        {
            get
            {
                if (GetCurrentOutputFlowRate() > 0 && _tank.CurrentLevel > 0)
                {
                    return MappingService.MapPhToColor2(_tank.Ph);
                }

                return Color.Gray;
            }
        }

        public Color ControlPanel_TitleLabelColorMap
        {
            get
            {
                return IsWorking ? Color.LawnGreen : Color.ForestGreen;
            }
        }
        public Color ControlPanel_ValueLabelColorMap
        {
            get
            {
                return IsWorking ? Color.Red : Color.Maroon;
            }
        }

        public bool InputPipeBlock_PanelVisibilityMap
        {
            get
            {
                return !IsWorking || _tank.InputPipe.FlowRate == 0;
            }
        }

        public bool OutputPipeBlock_PanelVisibilityMap
        {
            get
            {
                if (IsWorking && GetCurrentOutputFlowRate() > 0)
                {
                    return false;
                }

                return true;
            }
        }

        public bool BaseTankPipeBlock_PanelVisibilityMap
        {
            get
            {
                return !IsWorking || _tank.BaseTank.FlowRate == 0;
            }
        }

        public bool AcidTankPipeBlock_PanelVisibilityMap
        {
            get
            {
                return !IsWorking || _tank.AcidTank.FlowRate == 0;
            }
        }

        public string TimeLeftToFullMainTank
        {
            get
            {
                if (_tank.InputPipe.FlowRate + _tank.AcidTank.FlowRate + _tank.BaseTank.FlowRate - GetCurrentOutputFlowRate() <= 0)
                {
                    return double.PositiveInfinity.ToString();
                }

                var v = (_tank.Volume - _tank.CurrentLevel) / (_tank.InputPipe.FlowRate + _tank.AcidTank.FlowRate + _tank.BaseTank.FlowRate - GetCurrentOutputFlowRate());
                return double.IsInfinity(v) ? $"{v}" : $"{TimeSpan.FromSeconds(v).ToString(@"dd\.hh\:mm\:ss")}";
            }
        }
        public string TimeLeftToEmptyAcidTank
        {
            get
            {
                var v = _tank.AcidTank.CurrentLevel / _tank.AcidTank.FlowRate;
                return double.IsInfinity(v) ? $"{v}" : $"{TimeSpan.FromSeconds(v).ToString(@"dd\.hh\:mm\:ss")}";
            }
        }
        public string TimeLeftToEmptyBaseTank
        {
            get
            {
                var v = _tank.BaseTank.CurrentLevel / _tank.BaseTank.FlowRate;
                return double.IsInfinity(v) ? $"{v}" : $"{TimeSpan.FromSeconds(v).ToString(@"dd\.hh\:mm\:ss")}";
            }
        }
        public string TotalLiquidLitresIn_LabelMap
        {
            get
            {
                return $"{(int)TotalLiquidLitresIn} L";
            }
        }
        public string TotalLiquidLitresOut_LabelMap
        {
            get
            {
                return $"{(int)TotalLiquidLitresOut} L";
            }
        }
        public string TotalAcidLitresOut_LabelMap
        {
            get
            {
                return $"{(int)TotalAcidLitresOut} L";
            }
        }
        public string TotalBaseLitresOut_LabelMap
        {
            get
            {
                return $"{(int)TotalBaseLitresOut} L";
            }
        }

        public string LiquidDelta_LabelMap
        {
            get
            {
                var delta = _tank.InputPipe.FlowRate + _tank.AcidTank.FlowRate + _tank.BaseTank.FlowRate - GetCurrentOutputFlowRate();
                return MappingService.MapDoubleToTextBox3Dc(delta) + " L/s";
            }
        }

        public bool OutputPipeStreamVisibility_PanelMap
        {
            get
            {
                return IsWorking && OutputPipeIsOpen;
            }
        }
    }
}
