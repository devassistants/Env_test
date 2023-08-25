using System;using System.Collections.Generic;

namespace LibDataExchange.Parameters
{
    public class RowReadResult
    {

        #region Constructors

        public RowReadResult(int rowIndex, List<object> values)
        {
            RowIndex = rowIndex;
            Values = values;
        }

        #endregion

        #region Properties

        public int RowIndex { get; set; }

        public List<object> Values { get; set; }

        #endregion

    }
}
