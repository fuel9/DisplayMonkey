<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="DisplayMonkey._Default" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title></title>
    <style type="text/css">
        .style1
        {
            width: 880px;
            height: 123px;
            background-color: #376092;
            
        }
  
        .style2
        {
            width: auto;
            background-color: #376092;
        }
  
        .style3
        {
            margin: 0;
            padding: 0;
            min-height: 100%;
            position: relative;
            font-family: "Open Sans", Helvetica, Arial, Verdana, sans-serif;
            src: url(http://themes.googleusercontent.com/static/fonts/opensans/v7/cJZKeOuBrn4kERxqtaUH3T8E0i7KZn-EPnyo3HZu7kw.woff);
            font-size: 0.85em;
            color: #333;
        }
  
         a:link, a:visited, a:active, a:hover 
        {
        color: #c00000;
        }
  
        @font-face
        {
        font-family: "Open Sans";
        src: url(http://themes.googleusercontent.com/static/fonts/opensans/v7/cJZKeOuBrn4kERxqtaUH3T8E0i7KZn-EPnyo3HZu7kw.woff);
        }
 
        input[type="submit"],
        button 
        {
        background-color: #d3dce0;
        border: 1px solid #787878;
        cursor: pointer;
        font-size: 1.0em;
        font-weight: 600;
        padding: 4px;
        width: auto;
        float:left;
        border-radius: 5px;
        margin-right: 4px
        }
        
    </style>
    
<link rel="shortcut icon" type="image/x-icon" href="files/favicon.ico" />

</head>
<body class="style3">
<table width="100%" frame="void" cellpadding="0" cellspacing="0">
<tr>
<td class="style2">
    &nbsp;</td>
<td class="style1">
   <img style="position:relative; top:-20px" src="files/logo.png" alt="Display Monkey" ></td>
<td class="style2">
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
    
    </div><p />
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
