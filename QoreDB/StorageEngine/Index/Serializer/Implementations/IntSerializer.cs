using QoreDB.StorageEngine.Index.Interfaces;

namespace QoreDB.StorageEngine.Index.Serializer.Implementations
{
    /// <summary>
    /// A simple serializer for integer keys
    /// </summary>
    public class IntSerializer : ISerializer<int>
    {
        public byte[] Serialize(int obj)
        {
            return BitConverter.GetBytes(obj);
        }

        public int Deserialize(byte[] data)
        {
            return BitConverter.ToInt32(data, 0);
        }
    }
}
