
using DotNetNuke.Instrumentation;
using DotNetNuke.Modules.SQL.Components;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for SQLProvider
/// </summary>
/// 
namespace Satrabel.HttpModules.Provider
{
    public class SqlUrlRuleProvider : UrlRuleProvider
    {
        protected static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SqlUrlRuleProvider));
        public SqlUrlRuleProvider()
        {

        }
        public override List<UrlRule> GetRules(int PortalId)
        {
            List<UrlRule> Rules = new List<UrlRule>();
            SqlQueryController sqlCtrl = new SqlQueryController();
            var queries = sqlCtrl.GetQueries().Where(q => q.Name.StartsWith("OpenUrlRewriter")); ;
            try
            {
                foreach (var query in queries)
                {
                    //SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["SiteSqlServer"].ConnectionString);
                    SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings[query.ConnectionStringName].ConnectionString);
                    SqlCommand cmd = new SqlCommand();
                    SqlDataReader reader;
                    //cmd.CommandText = "SELECT * FROM OpenUrlRewriter_CustomUrlRule WHERE PortalId = " + PortalId;
                    cmd.CommandText = query.Query;
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = sqlConnection;
                    cmd.Parameters.Add("@PortalId", SqlDbType.Int);
                    cmd.Parameters["@PortalId"].Value = PortalId;

                    sqlConnection.Open();
                    reader = cmd.ExecuteReader();
                    try
                    {
                        while (reader.Read())
                        {
                            var rule = new UrlRule
                            {
                                RuleType = UrlRuleType.Module,
                                CultureCode = ColumnExists(reader, "CultureCode") ? (string)reader["CultureCode"] : null,
                                TabId = ColumnExists(reader, "TabId") ? (int)reader["TabId"] : 0,
                                Parameters = ColumnExists(reader, "Parameters") ? (string)reader["Parameters"] : "",
                                Action = ColumnExists(reader, "RuleAction") ? (UrlRuleAction)reader["RuleAction"] : UrlRuleAction.Rewrite,
                                Url = CleanupUrl((string)reader["Url"]),
                                RemoveTab = ColumnExists(reader, "RemoveTab") ? (bool)reader["RemoveTab"] : false,
                                RedirectDestination = ColumnExists(reader, "RedirectDestination") ? (string)reader["RedirectDestination"] : null,
                                RedirectStatus = ColumnExists(reader, "RedirectStatus") ? (int)reader["RedirectStatus"] : 0,
                                InSitemap = ColumnExists(reader, "InSitemap") ? (bool)reader["InSitemap"] : true
                            };
                            Rules.Add(rule);
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(" SqlUrlRuleProvider error : " + ex.Message, ex);
            }


            return Rules;
        }

        public bool ColumnExists(IDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }
            return false;
            /*
            return reader.GetSchemaTable()
                         .Rows
                         .OfType<DataRow>()
                         .Any(row => row["ColumnName"] == columnName);
             */
        }

    }
}