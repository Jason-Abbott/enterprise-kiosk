using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Idaho.Web.Controls {
	/// <summary>
	/// Container for multi-step forms
	/// </summary>
	public class StepContainer : HtmlContainerControl {

		private string _label = string.Empty;
		private Button _nextButton;
		private Button _backButton;
		private Field _postedStep;
		private LinkedList<Step> _steps = new LinkedList<Step>();
		private LinkedListNode<Step> _currentNode = null;
		private PlaceHolder _content;
		private Step _maximum = null;
		private Step _current = null;
		private string _contentID = string.Empty;
		private bool _loaded = false;

		#region Properties

		public Button NextButton { get { return _nextButton; } }
		public Button BackButton { get { return _backButton; } }

		public LinkedList<Step> List { get { return _steps; } }

		/// <summary>
		/// Specify order of steps
		/// </summary>
		/// <remarks>
		/// The default order is according to control arrangement in the markup. For an
		/// alternate order, set an array of steps indices. These values should be the
		/// keys to reference the actual step objects in the dictionary.
		/// </remarks>
		//public Step[] Sorted { set { _sorted = value; } }
		public string Content { set { _contentID = value; } }
		//public Step Current { set { _current = value; } get { return _current; } }

		/// <summary>
		/// Highest step (index) the user will be able to skip directly to
		/// </summary>
		public Step Maximum { set { _maximum = value; } get { return _maximum; } }
		public string Label { set { _label = value; } get { return _label; } }

		/// <summary>
		/// The current step
		/// </summary>
		public Step Current {
			get {
				if (!_loaded) {
					string stepID = (this.Page.IsPostBack) ? _postedStep.Value
						: this.Context.Request.QueryString[_postedStep.ID];
					if (!string.IsNullOrEmpty(stepID)) {
						Control match = this.FindControl(stepID);
						if (match != null) { _current = (Step)match; }
					}
					_loaded = true;
				}
				return _current;
			}
			set {
				_current = value;
				_postedStep.Value = _current.ID;
				_loaded = true;
			}
		}

		private LinkedListNode<Step> CurrentNode {
			get {
				if (_loaded && _currentNode == null) { _currentNode = _steps.Find(_current); }
				return _currentNode;
			}
		}

		#endregion

		public StepContainer() : base("fieldset") {
			base.AllowSelfClose = true;
			_nextButton = new Button("btnNext", "Next", Page.Actions.Next, true);
			_backButton = new Button("btnBack", "Back", Page.Actions.Previous, true);
			_postedStep = new Field("step", Field.Types.Hidden);
		}

		#region Events

		/// <summary>
		/// Add button controls
		/// </summary>
		protected override void OnInit(EventArgs e) {
			this.Controls.Add(_nextButton);
			this.Controls.Add(_backButton);
			this.Controls.Add(_postedStep);
			base.OnInit(e);
		}

		/// <summary>
		/// Identify content and determine next step
		/// </summary>
		/// <remarks>
		/// The host page should utilize the determined step in its
		/// LoadComplete or PreRender event
		/// </remarks>
		protected override void OnLoad(EventArgs e) {

			// find the content control if any
			using (Control c = this.FindControl(_contentID)) {
				if (c != null && c is PlaceHolder) { _content = (PlaceHolder)c; }
			}
			// infer steps list if not given
			if (_steps.Count == 0) {
				foreach (Control c in this.Controls) {
					if (c is Step) { _steps.AddLast((Step)c); }
				}
			}
			// set visibility and find current step
			foreach (Step s in _steps) {
				if (s.ID.Equals(_postedStep.Value)) {
					_current = s;
				} else {
					s.Visible = false;
				}
			}
			// configure buttons
			if (_current.Equals(_steps.First.Value)) {
				_backButton.Visible = false;
			} else if (_current.Equals(_steps.Last.Value)) {
				_nextButton.Visible = false;
			}
			// only postbacks change the current step
			if (this.Page.IsPostBack) {
				LinkedListNode<Step> node = _steps.Find(_current);
				_current = (this.Page.Action == Web.Page.Actions.Previous) ? node.Previous.Value : node.Next.Value;
			} 
			base.OnLoad(e);
		}

		/// <summary>
		/// Add final formatting to controls
		/// </summary>
		protected override void OnPreRender(EventArgs e) {
			HtmlContainerControl legend;
			HtmlContainerControl span;
			string url = this.Context.Request.Url.PathAndQuery;
			string stepField = _postedStep.ID + "=";
			Regex re = new Regex(stepField + "\\d");

			// legend for container
			if (!string.IsNullOrEmpty(_label)) {
				legend = new HtmlContainerControl("legend");
				legend.InnerHtml = _label;
				this.Controls.AddAt(0, legend);
			}
			// transfer content to active step
			_content.Visible = true;
			_current.Controls.AddAt(0, _content);
			
			// legend for the active step
			legend = new HtmlContainerControl("legend");
			legend.CssClass = "steps";

			// build progress indicator with all step labels
			foreach (Step s in _steps) {
				span = new HtmlContainerControl("span");
				if (s.Equals(_current)) {
					span.CssClass = "selected";
					span.InnerHtml = s.Label;
				} else {
					// allow link to other steps
					string format = "{0}";

					if (!s.Equals(_steps.Last.Value)) {
						if (true) { format = "<a href=\"{0}\" title=\"Jump to {1}\">{1}</a>"; }
					}

					span.InnerHtml = string.Format(format,
						re.Replace(url, stepField + s.ID), s.Label);
				}
				legend.Controls.Add(span);
			}
			_current.Controls.AddAt(0, legend);
			_current.Visible = true;

			//if (this.Page.Step == 0) { _backButton.Visible = false; }
			//if (this.Page.Step == _steps.Count) { _nextButton.Visible = false; }

			_postedStep.Value = _current.ID;

 			base.OnPreRender(e);
		}

		#endregion

	}
}
