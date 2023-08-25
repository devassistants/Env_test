using System.Collections.Generic;

namespace LibDataExchange.Parameters
{
    public class RowAnalyseResult
    {

        #region Constructors

        public RowAnalyseResult(RowTypes rowType, object headerValue = null, Dictionary<string, int> columnMapping = null)
        {
            RowType = rowType;
            HeaderValue = headerValue;
            ColumnMapping = columnMapping;
        }

        #endregion

        #region Enums

        public enum RowTypes
        {
            Unknown = 0,
            HeaderRow = 1,
            DataRow = 2,
            EmptyRow = 3
        }

        #endregion

        #region Properties

        public RowTypes RowType { get; private set; } = RowTypes.Unknown;

        public object HeaderValue { get; private set; }

        public Dictionary<string, int> ColumnMapping { get; private set; }

        #endregion

    }
}
