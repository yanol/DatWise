<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Auditscan.aspx.cs" Inherits="SafetyCompliance.AuditScan" Async="true" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Compliance Audit Scan</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <style>
        body { background-color: #f8f9fa; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; }
        .kpi-card { border-radius: 10px; transition: transform 0.2s; border: none; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }
        .kpi-card:hover { transform: translateY(-5px); }
        .severity-critical { border-left: 5px solid #dc3545; }
        .severity-medium { border-left: 5px solid #ffc107; }
        .severity-low { border-left: 5px solid #0dcaf0; }
        .row-critical { background-color: #f8d7da !important; }
        .row-medium { background-color: #fff3cd !important; }
        .row-low { background-color: #e0f7fa !important; }
        .status-info { color: #0d6efd; font-weight: bold; }
        .status-success { color: #198754; font-weight: bold; }
    </style>
</head>
<body class="container py-5">
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" />

        <div class="d-flex justify-content-between align-items-center mb-4">
            <h2 class="text-primary">Safety Compliance Audit</h2>
            <div class="text-muted">
                Logged in: <asp:Label ID="lblOfficerName" runat="server" CssClass="fw-bold" /> | 
                <asp:Label ID="lblStatus" runat="server" />
            </div>
        </div>

        <asp:Panel ID="pnlError" runat="server" Visible="false" CssClass="alert alert-danger">
            <asp:Label ID="lblError" runat="server" />
        </asp:Panel>

        <div class="card p-4 mb-4 shadow-sm">
            <div class="row g-3 align-items-end">
                <div class="col-md-3">
                    <label class="form-label">Audit Type</label>
                    <asp:DropDownList ID="ddlAuditType" runat="server" CssClass="form-select">
                        <asp:ListItem Value="full" Text="Full Scan" />
                        <asp:ListItem Value="training" Text="Training Records" />
                        <asp:ListItem Value="incidents" Text="Incidents" />
                        <asp:ListItem Value="permits" Text="Permits" />
                        <asp:ListItem Value="equipment" Text="Equipment" />
                    </asp:DropDownList>
                </div>
                <div class="col-md-3">
                    <label class="form-label">From</label>
                    <asp:TextBox ID="txtDateFrom" runat="server" TextMode="Date" CssClass="form-control" />
                </div>
                <div class="col-md-3">
                    <label class="form-label">To</label>
                    <asp:TextBox ID="txtDateTo" runat="server" TextMode="Date" CssClass="form-control" />
                </div>
                <div class="col-md-3">
                    <asp:Button ID="btnRunScan" runat="server" Text="Run AI Scan" CssClass="btn btn-primary w-100" OnClick="btnRunScan_Click" />
                </div>
            </div>
        </div>

        <asp:Panel ID="pnlResults" runat="server" Visible="false">
            <div class="row text-center mb-4">
                <div class="col-md-3">
                    <div class="card kpi-card p-3 bg-danger text-white">
                        <h6>Critical</h6>
                        <h3><asp:Label ID="lblCriticalCount" runat="server" Text="0" /></h3>
                    </div>
                </div>
                <div class="col-md-3">
                    <div class="card kpi-card p-3 bg-warning text-dark">
                        <h6>Medium</h6>
                        <h3><asp:Label ID="lblMediumCount" runat="server" Text="0" /></h3>
                    </div>
                </div>
                <div class="col-md-3">
                    <div class="card kpi-card p-3 bg-info text-white">
                        <h6>Low</h6>
                        <h3><asp:Label ID="lblLowCount" runat="server" Text="0" /></h3>
                    </div>
                </div>
                <div class="col-md-3">
                    <div class="card kpi-card p-3 bg-success text-white">
                        <h6>Readiness Score</h6>
                        <h3><asp:Label ID="lblReadinessScore" runat="server" Text="0%" /></h3>
                    </div>
                </div>
            </div>

            <div class="card mb-4 border-primary">
                <div class="card-header bg-primary text-white">AI Executive Summary</div>
                <div class="card-body">
                    <asp:Literal ID="litAiSummary" runat="server" />
                </div>
            </div>

            <div class="table-responsive shadow-sm">
                <asp:GridView ID="gvGaps" runat="server" AutoGenerateColumns="false" 
                    CssClass="table table-hover table-bordered bg-white" 
                    OnRowDataBound="gvGaps_RowDataBound">
                    <Columns>
                        <asp:BoundField DataField="Domain" HeaderText="Domain" HeaderStyle-CssClass="bg-light" />
                        <asp:BoundField DataField="Finding" HeaderText="Finding" HeaderStyle-CssClass="bg-light" />
                        <asp:BoundField DataField="Severity" HeaderText="Severity" HeaderStyle-CssClass="bg-light" />
                        <asp:BoundField DataField="Deadline" HeaderText="Deadline" HeaderStyle-CssClass="bg-light" />
                        <asp:BoundField DataField="Recommendation" HeaderText="Recommendation" HeaderStyle-CssClass="bg-light" />
                    </Columns>
                </asp:GridView>
            </div>

            <div class="mt-4 d-flex gap-2 justify-content-end">
                <asp:Button ID="btnExportPdf" runat="server" Text="Export PDF" CssClass="btn btn-outline-danger" OnClick="btnExportPdf_Click" />
                <asp:Button ID="btnExportExcel" runat="server" Text="Export Excel" CssClass="btn btn-outline-success" OnClick="btnExportExcel_Click" />
            </div>
            <asp:HiddenField ID="hfAuditId" runat="server" />
        </asp:Panel>
    </form>
</body>
</html>