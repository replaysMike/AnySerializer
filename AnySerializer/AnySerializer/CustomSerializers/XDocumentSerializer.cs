using System.Xml.Linq;

namespace AnySerializer.CustomSerializers
{
    public class XDocumentSerializer : ICustomSerializer<XDocument>
    {
        public short DataSize => sizeof(long);

        public XDocument Deserialize(byte[] bytes, uint length)
        {
            var xmlString = System.Text.ASCIIEncoding.Unicode.GetString(bytes);

            return XDocument.Parse(xmlString, LoadOptions.PreserveWhitespace);
        }

        public byte[] Serialize(XDocument type)
        {
            return System.Text.ASCIIEncoding.Unicode.GetBytes(type.ToString(SaveOptions.DisableFormatting));
        }

        public byte[] Serialize(object type)
        {
            return Serialize((XDocument)type);
        }

        object ICustomSerializer.Deserialize(byte[] bytes, uint length)
        {
            return Deserialize(bytes, length);
        }
    }
}
