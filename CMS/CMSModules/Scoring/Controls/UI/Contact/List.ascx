<%@ Control Language="C#" AutoEventWireup="true" CodeFile="List.ascx.cs" Inherits="CMSModules_Scoring_Controls_UI_Contact_List" %>

<%@ Register Src="~/CMSAdminControls/UI/UniGrid/UniGrid.ascx" TagName="UniGrid" TagPrefix="cms" %>
<%@ Register Namespace="CMS.UIControls.UniGridConfig" TagPrefix="ug" Assembly="CMS.UIControls" %>
<%@ Register Src="~/CMSFormControls/Filters/NumberFilter.ascx" TagName="NumberFilter"
    TagPrefix="cms" %>

<cms:CMSUpdatePanel ID="pnlUpdate" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
        <div class="form-horizontal form-filter">
            <div class="form-group">
                <div class="filter-form-label-cell">
                    <cms:LocalizedLabel runat="server" CssClass="control-label" ResourceString="om.score" DisplayColon="true"
                        EnableViewState="false" />
                </div>
                <div class="filter-form-value-cell-wide">
                    <cms:NumberFilter runat="server" ID="ucScoreFilter" />
                </div>
            </div>
            <div class="form-group form-group-buttons">
                <div class="filter-form-buttons-cell-wide">
                    <cms:LocalizedButton ID="btnSearch" runat="server" ResourceString="general.search"
                        ButtonStyle="Primary" EnableViewState="false" />
                </div>
            </div>
        </div>
        <cms:UniGrid runat="server" ID="gridElem" ShowObjectMenu="false">
            <GridActions>
                <ug:Action ExternalSourceName="edit" Name="edit" Caption="$om.contact.viewdetail$"
                    FontIconClass="icon-edit" FontIconStyle="Allow" ModuleName="CMS.OnlineMarketing" CommandArgument="ContactID" />
                <ug:Action Name="view" ExternalSourceName="view" Caption="$om.score.viewdetail$" FontIconClass="icon-eye" FontIconStyle="Allow"
                    ModuleName="OM.Scoring" Permissions="Read" CommandArgument="ContactID" />
            </GridActions>
            <GridColumns>
                <ug:Column Source="ContactID" ExternalSourceName="#contactfullname" Caption="$general.fullname$" Wrap="false">
                </ug:Column>
                <ug:Column Source="ContactID" ExternalSourceName="#statusdisplayname" Caption="$om.contactstatus$"
                    Wrap="false">
                </ug:Column>
                <ug:Column Source="Score" Caption="$om.score$" Wrap="false" />
                <ug:Column Width="100%" />
            </GridColumns>
            <GridOptions ShowSelection="true" SelectionColumn="ContactID" DisplayFilter="false" />
        </cms:UniGrid>
        <asp:Panel ID="pnlFooter" runat="server" CssClass="form-horizontal mass-action">
            <div class="form-group">
                <div class="mass-action-value-cell">
                    <cms:CMSDropDownList ID="drpWhat" runat="server" />
                    <cms:CMSDropDownList ID="drpAction" runat="server" />
                    <cms:LocalizedButton ID="btnOk" runat="server" ResourceString="general.ok" ButtonStyle="Primary"
                        EnableViewState="false" OnClick="btnOk_Click" />
                </div>
            </div>
            <asp:Label ID="lblInfo" runat="server" CssClass="InfoLabel" EnableViewState="false" />
        </asp:Panel>
        <asp:HiddenField ID="hdnIdentifier" runat="server" EnableViewState="false" />
    </ContentTemplate>
</cms:CMSUpdatePanel>