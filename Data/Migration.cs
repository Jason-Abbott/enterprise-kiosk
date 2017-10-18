using System;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace Idaho.Data {
	/// <summary>
	/// Serialization surrogate and selector that allows any new version of a
	/// serialiazable class to be loaded from an outdated serialized representation.
	/// </summary>
	/// <remarks>
	/// Any new class fields not present in the serialization data will be
	/// left uninitialized. Removed fields are ignored (no exceptions).
	/// Based on Bamboo Prevalence implemention (http://bbooprevalence.sourceforge.net/).
	/// </remarks>
	/// http://msdn.microsoft.com/msdnmag/issues/04/10/AdvancedSerialization/
	public class Migration : ISerializationSurrogate, ISurrogateSelector {
	   
		private System.Reflection.Assembly _assemblyToMigrate;
		private bool _convert = false;
	   
		#region Constructors
	   
		public Migration(System.Reflection.Assembly assemblyToMigrate) {
			_assemblyToMigrate = assemblyToMigrate;
		}
		public Migration(bool convert) { _convert = convert; }
		public Migration() { }
	   
		#endregion
	   
		#region ISerializationSurrogate

		public void GetObjectData(object entity, SerializationInfo info,
			StreamingContext context) { throw new NotImplementedException(); }

		/// <summary>
		/// Provide custom deserialization
		/// </summary>
		/// <param name="entity">Object instance being constructed with serialized data</param>
		/// <param name="info">Serialization data</param>
		/// <remarks>Do custom mapping to migrate serialized data to new entity version.</remarks>
		public object SetObjectData(object entity, SerializationInfo info,
			StreamingContext context, ISurrogateSelector selector) {

			MemberInfo[] members = FormatterServices.GetSerializableMembers(entity.GetType());
		   
			// spin through members to do custom mapping
			foreach (FieldInfo field in members) {
				field.SetValue(entity, this.GetValue(field, info));
			}
			return null;
		}

		/// <summary>
		/// Initialize field with value of matching name in serialization info
		///	or according to custom mapping attribute
		/// </summary>
		private object GetValue(FieldInfo field, SerializationInfo info){
			object value = null;
			Type fieldType = field.FieldType;
			string fieldName = this.FixFieldName(field.Name, info);
		   
			if (fieldName != null) {
				try {
					value = info.GetValue(fieldName, fieldType);
					if (!(value == null || field.FieldType.IsInstanceOfType(value))) {
						// convert type if changed in new member
						value = System.Convert.ChangeType(value, fieldType);
					}
					return value;
				} catch (System.Exception e) {
					Debug.BugOut("Getting value for {0} resulted in \"{1}\"", fieldName, e.Message);
				}
			}
		   
			// if we made it here then check for mapping to new member
			foreach (Attribute a in field.GetCustomAttributes(false)) {
				if (a is Mapping) {
					// get mapping
					Mapping map = (Mapping)a;
					switch (map.Type) {
						case Mapping.ChangeType.NewField:
							value = Activator.CreateInstance(fieldType); break;
						case Mapping.ChangeType.Rename:
							value = info.GetValue(map.OldName, fieldType); break;
					}
					return value;
				}
			}
		   
			// if we're still here then simply try to create instance of missing field
			if (!(fieldType.IsValueType || object.ReferenceEquals(fieldType, typeof(string)))) {
				//BugOut("Creating instance of {0}", fieldType.Name)
				try {
					value = Activator.CreateInstance(fieldType);
				} catch (System.Exception e) {
					Debug.BugOut("Failed to create instance of {0} with {1}", fieldType, e.Message);
				}
			}
			return value;
		}

		/// <summary>
		/// Determine if serialized information has corresponding field
		/// </summary>
		/// <remarks>
		/// Namespaces and subclassing change the name of the info field compared
		/// to the name of the corresponding field in the class file. This will
		/// attempt to find the match.
		/// </remarks>
		/// <param name="fieldName">Serialized field name</param>
		/// <param name="info">All serialization information</param>
		/// <returns>Field name corrected to match class definition</returns>
		private string FixFieldName(string fieldName, SerializationInfo info) {
			SerializationInfoEnumerator infoEnum = info.GetEnumerator();
			Regex re;
		   
			if (_convert && fieldName.IndexOf('+') > 0) {
				re = new Regex(string.Format("({0}|{1})$", fieldName, fieldName.Split('+')[1]),
					RegexOptions.IgnoreCase);
			} else {
				re = new Regex(string.Format("{0}$", fieldName), RegexOptions.IgnoreCase);
			}

			while (infoEnum.MoveNext()) {
				if (re.IsMatch(infoEnum.Name)) { return infoEnum.Name; }
			}
			//BugOut("No serialization data for field {0}", fieldName)
			return null;
		}
	   
		#endregion
	   
		#region ISurrogateSelector
	   
		public ISurrogateSelector GetNextSelector() { return null; }

		/// <summary>
		/// Custom surrogate selection
		/// </summary>
		public ISerializationSurrogate GetSurrogate(Type type,
			StreamingContext context, out ISurrogateSelector selector) {

			selector = this; return this;
			/*
			if (type.Assembly.FullName.StartsWith("AMP")) {
				selector = this; return this;
			} else {
				selector = null; return null;
			}
			*/
		}
	   
		public void ChainSelector(ISurrogateSelector selector) {
			throw new NotImplementedException("ChainSelector not supported");
		}
	   
		#endregion
	   
	}
}
