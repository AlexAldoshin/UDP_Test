using System;

namespace WebIoT.Models
{
    public class UserDevice
    {
        public string UserNameId { get; set; }
        public Guid UserReadKeyAPI { get; set; }
        public Guid UserKeyAPI { get; set; }
        public int IdDev { get; set; }
        public string DeviceName { get; set; }
        public string DeviceDescription { get; set; }
        public float DeviceLatitude { get; set; }
        public float DeviceLongitude { get; set; }
        public float DeviceElevation { get; set; }
        public byte[] DeviceImage { get; set; }
    }
}