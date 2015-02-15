<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="DisplayMonkey._Default" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Display Monkey</title>
    <link href="~/favicon.ico" rel="shortcut icon" type="image/x-icon" />
    <link rel="apple-touch-icon" sizes="57x57" href="~/files/apple-touch-icon-57x57.png">
    <link rel="apple-touch-icon" sizes="60x60" href="~/files/apple-touch-icon-60x60.png">
    <link rel="apple-touch-icon" sizes="72x72" href="~/files/apple-touch-icon-72x72.png">
    <link rel="apple-touch-icon" sizes="76x76" href="~/files/apple-touch-icon-76x76.png">
    <link rel="apple-touch-icon" sizes="114x114" href="~/files/apple-touch-icon-114x114.png">
    <link rel="apple-touch-icon" sizes="120x120" href="~/files/apple-touch-icon-120x120.png">
    <link rel="apple-touch-icon" sizes="144x144" href="~/files/apple-touch-icon-144x144.png">
    <link rel="apple-touch-icon" sizes="152x152" href="~/files/apple-touch-icon-152x152.png">
    <link rel="apple-touch-icon" sizes="180x180" href="~/files/apple-touch-icon-180x180.png">
    <link rel="icon" type="image/png" href="~/files/favicon-32x32.png" sizes="32x32">
    <link rel="icon" type="image/png" href="~/files/android-chrome-192x192.png" sizes="192x192">
    <link rel="icon" type="image/png" href="~/files/favicon-96x96.png" sizes="96x96">
    <link rel="icon" type="image/png" href="~/files/favicon-16x16.png" sizes="16x16">
    <link rel="manifest" href="~/files/manifest.json">
    <meta name="msapplication-TileColor" content="#cc0000">
    <meta name="msapplication-TileImage" content="~/files/mstile-144x144.png">
    <meta name="theme-color" content="#ffffff">
    <link rel="Stylesheet" href="~/styles/site.css" />

</head>
<body class="page_style">
<table width="100%" frame="void" cellpadding="0" cellspacing="0">
<tr>
<td class="left_right_style">
    &nbsp;</td>
<td class="center_style">
   <img class="logo" src="files/logo.png" alt="Display Monkey" ></td>
<td class="left_right_style">
    &nbsp;</td>
</tr>
<tr>
<td>
    &nbsp;</td>
<td>
    &nbsp;</td>
<td>
    &nbsp;</td>
</tr>
<tr>
<td>
    &nbsp;</td>
<td>
    <form id="form1" runat="server">
    <div id="RegisterDisplay"><fieldset><legend>This Display</legend><table>
    
        <tbody><tr><td>Address:</td>
        <td><asp:Label ID="labelHost" runat="server" Text="None Found"></asp:Label></td></tr>
		<tr><td>Name:</td>
        <td><asp:TextBox ID="textName" runat="server" MaxLength="50" 
            ToolTip="Enter a name to call this display"></asp:TextBox></td></tr>
		<tr><td>Canvas:</td>
        <td>
            <asp:ListBox ID="listCanvas" runat="server" Rows="1"></asp:ListBox>
            </td></tr>
		<tr><td>Location:</td>
        <td>
			<asp:ListBox ID="listLocation" runat="server" Rows="1"></asp:ListBox>
			</td></tr>
        <tr><td colspan="2" style="text-align:center"><asp:Button ID="buttonRegister" runat="server" Text="Register" 
            onclick="Register_Click" />
        </td></tr>
		</tbody></table></fieldset>
    
    </div>
    <div id="WebPageLinks"><fieldset><legend>Registered 
    Displays</legend>
    <ul><asp:Label ID="labelDisplays" runat="server"></asp:Label></ul></fieldset></div>
    </form>
</td>
<td>
    &nbsp;</td>
</tr>
</table>
</body>
</html>
