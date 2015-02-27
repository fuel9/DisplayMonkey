<%@ Page Language="C#" AutoEventWireup="true" masterpagefile="~/HomePage.Master" CodeBehind="Default.aspx.cs" Inherits="DisplayMonkey._Default" %>

<asp:Content ID="content1" runat="server" ContentPlaceHolderID="ContentPlaceHolder1">
        <form id="form1" runat="server">
            <h2>Select registered display</h2>

            <div id="WebPageLinks">
                <fieldset>
                    <legend>Registered Displays</legend>
                    <ul>
                        <asp:Label ID="labelDisplays" runat="server"></asp:Label>
                    </ul>
                </fieldset>
            </div>
        </form>
</asp:Content>
