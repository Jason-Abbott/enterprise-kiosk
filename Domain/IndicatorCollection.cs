using Idaho.Attributes;
using Idaho.Data;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace Idaho {
	public class IndicatorCollection<T> : List<T>, IListable where T: Indicator, new() {

		/// <summary>
		/// Indicator with name or tag matching given text
		/// </summary>
		public T this[string tag] {
			get {
				return this.Find(i =>
					i.Tag.Equals(tag, Format.IgnoreCase) ||
					i.Name.Equals(tag, Format.IgnoreCase));
			}
		}

		/// <summary>
		/// First indicator at given level
		/// </summary>
		public T AtLevel(int level) {
			return this.Find(i => i.Level == level);
		}

		/// <summary>
		/// Indicator with given description
		/// </summary>
		public T WithDescription(string text) {
			return this.Find(i =>
				text.Contains(i.Description) ||
				i.Description.Contains(text));
		}

		/// <summary>
		/// Name/value collection for binding to select lists
		/// </summary>
		public virtual SortedDictionary<string, string> KeysAndValues {
			get {
				var list = new SortedDictionary<string, string>();
				list.ResolveKeyConflicts = true;
				this.ForEach(e => list.Add(e.Level.ToString(), e.Name));
				return list;
			}
		}


		[Obsolete()]
		public int Add(string name, string description) {
			T i = new T();
			i.Name = name;
			i.Description = description;
			this.Add(i);
			return 0;
		}

		/// <summary>
		/// Indicator collection as sorted dictionary for AJAX update of select list
		/// </summary>
		[WebInvokable]
		public static SortedDictionary<int, string> Load(string procedure) {
			var sorted = new SortedDictionary<int, string>();
			Sql sql = new Sql() { ProcedureName = procedure };
			SqlReader reader = sql.GetReader(true);

			while (reader.Read()) {
				sorted.Add(new KeyValuePair<int, string>(
					reader.GetInt32("Level"), reader.GetString("Name")));
			}
			return sorted;
		}


		#region Factory

		/// <summary>
		/// Load indicators using SQL object (assumes standard column names)
		/// </summary>
		public static IndicatorCollection<T> Load(Data.Sql sql) {
			IndicatorCollection<T> list = new IndicatorCollection<T>();
			T i;
			SqlReader reader = sql.GetReader(true);

			while (reader.Read()) {
				i = new T();
				i.Level = reader.GetInt32("Level");
				i.Name = reader.GetString("Name");
				i.Description = reader.GetString("Description");
				i.Tag = reader.GetString("Abbreviation");
				list.Add(i);
			}
			sql.Finish();
			return list;
		}

		#endregion

	}
}
