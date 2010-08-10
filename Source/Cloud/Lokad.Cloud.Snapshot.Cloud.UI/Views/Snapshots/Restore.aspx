<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<Lokad.Cloud.Snapshot.Cloud.Reports.CompleteSnapshotReport>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Snapshots: Restore
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Restore Snapshot</h2>
	<p>Note that changes are not visible immediately!</p>
    <h3>Are you sure you want to restore this?</h3>
    <fieldset>
        <legend>Snapshot to restore</legend>
        
        <div class="display-label">Storage Account</div>
        <div class="display-field"><%= Html.Encode(Model.AccountName) %></div>

        <div class="display-label">Completed</div>
        <div class="display-field"><%= Html.Encode(String.Format("{0:g}", Model.Completed)) %></div>
        
        <div class="display-label">Id</div>
        <div class="display-field"><%= Html.Encode(Model.SnapshotId) %></div>
    </fieldset>
	<fieldset>
        <legend>Where to restore to</legend>
		<% using (Html.BeginForm()) { %>
		<p>
            <label for="RestoreAccountName">Storage Account Name:</label>
            <%= Html.TextBox("RestoreAccountName", Model.AccountName, new { style = "width:200px" })%><br />
        </p>
		<p>
            <label for="RestoreAccountKey">Storage Account Key:</label>
            <%= Html.TextBox("RestoreAccountKey", "", new { style = "width:700px" })%><br />
        </p>
        <p>
		    <input type="submit" value="Restore" />
			| <%= Html.ActionLink("Back to List", "Index")%>
        </p>
		<% } %>
    </fieldset>
</asp:Content>

