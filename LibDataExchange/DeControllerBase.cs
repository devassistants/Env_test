using ClosedXML.Excel;
using LibDataExchange.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibDataExchange
{
    public class DeControllerBase<T> where T : DeEntityBase
    {

        #region Constructors

        public DeControllerBase(int skippedFirstRows = 0, int skippedFirstColumns = 0)
        {
            EntityList = new List<T>();
            if (skippedFirstRows < 0)
                throw new ArgumentOutOfRangeException(nameof(skippedFirstRows), "Parameter must be number equal to or greather than zero.");
            SkippedFirstRows = skippedFirstRows;
            if (skippedFirstColumns < 0)
                throw new ArgumentOutOfRangeException(nameof(skippedFirstColumns), "Parameter must be number equal to or greather than zero.");
            SkippedFirstColumns = skippedFirstColumns;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Number of the first empty rows in data source.
        /// </summary>
        public int SkippedFirstRows { get; private set; }

        /// <summary>
        /// Number of the first empty columns in data source.
        /// </summary>
        public int SkippedFirstColumns { get; private set; }

        /// <summary>
        /// List of entities that controller methods refer to.
        /// </summary>
        public List<T> EntityList { get; set; }

        #endregion

        #region Private

        private RowReadResult ReadRowValues(IXLRangeRow row)
        {
            List<object> values = new();

            for (int i = SkippedFirstColumns + 1; i <= row.CellCount(); i++)
            {
                if (row.Cell(i).Value.IsBoolean)
                    values.Add((object)row.Cell(i).Value.GetBoolean());
                else if (row.Cell(i).Value.IsDateTime)
                    values.Add((object)row.Cell(i).Value.GetDateTime());
                else if (row.Cell(i).Value.IsNumber)
                    values.Add((object)row.Cell(i).Value.GetNumber());
                else if (row.Cell(i).Value.IsText)
                    values.Add((object)row.Cell(i).Value.GetText());
                else
                    values.Add(null);
            }
            return new RowReadResult(row.RowNumber(), values);
        }

        #endregion

        #region Public

        /// <summary>
        /// Reads excel file as data source and loads entity list.
        /// </summary>
        /// <param name="fullFilePath">FUll file path of a data source.</param>
        public async Task ReadExcelDataSourceAsync(string fullFilePath)
        {
            T previousEntity = null;
            T tempEntity = null;
            object headerValue = null;
            Dictionary<string, int> columnMapping = null;
            List<Task<RowReadResult>> tasks = new();

            using var workbook = new XLWorkbook(fullFilePath);
            var worksheet = workbook.Worksheet(1);
            var rows = worksheet.RangeUsed().RowsUsed(XLCellsUsedOptions.All).Skip(SkippedFirstRows);
            
            foreach (var row in rows)
            {
                tasks.Add(Task.Run(() => ReadRowValues(row)));
            }
            var result = await Task.WhenAll(tasks);

            foreach (RowReadResult row in result.OrderBy(x => x.RowIndex))
            {
                EntityMethodParm generalParms = new(SkippedFirstRows, SkippedFirstColumns, previousEntity, columnMapping, headerValue);

                tempEntity = Activator.CreateInstance(typeof(T)) as T;
                if (tempEntity == null)
                    throw new Exception($"Creation of instace of type '{typeof(T).FullName}' filed.");
                RowAnalyseResult analyseResult = tempEntity.AnalyseRow(generalParms, row.Values);

                if (analyseResult.RowType == RowAnalyseResult.RowTypes.HeaderRow)
                {
                    // Set current entity
                    tempEntity.HeaderRowBefore = true;
                    // Add previous to list
                    if (previousEntity != null && !EntityList.Contains(previousEntity))
                        EntityList.Add(previousEntity);
                    // Save current as previous
                    previousEntity = tempEntity;
                    // Save header value
                    headerValue = analyseResult.HeaderValue;
                    columnMapping = analyseResult.ColumnMapping;
                }
                else if (analyseResult.RowType == RowAnalyseResult.RowTypes.DataRow)
                {
                    if (previousEntity != null)
                    {
                        if (previousEntity.RowIndex.HasValue)
                        {
                            // Add previous to list
                            if (!EntityList.Contains(previousEntity))
                                EntityList.Add(previousEntity);
                            // Set current entity
                            tempEntity.LoadData(generalParms, row.RowIndex, row.Values);
                            // Save current as previous
                            previousEntity = tempEntity;
                        }
                        else
                            // Load data to previous entity
                            previousEntity.LoadData(generalParms, row.RowIndex, row.Values);
                    }
                    else
                    {
                        // Set current entity
                        tempEntity.LoadData(generalParms, row.RowIndex, row.Values);
                        // Save current as previous
                        previousEntity = tempEntity;
                    }
                }
                else if (analyseResult.RowType == RowAnalyseResult.RowTypes.EmptyRow)
                {
                    if (previousEntity == null)
                    {
                        tempEntity.EmptyRowsBefore++;
                        previousEntity = tempEntity;
                    }
                    else
                        previousEntity.EmptyRowsAfter++;
                }
                else
                    throw new Exception($"Analyse of row (index: {row.RowIndex}) cannot determine type of row.");
            }
            if (previousEntity != null && !EntityList.Contains(previousEntity))
                EntityList.Add(previousEntity);
        }

        public string GenerateCustomExportString()
        {
            string result = "";
            T previousEntity = null;

            foreach (T entity in EntityList)
            {
                result = $"{result}{entity.GenerateCustomExportString(new EntityMethodParm(SkippedFirstRows, SkippedFirstColumns, previousEntity, null, null))}";
                previousEntity = entity;
            }
            return result;
        }

        #endregion

    }
}
