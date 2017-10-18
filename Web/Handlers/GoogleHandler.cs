using GCheckout.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Web;

namespace Idaho.Web.Handlers {
	/// <summary>
	/// Handle Google notification updates
	/// </summary>
	public class Google : IHttpHandler {

		#region IHttpHandler

		public bool IsReusable { get { return true; } }

		/// <remarks>
		/// Sample XML:
		/// http://code.google.com/apis/checkout/developer/Google_Checkout_XML_API_Notification_API.html#new_order_notifications
		/// </remarks>
		public void ProcessRequest(HttpContext context) {
			string googleXml = EncodeHelper.Utf8StreamToString(context.Request.InputStream);

			SiteActivity.Log(SiteActivity.Types.GoogleNotification);

			if (!string.IsNullOrEmpty(googleXml)) {
				StreamWriter file = null;
				try {
					string fileName = string.Format("{0}-{1}-{2}.xml",
					  EncodeHelper.GetTopElement(googleXml),
					  EncodeHelper.GetElementValue(googleXml, "google-order-number"),
					  EncodeHelper.GetElementValue(googleXml, "serial-number"));
					string path = string.Format("{0}/google/{1}",
						Data.File.DataFolder, fileName);
					file = new StreamWriter(path, false, Encoding.UTF8);
					file.Write(googleXml);
					SiteActivity.Log(SiteActivity.Types.GoogleFileSave, path);
				} catch (System.Exception e) {
					Idaho.Exception.Log(e, Exception.Types.Google, googleXml);
					context.Response.StatusCode = 500;
				} finally {
					if (file != null) { file.Close(); }
				}
			} else {
				context.Response.Redirect("./", true);
			}
			context.Response.Write("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
			context.Response.Write("<notification-acknowledgment xmlns=\"http://checkout.google.com/schema/2\"/>");
		}

		#endregion

	}
}
