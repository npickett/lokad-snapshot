<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<Lokad.Cloud.Snapshot.Cloud.Reports.CompleteSnapshotReport>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Snapshots: Delete
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Delete Snapshot</h2>
	<p>Note that changes are not visible immediately!</p>
    <h3>Are you sure you want to delete this?</h3>
    <fieldset>
        <legend>Snapshot to delete</legend>

        <div class="display-label">Storage Account</div>
        <div class="display-field"><%= Html.Encode(Model.AccountName) %></div>

        <div class="display-label">Completed</div>
        <div class="display-field"><%= Html.Encode(String.Format("{0:g}", Model.Completed)) %></div>
		
        <div class="display-label">Id</div>
        <div class="display-field"><%= Html.Encode(Model.SnapshotId) %></div>
    </fieldset>
    <% using (Html.BeginForm()) { %>
        <p>
		    <input type="submit" value="Delete" /> |
		    <%= Html.ActionLink("Back to List", "Index") %>
        </p>
    <% } %>
</asp:Content>

