using FTD2XX_NET;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using static FTD2XX_NET.FTDI;
using FTDI_SPI_project;
using System.Runtime;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Runtime.ConstrainedExecution;

namespace FTDI_SPI_Functions
{
    public enum FTDI_Status_Message
    {
        FTDI_OK = 0,
        FTDI_RESPONSE_ERROR,
        FTDI_PARAMETER_ERROR
    }
    public enum Sensor_Parameter
    {
        SCA103_Temperature = 0,
        SCA103_Angle = 1
    }

    public static class SCA_Commands
    {
        public static readonly byte[] Temperature = new byte[5] { 0x31, 0x01, 0x00, 0x08, 0x00 };
        public static readonly byte[] X_Axis = new byte[6] { 0x31, 0x02, 0x00, 0x10, 0x00, 0x00 };
        public static readonly byte[] Y_Axis = new byte[6] { 0x31, 0x02, 0x00, 0x11, 0x00, 0x00 };
    }

    public class SCA_result
    {
        public byte Temperature_Raw = 0;
        public double Temperature_Real = 0.0;
        public uint Axis_X_Raw = 0;
        public uint Axis_Y_Raw = 0;
        public double Angle_real = 0.0;
    }

    public class FTDI_Calls
    {
        public static FTDI myFtdiDevice = new FTDI();
        public static FTDI.FT_STATUS ftStatus = FT_STATUS.FT_OK;
        public static UInt32 ftdiDeviceCount = 0;
        public static List<SCA_result> SCA_Sensor_Parameters = new List<SCA_result>();
        private const uint SCA_Sensor_Number = 2;


        public static UInt32 FTDI_SPI_Device_Detect()

        {
            ftStatus = myFtdiDevice.GetNumberOfDevices(ref ftdiDeviceCount);
            return ftdiDeviceCount;
        }

        public static FTDI_Status_Message FTDI_SPI_Open()
        {
            if (ftdiDeviceCount == 0)
            {
                return FTDI_Status_Message.FTDI_PARAMETER_ERROR;
            }

            SCA_Sensor_Parameters.Clear();

            for (int i=0; i < SCA_Sensor_Number; i++)
            {
                SCA_Sensor_Parameters.Add(new SCA_result());
            }

            myFtdiDevice.OpenByIndex(0);                        // Open device
            myFtdiDevice.ResetDevice();                         // Reset device
            myFtdiDevice.SetTimeouts(100, 100);                 // Set Rx and Tx timeout

            myFtdiDevice.SetCharacters(0, false, 0, false);     // Disable event and error handling
            myFtdiDevice.SetBitMode(0, 2);                      // Set MPPSE mode
            myFtdiDevice.SetLatency(5);                         // Latency timer set to 10ms 

            FTDI_Send_Command(0x80, 0xF8, 0xFB);                // Set data bits lowbyte

            FTDI_Send_Command(0x86, 0xF0, 0x00);                // Set CLK frequency

            FTDI_Send_Command(0x82, 0x00, 0x40);                // Set ACBUS port config 'Z0ZZZZZZ' (LED On)
            Thread.Sleep(500);
            FTDI_Send_Command(0x82, 0x40, 0x40);                // Set ACBUS output state 'Z1ZZZZZZ' (LED Off)

            return FTDI_Status_Message.FTDI_OK;
        }

