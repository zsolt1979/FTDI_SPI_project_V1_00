using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FTDI_SPI_project
{
    
    public partial class SCA_Sensor_List : ObservableObject
    {

        public ObservableCollection<SCA_DataBlock> FTDI_DataBlock { get; set; } = new();
        public SCA_Sensor_List()
        {
            FTDI_DataBlock.Add(new SCA_DataBlock());
            FTDI_DataBlock.Add(new SCA_DataBlock());
        }
    }
    
    public partial class SCA_DataBlock : ObservableObject
    {
        [ObservableProperty]
        private string _sensor_Temp_Raw = string.Empty ;
        [ObservableProperty]
        private string _sensor_Temp_Real = string.Empty ;
        
        [ObservableProperty]
        private string _sensor_X_Axis_Raw = string.Empty;
        [ObservableProperty]
        private string _sensor_Y_Axis_Raw = string.Empty;
        [ObservableProperty]
        private string _sensor_Angle_Real = string.Empty;
        [ObservableProperty]
        private double _sensor_Indicator_Value;
        [ObservableProperty]
        private double _canvas_Angle;


        public SCA_DataBlock()
        {
            Sensor_Temp_Raw = "--";
            Sensor_Temp_Real = "--";
            Sensor_X_Axis_Raw = "--";
            Sensor_Y_Axis_Raw = "--";
            Sensor_Angle_Real = "--";
            Sensor_Indicator_Value = 0.0;
            Canvas_Angle = 30.0;
        }
    }

    public partial class Button_Panel_Content : ObservableObject
    {
        [ObservableProperty]
        private string _start_Stop_Button_Text = string.Empty;
        [ObservableProperty]
        private string _device_MPSSE_Info;
        [ObservableProperty]
        private string _app_Status_Info;
        [ObservableProperty]
        private bool _enable_1_Status;
        [ObservableProperty]
        private int _meter_Scale;
        [ObservableProperty]
        private string _down_Counter_Value;
        [ObservableProperty]
        private bool _button_Counter_Running;

        public Button_Panel_Content()
        {
            Start_Stop_Button_Text = "Start";
            Device_MPSSE_Info = "N/A";
            App_Status_Info = "---";
            Enable_1_Status = false;
            Meter_Scale = 0;
            Down_Counter_Value = "--";
            Button_Counter_Running = false;
        }

        public void Start_Stop_Toogle()
        { 
            if (Start_Stop_Button_Text == "Start")
            {
                this.Start_Stop_Button_Text = "Stop";
            }
            else
            {
                this.Start_Stop_Button_Text = "Start";
            }
        
        }

    }



}
