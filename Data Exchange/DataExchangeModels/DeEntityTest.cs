using LibDataExchange;
using LibDataExchange.Parameters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Data_Exchange.DataExchangeModels
{
    public class DeEntityTest : DeEntityBase
    {

        #region Properties

        public string Month { get; set; }

        [JsonProperty("Latin America")]
        public int? LatinAmericaRigCount { get; set; }

        [JsonProperty("Europe")]
        public int? EuropeRigCount { get; set; }

        [JsonProperty("Africa")]
        public int? AfricaRigCount { get; set; }

        [JsonProperty("Middle East")]
        public int? MiddleEastRigCount { get; set; }

        [JsonProperty("Asia Pacific")]
        public int? AsiaPacificRigCount { get; set; }

        [JsonProperty("Total Intl.")]
        public int? TotalIntlRigCount { get; set; }

        [JsonProperty("Canada")]
        public int? CanadaRigCount { get; set; }

        [JsonProperty("U.S.")]
        public int? USRigCount { get; set; }

        [JsonProperty("Total World")]
        public int? TotalWorldRigCount { get; set; }

        #endregion

        #region Private

        private static string GenerateBeginningOfCustomExportString(EntityMethodParm generalParms)
        {
            string result = "";

            for (int i = 0; i < generalParms.SkippedFirstColumns; i++)
            {
                result = $"{result},";
            }
            return result;
        }

        #endregion

        #region Override

        public override string GenerateRowCode(EntityMethodParm generalParms)
        {
            return $"{HeaderValue}_{Month}";
        }

        public override Dictionary<string, int> GetColumnMapping(EntityMethodParm generalParms, List<object> values)
        {
            Dictionary<string, int> result = base.GetColumnMapping(generalParms, values);

            for (int i = 0; i < values.Count; i++)
            {
                if (values[i] != null && values[i].GetType() == typeof(double))
                {
                    result.Add(nameof(Month), i);
                    continue;
                }
            }
            return result;
        }

        public override RowAnalyseResult AnalyseRow(EntityMethodParm generalParms, List<object> values)
        {
            RowAnalyseResult result = base.AnalyseRow(generalParms, values);

            if (result.RowType != RowAnalyseResult.RowTypes.EmptyRow)
            {
                if (values.Where(x => x != null && x.GetType() == typeof(string)).Count() > 1)
                {
                    List<string> columnHeaders = GetColumnHeaders(generalParms);
                    Dictionary<string, int> mapping = GetColumnMapping(generalParms, values);

                    if (values.Where(x => x != null && x.GetType() == typeof(string) && columnHeaders.Contains(x.ToString() ?? "")).Count() > 3)
                        result = new RowAnalyseResult(RowAnalyseResult.RowTypes.HeaderRow, values[0], mapping);
                    else
                        result = new RowAnalyseResult(RowAnalyseResult.RowTypes.DataRow);
                }
                else
                    result = new RowAnalyseResult(RowAnalyseResult.RowTypes.DataRow);
            }
            return result;
        }

        public override string GenerateCustomExportString(EntityMethodParm generalParms)
        {
            string result = "";
            string beginning = GenerateBeginningOfCustomExportString(generalParms);

            for (int i = 0; i < EmptyRowsBefore; i++)
            {
                result = $"{result}{beginning},,,,,,,,,{Environment.NewLine}";
            }
            if (HeaderRowBefore)
                result = $"{result}{beginning}{HeaderValue},{string.Join(",", GetColumnHeaders(generalParms))}{Environment.NewLine}";
            result = $"{result}{beginning}{Month},{LatinAmericaRigCount},{EuropeRigCount},{AfricaRigCount},{MiddleEastRigCount},{AsiaPacificRigCount},{TotalIntlRigCount},{CanadaRigCount},{USRigCount},{TotalWorldRigCount}{Environment.NewLine}";
            for (int i = 0; i < EmptyRowsAfter; i++)
            {
                result = $"{result}{beginning},,,,,,,,,{Environment.NewLine}";
            }
            return result;
        }

        #endregion

    }
}
