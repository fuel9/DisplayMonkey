<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="DisplayMonkey._Default" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div id="RegisterDisplay"><table>
    
        <tr><td>This Display Address:</td>
        <td><asp:Label ID="labelHost" runat="server" Text="None Found"></asp:Label></td></tr>
		<tr><td>This Display Name:</td>
        <td><asp:TextBox ID="textName" runat="server" MaxLength="50" 
            ToolTip="Enter a name to call this display"></asp:TextBox></td></tr>
		<tr><td>This Display Canvas:</td>
        <td><asp:RadioButtonList ID="radioCanvas" runat="server">
		</asp:RadioButtonList></td></tr>
		<tr><td>This Display Location:</td>
        <td>
			<asp:RadioButtonList ID="radioLocation" runat="server">
			</asp:RadioButtonList>
			</td></tr>
        <tr><td colspan="2" style="text-align:center"><asp:Button ID="buttonRegister" runat="server" Text="Register" 
            onclick="Register_Click" />
        </td></tr>
		</table>
    
    </div><hr />
    <div id="WebPageLinks">Registered 
    Displays<br />
    <asp:Label ID="labelDisplays" runat="server"></asp:Label><hr /></div>
    </form>
</body>
</html>
