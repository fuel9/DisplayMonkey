<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="DisplayMonkey._Default" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Display Monkey</title>
    <link href="~/favicon.ico" rel="shortcut icon" type="image/x-icon" />
    <link rel="Stylesheet" href="~/styles/site.css" />

</head>
<body class="page_style">
<table width="100%" frame="void" cellpadding="0" cellspacing="0">
<tr>
<td class="left_right_style">
    &nbsp;</td>
<td class="center_style">
   <img style="position:relative; top:-20px" src="files/logo.png" alt="Display Monkey" ></td>
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
        <td><asp:RadioButtonList ID="radioCanvas" runat="server">
		</asp:RadioButtonList></td></tr>
		<tr><td>Location:</td>
        <td>
			<asp:RadioButtonList ID="radioLocation" runat="server">
			</asp:RadioButtonList>
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
