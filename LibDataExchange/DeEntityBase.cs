using LibDataExchange.Parameters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace LibDataExchange
{
    public class DeEntityBase
    {

        #region Members

        private static int uniqueIdCounter = 0;

        #endregion

        #region Constructors

        public DeEntityBase()
        {
            uniqueIdCounter++;
            UniqueID = uniqueIdCounter;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Unique identifier of entity per each instance.
        /// </summary>
        [JsonIgnore]
        public int UniqueID { get; private set; }

        [JsonIgnore]
        public int? RowIndex { get; set; }

        /// <summary>
        /// Value linked to entity from header row.
        /// </summary>
        [JsonIgnore]
        public object HeaderValue { get; private set; }

        /// <summary>
        /// Count of empty rows in data source before entity data.
        /// </summary>
        [JsonIgnore]
        public int EmptyRowsBefore { get; set; }

        /// <summary>
        /// Count of empty rows in data source before entity data.
        /// </summary>
        [JsonIgnore]
        public bool HeaderRowBefore { get; set; }

        /// <summary>
        /// Count of empty rows in data source after entity data.
        /// </summary>
        [JsonIgnore]
        public int EmptyRowsAfter { get; set; }

        /// <summary>
        /// List of last validation error messages per entity property value.
        /// </summary>
        [JsonIgnore]
        public Dictionary<string, string> ValidationMessagesError { get; protected set; } = new Dictionary<string, string>();

        /// <summary>
        /// List of last validation warning messages per entity property value.
        /// </summary>
        [JsonIgnore]
        public Dictionary<string, string> ValidationMessagesWarning { get; protected set; } = new Dictionary<string, string>();

        /// <summary>
        /// List of last validation info messages per entity property value.
        /// </summary>
        [JsonIgnore]
        public Dictionary<string, string> ValidationMessagesInfo { get; protected set; } = new Dictionary<string, string>();

        [JsonIgnore]
        public string ErrorMessages
        {
            get
            {
                if (ValidationMessagesError != null && ValidationMessagesError.Count > 0)
                {
                    return string.Join("; ", ValidationMessagesError.Values.ToList());
                }
                return null;
            }
        }

        [JsonIgnore]
        public string WarningMessages
        {
            get
            {
                if (ValidationMessagesWarning != null && ValidationMessagesWarning.Count > 0)
                {
                    return string.Join("; ", ValidationMessagesWarning.Values.ToList());
                }
                return null;
            }
        }

        [JsonIgnore]
        public string InfoMessages
        {
            get
            {
                if (ValidationMessagesInfo != null && ValidationMessagesInfo.Count > 0)
                {
                    return string.Join("; ", ValidationMessagesInfo.Values.ToList());
                }
                return null;
            }
        }

        #endregion

        #region Public

        /// <summary>
        /// Determines data exchange row code value for entity to help find duplicate entites.
        /// </summary>
        /// <param name="generalParms">General parameters of controller.</param>
        /// <returns>Data exchange row code value of entity.</returns>
        public virtual string GenerateRowCode(EntityMethodParm generalParms)
        {
            return UniqueID.ToString();
        }

        /// <summary>
        /// Generates list of column headers from source.
        /// </summary>
        /// <param name="generalParms">General parameters of controller.</param>
        /// <returns>List of column names in source.</returns>
        public virtual List<string> GetColumnHeaders(EntityMethodParm generalParms)
        {
            List<string> columnHeaders = new();

            columnHeaders = GetType().GetProperties()
                .Where(x => !string.IsNullOrEmpty(x.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName))
                .Select(x => x.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName ?? "")
                .ToList();
            return columnHeaders;
        }

        /// <summary>
        /// Generates list of properties in data source and ther column indexes.
        /// </summary>
        /// <param name="generalParms">General parameters of controller.</param>
        /// <param name="values">List of values from source header row.</param>
        /// <returns>List of properties and it's column indexes in source.</returns>
        public virtual Dictionary<string, int> GetColumnMapping(EntityMethodParm generalParms, List<object> values)
        {
            Dictionary<string, int> columnMapping = new();

            for (int i = 0; i < values.Count; i++)
            {
                if (values[i] == null || values[i].GetType() != typeof(string))
                    continue;
                PropertyInfo propertyInfo = GetType()
                    .GetProperties()
                    .Where(x => x.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName == values[i].ToString())
                    .FirstOrDefault();

                if (propertyInfo != null)
                {
                    if (columnMapping.ContainsKey(propertyInfo.Name))
                        throw new Exception($"Column {values[i]} repeated more than once in source.");
                    columnMapping.Add(propertyInfo.Name, i);
                }
            }
            return columnMapping;
        }

        /// <summary>
        /// Analyses row to determine row type, column mapping (if it is header row), header value if any.
        /// </summary>
        /// <param name="generalParms">General parameters of controller.</param>
        /// <param name="values">List of values from data source row.</param>
        /// <returns>Results of row analyse.</returns>
        public virtual RowAnalyseResult AnalyseRow(EntityMethodParm generalParms, List<object> values)
        {
            if (!values.Where(x => x != null).Any())
                return new RowAnalyseResult(RowAnalyseResult.RowTypes.EmptyRow);

            return new RowAnalyseResult(RowAnalyseResult.RowTypes.Unknown);
        }

        /// <summary>
        /// Loads data from data source row in to entity.
        /// </summary>
        /// <param name="generalParms">General parameters of controller.</param>
        /// <param name="rowIndex">Index of row in source.</param>
        /// <param name="values">List of values from data source row.</param>
        public virtual void LoadData(EntityMethodParm generalParms, int rowIndex, List<object> values)
        {
            RowIndex = rowIndex;
            HeaderValue = generalParms.HeaderValue;
            foreach (var columnMapping in generalParms.ColumnMapping)
            {
                PropertyInfo propertyInfo = GetType().GetProperties().Where(x => x.Name == columnMapping.Key).FirstOrDefault();

                if (propertyInfo != null)
                {
                    if (values.Count > columnMapping.Value && values[columnMapping.Value] != null)
                    {
                        if (propertyInfo.PropertyType.IsGenericType &&
                            propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)) // Is type nullable
                            propertyInfo.SetValue(this, Convert.ChangeType(values[columnMapping.Value], Nullable.GetUnderlyingType(propertyInfo.PropertyType)));
                        else
                            propertyInfo.SetValue(this, Convert.ChangeType(values[columnMapping.Value], propertyInfo.PropertyType));
                    }
                }
            }
        }

        public virtual string GenerateCustomExportString(EntityMethodParm generalParms)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
