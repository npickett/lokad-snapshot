<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>
<%@ Register Assembly="DotNetOpenAuth" Namespace="DotNetOpenAuth.OpenId.RelyingParty" TagPrefix="rp" %>

<asp:Content ID="loginTitle" ContentPlaceHolderID="TitleContent" runat="server">
    Login
</asp:Content>

<asp:Content ID="loginContent" ContentPlaceHolderID="MainContent" runat="server">
	<h2>Login</h2>
    
    <% if (ViewData["Message"] != null) { %>
	<div style="border: solid 1px red">
		<%= Html.Encode(ViewData["Message"].ToString())%>
	</div>
	<% } %>
	<p>You must log in before entering the Members Area: </p>
	<form action="Authenticate?ReturnUrl=<%=HttpUtility.UrlEncode(Request.QueryString["ReturnUrl"]) %>" method="post">
	<label for="openid_identifier">OpenID: </label>
	<input id="openid_identifier" name="openid_identifier" size="40" />
	<input type="submit" value="Login" />
	</form>

	<script type="text/javascript">
		document.getElementById("openid_identifier").focus();
	</script>
	
</asp:Content>
