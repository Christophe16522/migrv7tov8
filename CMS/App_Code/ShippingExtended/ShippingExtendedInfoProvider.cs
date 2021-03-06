﻿using System;
using System.Collections.Generic;
using System.Data;
using CMS.Ecommerce;
using CMS.SettingsProvider;
using CMS.DataEngine;
using CMS.SiteProvider;
using CMS.GlobalHelper;
using System.Data.SqlClient;
using System.Configuration;
using CultureInfo = System.Globalization.CultureInfo;
using CMS.EventLog;
using System;
using CMS.Helpers;
using CMS.Base;


/// <summary>
/// Customized ShippingExtendedInfoProvider
/// </summary>
public class ShippingExtendedInfoProvider : ShippingOptionInfoProvider
{
    public int ShippingExtendedPriceId = -1;

    public ShippingExtendedInfoProvider()
    {
        //
        // TODO: ajoutez ici la logique du constructeur
        //
    }

    /// <summary>
    /// Calculates the shipping price for the specified shopping cart (shipping taxes are not included).
    /// Returns the shipping price in the site's main currency.
    /// </summary>
    /// <param name="cart">
    /// Object representing the related shopping cart. Provides the data used to calculate the shipping price.
    /// </param>
    /// 
    protected override double CalculateShippingTaxInternal(ShoppingCartInfo cart)
    {
        double result = 0, vatRate = 0;
        // Checks that the shopping cart specified in the parameter exists
        if (cart != null)
        {

            int CartShippingUnit = GetCartShippingUnit(cart);
            if (CartShippingUnit > 0)
            {
                // int PriceID = ValidationHelper.GetInteger(SessionHelper.GetValue("PriceID"), 0);
                // int CountryID = ValidationHelper.GetInteger(SessionHelper.GetValue("CountryID"), 0);

                int PriceID = GetCustomFieldValue(cart, "ShoppingCartPriceID");
                int CountryID = GetCustomFieldValue(cart, "ShoppingCartCountryID");
                vatRate = GetCartShippingVatRate(cart);
                
                if (PriceID > 0 && CountryID > 0)
                {
                    result = GetTaxFromPriceID(PriceID, CountryID, CartShippingUnit, vatRate);
                }
            }

        }
        return result;
    }

    protected override double CalculateShippingInternal(ShoppingCartInfo cart)
    {
        double result = 0;
        // Checks that the shopping cart specified in the parameter exists
        if (cart != null)
        {
            int CartShippingUnit = GetCartShippingUnit(cart);
           
            if (CartShippingUnit > 0)
            {
                // int PriceID = ValidationHelper.GetInteger(SessionHelper.GetValue("PriceID"), 0);
                // int CountryID = ValidationHelper.GetInteger(SessionHelper.GetValue("CountryID"), 0);
                int PriceID = GetCustomFieldValue(cart, "ShoppingCartPriceID");
                int CountryID = GetCustomFieldValue(cart, "ShoppingCartCountryID");
                double vat = GetCartShippingVatRate(cart);
                if (vat > 0)
                {
                    vat = 6;
                }
                if (PriceID > 0 && CountryID > 0)
                {
                    result = GetPriceFromPriceID(PriceID, CountryID, CartShippingUnit, 1 + vat / 100);
                }
            }

        }
        return result;
    }

    private double GetPriceFromPriceID(int priceID, int countryID, int cartUnit, double vatrate)
    {
        double result = 0;
        QueryDataParameters parameters = new QueryDataParameters();
        parameters.Add("@ShippingUnits", cartUnit);
        parameters.Add("@CountryID", countryID);
        parameters.Add("@VATRate", vatrate);
        string where = string.Format("ItemID={0}", priceID.ToString());
        GeneralConnection cn = ConnectionHelper.GetConnection();
        DataSet ds = cn.ExecuteQuery("customtable.shippingextension.ShippingCostListByCountry", parameters, where);


        if (!DataHelper.DataSourceIsEmpty(ds))
        {
            foreach (DataRow drow in ds.Tables[0].Rows)
            {
                if ((int)drow["ItemId"] == priceID)
                {
                    result = ValidationHelper.GetDouble(drow["ShippingTotalPrice"], 0);
                    return result;
                }
            }
        }
        return result;
    }

