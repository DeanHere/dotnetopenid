﻿using System;
using System.Collections.Generic;

namespace DotNetOpenId {
	/// <summary>
	/// A dictionary of handle/Association pairs.
	/// </summary>
	/// <remarks>
	/// Each method is locked, even if it is only one line, so that they are thread safe
	/// against each other, particularly the ones that enumerate over the list, since they
	/// can break if the collection is changed by another thread during enumeration.
	/// </remarks>
	internal class Associations {

		Dictionary<string, Association> assocs;

		/// <summary>
		/// Instantiates a mapping between association handles and <see cref="Association"/> objects.
		/// </summary>
		public Associations() {
			this.assocs = new Dictionary<string, Association>();
		}

		/// <summary>
		/// Stores an <see cref="Association"/> in the collection.
		/// </summary>
		/// <param name="assoc"></param>
		public void Set(Association assoc) {
			lock (this) {
				this.assocs[assoc.Handle] = assoc;
			}
		}

		/// <summary>
		/// Returns the <see cref="Association"/> with the given handle.
		/// </summary>
		public Association Get(string handle) {
			lock (this) {
				Association assoc;
				assocs.TryGetValue(handle, out assoc);
				return assoc;
			}
		}

		/// <summary>
		/// Removes the <see cref="Association"/> with the given handle.
		/// </summary>
		/// <returns>Whether an <see cref="Association"/> with the given handle was in the collection for removal.</returns>
		public bool Remove(string handle) {
			lock (this) {
				return assocs.Remove(handle);
			}
		}

		/// <summary>
		/// Gets the <see cref="Association"/> issued most recently.
		/// </summary>
		public Association Best {
			get {
				lock (this) {
					Association best = null;

					foreach (Association assoc in assocs.Values) {
						if (best == null || best.Issued < assoc.Issued)
							best = assoc;
					}

					return best;
				}
			}
		}

		/// <summary>
		/// Removes all expired associations from the collection.
		/// </summary>
		public void ClearExpired() {
			lock (this) {
				var expireds = new List<Association>(assocs.Count);
				foreach (Association assoc in assocs.Values)
					if (assoc.IsExpired)
						expireds.Add(assoc);
				foreach (Association assoc in expireds)
					assocs.Remove(assoc.Handle);
			}
		}
	}

}
