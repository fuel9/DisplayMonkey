<%@ Register src="ErrorControl.ascx" tagname="ErrorControl" tagprefix="uc1" %>
<%--
* DisplayMonkey source file
* http://displaymonkey.org
*
* Copyright (c) 2015 Fuel9 LLC and contributors
*
* Released under the MIT license:
* http://opensource.org/licenses/MIT
--%>

<%@ Page Language="C#" AutoEventWireup="true" masterpagefile="~/HomePage.Master" CodeBehind="Default.aspx.cs" Inherits="DisplayMonkey._Default" %>

<asp:Content ID="content1" runat="server" ContentPlaceHolderID="ContentPlaceHolder1">
    <form id="form1" runat="server">
        <h2><%=DisplayMonkey.Language.Resources.SelectRegisteredDisplay%></h2>

        <div id="WebPageLinks">
            <fieldset>
                <legend><%=DisplayMonkey.Language.Resources.RegisteredDisplays%></legend>
                <ul>
                    <asp:Label ID="labelDisplays" runat="server"></asp:Label>
                </ul>
            </fieldset>
        </div>

        <uc1:ErrorControl ID="ctrError" runat="server" />

    </form>
</asp:Content>
