// #region Author Information
// // QueryInfo.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion
namespace SpruceFramework
{
    public class QueryInfo
    {
        public string LinkingOperator { get; set; }
        public string PropertyName { get; set; }
        public string ParameterName { get; set; }
        public object PropertyValue { get; set; }
        public string QueryOperator { get; set; }
        public bool IsPropertyValueAlsoProperty { get; set; }
        public bool SupportOperator { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryInfo" /> class.
        /// </summary>
        /// <param name="linkingOperator">The linking operator.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="propertyValue">The property value.</param>
        /// <param name="queryOperator">The query operator.</param>
        internal QueryInfo(string linkingOperator, string propertyName, object propertyValue, string queryOperator, string parameterName, bool isPropertyValueAlsoAProperty = false)
        {
            LinkingOperator = linkingOperator;
            PropertyName = propertyName;
            PropertyValue = propertyValue;
            QueryOperator = queryOperator;
            ParameterName = parameterName;
            IsPropertyValueAlsoProperty = isPropertyValueAlsoAProperty;
        }

        internal QueryInfo(bool supportOperator, string linkingOperator)
        {
            SupportOperator = supportOperator;
            LinkingOperator = linkingOperator;
        }

        public override string ToString()
        {
            return $@"Query Info:
                        Linking Operator: {LinkingOperator}
                        Query Operator: {QueryOperator}
                        Property Name: {PropertyName}
                        Property Value: {PropertyValue}
                        Parameter Name: {ParameterName}
                        Is Property: {IsPropertyValueAlsoProperty}
                        Support: {SupportOperator}";
        }
    }
}