﻿using System;
using System.Web.UI;

using CMS.Base;
using CMS.DataEngine;
using CMS.ExtendedControls;
using CMS.FormControls;
using CMS.Helpers;
using CMS.IO;

public partial class CMSModules_DocumentTypes_FormControls_DocumentTypeIconSelector : FormEngineUserControl, IPostBackEventHandler
{
    #region "Constants"

    private const string DEFAULT_IMAGE_CLASS_NAME = "cms_noimage";

    #endregion


    #region "Variables"

    private IconTypeEnum iconType = IconTypeEnum.Files;
    private BaseInfo mFormObject = null;
    private DataClassInfo mDocumentType = null;

    #endregion


    #region "Properties"

    /// <summary>
    /// Form engine user control value. 
    /// </summary>
    public override object Value
    {
        get
        {
            return iconType == IconTypeEnum.CssClass ? IconCssClass : null;
        }
        set
        {
            txtCssClass.Text = ValidationHelper.GetString(value, null);
        }
    }


    /// <summary>
    /// Icon css class.
    /// </summary>
    private string IconCssClass
    {
        get
        {
            if (string.IsNullOrEmpty(txtCssClass.Text))
            {
                return DocumentType.ClassIsCoupledClass ? "icon-doc-o" : "icon-folder-o";
            }
            return HTMLHelper.EncodeQuotes(txtCssClass.Text);
        }
    }


    /// <summary>
    /// Edited document type
    /// </summary>
    private DataClassInfo DocumentType
    {
        get
        {
            if ((mDocumentType == null) && (Form != null))
            {
                mDocumentType = Form.EditedObject as DataClassInfo;
            }

            return mDocumentType;
        }
    }


    /// <summary>
    /// Currently edited object in the form.
    /// </summary>
    private BaseInfo FormObject
    {
        get
        {
            if ((mFormObject == null) && (Form != null))
            {
                mFormObject = Form.Data as BaseInfo;
            }

            return mFormObject;
        }
    }

    #endregion


    #region "Page events"

    /// <summary>
    /// Page_Load event handler
    /// </summary>
    protected void Page_Load(object sender, EventArgs e)
    {
        // Hide the control if it shouldn't be processed or required document type data are not available
        if (StopProcessing || (DocumentType == null) || (FormObject == null))
        {
            pnlIcons.Visible = false;
            lblControlHidden.Visible = true;
            return;
        }

        lstOptions.SelectedIndexChanged += lstOptions_SelectedIndexChanged;

        if (Form != null)
        {
            InitializeControl();

            iconType = lstOptions.SelectedValue.ToEnum<IconTypeEnum>();
        }

        CheckFieldEmptiness = false;
    }


    /// <summary>
    /// OnPreRender event handler.
    /// </summary>
    protected override void OnPreRender(EventArgs e)
    {
        base.OnPreRender(e);

        // Setup control visibility
        switch (iconType)
        {
            case IconTypeEnum.CssClass:
                plcUploaders.Visible = false;
                plcCssClass.Visible = true;
                break;

            case IconTypeEnum.Files:
                plcUploaders.Visible = true;
                plcCssClass.Visible = false;
                break;
        }

        if (plcUploaders.Visible && (DocumentType != null))
        {
            string className = DocumentType.ClassName;

            // Small icon
            string imageUrl = GetIconUrl(className);
            imageUrl = URLHelper.AddParameterToUrl(imageUrl, "chset", Guid.NewGuid().ToString());
            imgSmall.ImageUrl = imageUrl;
            imgSmall.ToolTip = GetString("DocumentType.SmallIcon");
            dfuSmall.Text = GetString("DocumentType.UploadSmallIcon");

            // Large icon
            imageUrl = GetIconUrl(className, "48x48");
            imageUrl = URLHelper.AddParameterToUrl(imageUrl, "chset", Guid.NewGuid().ToString());
            imgLarge.ImageUrl = imageUrl;
            imgLarge.ToolTip = GetString("DocumentType.LargeIcon");
            dfuLarge.Text = GetString("DocumentType.UploadLargeIcon");

            // General settings
            string btnImage = GetImageUrl("/General/Transparent.gif");
            dfuLarge.ImageUrl = dfuSmall.ImageUrl = btnImage;
            dfuLarge.ControlGroup = dfuSmall.ControlGroup = "ButtonUploaderContainer";
            dfuLarge.TargetFileName = dfuSmall.TargetFileName = className.Replace('.', '_').ToLowerCSafe();
        }
    }


    /// <summary>
    /// Handles the SelectedIndexChanged event of the lstOptions control.
    /// </summary>
    private void lstOptions_SelectedIndexChanged(object sender, EventArgs e)
    {
        iconType = lstOptions.SelectedValue.ToEnum<IconTypeEnum>();
    }

    #endregion


    #region "PostBack event handler"

    /// <summary>
    /// RaisePostbackEvent event handler.
    /// </summary>
    public void RaisePostBackEvent(string eventArgument)
    {
        if (eventArgument == "update")
        {
            // Show changes were saved after update
            ShowChangesSaved();
        }
    }

    #endregion


    #region "Private methods"

    /// <summary>
    /// Returns image url to be displayed.
    /// </summary>
    /// <param name="className">Document class name.</param>
    /// <param name="iconSet">Icon set to be used.</param>
    private string GetIconUrl(string className, string iconSet = null)
    {
        var classIcon = UIHelper.GetDocumentTypeIconPath(Page, className, iconSet);
        if (File.Exists(URLHelper.GetPhysicalPath(classIcon)))
        {
            return ResolveUrl(classIcon);
        }
        return UIHelper.GetDocumentTypeIconPath(Page, DEFAULT_IMAGE_CLASS_NAME, iconSet);
    }


    /// <summary>
    /// Loads the form control data.
    /// </summary>
    private void InitializeControl()
    {
        if ((DocumentType == null) || (FormObject == null))
        {
            return;
        }

        // First load initialization
        if (!RequestHelper.IsPostBack())
        {
            // Icon CSS class
            txtCssClass.ToolTip = GetString("fontIconCss.tooltip");

            // Identify the currently selected icon type
            iconType = IconTypeEnum.Files;
            if (!string.IsNullOrEmpty(txtCssClass.Text))
            {
                iconType = IconTypeEnum.CssClass;
            }

            lstOptions.SelectedValue = iconType.ToStringRepresentation();
        }

        // This control can be used only under existing document type
        if (SystemContext.DevelopmentMode)
        {
            // Allow 'gif' images in development mode
            dfuSmall.AllowedExtensions = "gif;png";
            dfuLarge.AllowedExtensions = "gif;png";
        }

        // Hide direct file uploader for external storage
        if (StorageHelper.IsExternalStorage("~/App_Themes/Default/Images/DocumentTypeIcons"))
        {
            dfuLarge.Visible = false;
            dfuSmall.Visible = false;
        }

        // Register refresh script
        ScriptHelper.RegisterClientScriptBlock(this, typeof(string), "RefreshIcons", ScriptHelper.GetScript("function RefreshIcons() {" + ControlsHelper.GetPostBackEventReference(this, "update") + "}"));
    }

    #endregion


    #region "Icon type enum"

    /// <summary>
    /// Internal enum - used for distinguishing between icon types
    /// </summary>
    private enum IconTypeEnum
    {
        /// <summary>
        /// Icon for doc.type is defined by image files
        /// </summary>
        [EnumDefaultValue]
        [EnumStringRepresentation("files")]
        Files,

        /// <summary>
        /// Icon is a font icon and is defined by a css class
        /// </summary>
        [EnumStringRepresentation("cssclass")]
        CssClass
    }

    #endregion
}