    private double GetTaxFromPriceID(int priceID, int countryID, int cartUnit, double vatRate)
    {
        double result = 0;

        QueryDataParameters parameters = new QueryDataParameters();
        parameters.Add("@ShippingUnits", cartUnit);
        parameters.Add("@CountryID", countryID);
        parameters.Add("@VATRate", 1 + vatRate / 100);
        string where = string.Format("ItemID={0}", priceID.ToString(CultureInfo.InvariantCulture));
        GeneralConnection cn = ConnectionHelper.GetConnection();
        DataSet ds = cn.ExecuteQuery("customtable.shippingextension.ShippingCostListByCountry", parameters, where);
        if (!DataHelper.DataSourceIsEmpty(ds))
        {
            foreach (DataRow drow in ds.Tables[0].Rows)
            {
                if ((int)drow["ItemId"] == priceID)
                {
                    result = ValidationHelper.GetDouble(drow["ShippingTotalPrice"], 0);
                    result = ValidationHelper.GetDouble(drow["ShippingFinalCost"], 0) - result;
                    return result;
                }
            }
        }
        return result;
    }

    public static int GetCartShippingUnit(ShoppingCartInfo cart)
    {
        int result = 0;
        foreach (ShoppingCartItemInfo item in cart.CartItems)
        {
            int ShippingUnit = GetSKUShippingUnit(item.SKUID);
            int ShippingTotal = ShippingUnit * item.CartItemUnits;
            result += ShippingTotal;
        }
        return result;
    }

    public static double GetCartShippingVatRate(ShoppingCartInfo cart)
    {
        double result = 0;
        foreach (ShoppingCartItemInfo item in cart.CartItems)
        {
            double currentTax = 100 * (item.TotalPrice / (item.TotalPrice - item.TotalTax) - 1);
            EventLogProvider evt = new EventLogProvider();
            //evt.LogEvent("I",DateTime.Now,"SKUID = "+item.SKUID+"Tax = "+currentTax,"code");
            if (currentTax > result)
            {
                result = currentTax;
            }
        }
        //SERVRANX -- commenter la ligne suivante pour les autres projets
        result = result > 0 ? 6 : 0;
        return result;
    }


    private static int GetSKUShippingUnit(int SKIUD)
    {
        int result = 0;

        GeneralConnection cn = ConnectionHelper.GetConnection();
        string stringQuery = string.Format("SELECT ShippingUnit FROM CONTENT_Product WHERE ProductID=(SELECT Top 1 DocumentForeignKeyValue FROM view_cms_tree_joined WHERE ClassName='CMS.Product' AND SKUID={0})", SKIUD.ToString());
        DataSet ds = cn.ExecuteQuery(stringQuery, null, QueryTypeEnum.SQLQuery, false);
        if (!DataHelper.DataSourceIsEmpty(ds))
        {
            result = ValidationHelper.GetInteger(ds.Tables[0].Rows[0]["ShippingUnit"], 0);
        }
        return result;
    }

    public static List<AddressInfo> GetAdresses(bool billing, bool shipping, ShoppingCartInfo cart)
    {
        List<AddressInfo> Result = new List<AddressInfo>();
        if (ECommerceContext.CurrentCustomer == null)
        {
            return Result;
        }
        //if customer have no [AddressEnabled]
        int idCustomer = ECommerceContext.CurrentCustomer.CustomerID;
        SqlConnection con3 = new SqlConnection(ConfigurationManager.ConnectionStrings["CMSConnectionString"].ConnectionString);
        con3.Open();
        var stringQuery = "select count(AddressID) as NbAdress from COM_Address WHERE COM_Address.AddressEnabled = 'true'  AND COM_Address.AddressCustomerID  = " + idCustomer;
        SqlCommand cmd3 = new SqlCommand(stringQuery, con3);
        int nb = (int)cmd3.ExecuteScalar();

        if (nb == 0)
        {
            Result = null;
            return Result;
        }



        string where = string.Empty, orderby = string.Empty;
        AddressInfo ai;
        DataSet ds, dsoi = null;
        if (billing)
        {
            ai = AddressInfoProvider.GetAddressInfo(cart.ShoppingCartBillingAddressID);
            if (ai == null)
            {
                where = string.Format("OrderCustomerID={0}", ECommerceContext.CurrentCustomer.CustomerID.ToString());
                orderby = "OrderID DESC";
                dsoi = OrderInfoProvider.GetOrderList(where, orderby);
                if (!DataHelper.DataSourceIsEmpty(dsoi))
                {
                    foreach (DataRow drow in dsoi.Tables[0].Rows)
                    {
                        OrderInfo oi = new OrderInfo(drow);
                        AddressInfo bai = AddressInfoProvider.GetAddressInfo(oi.OrderBillingAddressID);
                        if (bai.AddressEnabled && bai.AddressIsBilling)
                        {
                            ai = bai;
                            cart.ShoppingCartBillingAddressID = bai.AddressID;
                            break;
                        }
                    }
                }
            }
            if (ai == null)
            {
                where = string.Format("AddressCustomerID={0} AND AddressIsBilling=1", ECommerceContext.CurrentCustomer.CustomerID.ToString());
                orderby = "AddressID";
                ds = AddressInfoProvider.GetAddresses(where, orderby);
                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    ai = new AddressInfo(ds.Tables[0].Rows[0]);
                    cart.ShoppingCartBillingAddressID = ai.AddressID;
                }
            }
            Result.Add(ai);
        }

