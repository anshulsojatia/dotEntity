using System.Collections.Generic;

namespace DotEntity
{
    public class QueryCache : IWrappedDisposable
    {
        internal QueryCache()
        {
            
        }

        private bool _disposed = false;
        public void Dispose()
        {
            _disposed = true;
        }

        public bool IsDisposed()
        {
            return _disposed;
        }

        internal string CachedQuery { get; set; }

        internal object[] ParameterValues { get; set; }

        private IList<QueryInfo> _queryInfos;
        internal IList<QueryInfo> QueryInfo
        {
            get
            {
                if (_queryInfos == null)
                    return null;
                for (var i = 0; i < ParameterValues.Length; i++)
                {
                    var qi = _queryInfos[i];
                    if (!qi.SupportOperator && !qi.IsPropertyValueAlsoProperty)
                    {
                        qi.PropertyValue = ParameterValues[i];
                    }
                }
                return _queryInfos;
            }
            set => _queryInfos = value;
        }
    }
}