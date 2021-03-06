﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CMS.Ecommerce;
using CMS.EcommerceProvider;
using System.Text;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using CMS.GlobalHelper;
using CMS.URLRewritingEngine;


public partial class Servranx_Controls_TzPaymentForm : CMSPaymentGatewayForm
{
    protected void Page_Load(object sender, EventArgs e)
    {
        fake.Value = ShoppingCartInfoObj.OrderId.ToString();
        fakeAmount.Value = GetPrice();
        fakeSHA.Value = CalculateSHA1(GenerateSHA1Input(), Encoding.UTF8);
        fakeLanguage.Value = ShoppingCartInfoObj.ShoppingCartCulture;
       
    }

    protected override void OnPreRender(EventArgs e)
    {
  
        base.OnPreRender(e);
    }

    public override void LoadData()
    {

        base.LoadData();
    }

    private string GetPrice()
    {
        return (ShoppingCartInfoObj.TotalPrice * 100).ToString();
    }

    private string GenerateSHA1Input()
    {
        string cle = "wazowamadaogone2011";
        StringBuilder sb = new StringBuilder();
        sb.AppendFormat("AMOUNT={0}{1}", GetPrice(), cle);
        sb.AppendFormat("CURRENCY=EUR{0}", cle);
        sb.AppendFormat("LANGUAGE={0}{1}",ShoppingCartInfoObj.ShoppingCartCulture, cle);
        sb.AppendFormat("ORDERID={0}{1}", ShoppingCartInfoObj.OrderId, cle);
        sb.AppendFormat("PSPID=wazosa{0}", cle);
        return sb.ToString();
    }

    public string CalculateSHA1(string input, Encoding enc)
    {
        byte[] buffer = enc.GetBytes(input);
        SHA1CryptoServiceProvider cryptoTransformSHA1 = new SHA1CryptoServiceProvider();
        return BitConverter.ToString(cryptoTransformSHA1.ComputeHash(buffer)).Replace("-", "");
    }


    protected void BtnSubmitClick(object sender , EventArgs e)
    {

        //btnSubmit.PostBackUrl = "https://secure.ogone.com/ncol/prod/orderstandard.asp";
    
        //this.Page.ClientScript.RegisterStartupScript(this.GetType(), "autopostback", this.Page.ClientScript.         GetPostBackEventReference(btnSubmit, ""));
    }

    protected void btnFakeClick(object sender, EventArgs e)
    {
        ShoppingCartControl.RaisePaymentCompletedEvent();
        ShoppingCartControl.CleanUpShoppingCart();
        Response.Redirect("http://www.google.fr");
    }
}
