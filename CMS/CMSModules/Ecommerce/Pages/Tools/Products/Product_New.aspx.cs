using System;

using CMS.Core;
using CMS.DataEngine;
using CMS.Ecommerce;
using CMS.Helpers;
using CMS.PortalEngine;
using CMS.SiteProvider;
using CMS.UIControls;

using TreeNode = CMS.DocumentEngine.TreeNode;

public partial class CMSModules_Ecommerce_Pages_Tools_Products_Product_New : CMSProductsPage
{
    #region "Variables"

    protected int newObjectSiteId = -1;
    private OptionCategoryInfo mOptionCategory;

    #endregion


    #region "Properties"

    /// <summary>
    /// Gets the value of the 'parentNodeId' URL parameter.
    /// </summary>
    public int ParentNodeID
    {
        get
        {
            return QueryHelper.GetInteger("parentNodeId", 0);
        }
    }


    /// <summary>
    /// Gets the value of the 'classId' URL parameter.
    /// </summary>
    public int DataClassID
    {
        get
        {
            return QueryHelper.GetInteger("classId", 0);
        }
    }


    /// <summary>
    /// Gets the value of the 'categoryId' URL parameter.
    /// </summary>
    public int OptionCategoryID
    {
        get
        {
            return QueryHelper.GetInteger("categoryId", 0);
        }
    }


    /// <summary>
    /// Gets a product option category object specified by the OptionCategoryID property.
    /// </summary>
    public OptionCategoryInfo OptionCategory
    {
        get
        {
            return mOptionCategory ?? (mOptionCategory = OptionCategoryInfoProvider.GetOptionCategoryInfo(OptionCategoryID));
        }
    }


    /// <summary>
    /// Identifies if the page is used for products UI.
    /// </summary>
    protected override bool IsProductsUI
    {
        get
        {
            return !QueryHelper.GetBoolean("content", false);
        }
    }


    /// <summary>
    /// Gets the value of the 'parentProductId' URL parameter.
    /// </summary>
    public int ParentProductID
    {
        get
        {
            return QueryHelper.GetInteger("parentProductId", 0);
        }
    }


    /// <summary>
    /// Gets a value that indicates if the document type selection should be displayed.
    /// </summary>
    private bool DisplayTypeSelection
    {
        get
        {
            return (ParentNodeID > 0) && (DataClassID == 0);
        }
    }

    #endregion


    #region "Lifecycle"

    protected override void OnPreInit(EventArgs e)
    {
        base.OnPreInit(e);
        PortalContext.ViewMode = ViewModeEnum.EditForm;
    }


    protected override void OnInit(EventArgs e)
    {
        ScriptHelper.RegisterLoader(Page);

        base.OnInit(e);
        IsProductOption = OptionCategoryID > 0;

        newObjectSiteId = ConfiguredSiteID;

        if (IsProductOption)
        {
            CheckUIElementAccessHierarchical(ModuleName.ECOMMERCE, "ProductOptions.Options.Edit");

            // Initialize creation of a new product option
            GlobalObjectsKeyName = ECommerceSettings.ALLOW_GLOBAL_PRODUCT_OPTIONS;
            if (OptionCategory != null)
            {
                // A new product option will be bound to the same site as the option category
                CheckEditedObjectSiteID(OptionCategory.CategorySiteID);
                newObjectSiteId = OptionCategory.CategorySiteID;
            }
        }
        else
        {
            // Initialize creation of a new product
            GlobalObjectsKeyName = ECommerceSettings.ALLOW_GLOBAL_PRODUCTS;

            if (IsProductsUI)
            {
                // Check explore tree permissions only in tree section (do not check Stand-alone SKU section)
                if (ParentNodeID > 0)
                {
                    CheckExploreTreePermission();
                }

                // Check content New permission
                var newElement = new UIElementAttribute(ModuleName.ECOMMERCE, "Products.Properties", false, true);
                newElement.Check(this);
            }
        }

        if (DisplayTypeSelection)
        {
            InitDocumentTypeSelection();
            DocumentManager.StopProcessing = true;
            productEditElem.StopProcessing = true;
        }
        else
        {
            InitProductEdit();
            docTypeElem.StopProcessing = true;
        }

        InitMasterPage();
    }


    protected override void OnPreRender(EventArgs e)
    {
        base.OnPreRender(e);

        ScriptHelper.RegisterEditScript(Page);

        // Get SKUSelector for bundle
        var bundle = productEditElem.SKURepresentingForms.Find(x => x.ID == "b").FieldControls["skubundleitemscount"];

        // Disable SKU binding form if products are selected
        productEditElem.SKUBindingForm.Enabled = (bundle == null) || bundle.Value.Equals(0);

    }

    #endregion


    #region "Initialization"

    /// <summary>
    /// Initializes the document type selection.
    /// </summary>
    private void InitDocumentTypeSelection()
    {
        docTypeElem.ParentNodeID = ParentNodeID;
        docTypeElem.Caption = GetString("com.product.selectdoctype");
        docTypeElem.AllowNewABTest = false;
        docTypeElem.AllowNewLink = false;
        docTypeElem.RedirectWhenNoChoice = true;
        docTypeElem.SelectionUrl = "Product_New.aspx";
        docTypeElem.Where = "ClassIsProduct = 1";
        docTypeElem.NoDataMessage = GetString("com.products.nodocumenttypeallowed");
        plcDocTypeSelection.Visible = true;
    }


    /// <summary>
    /// Initializes the product edit control.
    /// </summary>
    private void InitProductEdit()
    {
        plcProductEdit.Visible = true;
        productEditElem.ProductSaved += OnProductSaved;
    }


