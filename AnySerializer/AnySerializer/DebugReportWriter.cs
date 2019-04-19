using System;
using System.Text;
using TypeSupport;
using static AnySerializer.TypeManagement;

namespace AnySerializer
{
    /// <summary>
    /// Used for creating diagnostic logs
    /// </summary>
    public class DebugReportWriter
    {
        protected StringBuilder _debugWriter;

        public DebugReportWriter()
        {
            _debugWriter = new StringBuilder();
        }

        /// <summary>
        /// Write to the log
        /// </summary>
        /// <param name="value"></param>
        public void Write(string value) => _debugWriter.Append(value);

        /// <summary>
        /// Write to the log
        /// </summary>
        /// <param name="value"></param>
        public void WriteLine(string value) => _debugWriter.AppendLine(value);

        /// <summary>
        /// Write to the log
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="typeSupport"></param>
        /// <param name="typeId"></param>
        /// <param name="currentDepth"></param>
        /// <param name="path"></param>
        /// <param name="index"></param>
        /// <param name="dataLength"></param>
        /// <param name="objectReferenceId"></param>
        /// <param name="typeDescriptorId"></param>
        /// <param name="hashCode"></param>
        public void WriteLine(long pos, ExtendedType typeSupport, TypeId typeId, int currentDepth, string path, int index, int dataLength, ushort objectReferenceId, ushort typeDescriptorId, int hashCode)
        {
            var pathParts = path.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            var pathLocation = string.Empty;
            if (pathParts.Length > 0)
                pathLocation = pathParts[pathParts.Length - 1];
            _debugWriter.AppendLine($"{(Indent(currentDepth) + pathLocation)} [{index}] {typeDescriptorId} {typeId} {typeSupport.Name} {pos} {dataLength} {objectReferenceId} {hashCode}");
        }

        private string Indent(int tabCount)
        {
            if (tabCount == 0) return string.Empty;
            var str = "";
            for (var i = 0; i < tabCount; i++)
                str += "  ";
            return str;
        }

        public override string ToString()
        {
            var format = "# Format: [arrayindex] TypeDescriptorId TypeId ActualType DataPosition DataLength ObjectReferenceId Hashcode";
            return $"{format}\r\n{{\r\n{_debugWriter.ToString()}}}\r\n";
        }
    }
}
