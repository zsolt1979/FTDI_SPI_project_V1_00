using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FTDI_SPI_Functions;
using FTD2XX_NET;
using System.Collections.ObjectModel;
using System.Diagnostics.Eventing.Reader;

namespace FTDI_SPI_project
{
    public partial class MainWindow : Window
    {

        public const int Sensor_Number = 2;
        public static Button_Panel_Content Button_Content = new Button_Panel_Content();
        public static SCA_Sensor_List SensorList = new SCA_Sensor_List();
        public static bool Device_Connected = false;
        public static bool Measurement_Running = false;

        public MainWindow()
        {
            InitializeComponent();
            

            Button_Panel.DataContext = Button_Content;
            Sensor_1_Enable_Button.DataContext = Button_Content;
            Sensor_2_Enable_Button.DataContext = Button_Content;
            Angle_Sensor_Data_Panel.DataContext = SensorList;

        }

        public void FTDI_Device_Connect(object sender, RoutedEventArgs e)
        {
            UInt32 Device_count = FTDI_Calls.FTDI_SPI_Device_Detect();
            Debug.WriteLine("Detected device number: " + Device_count.ToString());
            if (Device_count == 0)
            {
                Debug.WriteLine("Connection is not possible");
                Device_Connected = false;
                return;
            }

            FTDI.FT_DEVICE_INFO_NODE[] ftdiDeviceList = new FTDI.FT_DEVICE_INFO_NODE[Device_count];

            FTDI_Calls.ftStatus = FTDI_Calls.myFtdiDevice.GetDeviceList(ftdiDeviceList);

            UInt32 i;
            for (i = 0; i < Device_count; i++)
            {
                Debug.WriteLine("Type: " + ftdiDeviceList[i].Type.ToString());
                Debug.WriteLine("ID: " + String.Format("0x{0:x}", ftdiDeviceList[i].ID));
                Debug.WriteLine("Location ID: " + String.Format("{0:x}", ftdiDeviceList[i].LocId));
                Debug.WriteLine("Serial Number: " + ftdiDeviceList[i].SerialNumber);
                Debug.WriteLine("Description: " + ftdiDeviceList[i].Description);
            }

            string FTDI_Type = ftdiDeviceList[0].Type.ToString();
            string FTDI_ID = String.Format("{0:x}", ftdiDeviceList[0].ID);
            string FTDI_Location_ID = String.Format("{0:x}", ftdiDeviceList[0].LocId);
            string FTDI_Serial = ftdiDeviceList[0].SerialNumber;
            string FTDI_Decription = ftdiDeviceList[0].Description;

            Button_Content.Device_MPSSE_Info = $"Type: {FTDI_Type}\r\n" +
                                                         $"ID: {FTDI_ID}\r\n" +
                                                         $"Location ID: {FTDI_Location_ID}\r\n" +
                                                         $"Serial Number: {FTDI_Serial}\r\n" +
                                                         $"Description: {FTDI_Decription}";

            FTDI_Status_Message FTDI_open_status = FTDI_Calls.FTDI_SPI_Open();
            if (FTDI_open_status == FTDI_Status_Message.FTDI_OK)
            {
                Debug.WriteLine("Device open was successful...");
                Device_Connected = true;
            }
        }

        public void Sensor_SPI_Data_1(object sender, RoutedEventArgs e)
        {
            if (Device_Connected && (!Measurement_Running))
            {
                SCA_Sensor_Full_Read(0);
            }
        }

        public void Sensor_SPI_Data_2(object sender, RoutedEventArgs e)
        {
            if (Device_Connected && (!Measurement_Running))
            {
                SCA_Sensor_Full_Read(1);
            }
        }


        public void StartStop_Click(object sender, RoutedEventArgs e)
        {
            if (Device_Connected)
            {
                Measurement_Running = ! Measurement_Running;
                Button_Content.Start_Stop_Toogle();
                if (Measurement_Running) 
                {
                    SCA_Info_Request();
                }
            }
            else
            {
                Measurement_Running = false;
            }
        }

        public void SCA1_Enable(object sender, RoutedEventArgs e)
        {  
            if (Device_Connected)
            {
                if (Button_Content.Meter_Scale < 6)
                {
                    Button_Content.Enable_1_Status = true;
                    Button_Content.Meter_Scale++;
                }
                else
                {
                    Button_Content.Enable_1_Status = false;
                    Button_Content.Meter_Scale = 0;
                }
                Debug.WriteLine($"Enable was pressed... and Meter_Scale = {Button_Content.Meter_Scale} Dec");
            }
                
        }

        public void SCA2_Enable(object sender, RoutedEventArgs e)
        {
            if (Device_Connected)
            {
                Button_Down_Counter_Start(20);
                Debug.WriteLine("SCA2 button was pressed...");
            }

        }

        public void Exit_Click(object sender, RoutedEventArgs e)
        {
            Main_WPF.Close();
        }

        public async void SCA_Info_Request()
        {
            int Counter = 0;

            while (Measurement_Running)
            {

                SCA_Sensor_Full_Read(0);
                SCA_Sensor_Full_Read(1);
                Debug.WriteLine(Counter);
                Counter++;
                await Task.Delay(350);
            }
            Debug.WriteLine("Info request task ended...");
        }

        public void SCA_Sensor_Full_Read (byte Sensor_Number)
        {
            FTDI_Calls.FTDI_Read_Parameter(Sensor_Number, Sensor_Parameter.SCA103_Temperature);
            FTDI_Calls.FTDI_Read_Parameter(Sensor_Number, Sensor_Parameter.SCA103_Angle);
            SensorList.FTDI_DataBlock[Sensor_Number].Sensor_Temp_Raw = Convert.ToString(FTDI_Calls.SCA_Sensor_Parameters[Sensor_Number].Temperature_Raw);
            SensorList.FTDI_DataBlock[Sensor_Number].Sensor_Temp_Real = Convert.ToString(FTDI_Calls.SCA_Sensor_Parameters[Sensor_Number].Temperature_Real);
            SensorList.FTDI_DataBlock[Sensor_Number].Sensor_X_Axis_Raw = Convert.ToString(FTDI_Calls.SCA_Sensor_Parameters[Sensor_Number].Axis_X_Raw);
            SensorList.FTDI_DataBlock[Sensor_Number].Sensor_Y_Axis_Raw = Convert.ToString(FTDI_Calls.SCA_Sensor_Parameters[Sensor_Number].Axis_Y_Raw);
            SensorList.FTDI_DataBlock[Sensor_Number].Sensor_Angle_Real = Convert.ToString(FTDI_Calls.SCA_Sensor_Parameters[Sensor_Number].Angle_real);
        }

        public async void Button_Down_Counter_Start(int Start_Number)
        {
            if (Button_Content.Button_Counter_Running == false)
            {
                Button_Content.Button_Counter_Running = true;
                for (int i = Start_Number; i >= 0; i--)
                {
                    Button_Content.Down_Counter_Value = ($"{i} sec");
                    Debug.WriteLine($"Actual counter value: {i} Dec");
                    await Task.Delay(500);
                }
                Button_Content.Button_Counter_Running = false;
            }
        }

    }
}
