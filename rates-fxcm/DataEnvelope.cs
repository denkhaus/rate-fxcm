using System.Text;

namespace ratesfxcm
{
    public class DataEnvelope
    {
        public byte[] Data { get; set; }

        public DataEnvelope(string data)
        {
            Data = Encoding.UTF8.GetBytes(data);
        }

        public string AsString()
        {
            return Encoding.UTF8.GetString(Data);
        }
    }
}