
using System;
using System.IO;
using System.Collections.Generic;

namespace FastSearcher {
	public class NoteStore {
		private string basedir;
		
		public NoteStore(string basedir) {
			this.basedir = basedir;
		}

		public List<Note> Notes() {
			List<Note> notes = new List<Note>();

			string[] files = Directory.GetFiles(basedir);
			foreach (string file in files) {
				notes.Add(Note.Load(file));
			}

			return notes;
		}

		public List<Note> NotesQuery(string query) {
			List<Note> notes = this.Notes();
			return notes.FindAll(delegate (Note n) { return n.Match(query); });
		}
	}
}
