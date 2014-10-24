using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DF1Write
{
    public partial class Form1 : Form
    {
        int Increment = new Random().Next(1, 127);
        int value = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Example: Starting From N7:0 with count=1
            int[] data = new int[1];          
            value += 1;
            if (value >= 4095) value = 0;
            data[0] = value;        
            WriteDataN7(0, data);           
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Example: Starting From N7:0 with count=1
            int[] data = new int[1];
            value -= 1;
            if (value < 0) value = 4095;
            data[0] = value;
            WriteDataN7(0, data);
        }

        public void WriteDataN7(byte StartingElementNumber, int[] dataToWrite)
        {
            try
            {
                if (!serialPort1.IsOpen) serialPort1.Open();
            }
            catch (Exception err)
            {
                MessageBox.Show(this, err.Message);
                return;
            }

            if (Increment < 65535)
            {
                Increment += 1;
            }
            else
            {
                Increment = 1;
            }

            byte total = 0;
            byte bytesize = 0;
            bytesize = (byte)(dataToWrite.Length * 2);
            total = (byte)(bytesize + 17);
            byte[] write = new byte[total + 1];



            write[0] = 0x10;//DLE
            write[1] = 0x2;//STX

            write[2] = 0;
            write[3] = 0;
            write[4] = 0xf;
            write[5] = 0;//STS
            write[6] = (byte)(Increment & 255);
            write[7] = (byte)(Increment >> 8);
            write[8] = 0xaa;


            //Byte Size
            write[9] = (byte)((bytesize & 0xff));
            //Example: Starting From N7:0
            //File Number
            write[10] = 7; //File number N7:0 is 7
            //File Type
            write[11] = 0x89;//File Type N7:0 is N
            //Starting Element Number
            write[12] = StartingElementNumber; //Element N7:0 is 0
            //Sub Element
            write[13] = 0;
            for (int i = 0; i <= dataToWrite.Length - 1; i += 1)
            {
                write[14 + i * 2] = (byte)(dataToWrite[i] & 0xff);
                write[15 + i * 2] = (byte)((dataToWrite[i] >> 8) & 0xff);
            }

            byte crcmax = 0;
            crcmax = (byte)(12 + bytesize);
            byte[] dataforcrc = new byte[crcmax];
            for (int i = 0; i < crcmax; i += 1)
            {
                dataforcrc[i] = write[2 + i];
            }

            int CheckSumCalc;
            CheckSumCalc = CalculateCRC16(dataforcrc);

            write[total - 3] = 0x10;//DLE
            write[total - 2] = 0x3;//ETX 

            write[total - 1] = (byte)(CheckSumCalc & 255);
            write[total] = (byte)(CheckSumCalc >> 8);

            byte[] ACK = { 0x10, 0x6 };

            if (serialPort1.IsOpen)
            {
                serialPort1.Write(ACK, 0, ACK.Length);
                serialPort1.Write(write, 0, write.Length);
            }


        }


        private int CalculateCRC16(byte[] DataInput)
        {
            int iCRC = 0;
            byte bytT = 0;

            for (int i = 0; i <= DataInput.Length - 1; i++)
            {
                bytT = (byte)((iCRC & 0xff) ^ DataInput[i]);
                iCRC = (int)((iCRC >> 8) ^ CRC16table[bytT]);

            }

            //*** must do one more with ETX char
            bytT = (byte)((iCRC & 0xff) ^ 3);
            iCRC = (int)((iCRC >> 8) ^ CRC16table[bytT]);

            return iCRC;
        }

        int[] CRC16table = {0x0,0xc0c1,0xc181,0x140,0xc301,0x3c0,0x280,	0xc241,
	                        0xc601,	0x6c0,	0x780,	0xc741,	0x500,	0xc5c1,	0xc481,	0x440,
	                        0xcc01,	0xcc0,	0xd80,	0xcd41,	0xf00,	0xcfc1,	0xce81,	0xe40,
	                        0xa00,	0xcac1,	0xcb81,	0xb40,	0xc901,	0x9c0,	0x880,
	                        0xc841,	0xd801,	0x18c0,	0x1980,	0xd941,	0x1b00,	0xdbc1,	0xda81,
	                        0x1a40,	0x1e00,	0xdec1,	0xdf81,	0x1f40,
	                        0xdd01,	0x1dc0,	0x1c80,	0xdc41,	0x1400,	0xd4c1,	0xd581,	0x1540,
	                        0xd701,	0x17c0,	0x1680,	0xd641,	0xd201,	0x12c0,	0x1380,	0xd341,
	                        0x1100,	0xd1c1,	0xd081,	0x1040,	0xf001,	0x30c0,	0x3180,	0xf141,
	                        0x3300,	0xf3c1,	0xf281,	0x3240,	0x3600,	0xf6c1,	0xf781,	0x3740,
	                        0xf501,	0x35c0,	0x3480,	0xf441,	0x3c00,	0xfcc1,	0xfd81,
	                        0x3d40,	0xff01,	0x3fc0,	0x3e80,	0xfe41,	0xfa01,	0x3ac0,	0x3b80,
	                        0xfb41,	0x3900,	0xf9c1,	0xf881,	0x3840,	0x2800,
	                        0xe8c1,	0xe981,	0x2940,	0xeb01,	0x2bc0,	0x2a80,	0xea41,	0xee01,
	                        0x2ec0,	0x2f80,	0xef41,	0x2d00,	0xedc1,	0xec81,	0x2c40,	0xe401,
	                        0x24c0,	0x2580,	0xe541,	0x2700,	0xe7c1,	0xe681,	0x2640,	0x2200,
	                        0xe2c1,	0xe381,	0x2340,	0xe101,	0x21c0,	0x2080,	0xe041,	0xa001,
	                        0x60c0,	0x6180,	0xa141,	0x6300,	0xa3c1,	0xa281,	0x6240,	0x6600,
	                        0xa6c1,	0xa781,	0x6740,	0xa501,	0x65c0,	0x6480,	0xa441,	0x6c00,
	                        0xacc1,	0xad81,	0x6d40,	0xaf01,	0x6fc0,	0x6e80,	0xae41,	0xaa01,
	                        0x6ac0,	0x6b80,	0xab41,	0x6900,	0xa9c1,	0xa881,	0x6840,	0x7800,
	                        0xb8c1,	0xb981,	0x7940,	0xbb01,	0x7bc0,	0x7a80,	0xba41,	0xbe01,
	                        0x7ec0,	0x7f80,	0xbf41,	0x7d00,	0xbdc1,	0xbc81,	0x7c40,	0xb401,
	                        0x74c0,	0x7580,	0xb541,	0x7700,	0xb7c1,	0xb681,	0x7640,	0x7200,
	                        0xb2c1,	0xb381,	0x7340,	0xb101,	0x71c0,	0x7080,	0xb041,	0x5000,
	                        0x90c1,	0x9181,	0x5140,	0x9301,	0x53c0,	0x5280,	0x9241,	0x9601,
	                        0x56c0,	0x5780,	0x9741,	0x5500,	0x95c1,	0x9481,	0x5440,	0x9c01,
	                        0x5cc0,	0x5d80,	0x9d41,	0x5f00,	0x9fc1,	0x9e81,	0x5e40,	0x5a00,
	                        0x9ac1,	0x9b81,	0x5b40,	0x9901,	0x59c0,	0x5880,	0x9841,	0x8801,
	                        0x48c0,	0x4980,	0x8941,	0x4b00,	0x8bc1,	0x8a81,	0x4a40,	0x4e00,
	                        0x8ec1,	0x8f81,	0x4f40,	0x8d01,	0x4dc0,	0x4c80,	0x8c41,	0x4400,
	                        0x84c1,	0x8581,	0x4540,	0x8701,	0x47c0,	0x4680,	0x8641,	0x8201,
	                        0x42c0,	0x4380,	0x8341,	0x4100,	0x81c1,	0x8081,	0x4040};

        int BytesToRead;
        byte[] BytesRead = new byte[256];
        private System.Collections.ObjectModel.Collection<byte> ReceivedDataPacket = new System.Collections.ObjectModel.Collection<byte>();
        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            BytesToRead = serialPort1.BytesToRead;
            serialPort1.Read(BytesRead, 0, BytesToRead);
            this.Invoke(new EventHandler(DoUpdate));
        }

        public void DoUpdate(object sender, System.EventArgs e)
        {
            for (int i = 0; i <= BytesToRead - 1; i += 1)
            {
                ReceivedDataPacket.Add(BytesRead[i]);
            }

            if (ReceivedDataPacket.Count >= 14)
            {
                byte[] ACKSequence = {16,6};
                serialPort1.Write(ACKSequence, 0, 2);
                richTextBox1.Clear();
                for (int i = 0; i <= ReceivedDataPacket.Count - 1; i += 1)
                {
                    richTextBox1.Text = richTextBox1.Text + "\n Index" + i + " = " + ReceivedDataPacket[i];
                }
                ReceivedDataPacket.Clear();
            }


        }


    }
}
