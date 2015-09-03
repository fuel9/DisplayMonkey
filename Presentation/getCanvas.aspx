<%--
* DisplayMonkey source file
* http://displaymonkey.org
*
* Copyright (c) 2015 Fuel9 LLC and contributors
*
* Released under the MIT license:
* http://opensource.org/licenses/MIT
--%>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="getCanvas.aspx.cs" Inherits="DisplayMonkey.getCanvas" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title><asp:Literal ID="lTitle" runat="server" EnableViewState="False"></asp:Literal></title>

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
	
    <asp:Literal ID="lHead" runat="server" Mode="PassThrough" EnableViewState="False"></asp:Literal>
</head>
<body>
    <form id="form1" runat="server">
    <asp:Literal ID="lContent" runat="server" EnableViewState="False" 
		ViewStateMode="Disabled" Mode="PassThrough"></asp:Literal>
    </form>
</body>
</html>
