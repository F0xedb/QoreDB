using QoreDB.StorageEngine.Index.Interfaces;
using System.Text;

namespace QoreDB.StorageEngine.Index.Serializer.Implementations
{
    /// <summary>
    /// A simple serializer for string values
    /// </summary>
    public class StringSerializer : ISerializer<string>
    {
        public byte[] Serialize(string obj)
        {
            return Encoding.UTF8.GetBytes(obj);
        }

        public string Deserialize(byte[] data)
        {
            return Encoding.UTF8.GetString(data);
        }
    }
}
