using System;
using System.Web.UI;

namespace Idaho.Web.Controls {
	/// <summary>
	/// Render a drop-down list of a range of numbers
	/// </summary>
	public class NumberList : SelectList {

		private int _first = -1;
		private int _last = -1;
		private int _step = 0;

		#region Properties

		public int First { set { _first = Math.Abs(value); } }
		public int Last { set { _last = Math.Abs(value); } }
		public int Step { set { _step = value; } }
		public new int[] Selected {
			get {
				string[] s = base.Selections;
				int[] selected = new int[s.Length];
				for (int x = 0; x < s.Length; x++) {
					selected[x] = int.Parse(s[x]);
				}
				return selected;
			}
			set {
				string[] s = new string[value.Length];
				for (int x = 0; x < value.Length; x++) {
					s[x] = value[x].ToString();
				}
				base.Selections = s;
			}
		}

		public new int FirstSelection {
			get {
				string top = base.Selection;
				if (!string.IsNullOrEmpty(top)) {
					return int.Parse(top);
				} else {
					return 0;
				}
			}
		}

		#endregion

		/// <summary>
		/// Write sequence of numbers
		/// </summary>
		protected override void Render(HtmlTextWriter writer) {
			if (_step == 0) { _step = (_last >= _first ? 1 : -1); }

			this.RenderBeginTag(writer);
			if (_last != -1 && _first != -1) {
				for (int x = _first; x <= _last; x += _step) {
					base.RenderOption(x.ToString(), writer);
				}
			}
			this.RenderEndTag(writer);
		}
	}
}
