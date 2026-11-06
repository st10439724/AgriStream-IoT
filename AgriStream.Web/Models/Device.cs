namespace AgriStream.Web.Models
{
    public class Device
    {
        public string DeviceId { get; set; }

        public string FieldId { get; set; }

        public string Status { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