    private void OnProductSaved(object sender, EventArgs args)
    {
        if (!IsProductsUI)
        {
            // If not EditLive view mode, switch to form mode to keep editing the form
            if (PortalContext.ViewMode != ViewModeEnum.EditLive)
            {
                PortalContext.ViewMode = ViewModeEnum.EditForm;
            }
        }

        bool refreshEcommerceTree = (ECommerceSettings.ProductsTree(SiteContext.CurrentSiteName) == ProductsTreeModeEnum.Sections) && (ECommerceSettings.DisplayProductsInSectionsTree(SiteContext.CurrentSiteName));

        if ((productEditElem.Product is TreeNode) && (!IsProductsUI || refreshEcommerceTree))
        {
            var node = (TreeNode)productEditElem.Product;
            if (productEditElem.ProductSavedCreateAnother)
            {
                ScriptHelper.RefreshTree(this, node.NodeParentID, node.NodeParentID);
                ScriptHelper.RegisterStartupScript(Page, typeof(string), "Refresh", ScriptHelper.GetScript("CreateAnother();"));
            }
            else
            {
                ScriptHelper.RefreshTree(this, node.NodeParentID, node.NodeID);
            }
        }
        else if (productEditElem.ProductSavedCreateAnother)
        {
            RedirectToNewProduct();
        }
        else
        {
            RedirectToSavedProduct(productEditElem.Product);
        }
    }


    /// <summary>
    /// Initializes the master page.
    /// </summary>
    private void InitMasterPage()
    {
        string titleText = "";
        string productText;

        // Initialize creation of new product option
        if (OptionCategoryID > 0)
        {
            productText = GetString("product_new.newproductoption");
        }
        // Initialize creation of new product
        else
        {
            if (ParentNodeID > 0)
            {
                titleText = GetString((newObjectSiteId > 0) ? "com_sku_edit_general.newitemcaption" : "com.product.newglobal");
            }
            else
            {
                titleText = GetString((newObjectSiteId > 0) ? "com.sku.newsku" : "com.sku.newglobalsku");
            }

            productText = titleText;
        }


        // Set breadcrumbs
        if (OptionCategoryID > 0)
        {
            var productListUrl = "~/CMSModules/Ecommerce/Pages/Tools/ProductOptions/OptionCategory_Edit_Options.aspx";
            productListUrl = URLHelper.AddParameterToUrl(productListUrl, "siteId", SiteID.ToString());
            productListUrl = URLHelper.AddParameterToUrl(productListUrl, "categoryId", OptionCategoryID.ToString());

            PageBreadcrumbs.AddBreadcrumb(new BreadcrumbItem
            {
                Text = GetString("product_new.productoptionslink"),
                RedirectUrl = ResolveUrl(productListUrl),
            });

            PageBreadcrumbs.AddBreadcrumb(new BreadcrumbItem
            {
                Text = FormatBreadcrumbObjectName(productText, newObjectSiteId)
            });
        }
        else
        {
            EnsureProductBreadcrumbs(PageBreadcrumbs, productText, false, DisplayTreeInProducts, false);
        }

        PageTitle.TitleText = titleText;
    }

    #endregion


    #region "Redirection"

    /// <summary>
    /// Redirects to the new product page.
    /// </summary>
    private void RedirectToNewProduct()
    {
        string url = "Product_New.aspx";

        url = URLHelper.AddParameterToUrl(url, "siteId", SiteID.ToString());
        url = URLHelper.AddParameterToUrl(url, "categoryId", OptionCategoryID.ToString());
        url = URLHelper.AddParameterToUrl(url, "parentNodeId", ParentNodeID.ToString());
        url = URLHelper.AddParameterToUrl(url, "classId", DataClassID.ToString());
        url = URLHelper.AddParameterToUrl(url, "saved", "1");

        URLHelper.Redirect(url);
    }


    /// <summary>
    /// Redirects to the edit page of the saved product.
    /// </summary>
    private void RedirectToSavedProduct(BaseInfo product)
    {
        string url = ProductUIHelper.GetProductEditUrl();

        // Creating product options of type other than products is redirected directly to form
        if (OptionCategoryID > 0)
        {
            var categoryInfo = OptionCategoryInfoProvider.GetOptionCategoryInfo(OptionCategoryID);
            if (categoryInfo != null)
            {
                url = "Product_Edit_General.aspx";

                if (categoryInfo.CategoryType == OptionCategoryTypeEnum.Products)
                {
                    url = UIContextHelper.GetElementUrl("cms.ecommerce", "ProductOptions.Options.Edit", false);
                }
            }
        }

        url = URLHelper.AddParameterToUrl(url, "siteId", SiteID.ToString());
        url = URLHelper.AddParameterToUrl(url, "categoryId", OptionCategoryID.ToString());
        url = URLHelper.AddParameterToUrl(url, "objectid", OptionCategoryID.ToString());

        if (product is TreeNode)
        {
            int nodeId = product.GetIntegerValue("NodeID", 0);
            url = URLHelper.AddParameterToUrl(url, "nodeId", nodeId.ToString());
        }
        else if (product is SKUInfo)
        {
            int skuId = product.GetIntegerValue("SKUID", 0);
            url = URLHelper.AddParameterToUrl(url, "productId", skuId.ToString());

            // Select general tab if stan-alone SKU is saved
            if (OptionCategoryID == 0)
            {
                url = URLHelper.AddParameterToUrl(url, "tabName", "Products.General");
            }
        }

        url = URLHelper.AddParameterToUrl(url, "saved", "1");

        URLHelper.Redirect(url);
    }

    #endregion

}
