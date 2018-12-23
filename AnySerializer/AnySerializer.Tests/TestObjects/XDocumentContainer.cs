using System;
using System.Xml.Linq;

namespace AnySerializer.Tests.TestObjects
{
    public class XDocumentContainer : IEquatable<XDocumentContainer>
    {
        public int Id { get; }
        public XDocument Document { get; }
        public string DocumentName { get; }
        public XDocumentContainer(int id, string documentName, XDocument document)
        {
            Id = id;
            DocumentName = documentName;
            Document = document;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj.GetType() != typeof(XDocumentContainer))
                return false;
            return Equals((XDocumentContainer)obj);
        }

        public bool Equals(XDocumentContainer other)
        {
            if (other == null)
                return false;
            return Id == other.Id
                && DocumentName == other.DocumentName
                && Document.ToString() == other.Document.ToString();
        }
    }
}
