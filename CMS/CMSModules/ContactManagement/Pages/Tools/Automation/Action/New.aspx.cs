using System;

using CMS.OnlineMarketing;
using CMS.UIControls;
using CMS.WorkflowEngine;

// Breadcrumbs
[Breadcrumbs()]
[Breadcrumb(0, "cms.workflowaction.action.list", "~/CMSModules/ContactManagement/Pages/Tools/Automation/Action/List.aspx?issitemanager={?issitemanager?}", null)]
[Breadcrumb(1, "cms.workflowaction.action.new")]

[Help("ma_action_new")]
public partial class CMSModules_ContactManagement_Pages_Tools_Automation_Action_New : CMSContactManagementConfigurationPage
{
    protected override void OnPreInit(EventArgs e)
    {
        base.OnPreInit(e);

        // Only global administrator can access automation process actions
        if (!CurrentUser.IsGlobalAdministrator)
        {
            RedirectToAccessDenied(GetString("security.accesspage.onlyglobaladmin"));
        }
    }


    protected void Page_Load(object sender, EventArgs e)
    {
        editElem.ObjectType = WorkflowActionInfo.OBJECT_TYPE_AUTOMATION;
        editElem.AllowedObjects = ContactInfo.OBJECT_TYPE;
        editElem.CurrentAction.ActionWorkflowType = WorkflowTypeEnum.Automation;
    }
}