        if (shipping)
        {
            ai = AddressInfoProvider.GetAddressInfo(cart.ShoppingCartShippingAddressID);
            if (ai == null)
            {
                if (DataHelper.DataSourceIsEmpty(dsoi))
                {
                    // where = string.Format("OrderCustomerID={0}", ECommerceContext.CurrentCustomer.CustomerID.ToString());
                    where = string.Format("OrderCustomerID={0} ", ECommerceContext.CurrentCustomer.CustomerID.ToString());
                    orderby = "OrderID DESC";
                    dsoi = OrderInfoProvider.GetOrderList(where, orderby);
                }
                if (!DataHelper.DataSourceIsEmpty(dsoi))
                {
                    foreach (DataRow drow in dsoi.Tables[0].Rows)
                    {
                        OrderInfo oi = new OrderInfo(drow);
                        AddressInfo sai = AddressInfoProvider.GetAddressInfo(oi.OrderShippingAddressID);
                        if (sai.AddressEnabled && sai.AddressIsShipping)
                        {
                            ai = sai;
                            cart.ShoppingCartShippingAddressID = sai.AddressID;
                            break;
                        }
                    }
                }
            }
            if (ai == null)
            {
                where = string.Format("AddressCustomerID={0} AND AddressIsShipping=1", ECommerceContext.CurrentCustomer.CustomerID.ToString());
                orderby = "AddressID";
                ds = AddressInfoProvider.GetAddresses(where, orderby);
                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    ai = new AddressInfo(ds.Tables[0].Rows[0]);

                    cart.ShoppingCartShippingAddressID = ai.AddressID;
                }
                else
                {
                    // NO SHIPPING ADDRESS DEFINED- PICK FIRST BILLING ADDRESS    
                    AddressInfo ai_shipping = AddressInfoProvider.GetAddressInfo(cart.ShoppingCartBillingAddressID);
                    ai_shipping.AddressIsShipping = true;
                    AddressInfoProvider.SetAddressInfo(ai_shipping);
                    // where = string.Format("AddressCustomerID={0} AND AddressIsShipping=1", ECommerceContext.CurrentCustomer.CustomerID.ToString());
                    where = string.Format("AddressCustomerID={0} AND AddressIsShipping=1", ECommerceContext.CurrentCustomer.CustomerID.ToString());
                    ds = AddressInfoProvider.GetAddresses(where, orderby);
                    if (!DataHelper.DataSourceIsEmpty(ds))
                    {
                        ai = new AddressInfo(ds.Tables[0].Rows[0]);
                        cart.ShoppingCartShippingAddressID = ai.AddressID;
                    }

                }
            }
            Result.Add(ai);
        }
        return Result;
    }

    public static void SetCustomFieldValue(ShoppingCartInfo cart, string field, int value)
    {
        cart.SetValue(field, value);
    }

    public static int GetCustomFieldValue(ShoppingCartInfo cart, string field)
    {
        int result = -1;
        result = cart.GetIntegerValue(field, -1);
        return result;
    }
}

[CustomProviderLoader]
public partial class CMSModuleLoader : CMSModuleLoaderBase
{
    /// <summary>
    /// Attribute class that ensures the loading of custom providers.
    /// </summary>
    private class CustomProviderLoader : CMSLoaderAttribute
    {
        /// <summary>
        /// Called automatically when the application starts.
        /// </summary>
        public override void Init()
        {
            // Registers the 'ShippingExtendedInfoProvider' class as the ShippingOptionInfoProvider
            ShippingOptionInfoProvider.ProviderObject = new ShippingExtendedInfoProvider();
        }
    }
}