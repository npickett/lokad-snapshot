<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<Lokad.Cloud.Snapshot.Cloud.Reports.CompleteSnapshotReport>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Snapshots: New Snapshot
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Create a new snapshot</h2>
	<p>Note that changes are not visible immediately!</p>
    <% using (Html.BeginForm()) {%>
        <fieldset>
            <legend>New Snapshot</legend>
			<p>
                <label for="AccountName">Account Name:</label>
                <%= Html.TextBox("AccountName", "", new { style = "width:200px" })%><br />
            </p>
			<p>
                <label for="AccountKey">Account Key:</label>
                <%= Html.TextBox("AccountKey", "", new { style = "width:700px" })%><br />
            </p>
            <p>
                <input type="submit" value="Create" />
            </p>
        </fieldset>
    <% } %>
    <p><%= Html.ActionLink("Back to List", "Index") %></p>
</asp:Content>

