<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="DisplayMonkey._Default" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div id="RegisterDisplay">
    
        Display Address:
        <asp:Label ID="lb_Display_Address" runat="server" Text="None Found"></asp:Label><br />Display 
        Name: 
        <asp:TextBox ID="tB_Display_Name" runat="server" MaxLength="50" 
            ToolTip="Enter a name to call this display"></asp:TextBox><br />Display Type:<asp:RadioButton 
            ID="Portrait" runat="server" GroupName="DisplayType" Text="Portrait" 
            Checked="True" /><asp:RadioButton
                ID="Landscape" runat="server" Text="Landscape" 
            GroupName="DisplayType" /><br />
        <asp:Button ID="Register" runat="server" Text="Register" 
            onclick="Register_Click" />
        <br /><hr />
    
    </div>
    <div id="WebPageLinks">Active 
    Displays<br />
    <asp:Label ID="RegisteredDisplays" runat="server"></asp:Label><hr /></div>
    </form>
</body>
</html>
