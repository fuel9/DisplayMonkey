<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="getCanvas.aspx.cs" Inherits="DisplayMonkey.getCanvas" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title><asp:Literal ID="lTitle" runat="server" EnableViewState="False"></asp:Literal></title>
    <link href="~/favicon.ico" rel="shortcut icon" type="image/x-icon" />
	<asp:Literal ID="lHead" runat="server" Mode="PassThrough" EnableViewState="False"></asp:Literal>
</head>
<body>
    <form id="form1" runat="server">
    <asp:Literal ID="lContent" runat="server" EnableViewState="False" 
		ViewStateMode="Disabled" Mode="PassThrough"></asp:Literal>
    </form>
</body>
</html>
