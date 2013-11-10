using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solid.Arduino.Firmata
{
    public delegate void I2cReplyReceivedHandler(object par_Sender, FirmataEventArgs<I2cReply> par_EventArgs);

    public interface II2cProtocol
    {
        event I2cReplyReceivedHandler OnI2cReplyReceived;

        void I2cSetInterval(int microseconds);
        void I2cWrite(int slaveAddress, byte[] data);
        void I2cReadOnce(int slaveAddress, int bytesToRead);
        void I2cReadOnce(int slaveAddress, int slaveRegister, int bytesToRead);
        void I2cReadContinuous(int slaveAddress, int bytesToRead);
        void I2cReadContinuous(int slaveAddress, int slaveRegister, int bytesToRead);
        void I2cStopReading();
        I2cReply GetI2cReply(int slaveAddress, int bytesToRead);
        Task<I2cReply> GetI2cReplyAsync(int slaveAddress, int bytesToRead);
        I2cReply GetI2cReply(int slaveAddress, int slaveRegister, int bytesToRead);
        Task<I2cReply> GetI2cReplyAsync(int slaveAddress, int slaveRegister, int bytesToRead);
    }
}
