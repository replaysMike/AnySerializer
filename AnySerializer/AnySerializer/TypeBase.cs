using AnySerializer.CustomSerializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TypeSupport.Extensions;

namespace AnySerializer
{
    public class TypeBase
    {
        protected uint _maxDepth;
        protected SerializerDataSettings _dataSettings;
        protected SerializerOptions _options;
        protected TypeDescriptors _typeDescriptors;
        protected IDictionary<Type, Lazy<ICustomSerializer>> _customSerializers;
        protected ICollection<object> _ignoreAttributes;
        protected ICollection<string> _ignorePropertiesOrPaths;
        protected DebugReportWriter _debugWriter;

        /// <summary>
        /// Returns true if object name should be ignored
        /// </summary>
        /// <param name="name">Property or field name</param>
        /// <param name="path">Full path to object</param>
        /// <param name="options">Comparison options</param>
        /// <param name="ignorePropertiesOrPaths">List of names or paths to ignore</param>
        /// <returns></returns>
        protected bool IgnoreObjectName(string name, string path, IEnumerable<CustomAttributeData> attributes = null)
        {
            var ignoreByNameOrPath = _ignorePropertiesOrPaths?.Contains(name) == true || _ignorePropertiesOrPaths?.Contains(path) == true;
            if (ignoreByNameOrPath)
            {
                return true;
            }
#if FEATURE_CUSTOM_ATTRIBUTES
            if (attributes?.Any(x => !_options.BitwiseHasFlag(SerializerOptions.DisableIgnoreAttributes) && (_ignoreAttributes.Contains(x.AttributeType) || _ignoreAttributes.Contains(x.AttributeType.Name))) == true)
            {
                return true;
            }
#else
            if (attributes?.Any(x => !_options.BitwiseHasFlag(SerializerOptions.DisableIgnoreAttributes) && (_ignoreAttributes.Contains(x.Constructor.DeclaringType) || _ignoreAttributes.Contains(x.Constructor.DeclaringType.Name))) == true)
            {
                return true;
            }
#endif
            return false;
        }
    }
}
