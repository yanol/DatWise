using SafetyCompliance.Helpers;
using SafetyCompliance.Models;
using SafetyCompliance.Services;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SafetyCompliance
{
    public partial class AuditScan : Page
    {

        public IComplianceAgentService AgentService { get; set; }
        public ISystemLogger Logger { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Default date range: last 6 months → today
                txtDateFrom.Text = DateTime.Now.AddMonths(-6).ToString("yyyy-MM-dd");
                txtDateTo.Text = DateTime.Now.ToString("yyyy-MM-dd");

                lblOfficerName.Text = Session["UserName"]?.ToString() ?? "Guest";

                pnlResults.Visible = false;
                pnlError.Visible = false;
            }
        }

        protected void btnRunScan_Click(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(RunScanAsync));
        }

        private async Task RunScanAsync()
        {
            btnRunScan.Enabled = false;
            pnlResults.Visible = false;
            pnlError.Visible = false;

            if (!int.TryParse(Session["UserId"]?.ToString(), out int officerId))
                officerId = 5;

            if (!DateTime.TryParse(txtDateFrom.Text, out DateTime dateFrom))
                dateFrom = DateTime.Now.AddMonths(-6);
            if (!DateTime.TryParse(txtDateTo.Text, out DateTime dateTo))
                dateTo = DateTime.Now;

            SetStatus("Fetching compliance data...", "info");

            try
            {
                SetStatus("AI analyzing gaps...", "info");

                await Logger.LogAsync(1, "Start Scan", ddlAuditType.SelectedValue);

                var result = await AgentService.RunScanAsync(
                    ddlAuditType.SelectedValue, dateFrom, dateTo, officerId);

                if (result != null)
                {
                    if (result.Gaps != null && result.Gaps.Count > 0)
                    {
                        result.Gaps = result.Gaps
                            .OrderBy(g => GetSeverityPriority(g.Severity))
                            .ToList();
                    }

                    lblCriticalCount.Text = result.CriticalCount.ToString();
                    lblMediumCount.Text = result.MediumCount.ToString();
                    lblLowCount.Text = result.LowCount.ToString();
                    lblReadinessScore.Text = result.ReadinessScore + "%";

                    litAiSummary.Text = result.Summary;

                    gvGaps.DataSource = result.Gaps;
                    gvGaps.DataBind();

                    hfAuditId.Value = result.AuditId.ToString();

                    await Logger.LogAsync(officerId, "AuditScan.Complete",
                        $"Type:{ddlAuditType.SelectedValue} Gaps:{result.Gaps.Count} Score:{result.ReadinessScore}%");

                    SetStatus($"Scan Complete — {result.Gaps.Count} findings", "success");

                    pnlResults.Visible = true;
                    btnExportPdf.Visible = true;
                    btnExportExcel.Visible = true;
                }
            }
            catch (Exception ex)
            {
                await Logger.LogErrorAsync(officerId, "AuditScan.General", ex.Message);
                ShowError("An unexpected error occurred: " + ex.Message);
            }
            finally
            {
                btnRunScan.Enabled = true;
            }
        }

        private int GetSeverityPriority(string severity)
        {
            return (severity?.ToLower()) switch
            {
                "critical" => 1,
                "medium" => 2,
                "low" => 3,
                _ => 4
            };
        }

        private void BindResults(AuditResult result)
        {
            lblCriticalCount.Text = result.CriticalCount.ToString();
            lblMediumCount.Text = result.MediumCount.ToString();
            lblLowCount.Text = result.LowCount.ToString();
            lblReadinessScore.Text = $"{result.ReadinessScore}%";

            litAiSummary.Text = $@"
                <div class='ai-summary'>
                    <strong>AI Summary:</strong><br/>
                    {System.Web.HttpUtility.HtmlEncode(result.Summary)}
                </div>";

            
            gvGaps.DataSource = result.Gaps;
            gvGaps.DataBind();

            hfAuditId.Value = result.AuditId.ToString();
        }

        protected void gvGaps_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;

            var gap = (ComplianceGap)e.Row.DataItem;
            string sev = gap.Severity?.ToLower() ?? "";

            e.Row.CssClass = sev switch
            {
                "critical" => "row-critical",
                "medium" => "row-medium",
                "low" => "row-low",
                _ => string.Empty
            };
        }

        protected void btnExportPdf_Click(object sender, EventArgs e)
        {
            if (int.TryParse(hfAuditId.Value, out int auditId) && auditId > 0)
                Response.Redirect($"~/Handlers/ExportPdf.ashx?auditId={auditId}");
        }

        protected void btnExportExcel_Click(object sender, EventArgs e)
        {
            if (int.TryParse(hfAuditId.Value, out int auditId) && auditId > 0)
                Response.Redirect($"~/Handlers/ExportExcel.ashx?auditId={auditId}");
        }

        private void SetStatus(string message, string type)
        {
            lblStatus.Text = message;
            lblStatus.CssClass = $"status-{type}";
        }

        private void ShowError(string message)
        {
            lblError.Text = message;
            pnlError.Visible = true;
            pnlResults.Visible = false;
            SetStatus("Scan failed.", "error");
        }
    }
}