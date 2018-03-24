<%@ Page Language="C#" AutoEventWireup="true" Inherits="CMSModules_MessageBoards_Tools_Messages_Message_Edit"
    Theme="Default" MasterPageFile="~/CMSMasterPages/UI/Dialogs/ModalSimplePage.master"
    Title="Message - Edit" CodeFile="Message_Edit.aspx.cs" %>

<%@ Register Src="~/CMSModules/MessageBoards/Controls/Messages/MessageEdit.ascx"
    TagName="MessageEdit" TagPrefix="cms" %>

<asp:Content ID="cntBody" runat="server" ContentPlaceHolderID="plcContent">
    <div class="PageContent">
        <cms:MessageEdit ID="messageEditElem" runat="server" IsLiveSite="false" ModalMode="true" />
        <asp:Literal ID="ltlScript" runat="server" EnableViewState="false" />
    </div>
</asp:Content>