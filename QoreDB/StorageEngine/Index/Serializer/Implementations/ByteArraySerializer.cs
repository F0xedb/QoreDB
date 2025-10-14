using QoreDB.StorageEngine.Index.Interfaces;
using System.Text;

namespace QoreDB.StorageEngine.Index.Serializer.Implementations
{
    /// <summary>
    /// A simple serializer for byte arrays values
    /// </summary>
    public class ByteArraySerializer : ISerializer<byte[]>
    {
        public byte[] Serialize(byte[] obj)
            => obj;

        public byte[] Deserialize(byte[] data)
            => data;
    }
}
