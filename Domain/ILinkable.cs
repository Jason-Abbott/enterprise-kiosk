using System;

namespace Idaho {
	/// <summary>
	/// Require methods to return bare URL and link for the object
	/// </summary>
	public interface ILinkable {
		string DetailUrl { get; }
		string DetailLink { get; }
	}
}
