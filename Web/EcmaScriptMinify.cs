using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;

namespace Idaho.Web {
	/// <summary>
	/// Minify EcmaScript
	/// </summary>
	/// <remarks>
	/// Based on code by Douglas Crockford
	/// http://www.crockford.com/javascript/jsmin.cs
	/// </remarks>
	internal class EcmaScriptMinify {

		[Flags()]
		private enum Action {
			/// <summary>
			/// Save current character to output
			/// </summary>
			Output = 0x2,
			/// <summary>
			/// Copy next character to current character
			/// </summary>
			Copy = 0x4,
			/// <summary>
			/// Get the next character from input
			/// </summary>
			Get = 0x8
		}

		const int EOF = -1;
		int _current;
		int _next;
		int _lookAhead = EOF;
		StreamReader _reader;
		HtmlTextWriter _writer;
		StringBuilder sb;
		StringWriter sw;

		private HtmlTextWriter Writer {
			get {
				if (_writer == null) {
					sb = new StringBuilder();
					sw = new StringWriter(sb);
					_writer = new HtmlTextWriter(sw);
				}
				return _writer;
			}
			set { _writer = value; }
		}

		public override string ToString() {
			if (_writer != null) { return sb.ToString(); }
			return string.Empty;
		}

		/// <summary>
		/// Remove unecessary EcmaScript characters
		/// </summary>
		public void WriteMinified(Stream input, TextWriter writer) {
			Action action;
			_writer = new HtmlTextWriter(writer);
			_reader = new StreamReader(input);
			_current = '\n';

			this.Process(Action.Get);
			while (_current != EOF) {
				switch (_current) {
					case ' ':
						action = Action.Copy | Action.Get;
						if (IsAlphanumeric(_next)) { action |= Action.Output; }
						this.Process(action); break;
					case '\n':
						switch (_next) {
							case '{':
							case '[':
							case '(':
							case '+':
							case '-':
								this.Process(Action.Output | Action.Copy | Action.Get); break;
							case ' ':
								this.Process(Action.Get); break;
							default:
								action = Action.Copy | Action.Get;
								if (IsAlphanumeric(_next)) { action |= Action.Output; }
								this.Process(action); break;
						}
						break;
					default:
						switch (_next) {
							case ' ':
								action = Action.Get;
								if (IsAlphanumeric(_current)) { action |= Action.Copy | Action.Output; }
								this.Process(action); break;
							case '\n':
								switch (_current) {
									case '}':
									case ']':
									case ')':
									case '+':
									case '-':
									case '"':
									case '\'':
										this.Process(Action.Output | Action.Copy | Action.Get); break;
									default:
										action = Action.Get;
										if (IsAlphanumeric(_current)) { action |= Action.Copy | Action.Output; }
										this.Process(action); break;
								}
								break;
							default:
								this.Process(Action.Output | Action.Copy | Action.Get);
								break;
						}
						break;
				}
			}
		}
		public void WriteMinified(string script) {
			MemoryStream stream = new MemoryStream(ASCIIEncoding.ASCII.GetBytes(script));
			this.WriteMinified(stream, this.Writer);
		}
		private void Process(Action action) {
			if (action.Contains(Action.Output)) { this.Put(_current); }
			if (action.Contains(Action.Copy)) {
				_current = _next;
				if (_current == '\'' || _current == '"') {
					while (!_reader.EndOfStream) {
						this.Put(_current);
						_current = this.Get();
						if (_current == _next) { break; }
						if (_current <= '\n') {
							throw new System.Exception(string.Format(
								"Unterminated string literal: {0}", _current));
						}
						if (_current == '\\') { this.Put(_current); _current = this.Get(); }
					}
				}

			}
			if (action.Contains(Action.Get)) {
				_next = this.Next();
				if (_next == '/' && (_current == '(' || _current == ',' || _current == '=' ||
					_current == '[' || _current == '!' || _current == ':' ||
					_current == '&' || _current == '|' || _current == '?' ||
					_current == '{' || _current == '}' || _current == ';' ||
					_current == '\n')) {

					this.Put(_current);
					this.Put(_next);

					while (!_reader.EndOfStream) {
						_current = this.Get();
						if (_current == '/') { break; } else if (_current == '\\') { this.Put(_current); _current = this.Get(); }
						else if (_current <= '\n') {
							throw new System.Exception(string.Format(
								"Unterminated Regular Expression literal : {0}", _current));
						}
						this.Put(_current);
					}
					_next = this.Next();
				}
			}
		}

		/// <summary>
		/// Get the next character excluding comments
		/// </summary>
		private int Next() {
			int c = this.Get();
			if (c == '/') {
				// might be a comment--peek at what's next
				switch (this.Peek()) {
					case '/':
						while (!_reader.EndOfStream) {
							c = this.Get();
							if (c <= '\n') { return c; }
						}
						break;
					case '*':
						// discard this character
						this.Get();
						while (!_reader.EndOfStream) {
							switch (this.Get()) {
								case '*':
									if (this.Peek() == '/') {
										this.Get();
										return ' ';
									}
									break;
								case EOF:
									throw new System.Exception("Unterminated comment");

							}
						}
						break;
					default:
						return c;
				}
			}
			// simply return c if not a comment character
			return c;
		}

		private int Peek() {
			_lookAhead = this.Get();
			return _lookAhead;
		}
		private int Get() {
			int c = _lookAhead;
			_lookAhead = EOF;
			if (c == EOF) {	c = _reader.Read(); }
			if (c >= ' ' || c == '\n' || c == EOF) { return c; }
			if (c == '\r') { return '\n'; }
			return ' ';
		}
		private void Put(int c) { this.Writer.Write((char)c); }

		bool IsAlphanumeric(int c) {
			return ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') ||
				(c >= 'A' && c <= 'Z') || c == '_' || c == '$' || c == '\\' ||
				c > 126);
		}
	}
}
