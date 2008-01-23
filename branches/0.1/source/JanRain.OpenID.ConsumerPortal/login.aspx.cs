using System;
using System.Collections.Specialized;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Janrain.OpenId.Consumer;
using Janrain.OpenId.RegistrationExtension;

public partial class login : System.Web.UI.Page
{
    /// <summary>
    /// Handles the Load event of the Page control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    protected void Page_Load(object sender, EventArgs e)
    {

    }
    
    /// <summary>
    /// Fired upon login.
    /// Note, that straight after login, forms auth will redirect the user to their original page. So this page may never be rendererd.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void OpenIdLogin1_LoggedIn(object sender, NerdBank.OpenId.Consumer.OpenIdTextBox.OpenIdEventArgs e)
    {
        State.ProfileFields = e.ProfileFields;
    }
    protected void OpenIdLogin1_Error(object sender, NerdBank.OpenId.Consumer.OpenIdTextBox.ErrorEventArgs e)
    {
        loginFailedLabel.Visible = true;
        loginFailedLabel.Text += ": " + e.ErrorMessage;
    }
    protected void OpenIdLogin1_Canceled(object sender, NerdBank.OpenId.Consumer.OpenIdTextBox.ErrorEventArgs e)
    {
        loginCanceledLabel.Visible = true;
    }
}