        public static FTDI_Status_Message FTDI_Read_Parameter(byte Device_No, Sensor_Parameter Sensor_Parameter)
        {

            FTDI_Activate_CS(Device_No);
            // ----------------------------------------------------------------            
            byte[] SPI_TX_Buffer = new byte[8];
            byte[] SPI_RX_Buffer = new byte[8];
            uint Received_Byte_Number = 0;
            uint numBytesRead = 0;
            uint numBytesWritten = 0;
            bool data_is_valid = false;

            switch (Sensor_Parameter)
            {
                case Sensor_Parameter.SCA103_Temperature:
                    {
                        numBytesWritten = 0;
                        myFtdiDevice.Write(SCA_Commands.Temperature, (SCA_Commands.Temperature).Length, ref numBytesWritten);     
                        FTDI_Deactivate_CS();               // Set -CS to high logic level

                        for (int i = 0; i < 5; i++)
                        {
                            Received_Byte_Number = Read_SCA_Byte_Number();
                            if (Received_Byte_Number == 2)
                            {
                                Debug.WriteLine("TEMP - Valid data lenght was detected... " + $"Cycle number: {i}");
                                ftStatus = myFtdiDevice.Read(SPI_RX_Buffer, Received_Byte_Number, ref numBytesRead);
                                data_is_valid = true;
                                break;
                            }
                        }

                        if (data_is_valid != true)
                        {
                            Debug.WriteLine("TEMP - Buffer read exit with timeout...");
                        }
                        else
                        {
                            SCA_Sensor_Parameters[Device_No].Temperature_Raw = SPI_RX_Buffer[1];
                            SCA_Sensor_Parameters[Device_No].Temperature_Real = SCA_Calculate_Temperature(SPI_RX_Buffer[1]);
                        }
                        break;
                    };

                case Sensor_Parameter.SCA103_Angle:
                    {
                        numBytesWritten = 0;
                        myFtdiDevice.Write(SCA_Commands.X_Axis, (SCA_Commands.X_Axis).Length, ref numBytesWritten);
                        FTDI_Deactivate_CS();               // Set -CS to high logic level


                        FTDI_Activate_CS(Device_No);
                        numBytesWritten = 0;
                        myFtdiDevice.Write(SCA_Commands.Y_Axis, (SCA_Commands.Y_Axis).Length, ref numBytesWritten);
                        FTDI_Deactivate_CS();               // Set -CS to high logic level

                        for (int i = 0; i < 5; i++)
                        {
                            Received_Byte_Number = Read_SCA_Byte_Number();
                            if (Received_Byte_Number == 6)
                            {
                                Debug.WriteLine("Axis X/Y - Valid data lenght was detected... " + $"Cycle number: {i}");
                                ftStatus = myFtdiDevice.Read(SPI_RX_Buffer, Received_Byte_Number, ref numBytesRead);
                                data_is_valid = true;
                                break;
                            }
                        }

                        if (data_is_valid != true)
                        {
                            Debug.WriteLine("Axis X/Y - Buffer read exit with timeout...");
                            break;
                        }
                        else
                        {
                            SCA_Sensor_Parameters[Device_No].Axis_X_Raw = (uint)((SPI_RX_Buffer[1] << 8) | SPI_RX_Buffer[2]) >> 5;
                            SCA_Sensor_Parameters[Device_No].Axis_Y_Raw = (uint)((SPI_RX_Buffer[4] << 8) | SPI_RX_Buffer[5]) >> 5;
                            SCA_Sensor_Parameters[Device_No].Angle_real = SCA_Calculate_Angle(SCA_Sensor_Parameters[Device_No].Axis_X_Raw, SCA_Sensor_Parameters[Device_No].Axis_Y_Raw);
                        }

                        break;
                    };

                default:
                    {
                        return FTDI_Status_Message.FTDI_PARAMETER_ERROR;
                    }
            }

            return FTDI_Status_Message.FTDI_OK;
        }
        
        private static void FTDI_Activate_CS(byte Device_No)
        {
            byte CS_pin_calculation = 0;
            if (Device_No < 5)
            {
                CS_pin_calculation = (byte)(0x01 << (Device_No + 3));
                CS_pin_calculation = (byte)(0xF8 & ((byte)(~CS_pin_calculation)));
                FTDI_Send_Command(0x80, CS_pin_calculation, 0xFB);                      // Set -CS to low logic level
                Thread.Sleep(5);
            }
        }
        private static void FTDI_Deactivate_CS()
        {
            Thread.Sleep(10);
            FTDI_Send_Command(0x80, 0xF8, 0xFB);                                        // Set -CS to high logic level
        }

        private static void FTDI_Send_Command(byte FTDI_Cmd, byte Data_1, byte Data_2)
        {
            uint numBytesWritten = 0;
            byte[] MPSSEbuffer = new byte[5];
            MPSSEbuffer[0] = FTDI_Cmd;
            MPSSEbuffer[1] = Data_1;
            MPSSEbuffer[2] = Data_2;
            myFtdiDevice.Write(MPSSEbuffer, 3, ref numBytesWritten);
        }

        private static double SCA_Calculate_Temperature (byte Temp_Raw_Data)
        {
            double Real_Temperature = Math.Round(( (double)(Temp_Raw_Data - 197) / -1.083), 1);
            Debug.WriteLine($"Real temp: {Real_Temperature} Celsius");
            return Real_Temperature;
        }

        private static double SCA_Calculate_Angle(uint Axis_X_Raw, uint Axis_Y_Raw)
        {
            int SCA_Dout = (int) Axis_X_Raw - (int) Axis_Y_Raw ;
            double SCA_Angle_Div = (double)(SCA_Dout / 6554.0);
            double SCA_Angle = Math.Round( ((Math.Asin(SCA_Angle_Div) / 3.14) * 180), 3);
            Debug.WriteLine($"SCA Dout value: {SCA_Dout} Dec ---  SCA Angle: {SCA_Angle} degree");
            return SCA_Angle;
        }


        private static uint Read_SCA_Byte_Number()
        {
            uint RX_Buffer_Count = 0;
            ftStatus = myFtdiDevice.GetRxBytesAvailable(ref RX_Buffer_Count);
            Debug.WriteLine($"Data count in the Rx buffer: {RX_Buffer_Count}");
            return RX_Buffer_Count;
        }
    }
}
