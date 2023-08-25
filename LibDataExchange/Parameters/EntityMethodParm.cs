using System.Collections.Generic;

namespace LibDataExchange.Parameters
{
    public class EntityMethodParm
    {

        #region Constructors

        public EntityMethodParm(int skippedFirstRows, 
            int skippedFirstColumns, 
            DeEntityBase previousEntity, 
            Dictionary<string, int> columnMapping = null,
            object headerValue = null)
        {
            SkippedFirstRows = skippedFirstRows;
            SkippedFirstColumns = skippedFirstColumns;
            PreviousEntity = previousEntity;
            HeaderValue = headerValue;
            if (columnMapping != null)
                ColumnMapping = columnMapping;
        }

        #endregion

        #region Properties

        public int SkippedFirstRows { get; private set; }

        public int SkippedFirstColumns { get; private set; }

        public DeEntityBase PreviousEntity { get; private set; }

        public object HeaderValue { get; private set; }

        public Dictionary<string, int> ColumnMapping { get; private set; } = new Dictionary<string, int>();

        #endregion

    }
}
