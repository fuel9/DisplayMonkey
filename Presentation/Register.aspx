<%--
* DisplayMonkey source file
* http://displaymonkey.org
*
* Copyright (c) 2015 Fuel9 LLC and contributors
*
* Released under the MIT license:
* http://opensource.org/licenses/MIT
--%>

<%@ Page Language="C#" AutoEventWireup="true" masterpagefile="~/HomePage.Master" CodeBehind="Register.aspx.cs" Inherits="DisplayMonkey.Register" %>

<asp:Content ID="content1" runat="server" ContentPlaceHolderID="ContentPlaceHolder1">
        <form id="form1" runat="server">
            <h2>Register this display</h2>

            <div id="RegisterDisplay">
                <fieldset>
                    <legend>This Display</legend>

                    <div class="editor-label">
                        <label for="labelHost">Network address</label>
                    </div>
                    <div class="editor-field">
                        <asp:Label ID="labelHost" runat="server" Text="None Found"></asp:Label>
                    </div>

                    <div class="editor-label">
                        <label for="textName">Name</label>
                    </div>
                    <div class="editor-field">
                        <asp:TextBox ID="textName" runat="server" MaxLength="50" ToolTip="Enter a name to call this display"></asp:TextBox>
                    </div>

                    <div class="editor-label">
                        <label for="listCanvas">Canvas</label>
                    </div>
                    <div class="editor-field select">
                        <asp:ListBox ID="listCanvas" runat="server" Rows="1"></asp:ListBox>
                    </div>

                    <div class="editor-label">
                        <label for="listLocation">Location</label>
                    </div>
                    <div class="editor-field select">
                        <asp:ListBox ID="listLocation" runat="server" Rows="1"></asp:ListBox>
                    </div>

                <p>
                    <asp:Button ID="buttonRegister" runat="server" Text="Register" onclick="Register_Click" />
                </p>
                </fieldset>
            </div>
        </form>
</asp:Content>
