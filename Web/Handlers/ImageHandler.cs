using System.Web;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Idaho.Web.Handlers {
	/// <summary>
	/// Output an image from cache
	/// </summary>
	/// <remarks>
	/// http://msdn.microsoft.com/msdnmag/issues/04/04/CuttingEdge/default.aspx
	/// </remarks>
	public class Image : IHttpHandler {

		#region IHttpHandler

		public bool IsReusable { get { return true; } }

		public void ProcessRequest(HttpContext context) {
			ImageFormat format = Draw.Utility.GetFormat(context.Request["type"]);
			object graphic = context.Cache[context.Request["key"]];
			byte[] bytes;

			if (graphic == null) { return; }
			if (format == null) { format = ImageFormat.Jpeg; }

			context.Response.ContentType = "image/" + format.ToString().ToLower();

			if (graphic is Bitmap) {
				System.Drawing.Image image = (System.Drawing.Image)graphic;
				MemoryStream ms = new MemoryStream();
				image.Save(ms, format);
				bytes = ms.ToArray();
			} else {
				bytes = (byte[])graphic;
			}
			context.Response.OutputStream.Write(bytes, 0, bytes.Length);
			return;

			// the code below should work but a GDI+ generic error occurs if
			// the image.Save() format is PNG
			// http://www.eggheadcafe.com/forumarchives/NETFrameworkdrawing/Jan2006/post25815533.asp
			/*
			if (graphic is Bitmap) {
				//this.WriteImage((System.Drawing.Image)image, context);
				System.Drawing.Image image = (System.Drawing.Image)graphic;
				image.Save(context.Response.OutputStream, format);
			} else {
				byte[] image = (byte[])graphic;
				context.Response.OutputStream.Write(image, 0, image.Length);
			}
			*/
		}

		#endregion

	}
}