<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<IEnumerable<Lokad.Cloud.Snapshot.Cloud.Reports.CompleteSnapshotReport>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Snapshots
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Available Snapshots</h2>
	<p>Only completed and available snapshots are shown. Note that changes are not visible immediately!</p>
    <table>
        <tr>
            <th></th>
            <th>Storage Account</th>
            <th>Completed</th>
        </tr>
    <% foreach (var item in Model) { %>
        <tr>
            <td>
                <%= Html.ActionLink("Delete", "Delete", new { account=item.AccountName, id=item.SnapshotId })%> |
				<%= Html.ActionLink("Restore", "Restore", new { account = item.AccountName, id = item.SnapshotId })%>
            </td>
            <td><%= Html.Encode(item.AccountName) %></td>
            <td><%= Html.Encode(String.Format("{0:g}", item.Completed)) %></td>
        </tr>
    <% } %>
    </table>
    <p><%= Html.ActionLink("Create New Snapshot", "Create") %></p>
</asp:Content>

